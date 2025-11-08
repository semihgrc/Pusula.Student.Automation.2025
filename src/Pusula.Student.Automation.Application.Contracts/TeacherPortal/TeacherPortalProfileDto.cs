using System;

namespace Pusula.Student.Automation.TeacherPortal;

public class TeacherPortalProfileDto
{
    public Guid TeacherId { get; set; }
    public Guid IdentityUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Title { get; set; }
}
