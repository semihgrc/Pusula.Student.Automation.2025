using System;

namespace Pusula.Student.Automation.AdminManagement;

public class AdminUserSummaryDto
{
    public Guid Id { get; set; }

    public string Role { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string ExtraInfo { get; set; } = string.Empty;
}
