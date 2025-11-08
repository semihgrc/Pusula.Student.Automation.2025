using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Pusula.Student.Automation.LessonDailyReports;

public class LessonDailyReport : FullAuditedAggregateRoot<Guid>, IHasConcurrencyStamp
{
    private readonly List<LessonDailyReportEntry> _entries = new();

    public Guid LessonId { get; private set; }
    public Guid TeacherId { get; private set; }
    public DateTime Date { get; private set; }
    public override string ConcurrencyStamp { get; set; } = string.Empty;

    public IReadOnlyCollection<LessonDailyReportEntry> Entries => _entries.AsReadOnly();

    protected LessonDailyReport()
    {
    }

    public LessonDailyReport(Guid id, Guid lessonId, Guid teacherId, DateTime date) : base(id)
    {
        LessonId = lessonId;
        TeacherId = teacherId;
        SetDate(date);
    }

    public void SetDate(DateTime date)
    {
        Date = date.Date;
    }

    public void SetTeacher(Guid teacherId)
    {
        TeacherId = teacherId;
    }

    public void ReplaceEntries(IEnumerable<LessonDailyReportEntry> entries)
    {
        _entries.Clear();
        _entries.AddRange(entries);
    }

    public LessonDailyReportEntry? FindEntry(Guid studentId)
    {
        return _entries.FirstOrDefault(x => x.StudentId == studentId);
    }
}
