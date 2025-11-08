using System;

namespace Pusula.Student.Automation.TeacherPortal;

public class StudentLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;

    public string DisplayName => $"{Name} {Surname} ({StudentNumber})";
}
