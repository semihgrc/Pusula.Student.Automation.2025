using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pusula.Student.Automation.LessonDailyReports;
using Volo.Abp.Application.Dtos;

namespace Pusula.Student.Automation.Lessons;

public class LessonDailyReportDto : FullAuditedEntityDto<Guid>
{
    public Guid LessonId { get; set; }
    public Guid TeacherId { get; set; }
    public DateTime Date { get; set; }
    public List<LessonDailyReportEntryDto> Entries { get; set; } = new();
}

public class LessonDailyReportEntryDto : EntityDto<Guid>
{
    public Guid LessonDailyReportId { get; set; }
    public Guid LessonId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public bool IsPresent { get; set; }
    public decimal? DailyGrade { get; set; }
    public string? DailyComment { get; set; }
}

public class LessonDailyReportSummaryDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public int StudentCount { get; set; }
}

public class LessonDailyReportEntrySaveDto
{
    public Guid? EntryId { get; set; }
    [Required]
    public Guid StudentId { get; set; }
    public bool IsPresent { get; set; }
    public decimal? DailyGrade { get; set; }

    [StringLength(LessonDailyReportConsts.MaxDailyCommentLength)]
    public string? DailyComment { get; set; }
}

public class LessonDailyReportSaveDto
{
    public Guid? ReportId { get; set; }

    [Required]
    public Guid LessonId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public List<LessonDailyReportEntrySaveDto> Entries { get; set; } = new();
}
