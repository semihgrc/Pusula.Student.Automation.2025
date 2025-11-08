using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Pusula.Student.Automation.AdminManagement;
using Pusula.Student.Automation.Authorization;
using Pusula.Student.Automation.Enums;
using Pusula.Student.Automation.Permissions;
using Pusula.Student.Automation.Students;
using Pusula.Student.Automation.Teachers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Volo.Abp.Content;

namespace Pusula.Student.Automation.AdminManagement;

[Authorize(AutomationPermissions.AdminManagement)]
public class AdminManagementAppService : AutomationAppService, IAdminManagementAppService
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly IStudentRepository _studentRepository;

    static AdminManagementAppService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public AdminManagementAppService(
        ITeacherRepository teacherRepository,
        IStudentRepository studentRepository)
    {
        _teacherRepository = teacherRepository;
        _studentRepository = studentRepository;
    }

    public async Task<List<AdminUserSummaryDto>> SearchAsync(AdminUserSearchRequestDto input)
    {
        var result = new List<AdminUserSummaryDto>();

        if (input.Role == AdminUserRole.All || input.Role == AdminUserRole.Teacher)
        {
            var teachers = await _teacherRepository.GetListAsync(
                input.Filter,
                0,
                input.MaxResultCount,
                CancellationToken.None);

            if (input.Gender.HasValue)
            {
                teachers = teachers
                    .Where(t => t.Gender == input.Gender.Value)
                    .ToList();
            }

            result.AddRange(teachers.Select(t => new AdminUserSummaryDto
            {
                Id = t.Id,
                Role = L["Role:Teacher"],
                FullName = $"{t.Name} {t.Surname}",
                Email = t.Email ?? string.Empty,
                PhoneNumber = t.PhoneNumber ?? string.Empty,
                ExtraInfo = t.Title ?? string.Empty
            }));
        }

        if (input.Role == AdminUserRole.All || input.Role == AdminUserRole.Student)
        {
            var students = await _studentRepository.GetListAsync(
                input.Filter,
                0,
                input.MaxResultCount,
                CancellationToken.None);

            if (input.Gender.HasValue)
            {
                students = students
                    .Where(s => s.Gender == input.Gender.Value)
                    .ToList();
            }

            result.AddRange(students.Select(s => new AdminUserSummaryDto
            {
                Id = s.Id,
                Role = L["Role:Student"],
                FullName = $"{s.Name} {s.Surname}",
                Email = s.Email ?? string.Empty,
                PhoneNumber = s.PhoneNumber ?? string.Empty,
                ExtraInfo = s.StudentNumber ?? string.Empty
            }));
        }

        return result
            .OrderBy(x => x.Role)
            .ThenBy(x => x.FullName)
            .ToList();
    }

    public async Task<IRemoteStreamContent> ExportAsync(AdminUserExportRequestDto input)
    {
        var data = await SearchAsync(input);

        return input.Format == AdminUserExportFormat.Pdf
            ? CreatePdf(data)
            : CreateCsv(data);
    }

    private IRemoteStreamContent CreateCsv(IEnumerable<AdminUserSummaryDto> data)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Role,FullName,Email,Phone,Extra");

        foreach (var item in data)
        {
            builder.AppendLine(string.Join(",",
                EscapeCsv(item.Role),
                EscapeCsv(item.FullName),
                EscapeCsv(item.Email),
                EscapeCsv(item.PhoneNumber),
                EscapeCsv(item.ExtraInfo)));
        }

        var bytes = Encoding.UTF8.GetBytes(builder.ToString());
        var stream = new MemoryStream(bytes);
        var fileName = $"admin-users-{Clock.Now:yyyyMMddHHmmss}.csv";
        return new RemoteStreamContent(stream, "text/csv", fileName);
    }

    private IRemoteStreamContent CreatePdf(IReadOnlyList<AdminUserSummaryDto> data)
    {
        var stream = new MemoryStream();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text(L["AdminUserReport"]).FontSize(20).SemiBold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(3);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text(L["Role"]);
                        header.Cell().Element(HeaderCell).Text(L["FullName"]);
                        header.Cell().Element(HeaderCell).Text("Email");
                        header.Cell().Element(HeaderCell).Text(L["PhoneNumber"]);
                        header.Cell().Element(HeaderCell).Text(L["ExtraInfo"]);
                    });

                    foreach (var item in data)
                    {
                        table.Cell().Element(Cell).Text(item.Role);
                        table.Cell().Element(Cell).Text(item.FullName);
                        table.Cell().Element(Cell).Text(item.Email);
                        table.Cell().Element(Cell).Text(item.PhoneNumber);
                        table.Cell().Element(Cell).Text(item.ExtraInfo);
                    }
                });
            });
        }).GeneratePdf(stream);

        stream.Position = 0;
        var fileName = $"admin-users-{Clock.Now:yyyyMMddHHmmss}.pdf";
        return new RemoteStreamContent(stream, "application/pdf", fileName);

        IContainer HeaderCell(IContainer container) =>
            container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);

        IContainer Cell(IContainer container) =>
            container.PaddingVertical(4).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten4);
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var needsQuotes = value.Contains(",") || value.Contains("\"") || value.Contains("\n");
        var escaped = value.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{escaped}\"" : escaped;
    }
}
