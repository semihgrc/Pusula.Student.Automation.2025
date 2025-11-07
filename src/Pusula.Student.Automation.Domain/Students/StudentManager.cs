using System;
using System.Threading;
using System.Threading.Tasks;
using Pusula.Student.Automation.Enums;
using Pusula.Student.Automation;
using Pusula.Student.Automation.GlobalExceptions;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Services;

namespace Pusula.Student.Automation.Students;

public class StudentManager : DomainService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAutomationException _automationException;

    public StudentManager(
        IStudentRepository studentRepository,
        IAutomationException automationException)
    {
        _studentRepository = studentRepository;
        _automationException = automationException;
    }

    public virtual async Task<Student> CreateAsync(
        Guid identityUserId,
        string name,
        string surname,
        EnumGender gender,
        string email,
        string phoneNumber,
        string studentNumber,
        CancellationToken cancellationToken = default)
    {
        await EnsureEmailUniqueAsync(email, null, cancellationToken);
        await EnsureStudentNumberUniqueAsync(studentNumber, null, cancellationToken);

        var student = new Student(
            GuidGenerator.Create(),
            identityUserId,
            name,
            surname,
            gender,
            email,
            phoneNumber,
            studentNumber);

        return await _studentRepository.InsertAsync(student, true, cancellationToken);
    }

    public virtual async Task<Student> UpdateAsync(
        Guid id,
        Guid identityUserId,
        string name,
        string surname,
        EnumGender gender,
        string email,
        string phoneNumber,
        string studentNumber,
        string? concurrencyStamp = null,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetAsync(id, cancellationToken: cancellationToken);

        await EnsureEmailUniqueAsync(email, id, cancellationToken);
        await EnsureStudentNumberUniqueAsync(studentNumber, id, cancellationToken);

        if (!string.IsNullOrWhiteSpace(concurrencyStamp))
        {
            student.ConcurrencyStamp = concurrencyStamp;
        }

        student.SetIdentityUser(identityUserId);
        student.SetName(name);
        student.SetSurname(surname);
        student.SetGender(gender);
        student.SetEmail(email);
        student.SetPhoneNumber(phoneNumber);
        student.SetStudentNumber(studentNumber);

        return await _studentRepository.UpdateAsync(student, true, cancellationToken);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _studentRepository.DeleteAsync(id, cancellationToken: cancellationToken);
    }

    private async Task EnsureEmailUniqueAsync(string email, Guid? ignoredId, CancellationToken cancellationToken)
    {
        var existingStudent = await _studentRepository.FindByEmailAsync(email, cancellationToken);

        if (existingStudent != null && existingStudent.Id != ignoredId)
        {
            _automationException.Throw(
                AutomationDomainErrorCodes.StudentEmailAlreadyExists,
                "Student email already exists.");
        }
    }

    private async Task EnsureStudentNumberUniqueAsync(string studentNumber, Guid? ignoredId, CancellationToken cancellationToken)
    {
        var existingStudent = await _studentRepository.FindByStudentNumberAsync(studentNumber, cancellationToken);

        if (existingStudent != null && existingStudent.Id != ignoredId)
        {
            _automationException.Throw(
                AutomationDomainErrorCodes.StudentNumberAlreadyExists,
                "Student number already exists.");
        }
    }
}
