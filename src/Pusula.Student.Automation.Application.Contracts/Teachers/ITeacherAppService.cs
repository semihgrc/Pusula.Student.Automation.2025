using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Pusula.Student.Automation.Teachers;

public interface ITeacherAppService : IApplicationService
{
    Task<TeacherDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResultDto<TeacherDto>> GetListAsync(TeacherListRequestDto input, CancellationToken cancellationToken = default);

    Task<TeacherDto> CreateAsync(TeacherCreateDto input, CancellationToken cancellationToken = default);
    Task<TeacherDto> CreateWithIdentityAsync(TeacherCreateWithIdentityDto input, CancellationToken cancellationToken = default);

    Task<TeacherDto> UpdateAsync(Guid id, TeacherUpdateDto input, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
