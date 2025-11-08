using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Pusula.Student.Automation.LessonDailyReports;

public interface ILessonDailyReportRepository : IRepository<LessonDailyReport, Guid>
{
    Task<LessonDailyReport> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<LessonDailyReport?> FindByLessonAndDateAsync(Guid lessonId, DateTime date, bool includeDetails = true, CancellationToken cancellationToken = default);

    Task<LessonDailyReport?> FindByLessonAndDateIncludingDeletedAsync(Guid lessonId, DateTime date, bool includeDetails = true, CancellationToken cancellationToken = default);

    Task<List<LessonDailyReport>> GetListByLessonAsync(Guid lessonId, CancellationToken cancellationToken = default);

    Task<List<LessonDailyReport>> GetRecentByLessonAsync(Guid lessonId, int maxCount, CancellationToken cancellationToken = default);
}
