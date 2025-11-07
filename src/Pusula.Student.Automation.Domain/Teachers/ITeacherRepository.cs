using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Pusula.Student.Automation.Teachers;

public interface ITeacherRepository : IRepository<Teacher, Guid>
{
    Task<Teacher?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<List<Teacher>> GetListAsync(
        string? filter = null,
        int skipCount = 0,
        int maxResultCount = int.MaxValue,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(string? filter = null, CancellationToken cancellationToken = default);
}
