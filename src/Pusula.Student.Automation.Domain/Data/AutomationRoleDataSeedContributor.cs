using System.Threading.Tasks;
using Pusula.Student.Automation.Authorization;
using Pusula.Student.Automation.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;

namespace Pusula.Student.Automation.Data;

public class AutomationRoleDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IdentityRoleManager _identityRoleManager;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly ICurrentTenant _currentTenant;

    public AutomationRoleDataSeedContributor(
        IdentityRoleManager identityRoleManager,
        IdentityUserManager identityUserManager,
        IGuidGenerator guidGenerator,
        IPermissionDataSeeder permissionDataSeeder,
        ICurrentTenant currentTenant)
    {
        _identityRoleManager = identityRoleManager;
        _identityUserManager = identityUserManager;
        _guidGenerator = guidGenerator;
        _permissionDataSeeder = permissionDataSeeder;
        _currentTenant = currentTenant;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            await EnsureRoleExistsAsync(AutomationRoleNames.Admin, context);
            await EnsureRoleExistsAsync(AutomationRoleNames.Teacher, context);
            await EnsureRoleExistsAsync(AutomationRoleNames.Student, context);

            var adminUser = await _identityUserManager.FindByNameAsync("admin");
            if (adminUser != null && !await _identityUserManager.IsInRoleAsync(adminUser, AutomationRoleNames.Admin))
            {
                await _identityUserManager.AddToRoleAsync(adminUser, AutomationRoleNames.Admin);
            }

            await _permissionDataSeeder.SeedAsync(
                RolePermissionValueProvider.ProviderName,
                AutomationRoleNames.Admin,
                new[] { AutomationPermissions.AdminManagement },
                context?.TenantId);
        }
    }

    private async Task EnsureRoleExistsAsync(string roleName, DataSeedContext context)
    {
        if (await _identityRoleManager.FindByNameAsync(roleName) != null)
        {
            return;
        }

        var role = new IdentityRole(_guidGenerator.Create(), roleName, context?.TenantId);
        await _identityRoleManager.CreateAsync(role);
    }
}
