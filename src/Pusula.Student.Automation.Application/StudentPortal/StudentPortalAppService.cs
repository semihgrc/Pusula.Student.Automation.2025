using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Pusula.Student.Automation.Authorization;
using Pusula.Student.Automation.LessonDailyReports;
using Pusula.Student.Automation.LessonEnrollments;
using Pusula.Student.Automation.Permissions;
using Pusula.Student.Automation.StudentPortal;
using Pusula.Student.Automation.Students;
using Volo.Abp;
using StudentEntity = Pusula.Student.Automation.Students.Student;

namespace Pusula.Student.Automation.StudentPortal;

[Authorize(Roles = AutomationRoleNames.Student)]
public class StudentPortalAppService : AutomationAppService, IStudentPortalAppService
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILessonEnrollmentRepository _lessonEnrollmentRepository;
    private readonly ILessonDailyReportRepository _lessonDailyReportRepository;

    public StudentPortalAppService(
        IStudentRepository studentRepository,
        ILessonEnrollmentRepository lessonEnrollmentRepository,
        ILessonDailyReportRepository lessonDailyReportRepository)
    {
        _studentRepository = studentRepository;
        _lessonEnrollmentRepository = lessonEnrollmentRepository;
        _lessonDailyReportRepository = lessonDailyReportRepository;
    }

    public virtual async Task<StudentDashboardDto> GetDashboardAsync()
    {
        var student = await GetCurrentStudentAsync();
        var enrollments = await _lessonEnrollmentRepository.GetByStudentAsync(student.Id);
        Logger.LogWarning("StudentDashboard: Student {StudentId} requested dashboard. Enrollment count: {EnrollmentCount}", student.Id, enrollments.Count);

        var lessonSummaries = new List<StudentLessonDashboardDto>();

        foreach (var enrollment in enrollments)
        {
            var lesson = enrollment.Lesson;
            if (lesson == null)
            {
                continue;
            }

            var summary = new StudentLessonDashboardDto
            {
                LessonId = lesson.Id,
                LessonName = lesson.Name,
                Status = lesson.Status,
                TeacherName = lesson.Teacher != null
                    ? $"{lesson.Teacher.Name} {lesson.Teacher.Surname}"
                    : string.Empty,
                TeacherTitle = lesson.Teacher?.Title,
                Grade = enrollment.Grade,
                MidtermGrade = enrollment.MidtermGrade,
                FinalGrade = enrollment.FinalGrade,
                AbsenceCount = enrollment.AbsenceCount,
                TeacherComment = enrollment.TeacherComment
            };

            summary.DailyReports = await GetDailyReportsForLessonAsync(lesson.Id, student.Id);
            lessonSummaries.Add(summary);
            Logger.LogDebug("Prepared summary for lesson {LessonId} ({LessonName}) with {ReportCount} daily reports",
                lesson.Id,
                lesson.Name,
                summary.DailyReports.Count);
        }

        var dashboard = new StudentDashboardDto
        {
            StudentId = student.Id,
            Name = student.Name,
            Surname = student.Surname,
            Lessons = lessonSummaries
                .OrderBy(l => l.LessonName)
                .ToList()
        };

        Logger.LogWarning("StudentDashboard: Student {StudentId} dashboard prepared with {LessonCount} lessons", student.Id, dashboard.Lessons.Count);
        return dashboard;
    }

    private async Task<StudentEntity> GetCurrentStudentAsync(CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new BusinessException(AutomationDomainErrorCodes.IdentityUserNotFound);
        }

        var student = await _studentRepository.FindByIdentityUserIdAsync(CurrentUser.Id.Value, cancellationToken);
        if (student == null)
        {
            throw new BusinessException(AutomationDomainErrorCodes.StudentNotFound)
                .WithData(nameof(CurrentUser.Id), CurrentUser.Id);
        }

        Logger.LogDebug("Resolved current student {StudentId} for user {UserId}", student.Id, CurrentUser.Id);
        return student;
    }

    private async Task<List<StudentDailyReportEntryDto>> GetDailyReportsForLessonAsync(Guid lessonId, Guid studentId)
    {
        var reports = await _lessonDailyReportRepository.GetListByLessonAsync(lessonId);
        var reportEntries = reports
            .Select(report => new
            {
                Report = report,
                Entry = report.Entries.FirstOrDefault(e => e.StudentId == studentId)
            })
            .Where(x => x.Entry != null)
            .Select(x => new StudentDailyReportEntryDto
            {
                ReportId = x.Report.Id,
                Date = x.Report.Date,
                IsPresent = x.Entry!.IsPresent,
                DailyGrade = x.Entry.DailyGrade,
                DailyComment = x.Entry.DailyComment
            })
            .OrderByDescending(x => x.Date)
            .ToList();

        Logger.LogDebug("Collected {Count} daily reports for lesson {LessonId} and student {StudentId}", reportEntries.Count, lessonId, studentId);
        return reportEntries;
    }
}
