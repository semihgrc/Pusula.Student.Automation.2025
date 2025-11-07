using System;
using Pusula.Student.Automation.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Pusula.Student.Automation.Students;

public class Student : FullAuditedAggregateRoot<Guid>, IHasConcurrencyStamp
{
    public Guid IdentityUserId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Surname { get; private set; } = default!;
    public EnumGender Gender { get; private set; }
    public string Email { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public string StudentNumber { get; private set; } = default!;
    public override string ConcurrencyStamp { get; set; } = string.Empty;

    private Student()
    {
    }

    public Student(
        Guid id,
        Guid identityUserId,
        string name,
        string surname,
        EnumGender gender,
        string email,
        string phoneNumber,
        string studentNumber) : base(id)
    {
        SetIdentityUser(identityUserId);
        SetName(name);
        SetSurname(surname);
        SetGender(gender);
        SetEmail(email);
        SetPhoneNumber(phoneNumber);
        SetStudentNumber(studentNumber);
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        Check.Length(Name, nameof(name), StudentConsts.MaxNameLength, StudentConsts.MinNameLength);
    }

    public void SetSurname(string surname)
    {
        Surname = Check.NotNullOrWhiteSpace(surname, nameof(surname));
        Check.Length(Surname, nameof(surname), StudentConsts.MaxSurnameLength, StudentConsts.MinSurnameLength);
    }

    public void SetGender(EnumGender gender)
    {
        Gender = Check.NotNull(gender, nameof(gender));
    }

    public void SetEmail(string email)
    {
        Email = Check.NotNullOrWhiteSpace(email, nameof(email));
        Check.Length(Email, nameof(email), StudentConsts.MaxEmailLength, StudentConsts.MinEmailLength);
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        PhoneNumber = Check.NotNullOrWhiteSpace(phoneNumber, nameof(phoneNumber));
        Check.Length(PhoneNumber, nameof(phoneNumber), StudentConsts.MaxPhoneNumberLength, StudentConsts.MinPhoneNumberLength);
    }

    public void SetStudentNumber(string studentNumber)
    {
        StudentNumber = Check.NotNullOrWhiteSpace(studentNumber, nameof(studentNumber));
        Check.Length(StudentNumber, nameof(studentNumber), StudentConsts.MaxStudentNumberLength, StudentConsts.MinStudentNumberLength);
    }

    public void SetIdentityUser(Guid identityUserId)
    {
        IdentityUserId = Check.NotNull(identityUserId, nameof(identityUserId));
    }
}
