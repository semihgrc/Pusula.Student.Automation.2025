using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Pusula.Student.Automation.Authorization;
using Pusula.Student.Automation.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace Pusula.Student.Automation.Teachers;

[Authorize(AutomationPermissions.AdminManagement)]
public class TeacherAppService : AutomationAppService, ITeacherAppService
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly TeacherManager _teacherManager;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IGuidGenerator _guidGenerator;

    public TeacherAppService(
        ITeacherRepository teacherRepository,
        TeacherManager teacherManager,
        IdentityUserManager identityUserManager,
        IGuidGenerator guidGenerator)
    {
        _teacherRepository = teacherRepository;
        _teacherManager = teacherManager;
        _identityUserManager = identityUserManager;
        _guidGenerator = guidGenerator;
    }

    public virtual async Task<TeacherDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var teacher = await _teacherRepository.GetAsync(id, cancellationToken: cancellationToken);
        return ObjectMapper.Map<Teacher, TeacherDto>(teacher);
    }

    public virtual async Task<PagedResultDto<TeacherDto>> GetListAsync(TeacherListRequestDto input, CancellationToken cancellationToken = default)
    {
        var totalCount = await _teacherRepository.GetCountAsync(input.Filter, cancellationToken);
        var teachers = await _teacherRepository.GetListAsync(
            input.Filter,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        var items = ObjectMapper.Map<List<Teacher>, List<TeacherDto>>(teachers);

        return new PagedResultDto<TeacherDto>(totalCount, items);
    }

    public virtual async Task<TeacherDto> CreateAsync(TeacherCreateDto input, CancellationToken cancellationToken = default)
    {
        await EnsureIdentityUserRoleAsync(input.IdentityUserId, AutomationRoleNames.Teacher);

        var teacher = await _teacherManager.CreateAsync(
            input.IdentityUserId,
            input.Name,
            input.Surname,
            input.Gender,
            input.Title,
            input.Email,
            input.PhoneNumber,
            cancellationToken);

        return ObjectMapper.Map<Teacher, TeacherDto>(teacher);
    }

    public virtual async Task<TeacherDto> UpdateAsync(Guid id, TeacherUpdateDto input, CancellationToken cancellationToken = default)
    {
        await EnsureIdentityUserRoleAsync(input.IdentityUserId, AutomationRoleNames.Teacher);

        var teacher = await _teacherManager.UpdateAsync(
            id,
            input.IdentityUserId,
            input.Name,
            input.Surname,
            input.Gender,
            input.Title,
            input.Email,
            input.PhoneNumber,
            NormalizeConcurrencyStamp(input.ConcurrencyStamp),
            cancellationToken);

        return ObjectMapper.Map<Teacher, TeacherDto>(teacher);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _teacherManager.DeleteAsync(id, cancellationToken);
    }

    public virtual async Task<TeacherDto> CreateWithIdentityAsync(TeacherCreateWithIdentityDto input, CancellationToken cancellationToken = default)
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

        await EnsureIdentityUserRoleAsync(identityUserId, AutomationRoleNames.Teacher);

        var teacher = await _teacherManager.CreateAsync(
            identityUserId,
            input.Name,
            input.Surname,
            input.Gender,
            input.Title,
            input.Email,
            input.PhoneNumber,
            cancellationToken);

        return ObjectMapper.Map<Teacher, TeacherDto>(teacher);
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
