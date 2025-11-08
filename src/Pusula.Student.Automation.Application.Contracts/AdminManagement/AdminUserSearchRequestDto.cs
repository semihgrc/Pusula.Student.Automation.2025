using System.ComponentModel.DataAnnotations;
using Pusula.Student.Automation.Enums;
using Volo.Abp.Application.Dtos;

namespace Pusula.Student.Automation.AdminManagement;

public class AdminUserSearchRequestDto : LimitedResultRequestDto
{
    public string? Filter { get; set; }

    public AdminUserRole Role { get; set; } = AdminUserRole.All;

    public EnumGender? Gender { get; set; }

    [Range(1, 5000)]
    public override int MaxResultCount { get; set; } = 200;
}
