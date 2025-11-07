using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Pusula.Student.Automation.LessonEnrollments;

public interface ILessonEnrollmentRepository : IRepository<LessonEnrollment, Guid>
{
    Task<LessonEnrollment?> FindAsync(Guid lessonId, Guid studentId, CancellationToken cancellationToken = default);

    Task<List<LessonEnrollment>> GetByLessonAsync(Guid lessonId, CancellationToken cancellationToken = default);

    Task<List<LessonEnrollment>> GetByStudentAsync(Guid studentId, CancellationToken cancellationToken = default);

    Task<List<LessonEnrollment>> GetByTeacherAsync(Guid teacherId, CancellationToken cancellationToken = default);
}
