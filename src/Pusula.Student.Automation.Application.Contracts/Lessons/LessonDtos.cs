using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pusula.Student.Automation.Enums;
using Pusula.Student.Automation.LessonEnrollments;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace Pusula.Student.Automation.Lessons;

public class LessonDto : FullAuditedEntityDto<Guid>, IHasConcurrencyStamp
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public LessonStatus Status { get; set; }
    public Guid TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public List<LessonEnrollmentDto> Enrollments { get; set; } = new();
    public string ConcurrencyStamp { get; set; } = string.Empty;
}

public abstract class LessonCreateUpdateDtoBase : IHasConcurrencyStamp
{
    [Required]
    public Guid TeacherId { get; set; }

    [Required]
    [StringLength(LessonConsts.MaxNameLength, MinimumLength = LessonConsts.MinNameLength)]
    public string Name { get; set; } = default!;

    [StringLength(LessonConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    public string ConcurrencyStamp { get; set; } = string.Empty;
}

public class LessonCreateDto : LessonCreateUpdateDtoBase
{
}

public class LessonUpdateDto : LessonCreateUpdateDtoBase
{
    [Required]
    public LessonStatus Status { get; set; }
}

public class LessonStatusUpdateDto : IHasConcurrencyStamp
{
    [Required]
    public LessonStatus Status { get; set; }

    public string ConcurrencyStamp { get; set; } = string.Empty;
}

public class LessonListRequestDto : PagedAndSortedResultRequestDto
{
    public Guid? TeacherId { get; set; }
    public LessonStatus? Status { get; set; }
    public string? Filter { get; set; }
}

public class LessonEnrollmentDto : FullAuditedEntityDto<Guid>, IHasConcurrencyStamp
{
    public Guid LessonId { get; set; }
    public Guid StudentId { get; set; }
    public decimal? Grade { get; set; }
    public decimal? MidtermGrade { get; set; }
    public decimal? FinalGrade { get; set; }
    public string? TeacherComment { get; set; }
    public int AbsenceCount { get; set; }
    public string StudentName { get; set; } = default!;
    public string StudentNumber { get; set; } = default!;
    public string ConcurrencyStamp { get; set; } = string.Empty;
}

public class LessonEnrollmentCreateDto
{
    [Required]
    public Guid LessonId { get; set; }

    [Required]
    public Guid StudentId { get; set; }
}

public class LessonEnrollmentUpdateDto : IHasConcurrencyStamp
{
    public decimal? Grade { get; set; }
    public decimal? MidtermGrade { get; set; }
    public decimal? FinalGrade { get; set; }

    [StringLength(LessonEnrollmentConsts.MaxTeacherCommentLength)]
    public string? TeacherComment { get; set; }

    public int? AbsenceCount { get; set; }

    public string ConcurrencyStamp { get; set; } = string.Empty;
}
