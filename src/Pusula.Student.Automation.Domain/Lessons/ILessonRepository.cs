using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pusula.Student.Automation.Enums;
using Volo.Abp.Domain.Repositories;

namespace Pusula.Student.Automation.Lessons;

public interface ILessonRepository : IRepository<Lesson, Guid>
{
    Task<List<Lesson>> GetListAsync(
        Guid? teacherId = null,
        string? filter = null,
        LessonStatus? status = null,
        int skipCount = 0,
        int maxResultCount = int.MaxValue,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        Guid? teacherId = null,
        string? filter = null,
        LessonStatus? status = null,
        CancellationToken cancellationToken = default);
}
