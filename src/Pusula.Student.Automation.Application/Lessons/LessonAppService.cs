using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pusula.Student.Automation.Authorization;
using Pusula.Student.Automation.LessonDailyReports;
using Pusula.Student.Automation.LessonEnrollments;
using Pusula.Student.Automation.Teachers;
using Pusula.Student.Automation.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Pusula.Student.Automation.Lessons;

[Authorize(Roles = AutomationRoleNames.Admin + "," + AutomationRoleNames.Teacher)]
public class LessonAppService : AutomationAppService, ILessonAppService
{
    private readonly ILessonRepository _lessonRepository;
    private readonly LessonManager _lessonManager;
    private readonly ILessonEnrollmentRepository _lessonEnrollmentRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly ILessonDailyReportRepository _lessonDailyReportRepository;
    public LessonAppService(
        ILessonRepository lessonRepository,
        LessonManager lessonManager,
        ILessonEnrollmentRepository lessonEnrollmentRepository,
        ITeacherRepository teacherRepository,
        ILessonDailyReportRepository lessonDailyReportRepository)
    {
        _lessonRepository = lessonRepository;
        _lessonManager = lessonManager;
        _lessonEnrollmentRepository = lessonEnrollmentRepository;
        _teacherRepository = teacherRepository;
        _lessonDailyReportRepository = lessonDailyReportRepository;
    }

    public virtual async Task<LessonDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lesson = await _lessonRepository.GetAsync(id, includeDetails: true, cancellationToken: cancellationToken);
        return await MapLessonAsync(lesson, cancellationToken, lesson.Enrollments?.ToList());
    }

    public virtual async Task<PagedResultDto<LessonDto>> GetListAsync(LessonListRequestDto input, CancellationToken cancellationToken = default)
    {
        var totalCount = await _lessonRepository.GetCountAsync(
            input.TeacherId,
            input.Filter,
            input.Status,
            cancellationToken);

        var lessons = await _lessonRepository.GetListAsync(
            input.TeacherId,
            input.Filter,
            input.Status,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        var items = new List<LessonDto>();

        foreach (var lesson in lessons)
        {
            items.Add(await MapLessonAsync(lesson, cancellationToken, lesson.Enrollments?.ToList()));
        }

        return new PagedResultDto<LessonDto>(totalCount, items);
    }

    [Authorize(AutomationPermissions.AdminManagement)]
    public virtual async Task<LessonDto> CreateAsync(LessonCreateDto input, CancellationToken cancellationToken = default)
    {
        var lesson = await _lessonManager.CreateAsync(
            input.TeacherId,
            input.Name,
            input.Description,
            cancellationToken);

        return await MapLessonAsync(lesson, cancellationToken);
    }

    [Authorize(AutomationPermissions.AdminManagement)]
    public virtual async Task<LessonDto> UpdateAsync(Guid id, LessonUpdateDto input, CancellationToken cancellationToken = default)
    {
        var lesson = await _lessonManager.UpdateAsync(
            id,
            input.TeacherId,
            input.Name,
            input.Description,
            NormalizeConcurrencyStamp(input.ConcurrencyStamp),
            input.Status,
            cancellationToken);

        return await MapLessonAsync(lesson, cancellationToken);
    }

    [Authorize(AutomationPermissions.AdminManagement)]
    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _lessonManager.DeleteAsync(id, cancellationToken);
    }

    public virtual async Task ChangeStatusAsync(Guid id, LessonStatusUpdateDto input, CancellationToken cancellationToken = default)
    {
        await _lessonManager.ChangeStatusAsync(
            id,
            input.Status,
            NormalizeConcurrencyStamp(input.ConcurrencyStamp),
            cancellationToken);
    }

    public virtual async Task<LessonEnrollmentDto> AddStudentAsync(LessonEnrollmentCreateDto input, CancellationToken cancellationToken = default)
    {
        try
        {
            var enrollment = await _lessonManager.AddStudentAsync(input.LessonId, input.StudentId, cancellationToken);
            var detailedEnrollment = await _lessonEnrollmentRepository.GetAsync(
                enrollment.Id,
                includeDetails: true,
                cancellationToken: cancellationToken);

            return ObjectMapper.Map<LessonEnrollment, LessonEnrollmentDto>(detailedEnrollment);
        }
        catch (DbUpdateException ex) when (IsDuplicateLessonEnrollmentException(ex))
        {
            throw new BusinessException(AutomationDomainErrorCodes.LessonAlreadyHasStudent)
                .WithData(nameof(input.LessonId), input.LessonId)
                .WithData(nameof(input.StudentId), input.StudentId);
        }
    }

    public virtual async Task RemoveStudentAsync(Guid lessonId, Guid studentId, CancellationToken cancellationToken = default)
    {
        await _lessonManager.RemoveStudentAsync(lessonId, studentId, cancellationToken);
        await RemoveStudentReportEntriesAsync(lessonId, studentId, cancellationToken);
    }

    public virtual async Task<LessonEnrollmentDto> UpdateEnrollmentAsync(
        Guid lessonId,
        Guid studentId,
        LessonEnrollmentUpdateDto input,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await _lessonManager.UpdateEnrollmentAsync(
            lessonId,
            studentId,
            input.Grade,
            input.TeacherComment,
            input.AbsenceCount,
            NormalizeConcurrencyStamp(input.ConcurrencyStamp),
            cancellationToken);

        enrollment.SetMidtermGrade(input.MidtermGrade);
        enrollment.SetFinalGrade(input.FinalGrade);

        var detailedEnrollment = await _lessonEnrollmentRepository.GetAsync(
            enrollment.Id,
            includeDetails: true,
            cancellationToken: cancellationToken);

        return ObjectMapper.Map<LessonEnrollment, LessonEnrollmentDto>(detailedEnrollment);
    }

    public virtual async Task<List<LessonEnrollmentDto>> GetLessonStudentsAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        var enrollments = await _lessonEnrollmentRepository.GetByLessonAsync(lessonId, cancellationToken);
        return ObjectMapper.Map<List<LessonEnrollment>, List<LessonEnrollmentDto>>(enrollments);
    }

    [Authorize(Roles = AutomationRoleNames.Admin + "," + AutomationRoleNames.Teacher + "," + AutomationRoleNames.Student)]
    public virtual async Task<List<LessonEnrollmentDto>> GetStudentEnrollmentsAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var enrollments = await _lessonEnrollmentRepository.GetByStudentAsync(studentId, cancellationToken);
        return ObjectMapper.Map<List<LessonEnrollment>, List<LessonEnrollmentDto>>(enrollments);
    }

    public virtual async Task<List<LessonDailyReportSummaryDto>> GetDailyReportsAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        var reports = await _lessonDailyReportRepository.GetListByLessonAsync(lessonId, cancellationToken);
        return reports
            .OrderByDescending(r => r.Date)
            .Select(r => new LessonDailyReportSummaryDto
            {
                Id = r.Id,
                Date = r.Date,
                StudentCount = r.Entries.Count
            })
            .ToList();
    }

    public virtual async Task<LessonDailyReportDto?> GetDailyReportAsync(Guid lessonId, DateTime date, CancellationToken cancellationToken = default)
    {
        var report = await _lessonDailyReportRepository.FindByLessonAndDateAsync(
            lessonId,
            date,
            includeDetails: true,
            cancellationToken: cancellationToken);

        if (report == null)
        {
            return null;
        }

        return await MapDailyReportAsync(report, cancellationToken);
    }
    public virtual async Task<LessonDailyReportDto> SaveDailyReportAsync(LessonDailyReportSaveDto input, CancellationToken cancellationToken = default)
    {
        if (input.Entries == null || input.Entries.Count == 0)
        {
            throw new BusinessException(AutomationDomainErrorCodes.LessonDailyReportHasNoEntries)
                .WithData(nameof(input.LessonId), input.LessonId)
                .WithData(nameof(input.Date), input.Date.ToShortDateString());
        }

        var lesson = await _lessonRepository.GetAsync(input.LessonId, includeDetails: false, cancellationToken: cancellationToken);
        var enrollments = await _lessonEnrollmentRepository.GetByLessonAsync(input.LessonId, cancellationToken);
        var enrollmentDict = enrollments.ToDictionary(e => e.StudentId, e => e);

        foreach (var entry in input.Entries)
        {
            if (!enrollmentDict.ContainsKey(entry.StudentId))
            {
                throw new BusinessException(AutomationDomainErrorCodes.LessonEnrollmentNotFound)
                    .WithData(nameof(entry.StudentId), entry.StudentId);
            }
        }

        LessonDailyReport? report = null;
        if (input.ReportId.HasValue)
        {
            report = await _lessonDailyReportRepository.GetWithDetailsAsync(
                input.ReportId.Value,
                cancellationToken: cancellationToken);

            if (report.LessonId != input.LessonId)
            {
                throw new BusinessException(AutomationDomainErrorCodes.LessonDailyReportNotFound)
                    .WithData(nameof(input.ReportId), input.ReportId);
            }

            report.SetDate(input.Date);
            report.SetTeacher(lesson.TeacherId);
            ApplyDailyReportEntries(report, input);

            await _lessonDailyReportRepository.UpdateAsync(report, autoSave: true, cancellationToken);
        }
        else
        {
            var existingReport = await _lessonDailyReportRepository.FindByLessonAndDateAsync(
                input.LessonId,
                input.Date,
                includeDetails: false,
                cancellationToken: cancellationToken);

            if (existingReport != null)
            {
                throw new BusinessException(AutomationDomainErrorCodes.LessonDailyReportAlreadyExists)
                    .WithData(nameof(input.Date), input.Date.ToShortDateString());
            }

            var deletedReport = await _lessonDailyReportRepository.FindByLessonAndDateIncludingDeletedAsync(
                input.LessonId,
                input.Date,
                includeDetails: true,
                cancellationToken: cancellationToken);

            if (deletedReport != null && deletedReport.IsDeleted)
            {
                deletedReport.IsDeleted = false;
                deletedReport.DeleterId = null;
                deletedReport.DeletionTime = null;
                deletedReport.SetDate(input.Date);
                deletedReport.SetTeacher(lesson.TeacherId);

                var entryEntities = input.Entries
                    .Select(entry => CreateDailyReportEntry(deletedReport.Id, input.LessonId, entry))
                    .ToList();

                deletedReport.ReplaceEntries(entryEntities);

                report = await _lessonDailyReportRepository.UpdateAsync(deletedReport, autoSave: true, cancellationToken);
            }
            else
            {
                report = new LessonDailyReport(GuidGenerator.Create(), input.LessonId, lesson.TeacherId, input.Date);

                var entryEntities = input.Entries
                    .Select(entry => CreateDailyReportEntry(report!.Id, input.LessonId, entry))
                    .ToList();

                report.ReplaceEntries(entryEntities);

                await _lessonDailyReportRepository.InsertAsync(report, autoSave: true, cancellationToken);
            }
        }

        return await MapDailyReportAsync(report!, cancellationToken);
    }

    public virtual async Task DeleteDailyReportAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        await _lessonDailyReportRepository.DeleteAsync(reportId, cancellationToken: cancellationToken);
    }

    private async Task<LessonDto> MapLessonAsync(
        Lesson lesson,
        CancellationToken cancellationToken,
        List<LessonEnrollment>? enrollments = null)
    {
        var dto = ObjectMapper.Map<Lesson, LessonDto>(lesson);

        var enrollmentList = enrollments ?? lesson.Enrollments?.ToList();

        if (enrollmentList == null || enrollmentList.Count == 0)
        {
            enrollmentList = await _lessonEnrollmentRepository.GetByLessonAsync(lesson.Id, cancellationToken);
        }

        dto.Enrollments = ObjectMapper.Map<List<LessonEnrollment>, List<LessonEnrollmentDto>>(enrollmentList);

        if (string.IsNullOrWhiteSpace(dto.TeacherName))
        {
            var teacher = await _teacherRepository.FindAsync(
                lesson.TeacherId,
                cancellationToken: cancellationToken);

            if (teacher != null)
            {
                dto.TeacherName = $"{teacher.Name} {teacher.Surname}";
            }
        }

        return dto;
    }

    private async Task<LessonDailyReportDto> MapDailyReportAsync(
        LessonDailyReport report,
        CancellationToken cancellationToken)
    {
        var dto = ObjectMapper.Map<LessonDailyReport, LessonDailyReportDto>(report);
        var entryDtos = ObjectMapper.Map<List<LessonDailyReportEntry>, List<LessonDailyReportEntryDto>>(report.Entries.ToList());

        var enrollments = await _lessonEnrollmentRepository.GetByLessonAsync(report.LessonId, cancellationToken);
        var enrollmentDict = enrollments.ToDictionary(e => e.StudentId, e => e);

        foreach (var entry in entryDtos)
        {
            if (enrollmentDict.TryGetValue(entry.StudentId, out var enrollment) && enrollment.Student != null)
            {
                entry.StudentName = $"{enrollment.Student.Name} {enrollment.Student.Surname}";
                entry.StudentNumber = enrollment.Student.StudentNumber ?? string.Empty;
            }
        }

        dto.Entries = entryDtos.OrderBy(e => e.StudentName).ToList();
        return dto;
    }

    private LessonDailyReportEntry CreateDailyReportEntry(Guid reportId, Guid lessonId, LessonDailyReportEntrySaveDto entry)
    {
        return new LessonDailyReportEntry(
            entry.EntryId ?? GuidGenerator.Create(),
            reportId,
            lessonId,
            entry.StudentId,
            entry.IsPresent,
            entry.DailyGrade,
            entry.DailyComment);
    }

    private void ApplyDailyReportEntries(LessonDailyReport report, LessonDailyReportSaveDto input)
    {
        var processedEntryIds = new HashSet<Guid>();
        var entriesById = report.Entries.ToDictionary(e => e.Id);
        var entriesByStudent = report.Entries.ToDictionary(e => e.StudentId);

        foreach (var entry in input.Entries)
        {
            LessonDailyReportEntry? target = null;

            if (entry.EntryId.HasValue && entriesById.TryGetValue(entry.EntryId.Value, out var byId))
            {
                target = byId;
            }
            else if (entriesByStudent.TryGetValue(entry.StudentId, out var byStudent))
            {
                target = byStudent;
            }

            if (target != null)
            {
                target.Update(entry.IsPresent, entry.DailyGrade, entry.DailyComment);
                processedEntryIds.Add(target.Id);
            }
            else
            {
                var newEntry = CreateDailyReportEntry(report.Id, report.LessonId, entry);
                report.AddEntry(newEntry);
                processedEntryIds.Add(newEntry.Id);
            }
        }

        var entriesToRemove = report.Entries
            .Where(e => !processedEntryIds.Contains(e.Id))
            .Select(e => e.Id)
            .ToList();

        foreach (var entryId in entriesToRemove)
        {
            report.RemoveEntry(entryId);
        }
    }

    private async Task RemoveStudentReportEntriesAsync(Guid lessonId, Guid studentId, CancellationToken cancellationToken)
    {
        var reports = await _lessonDailyReportRepository.GetListByLessonAsync(lessonId, cancellationToken);
        var reportsToUpdate = new List<LessonDailyReport>();

        foreach (var report in reports)
        {
            var entry = report.Entries.FirstOrDefault(e => e.StudentId == studentId);
            if (entry != null)
            {
                report.RemoveEntry(entry.Id);
                reportsToUpdate.Add(report);
            }
        }

        foreach (var report in reportsToUpdate)
        {
            await _lessonDailyReportRepository.UpdateAsync(report, autoSave: true, cancellationToken);
        }
    }

    private static bool IsDuplicateLessonEnrollmentException(DbUpdateException exception)
    {
        if (exception.InnerException is PostgresException postgresException)
        {
            return postgresException.SqlState == PostgresErrorCodes.UniqueViolation
                   && string.Equals(
                       postgresException.ConstraintName,
                       "IX_AppLessonEnrollments_LessonId_StudentId",
                       StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}
