using System;
using System.Threading;
using System.Threading.Tasks;
using Pusula.Student.Automation.Enums;
using Pusula.Student.Automation;
using Pusula.Student.Automation.GlobalExceptions;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Services;

namespace Pusula.Student.Automation.Teachers;

public class TeacherManager : DomainService
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly IAutomationException _automationException;

    public TeacherManager(
        ITeacherRepository teacherRepository,
        IAutomationException automationException)
    {
        _teacherRepository = teacherRepository;
        _automationException = automationException;
    }

    public virtual async Task<Teacher> CreateAsync(
        Guid identityUserId,
        string name,
        string surname,
        EnumGender gender,
        string title,
        string email,
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        await EnsureEmailUniqueAsync(email, null, cancellationToken);

        var teacher = new Teacher(
            GuidGenerator.Create(),
            identityUserId,
            name,
            surname,
            gender,
            title,
            email,
            phoneNumber);

        return await _teacherRepository.InsertAsync(teacher, true, cancellationToken);
    }

    public virtual async Task<Teacher> UpdateAsync(
        Guid id,
        Guid identityUserId,
        string name,
        string surname,
        EnumGender gender,
        string title,
        string email,
        string phoneNumber,
        string? concurrencyStamp = null,
        CancellationToken cancellationToken = default)
    {
        var teacher = await _teacherRepository.GetAsync(id, cancellationToken: cancellationToken);

        await EnsureEmailUniqueAsync(email, id, cancellationToken);

        if (!string.IsNullOrWhiteSpace(concurrencyStamp))
        {
            teacher.ConcurrencyStamp = concurrencyStamp;
        }

        teacher.SetIdentityUser(identityUserId);
        teacher.SetName(name);
        teacher.SetSurname(surname);
        teacher.SetGender(gender);
        teacher.SetTitle(title);
        teacher.SetEmail(email);
        teacher.SetPhoneNumber(phoneNumber);

        return await _teacherRepository.UpdateAsync(teacher, true, cancellationToken);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _teacherRepository.DeleteAsync(id, cancellationToken: cancellationToken);
    }

    private async Task EnsureEmailUniqueAsync(string email, Guid? ignoredId, CancellationToken cancellationToken)
    {
        var existingTeacher = await _teacherRepository.FindByEmailAsync(email, cancellationToken);

        if (existingTeacher != null && existingTeacher.Id != ignoredId)
        {
            _automationException.Throw(
                AutomationDomainErrorCodes.TeacherEmailAlreadyExists,
                "Teacher email already exists.");
        }
    }
}
