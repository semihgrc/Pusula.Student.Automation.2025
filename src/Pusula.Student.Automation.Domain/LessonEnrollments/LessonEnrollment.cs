using System;
using Pusula.Student.Automation.Students;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Pusula.Student.Automation.LessonEnrollments;

public class LessonEnrollment : FullAuditedAggregateRoot<Guid>, IHasConcurrencyStamp
{
    public Guid LessonId { get; private set; }
    public Guid StudentId { get; private set; }
    public decimal? Grade { get; private set; }
    public decimal? MidtermGrade { get; private set; }
    public decimal? FinalGrade { get; private set; }
    public string? TeacherComment { get; private set; }
    public int AbsenceCount { get; private set; }

    public virtual Lessons.Lesson Lesson { get; private set; } = default!;
    public virtual Students.Student Student { get; private set; } = default!;
    public override string ConcurrencyStamp { get; set; } = string.Empty;

    private LessonEnrollment()
    {
    }

    public LessonEnrollment(Guid id, Guid lessonId, Guid studentId) : base(id)
    {
        LessonId = lessonId;
        StudentId = studentId;
        AbsenceCount = LessonEnrollmentConsts.MinAbsenceCount;
    }

    public void SetGrade(decimal? grade)
    {
        if (grade.HasValue)
        {
            Check.Range(
                grade.Value,
                nameof(grade),
                LessonEnrollmentConsts.MinGrade,
                LessonEnrollmentConsts.MaxGrade);
        }

        Grade = grade;
    }

    public void SetTeacherComment(string? comment)
    {
        if (!comment.IsNullOrWhiteSpace())
        {
            Check.Length(comment!, nameof(comment), LessonEnrollmentConsts.MaxTeacherCommentLength);
        }

        TeacherComment = comment;
    }

    public void SetMidtermGrade(decimal? grade)
    {
        if (grade.HasValue)
        {
            Check.Range(
                grade.Value,
                nameof(grade),
                LessonEnrollmentConsts.MinGrade,
                LessonEnrollmentConsts.MaxGrade);
        }

        MidtermGrade = grade;
    }

    public void SetFinalGrade(decimal? grade)
    {
        if (grade.HasValue)
        {
            Check.Range(
                grade.Value,
                nameof(grade),
                LessonEnrollmentConsts.MinGrade,
                LessonEnrollmentConsts.MaxGrade);
        }

        FinalGrade = grade;
    }

    public void SetAbsenceCount(int absenceCount)
    {
        Check.Range(
            absenceCount,
            nameof(absenceCount),
            LessonEnrollmentConsts.MinAbsenceCount,
            LessonEnrollmentConsts.MaxAbsenceCount);

        AbsenceCount = absenceCount;
    }

    public void IncreaseAbsence(int amount = 1)
    {
        SetAbsenceCount(AbsenceCount + amount);
    }
}
