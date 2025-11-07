using System;
using System.Collections.Generic;
using Pusula.Student.Automation.Enums;
using Pusula.Student.Automation.LessonEnrollments;
using Pusula.Student.Automation.Teachers;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Pusula.Student.Automation.Lessons;

public class Lesson : FullAuditedAggregateRoot<Guid>, IHasConcurrencyStamp
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public LessonStatus Status { get; private set; }
    public Guid TeacherId { get; private set; }
    public virtual ICollection<LessonEnrollment> Enrollments { get; private set; }
    public virtual Teacher Teacher { get; private set; } = default!;
    public override string ConcurrencyStamp { get; set; } = string.Empty;

    private Lesson()
    {
        Enrollments = new List<LessonEnrollment>();
    }

    public Lesson(
        Guid id,
        Guid teacherId,
        string name,
        string? description = null) : base(id)
    {
        TeacherId = teacherId;
        SetName(name);
        SetDescription(description);
        SetStatus(LessonStatus.Planned);
        Enrollments = new List<LessonEnrollment>();
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        Check.Length(Name, nameof(name), LessonConsts.MaxNameLength, LessonConsts.MinNameLength);
    }

    public void SetDescription(string? description)
    {
        if (!description.IsNullOrWhiteSpace())
        {
            Check.Length(description!, nameof(description), LessonConsts.MaxDescriptionLength);
        }

        Description = description;
    }

    public void SetStatus(LessonStatus status)
    {
        Status = status;
    }

    public void SetTeacher(Guid teacherId)
    {
        TeacherId = Check.NotNull(teacherId, nameof(teacherId));
    }

    public void AddEnrollment(LessonEnrollment enrollment)
    {
        Check.NotNull(enrollment, nameof(enrollment));
        Enrollments.Add(enrollment);
    }

    public void RemoveEnrollment(LessonEnrollment enrollment)
    {
        Check.NotNull(enrollment, nameof(enrollment));
        Enrollments.Remove(enrollment);
    }
}
