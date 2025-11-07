using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Pusula.Student.Automation.Enums;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace Pusula.Student.Automation.Students;

public class StudentDto : FullAuditedEntityDto<Guid>, IHasConcurrencyStamp
{
    public Guid IdentityUserId { get; set; }
    public string Name { get; set; } = default!;
    public string Surname { get; set; } = default!;
    public EnumGender Gender { get; set; }
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string StudentNumber { get; set; } = default!;
    public List<StudentLessonDto> Lessons { get; set; } = new();
    public decimal? AverageGrade { get; set; }
    public string ConcurrencyStamp { get; set; } = string.Empty;
}

public class StudentLessonDto
{
    public Guid LessonId { get; set; }
    public string LessonName { get; set; } = default!;
    public LessonStatus Status { get; set; }
    public decimal? Grade { get; set; }
    public string? TeacherComment { get; set; }
    public int AbsenceCount { get; set; }
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = default!;
}

public abstract class StudentCreateUpdateDtoBase : IHasConcurrencyStamp
{
    [Required]
    public Guid IdentityUserId { get; set; }

    [Required]
    [StringLength(StudentConsts.MaxNameLength, MinimumLength = StudentConsts.MinNameLength)]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(StudentConsts.MaxSurnameLength, MinimumLength = StudentConsts.MinSurnameLength)]
    public string Surname { get; set; } = default!;

    [Required]
    public EnumGender Gender { get; set; }

    [Required]
    [StringLength(StudentConsts.MaxEmailLength, MinimumLength = StudentConsts.MinEmailLength)]
    public string Email { get; set; } = default!;

    [Required]
    [StringLength(StudentConsts.MaxPhoneNumberLength, MinimumLength = StudentConsts.MinPhoneNumberLength)]
    public string PhoneNumber { get; set; } = default!;

    [Required]
    [StringLength(StudentConsts.MaxStudentNumberLength, MinimumLength = StudentConsts.MinStudentNumberLength)]
    public string StudentNumber { get; set; } = default!;

    public string ConcurrencyStamp { get; set; } = string.Empty;
}

public class StudentCreateDto : StudentCreateUpdateDtoBase
{
}

public class StudentUpdateDto : StudentCreateUpdateDtoBase
{
}

public class StudentCreateWithIdentityDto : StudentCreateDto
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

public class StudentListRequestDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
