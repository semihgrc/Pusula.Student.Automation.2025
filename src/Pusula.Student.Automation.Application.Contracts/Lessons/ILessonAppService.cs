using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Pusula.Student.Automation.Lessons;

public interface ILessonAppService : IApplicationService
{
    Task<LessonDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResultDto<LessonDto>> GetListAsync(LessonListRequestDto input, CancellationToken cancellationToken = default);

    Task<LessonDto> CreateAsync(LessonCreateDto input, CancellationToken cancellationToken = default);

    Task<LessonDto> UpdateAsync(Guid id, LessonUpdateDto input, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task ChangeStatusAsync(Guid id, LessonStatusUpdateDto input, CancellationToken cancellationToken = default);

    Task<LessonEnrollmentDto> AddStudentAsync(LessonEnrollmentCreateDto input, CancellationToken cancellationToken = default);

    Task RemoveStudentAsync(Guid lessonId, Guid studentId, CancellationToken cancellationToken = default);

    Task<LessonEnrollmentDto> UpdateEnrollmentAsync(
        Guid lessonId,
        Guid studentId,
        LessonEnrollmentUpdateDto input,
        CancellationToken cancellationToken = default);

    Task<List<LessonEnrollmentDto>> GetLessonStudentsAsync(Guid lessonId, CancellationToken cancellationToken = default);

    Task<List<LessonEnrollmentDto>> GetStudentEnrollmentsAsync(Guid studentId, CancellationToken cancellationToken = default);

    Task<List<LessonDailyReportSummaryDto>> GetDailyReportsAsync(Guid lessonId, CancellationToken cancellationToken = default);

    Task<LessonDailyReportDto> GetDailyReportAsync(Guid lessonId, DateTime date, CancellationToken cancellationToken = default);

    Task<LessonDailyReportDto> SaveDailyReportAsync(LessonDailyReportSaveDto input, CancellationToken cancellationToken = default);

    Task DeleteDailyReportAsync(Guid reportId, CancellationToken cancellationToken = default);
}
