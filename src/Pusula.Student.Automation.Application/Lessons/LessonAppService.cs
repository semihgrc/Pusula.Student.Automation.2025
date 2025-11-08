using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
        var enrollment = await _lessonManager.AddStudentAsync(input.LessonId, input.StudentId, cancellationToken);
        var detailedEnrollment = await _lessonEnrollmentRepository.GetAsync(
            enrollment.Id,
            includeDetails: true,
            cancellationToken: cancellationToken);

        return ObjectMapper.Map<LessonEnrollment, LessonEnrollmentDto>(detailedEnrollment);
    }

    public virtual async Task RemoveStudentAsync(Guid lessonId, Guid studentId, CancellationToken cancellationToken = default)
    {
        await _lessonManager.RemoveStudentAsync(lessonId, studentId, cancellationToken);
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

    public virtual async Task<LessonDailyReportDto> GetDailyReportAsync(Guid lessonId, DateTime date, CancellationToken cancellationToken = default)
    {
        var report = await _lessonDailyReportRepository.FindByLessonAndDateAsync(lessonId, date, includeDetails: true, cancellationToken: cancellationToken);
        if (report == null)
        {
            throw new BusinessException(AutomationDomainErrorCodes.LessonDailyReportNotFound)
                .WithData(nameof(lessonId), lessonId)
                .WithData(nameof(date), date.ToShortDateString());
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
            report = await _lessonDailyReportRepository.GetAsync(input.ReportId.Value, includeDetails: true, cancellationToken: cancellationToken);
            if (report.LessonId != input.LessonId)
            {
                throw new BusinessException(AutomationDomainErrorCodes.LessonDailyReportNotFound)
                    .WithData(nameof(input.ReportId), input.ReportId);
            }
            report.SetDate(input.Date);
            report.SetTeacher(lesson.TeacherId);
        }
        else
        {
            var existingReport = await _lessonDailyReportRepository.FindByLessonAndDateAsync(input.LessonId, input.Date, includeDetails: false, cancellationToken: cancellationToken);
            if (existingReport != null)
            {
                throw new BusinessException(AutomationDomainErrorCodes.LessonDailyReportAlreadyExists)
                    .WithData(nameof(input.Date), input.Date.ToShortDateString());
            }

            report = new LessonDailyReport(GuidGenerator.Create(), input.LessonId, lesson.TeacherId, input.Date);
        }

        var entryEntities = input.Entries
            .Select(entry =>
                new LessonDailyReportEntry(
                    entry.EntryId ?? GuidGenerator.Create(),
                    report!.Id,
                    input.LessonId,
                    entry.StudentId,
                    entry.IsPresent,
                    entry.DailyGrade,
                    entry.DailyComment))
            .ToList();

        report.ReplaceEntries(entryEntities);

        if (input.ReportId.HasValue)
        {
            await _lessonDailyReportRepository.UpdateAsync(report, autoSave: true, cancellationToken);
        }
        else
        {
            await _lessonDailyReportRepository.InsertAsync(report, autoSave: true, cancellationToken);
        }

        return await MapDailyReportAsync(report, cancellationToken);
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
}
