namespace Pusula.Student.Automation.AdminManagement;

public class AdminUserExportRequestDto : AdminUserSearchRequestDto
{
    public AdminUserExportFormat Format { get; set; } = AdminUserExportFormat.Csv;
}
