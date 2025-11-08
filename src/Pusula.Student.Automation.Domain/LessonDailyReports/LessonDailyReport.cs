using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Pusula.Student.Automation.LessonDailyReports;

public class LessonDailyReport : FullAuditedAggregateRoot<Guid>, IHasConcurrencyStamp
{
    public Guid LessonId { get; private set; }
    public Guid TeacherId { get; private set; }
    public DateTime Date { get; private set; }
    public override string ConcurrencyStamp { get; set; } = string.Empty;

    public virtual ICollection<LessonDailyReportEntry> Entries { get; private set; } = new List<LessonDailyReportEntry>();

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
        Entries.Clear();
        foreach (var entry in entries)
        {
            Entries.Add(entry);
        }
    }

    public void AddEntry(LessonDailyReportEntry entry)
    {
        var existingEntry = Entries.FirstOrDefault(x => x.Id == entry.Id || x.StudentId == entry.StudentId);
        if (existingEntry != null)
        {
            Entries.Remove(existingEntry);
        }

        Entries.Add(entry);
    }

    public void RemoveEntry(Guid entryId)
    {
        var existingEntry = Entries.FirstOrDefault(x => x.Id == entryId);
        if (existingEntry != null)
        {
            Entries.Remove(existingEntry);
        }
    }

    public LessonDailyReportEntry? FindEntry(Guid studentId)
    {
        return Entries.FirstOrDefault(x => x.StudentId == studentId);
    }
}
