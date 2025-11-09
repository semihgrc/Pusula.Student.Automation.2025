using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
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
    private readonly IDistributedCache _distributedCache;

    private static readonly JsonSerializerOptions CacheSerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan TeacherListCacheDuration = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan TeacherListVersionDuration = TimeSpan.FromHours(6);

    public TeacherAppService(
        ITeacherRepository teacherRepository,
        TeacherManager teacherManager,
        IdentityUserManager identityUserManager,
        IGuidGenerator guidGenerator,
        IDistributedCache distributedCache)
    {
        _teacherRepository = teacherRepository;
        _teacherManager = teacherManager;
        _identityUserManager = identityUserManager;
        _guidGenerator = guidGenerator;
        _distributedCache = distributedCache;
    }

    public virtual async Task<TeacherDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var teacher = await _teacherRepository.GetAsync(id, cancellationToken: cancellationToken);
        return ObjectMapper.Map<Teacher, TeacherDto>(teacher);
    }

    public virtual async Task<PagedResultDto<TeacherDto>> GetListAsync(TeacherListRequestDto input, CancellationToken cancellationToken = default)
    {
        var cacheVersion = await GetTeacherListCacheVersionAsync(cancellationToken);
        var cacheKey = BuildTeacherListCacheKey(input, cacheVersion);
        var cachedBytes = await _distributedCache.GetAsync(cacheKey, cancellationToken);

        if (cachedBytes is { Length: > 0 })
        {
            var cachedItem = JsonSerializer.Deserialize<TeacherListCacheItem>(cachedBytes, CacheSerializerOptions);
            if (cachedItem != null)
            {
                return new PagedResultDto<TeacherDto>(cachedItem.TotalCount, cachedItem.Items);
            }
        }

        var totalCount = await _teacherRepository.GetCountAsync(input.Filter, cancellationToken);
        var teachers = await _teacherRepository.GetListAsync(
            input.Filter,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        var items = ObjectMapper.Map<List<Teacher>, List<TeacherDto>>(teachers);

        await CacheTeacherListAsync(cacheKey, totalCount, items, cancellationToken);

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

        await InvalidateTeacherListCacheAsync(cancellationToken);

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

        await InvalidateTeacherListCacheAsync(cancellationToken);

        return ObjectMapper.Map<Teacher, TeacherDto>(teacher);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _teacherManager.DeleteAsync(id, cancellationToken);
        await InvalidateTeacherListCacheAsync(cancellationToken);
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

        await InvalidateTeacherListCacheAsync(cancellationToken);

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

    private async Task CacheTeacherListAsync(
        string cacheKey,
        long totalCount,
        List<TeacherDto> items,
        CancellationToken cancellationToken)
    {
        var cacheItem = new TeacherListCacheItem
        {
            TotalCount = totalCount,
            Items = items
        };

        var payload = JsonSerializer.SerializeToUtf8Bytes(cacheItem, CacheSerializerOptions);
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TeacherListCacheDuration
        };

        await _distributedCache.SetAsync(cacheKey, payload, cacheOptions, cancellationToken);
    }

    private async Task<string> GetTeacherListCacheVersionAsync(CancellationToken cancellationToken)
    {
        var versionKey = BuildTeacherListVersionKey();
        var existingVersion = await _distributedCache.GetStringAsync(versionKey, cancellationToken);
        if (!existingVersion.IsNullOrWhiteSpace())
        {
            return existingVersion!;
        }

        var newVersion = Guid.NewGuid().ToString("N");
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TeacherListVersionDuration
        };

        await _distributedCache.SetStringAsync(versionKey, newVersion, options, cancellationToken);
        return newVersion;
    }

    private async Task InvalidateTeacherListCacheAsync(CancellationToken cancellationToken)
    {
        var versionKey = BuildTeacherListVersionKey();
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TeacherListVersionDuration
        };

        await _distributedCache.SetStringAsync(versionKey, Guid.NewGuid().ToString("N"), options, cancellationToken);
    }

    private string BuildTeacherListCacheKey(TeacherListRequestDto input, string version)
    {
        var tenant = CurrentTenant.Id?.ToString() ?? "host";
        var normalizedFilter = input.Filter?.Trim() ?? string.Empty;
        var normalizedSorting = input.Sorting?.Trim() ?? string.Empty;
        return $"TeacherList:{tenant}:{version}:{normalizedFilter}:{input.SkipCount}:{input.MaxResultCount}:{normalizedSorting}";
    }

    private string BuildTeacherListVersionKey()
    {
        var tenant = CurrentTenant.Id?.ToString() ?? "host";
        return $"TeacherList:Version:{tenant}";
    }

    private sealed class TeacherListCacheItem
    {
        public long TotalCount { get; set; }
        public List<TeacherDto> Items { get; set; } = new();
    }
}
