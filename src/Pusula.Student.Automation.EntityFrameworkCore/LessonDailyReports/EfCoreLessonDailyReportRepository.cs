using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pusula.Student.Automation.LessonDailyReports;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Pusula.Student.Automation.EntityFrameworkCore.LessonDailyReports;

public class EfCoreLessonDailyReportRepository
    : EfCoreRepository<AutomationDbContext, LessonDailyReport, Guid>,
        ILessonDailyReportRepository
{
    public EfCoreLessonDailyReportRepository(
        IDbContextProvider<AutomationDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<LessonDailyReport?> FindByLessonAndDateAsync(Guid lessonId, DateTime date, bool includeDetails = true, CancellationToken cancellationToken = default)
    {
        date = date.Date;

        var dbSet = await GetDbSetAsync();

        var query = dbSet.Where(report => report.LessonId == lessonId && report.Date == date);

        if (includeDetails)
        {
            query = query.Include(r => r.Entries);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<LessonDailyReport>> GetListByLessonAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Include(r => r.Entries)
            .Where(r => r.LessonId == lessonId)
            .OrderByDescending(r => r.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LessonDailyReport>> GetRecentByLessonAsync(Guid lessonId, int maxCount, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(r => r.LessonId == lessonId)
            .OrderByDescending(r => r.Date)
            .Take(maxCount)
            .Include(r => r.Entries)
            .ToListAsync(cancellationToken);
    }
}
