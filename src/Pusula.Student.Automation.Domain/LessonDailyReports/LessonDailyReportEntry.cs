using System;
using Pusula.Student.Automation.LessonDailyReports;
using Pusula.Student.Automation.LessonEnrollments;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Pusula.Student.Automation.LessonDailyReports;

public class LessonDailyReportEntry : Entity<Guid>, IHasConcurrencyStamp
{
    public Guid LessonDailyReportId { get; private set; }
    public Guid LessonId { get; private set; }
    public Guid StudentId { get; private set; }
    public bool IsPresent { get; private set; }
    public decimal? DailyGrade { get; private set; }
    public string? DailyComment { get; private set; }
    public string ConcurrencyStamp { get; set; } = string.Empty;

    protected LessonDailyReportEntry()
    {
    }

    public LessonDailyReportEntry(
        Guid id,
        Guid lessonDailyReportId,
        Guid lessonId,
        Guid studentId,
        bool isPresent,
        decimal? dailyGrade,
        string? dailyComment) : base(id)
    {
        LessonDailyReportId = lessonDailyReportId;
        LessonId = lessonId;
        StudentId = studentId;
        SetPresence(isPresent);
        SetDailyGrade(dailyGrade);
        SetDailyComment(dailyComment);
    }

    public void Update(bool isPresent, decimal? grade, string? comment)
    {
        SetPresence(isPresent);
        SetDailyGrade(grade);
        SetDailyComment(comment);
    }

    public void SetPresence(bool isPresent)
    {
        IsPresent = isPresent;
    }

    public void SetDailyGrade(decimal? grade)
    {
        if (grade.HasValue)
        {
            Check.Range(grade.Value, nameof(grade), LessonEnrollmentConsts.MinGrade, LessonEnrollmentConsts.MaxGrade);
        }

        DailyGrade = grade;
    }

    public void SetDailyComment(string? comment)
    {
        if (!comment.IsNullOrWhiteSpace())
        {
            Check.Length(comment!, nameof(comment), LessonDailyReportConsts.MaxDailyCommentLength);
        }

        DailyComment = comment;
    }
}
