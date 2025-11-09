using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pusula.Student.Automation.Students;
using Volo.Abp;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using StudentEntity = Pusula.Student.Automation.Students.Student;

namespace Pusula.Student.Automation.EntityFrameworkCore.Students;

public class EfCoreStudentRepository
    : EfCoreRepository<AutomationDbContext, StudentEntity, Guid>,
        IStudentRepository
{
    public EfCoreStudentRepository(IDbContextProvider<AutomationDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<StudentEntity?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<StudentEntity?> FindByStudentNumberAsync(string studentNumber, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.StudentNumber == studentNumber, cancellationToken);
    }

    public async Task<StudentEntity?> FindByIdentityUserIdAsync(Guid identityUserId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.IdentityUserId == identityUserId, cancellationToken);
    }

    public async Task<List<StudentEntity>> GetListAsync(
        string? filter = null,
        int skipCount = 0,
        int maxResultCount = int.MaxValue,
        CancellationToken cancellationToken = default)
    {
        var query = await ApplyFilterAsync(filter);

        return await query
            .OrderBy(s => s.Name)
            .ThenBy(s => s.Surname)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(string? filter = null, CancellationToken cancellationToken = default)
    {
        var query = await ApplyFilterAsync(filter);
        return await query.LongCountAsync(cancellationToken);
    }

    private async Task<IQueryable<StudentEntity>> ApplyFilterAsync(string? filter)
    {
        var queryable = await GetQueryableAsync();

        if (!filter.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(s =>
                s.Name.Contains(filter!) ||
                s.Surname.Contains(filter!) ||
                s.Email.Contains(filter!) ||
                s.StudentNumber.Contains(filter!));
        }

        return queryable;
    }
}
