using System;
using Pusula.Student.Automation.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;

namespace Pusula.Student.Automation.Teachers;

public class Teacher : FullAuditedAggregateRoot<Guid>, IHasConcurrencyStamp
{
    public Guid IdentityUserId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Surname { get; private set; } = default!;
    public EnumGender Gender { get; private set; }
    public string Title { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public override string ConcurrencyStamp { get; set; } = string.Empty;

    private Teacher()
    {
    }

    public Teacher(
        Guid id,
        Guid identityUserId,
        string name,
        string surname,
        EnumGender gender,
        string title,
        string email,
        string phoneNumber) : base(id)
    {
        SetIdentityUser(identityUserId);
        SetName(name);
        SetSurname(surname);
        SetGender(gender);
        SetTitle(title);
        SetEmail(email);
        SetPhoneNumber(phoneNumber);
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        Check.Length(Name, nameof(name), TeacherConsts.MaxNameLength, TeacherConsts.MinNameLength);
    }

    public void SetSurname(string surname)
    {
        Surname = Check.NotNullOrWhiteSpace(surname, nameof(surname));
        Check.Length(Surname, nameof(surname), TeacherConsts.MaxSurnameLength, TeacherConsts.MinSurnameLength);
    }

    public void SetGender(EnumGender gender)
    {
        Gender = Check.NotNull(gender, nameof(gender));
    }

    public void SetTitle(string title)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title));
        Check.Length(Title, nameof(title), TeacherConsts.MaxTitleLength, TeacherConsts.MinTitleLength);
    }

    public void SetEmail(string email)
    {
        Email = Check.NotNullOrWhiteSpace(email, nameof(email));
        Check.Length(Email, nameof(email), TeacherConsts.MaxEmailLength, TeacherConsts.MinEmailLength);
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        PhoneNumber = Check.NotNullOrWhiteSpace(phoneNumber, nameof(phoneNumber));
        Check.Length(PhoneNumber, nameof(phoneNumber), TeacherConsts.MaxPhoneNumberLength, TeacherConsts.MinPhoneNumberLength);
    }

    public void SetIdentityUser(Guid identityUserId)
    {
        IdentityUserId = Check.NotNull(identityUserId, nameof(identityUserId));
    }
}
