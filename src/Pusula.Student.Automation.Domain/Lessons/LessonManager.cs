using System;
using System.Threading;
using System.Threading.Tasks;
using Pusula.Student.Automation.Enums;
using Pusula.Student.Automation;
using Pusula.Student.Automation.GlobalExceptions;
using Pusula.Student.Automation.LessonEnrollments;
using Pusula.Student.Automation.Students;
using Pusula.Student.Automation.Teachers;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Services;

namespace Pusula.Student.Automation.Lessons;

public class LessonManager : DomainService
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILessonEnrollmentRepository _lessonEnrollmentRepository;
    private readonly IAutomationException _automationException;

    public LessonManager(
        ILessonRepository lessonRepository,
        ITeacherRepository teacherRepository,
        IStudentRepository studentRepository,
        ILessonEnrollmentRepository lessonEnrollmentRepository,
        IAutomationException automationException)
    {
        _lessonRepository = lessonRepository;
        _teacherRepository = teacherRepository;
        _studentRepository = studentRepository;
        _lessonEnrollmentRepository = lessonEnrollmentRepository;
        _automationException = automationException;
    }

    public virtual async Task<Lesson> CreateAsync(
        Guid teacherId,
        string name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        await EnsureTeacherExistsAsync(teacherId, cancellationToken);

        var lesson = new Lesson(
            GuidGenerator.Create(),
            teacherId,
            name,
            description);

        return await _lessonRepository.InsertAsync(lesson, true, cancellationToken);
    }

    public virtual async Task<Lesson> UpdateAsync(
        Guid id,
        Guid teacherId,
        string name,
        string? description,
        string? concurrencyStamp = null,
        LessonStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var lesson = await _lessonRepository.GetAsync(id, cancellationToken: cancellationToken);

        await EnsureTeacherExistsAsync(teacherId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(concurrencyStamp))
        {
            lesson.ConcurrencyStamp = concurrencyStamp!;
        }
        lesson.SetTeacher(teacherId);
        lesson.SetName(name);
        lesson.SetDescription(description);
        if (status.HasValue)
        {
            lesson.SetStatus(status.Value);
        }

        return await _lessonRepository.UpdateAsync(lesson, true, cancellationToken);
    }

    public virtual async Task ChangeStatusAsync(
        Guid lessonId,
        LessonStatus status,
        string? concurrencyStamp = null,
        CancellationToken cancellationToken = default)
    {
        var lesson = await _lessonRepository.GetAsync(lessonId, cancellationToken: cancellationToken);
        if (!string.IsNullOrWhiteSpace(concurrencyStamp))
        {
            lesson.ConcurrencyStamp = concurrencyStamp!;
        }
        lesson.SetStatus(status);

        await _lessonRepository.UpdateAsync(lesson, true, cancellationToken);
    }

    public virtual async Task DeleteAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        await _lessonRepository.DeleteAsync(lessonId, cancellationToken: cancellationToken);
    }

    public virtual async Task<LessonEnrollment> AddStudentAsync(
        Guid lessonId,
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        await EnsureLessonExistsAsync(lessonId, cancellationToken);
        await EnsureStudentExistsAsync(studentId, cancellationToken);

        var existingEnrollment = await _lessonEnrollmentRepository.FindAsync(lessonId, studentId, cancellationToken);
        _automationException.ThrowIf(
            existingEnrollment != null,
            AutomationDomainErrorCodes.LessonAlreadyHasStudent,
            "Student already assigned to this lesson.");

        var enrollment = new LessonEnrollment(GuidGenerator.Create(), lessonId, studentId);

        return await _lessonEnrollmentRepository.InsertAsync(enrollment, true, cancellationToken);
    }

    public virtual async Task RemoveStudentAsync(Guid lessonId, Guid studentId, CancellationToken cancellationToken = default)
    {
        var enrollment = await GetEnrollmentAsync(lessonId, studentId, cancellationToken);
        await _lessonEnrollmentRepository.DeleteAsync(enrollment, cancellationToken: cancellationToken);
    }

    public virtual async Task<LessonEnrollment> UpdateGradeAsync(
        Guid lessonId,
        Guid studentId,
        decimal? grade,
        string? concurrencyStamp = null,
        CancellationToken cancellationToken = default)
    {
        return await UpdateEnrollmentInternalAsync(
            lessonId,
            studentId,
            concurrencyStamp,
            grade,
            null,
            null,
            cancellationToken);
    }

    public virtual async Task<LessonEnrollment> UpdateTeacherCommentAsync(
        Guid lessonId,
        Guid studentId,
        string? comment,
        string? concurrencyStamp = null,
        CancellationToken cancellationToken = default)
    {
        return await UpdateEnrollmentInternalAsync(
            lessonId,
            studentId,
            concurrencyStamp,
            null,
            comment,
            null,
            cancellationToken);
    }

    public virtual async Task<LessonEnrollment> UpdateAbsenceAsync(
        Guid lessonId,
        Guid studentId,
        int absenceCount,
        string? concurrencyStamp = null,
        CancellationToken cancellationToken = default)
    {
        return await UpdateEnrollmentInternalAsync(
            lessonId,
            studentId,
            concurrencyStamp,
            null,
            null,
            absenceCount,
            cancellationToken);
    }

    public virtual Task<LessonEnrollment> UpdateEnrollmentAsync(
        Guid lessonId,
        Guid studentId,
        decimal? grade,
        string? comment,
        int? absenceCount,
        string? concurrencyStamp = null,
        CancellationToken cancellationToken = default)
    {
        return UpdateEnrollmentInternalAsync(
            lessonId,
            studentId,
            concurrencyStamp,
            grade,
            comment,
            absenceCount,
            cancellationToken);
    }

    private async Task EnsureTeacherExistsAsync(Guid teacherId, CancellationToken cancellationToken)
    {
        var teacher = await _teacherRepository.FindAsync(
            teacherId,
            cancellationToken: cancellationToken);
        _automationException.ThrowIf(
            teacher == null,
            AutomationDomainErrorCodes.TeacherNotFound,
            "Teacher not found.");
    }

    private async Task EnsureStudentExistsAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.FindAsync(
            studentId,
            cancellationToken: cancellationToken);
        _automationException.ThrowIf(
            student == null,
            AutomationDomainErrorCodes.StudentNotFound,
            "Student not found.");
    }

    private async Task EnsureLessonExistsAsync(Guid lessonId, CancellationToken cancellationToken)
    {
        var lesson = await _lessonRepository.FindAsync(
            lessonId,
            cancellationToken: cancellationToken);
        _automationException.ThrowIf(
            lesson == null,
            AutomationDomainErrorCodes.LessonNotFound,
            "Lesson not found.");
    }

    private async Task<LessonEnrollment> GetEnrollmentAsync(Guid lessonId, Guid studentId, CancellationToken cancellationToken)
    {
        var enrollment = await _lessonEnrollmentRepository.FindAsync(lessonId, studentId, cancellationToken);

        if (enrollment == null)
        {
            _automationException.Throw(
                AutomationDomainErrorCodes.LessonEnrollmentNotFound,
                "Lesson enrollment not found.");
        }

        return enrollment!;
    }

    private async Task<LessonEnrollment> UpdateEnrollmentInternalAsync(
        Guid lessonId,
        Guid studentId,
        string? concurrencyStamp,
        decimal? grade,
        string? comment,
        int? absenceCount,
        CancellationToken cancellationToken)
    {
        var enrollment = await GetEnrollmentAsync(lessonId, studentId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(concurrencyStamp))
        {
            enrollment.ConcurrencyStamp = concurrencyStamp!;
        }

        if (grade.HasValue)
        {
            enrollment.SetGrade(grade);
        }

        if (comment != null)
        {
            enrollment.SetTeacherComment(comment);
        }

        if (absenceCount.HasValue)
        {
            enrollment.SetAbsenceCount(absenceCount.Value);
        }

        return await _lessonEnrollmentRepository.UpdateAsync(enrollment, true, cancellationToken);
    }
}
