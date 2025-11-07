using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Pusula.Student.Automation.Students;

public interface IStudentAppService : IApplicationService
{
    Task<StudentDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResultDto<StudentDto>> GetListAsync(StudentListRequestDto input, CancellationToken cancellationToken = default);

    Task<StudentDto> CreateAsync(StudentCreateDto input, CancellationToken cancellationToken = default);
    Task<StudentDto> CreateWithIdentityAsync(StudentCreateWithIdentityDto input, CancellationToken cancellationToken = default);

    Task<StudentDto> UpdateAsync(Guid id, StudentUpdateDto input, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
