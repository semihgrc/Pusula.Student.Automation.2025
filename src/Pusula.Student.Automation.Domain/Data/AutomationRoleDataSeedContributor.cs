using System.Threading.Tasks;
using Pusula.Student.Automation.Authorization;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace Pusula.Student.Automation.Data;

public class AutomationRoleDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IdentityRoleManager _identityRoleManager;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IGuidGenerator _guidGenerator;

    public AutomationRoleDataSeedContributor(
        IdentityRoleManager identityRoleManager,
        IdentityUserManager identityUserManager,
        IGuidGenerator guidGenerator)
    {
        _identityRoleManager = identityRoleManager;
        _identityUserManager = identityUserManager;
        _guidGenerator = guidGenerator;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await EnsureRoleExistsAsync(AutomationRoleNames.Admin, context);
        await EnsureRoleExistsAsync(AutomationRoleNames.Teacher, context);
        await EnsureRoleExistsAsync(AutomationRoleNames.Student, context);

        var adminUser = await _identityUserManager.FindByNameAsync("admin");
        if (adminUser != null && !await _identityUserManager.IsInRoleAsync(adminUser, AutomationRoleNames.Admin))
        {
            await _identityUserManager.AddToRoleAsync(adminUser, AutomationRoleNames.Admin);
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
