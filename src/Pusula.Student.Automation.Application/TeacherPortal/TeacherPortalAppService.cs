using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Pusula.Student.Automation.Authorization;
using Pusula.Student.Automation.Permissions;
using Pusula.Student.Automation.Students;
using Pusula.Student.Automation.TeacherPortal;
using Pusula.Student.Automation.Teachers;
using Volo.Abp;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using StudentEntity = Pusula.Student.Automation.Students.Student;

namespace Pusula.Student.Automation.TeacherPortal;

[Authorize(AutomationPermissions.TeacherPortal)]
public class TeacherPortalAppService : AutomationAppService, ITeacherPortalAppService
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly StudentManager _studentManager;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IGuidGenerator _guidGenerator;

    public TeacherPortalAppService(
        ITeacherRepository teacherRepository,
        IStudentRepository studentRepository,
        StudentManager studentManager,
        IdentityUserManager identityUserManager,
        IGuidGenerator guidGenerator)
    {
        _teacherRepository = teacherRepository;
        _studentRepository = studentRepository;
        _studentManager = studentManager;
        _identityUserManager = identityUserManager;
        _guidGenerator = guidGenerator;
    }

    public async Task<TeacherPortalProfileDto> GetProfileAsync()
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new BusinessException(AutomationDomainErrorCodes.IdentityUserNotFound);
        }

        var teacher = await _teacherRepository.FindByIdentityUserIdAsync(CurrentUser.Id.Value);
        if (teacher == null)
        {
            throw new BusinessException(AutomationDomainErrorCodes.TeacherNotFound)
                .WithData("IdentityUserId", CurrentUser.Id);
        }

        return new TeacherPortalProfileDto
        {
            TeacherId = teacher.Id,
            IdentityUserId = teacher.IdentityUserId,
            Name = teacher.Name,
            Surname = teacher.Surname,
            Title = teacher.Title
        };
    }

    public async Task<StudentDto> CreateStudentAsync(StudentCreateWithIdentityDto input)
    {
        var identityUserId = _guidGenerator.Create();
        var user = new IdentityUser(
            identityUserId,
            input.UserName,
            input.Email,
            CurrentTenant.Id)
        {
            Name = input.Name,
            Surname = input.Surname
        };

        if (!input.PhoneNumber.IsNullOrWhiteSpace())
        {
            user.SetPhoneNumber(input.PhoneNumber, false);
        }

        ThrowIdentityErrors(await _identityUserManager.CreateAsync(user, input.Password));
        await EnsureIdentityUserRoleAsync(identityUserId, AutomationRoleNames.Student);

        var student = await _studentManager.CreateAsync(
            identityUserId,
            input.Name,
            input.Surname,
            input.Gender,
            input.Email,
            input.PhoneNumber,
            input.StudentNumber,
            CancellationToken.None);

        return ObjectMapper.Map<StudentEntity, StudentDto>(student);
    }

    public async Task<List<StudentLookupDto>> SearchStudentsAsync(string? filter)
    {
        var students = await _studentRepository.GetListAsync(filter, 0, 200, CancellationToken.None);

        return students
            .OrderBy(s => s.Name)
            .ThenBy(s => s.Surname)
            .Select(s => new StudentLookupDto
            {
                Id = s.Id,
                Name = s.Name,
                Surname = s.Surname,
                Email = s.Email ?? string.Empty,
                StudentNumber = s.StudentNumber ?? string.Empty
            })
            .ToList();
    }

    private async Task EnsureIdentityUserRoleAsync(Guid identityUserId, string roleName)
    {
        var user = await _identityUserManager.FindByIdAsync(identityUserId.ToString());
        if (user == null)
        {
            throw new BusinessException(AutomationDomainErrorCodes.IdentityUserNotFound)
                .WithData(nameof(identityUserId), identityUserId);
        }

        if (!await _identityUserManager.IsInRoleAsync(user, roleName))
        {
            await _identityUserManager.AddToRoleAsync(user, roleName);
        }
    }

    private static void ThrowIdentityErrors(IdentityResult identityResult)
    {
        if (identityResult.Succeeded)
        {
            return;
        }

        var message = string.Join(" ", identityResult.Errors.Select(e => e.Description));
        throw new BusinessException(AutomationDomainErrorCodes.IdentityOperationFailed, message);
    }
}
