using System;
using System.Collections.Generic;
using Pusula.Student.Automation.Enums;

namespace Pusula.Student.Automation.StudentPortal;

public class StudentDashboardDto
{
    public Guid StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public List<StudentLessonDashboardDto> Lessons { get; set; } = new();
}

public class StudentLessonDashboardDto
{
    public Guid LessonId { get; set; }
    public string LessonName { get; set; } = string.Empty;
    public LessonStatus Status { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string? TeacherTitle { get; set; }
    public decimal? Grade { get; set; }
    public decimal? MidtermGrade { get; set; }
    public decimal? FinalGrade { get; set; }
    public int AbsenceCount { get; set; }
    public string? TeacherComment { get; set; }
    public List<StudentDailyReportEntryDto> DailyReports { get; set; } = new();
}

public class StudentDailyReportEntryDto
{
    public Guid ReportId { get; set; }
    public DateTime Date { get; set; }
    public bool IsPresent { get; set; }
    public decimal? DailyGrade { get; set; }
    public string? DailyComment { get; set; }
}
