using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pusula.Student.Automation.Enums;
using Pusula.Student.Automation.Lessons;
using Volo.Abp;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Pusula.Student.Automation.EntityFrameworkCore.Lessons;

public class EfCoreLessonRepository
    : EfCoreRepository<AutomationDbContext, Lesson, Guid>,
        ILessonRepository
{
    public EfCoreLessonRepository(IDbContextProvider<AutomationDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<Lesson>> GetListAsync(
        Guid? teacherId = null,
        string? filter = null,
        LessonStatus? status = null,
        int skipCount = 0,
        int maxResultCount = int.MaxValue,
        CancellationToken cancellationToken = default)
    {
        var query = await ApplyFilterAsync(teacherId, filter, status);

        return await query
            .OrderBy(l => l.Name)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(
        Guid? teacherId = null,
        string? filter = null,
        LessonStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = await ApplyFilterAsync(teacherId, filter, status);
        return await query.LongCountAsync(cancellationToken);
    }

    private async Task<IQueryable<Lesson>> ApplyFilterAsync(Guid? teacherId, string? filter, LessonStatus? status)
    {
        var queryable = await GetQueryableAsync();

        queryable = queryable
            .Include(l => l.Teacher)
            .Include(l => l.Enrollments)
            .ThenInclude(e => e.Student);

        if (teacherId.HasValue)
        {
            queryable = queryable.Where(l => l.TeacherId == teacherId.Value);
        }

        if (!filter.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(l =>
                l.Name.Contains(filter!) ||
                (l.Description != null && l.Description.Contains(filter!)));
        }

        if (status.HasValue)
        {
            queryable = queryable.Where(l => l.Status == status.Value);
        }

        return queryable;
    }
}
