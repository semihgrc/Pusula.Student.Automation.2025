using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Pusula.Student.Automation.Enums;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace Pusula.Student.Automation.Teachers;

public class TeacherDto : FullAuditedEntityDto<Guid>, IHasConcurrencyStamp
{
    public Guid IdentityUserId { get; set; }
    public string Name { get; set; } = default!;
    public string Surname { get; set; } = default!;
    public EnumGender Gender { get; set; }
    public string Title { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string ConcurrencyStamp { get; set; } = string.Empty;
}

public abstract class TeacherCreateUpdateDtoBase : IHasConcurrencyStamp
{
    [Required]
    public Guid IdentityUserId { get; set; }

    [Required]
    [StringLength(TeacherConsts.MaxNameLength, MinimumLength = TeacherConsts.MinNameLength)]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(TeacherConsts.MaxSurnameLength, MinimumLength = TeacherConsts.MinSurnameLength)]
    public string Surname { get; set; } = default!;

    [Required]
    public EnumGender Gender { get; set; }

    [Required]
    [StringLength(TeacherConsts.MaxTitleLength, MinimumLength = TeacherConsts.MinTitleLength)]
    public string Title { get; set; } = default!;

    [Required]
    [StringLength(TeacherConsts.MaxEmailLength, MinimumLength = TeacherConsts.MinEmailLength)]
    public string Email { get; set; } = default!;

    [Required]
    [StringLength(TeacherConsts.MaxPhoneNumberLength, MinimumLength = TeacherConsts.MinPhoneNumberLength)]
    public string PhoneNumber { get; set; } = default!;

    public string ConcurrencyStamp { get; set; } = string.Empty;
}

public class TeacherCreateDto : TeacherCreateUpdateDtoBase
{
}

public class TeacherUpdateDto : TeacherCreateUpdateDtoBase
{
}

public class TeacherCreateWithIdentityDto : TeacherCreateDto
{
    [Required]
    [StringLength(64)]
    public string UserName { get; set; } = default!;

    [Required]
    [StringLength(128)]
    public string Password { get; set; } = default!;

    [JsonIgnore]
    public new Guid IdentityUserId { get; set; }
}

public class TeacherListRequestDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
