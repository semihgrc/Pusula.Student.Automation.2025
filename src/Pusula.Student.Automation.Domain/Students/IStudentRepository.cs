using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Pusula.Student.Automation.Students;

public interface IStudentRepository : IRepository<Student, Guid>
{
    Task<Student?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<Student?> FindByStudentNumberAsync(string studentNumber, CancellationToken cancellationToken = default);

    Task<Student?> FindByIdentityUserIdAsync(Guid identityUserId, CancellationToken cancellationToken = default);

    Task<List<Student>> GetListAsync(
        string? filter = null,
        int skipCount = 0,
        int maxResultCount = int.MaxValue,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(string? filter = null, CancellationToken cancellationToken = default);
}
