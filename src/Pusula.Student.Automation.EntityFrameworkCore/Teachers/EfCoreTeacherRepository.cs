using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pusula.Student.Automation.Teachers;
using Volo.Abp;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Pusula.Student.Automation.EntityFrameworkCore.Teachers;

public class EfCoreTeacherRepository
    : EfCoreRepository<AutomationDbContext, Teacher, Guid>,
        ITeacherRepository
{
    public EfCoreTeacherRepository(IDbContextProvider<AutomationDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<Teacher?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<Teacher?> FindByIdentityUserIdAsync(Guid identityUserId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.IdentityUserId == identityUserId, cancellationToken);
    }

    public async Task<List<Teacher>> GetListAsync(
        string? filter = null,
        int skipCount = 0,
        int maxResultCount = int.MaxValue,
        CancellationToken cancellationToken = default)
    {
        var query = await ApplyFilterAsync(filter);

        return await query
            .OrderBy(t => t.Name)
            .ThenBy(t => t.Surname)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(string? filter = null, CancellationToken cancellationToken = default)
    {
        var query = await ApplyFilterAsync(filter);
        return await query.LongCountAsync(cancellationToken);
    }

    private async Task<IQueryable<Teacher>> ApplyFilterAsync(string? filter)
    {
        var queryable = await GetQueryableAsync();

        if (!filter.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(t =>
                t.Name.Contains(filter!) ||
                t.Surname.Contains(filter!) ||
                t.Email.Contains(filter!));
        }

        return queryable;
    }
}
