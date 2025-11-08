using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pusula.Student.Automation.LessonEnrollments;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Pusula.Student.Automation.EntityFrameworkCore.LessonEnrollments;

public class EfCoreLessonEnrollmentRepository
    : EfCoreRepository<AutomationDbContext, LessonEnrollment, Guid>,
        ILessonEnrollmentRepository
{
    public EfCoreLessonEnrollmentRepository(IDbContextProvider<AutomationDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<LessonEnrollment?> FindAsync(Guid lessonId, Guid studentId, CancellationToken cancellationToken = default)
    {
        var queryable = await GetQueryableWithDetailsAsync();
        return await queryable.FirstOrDefaultAsync(
            e => e.LessonId == lessonId && e.StudentId == studentId,
            cancellationToken);
    }

    public async Task<LessonEnrollment?> FindIncludingDeletedAsync(Guid lessonId, Guid studentId, CancellationToken cancellationToken = default)
    {
        var queryable = (await GetQueryableWithDetailsAsync())
            .IgnoreQueryFilters();

        return await queryable.FirstOrDefaultAsync(
            e => e.LessonId == lessonId && e.StudentId == studentId,
            cancellationToken);
    }

    public async Task<List<LessonEnrollment>> GetByLessonAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        var queryable = await GetQueryableWithDetailsAsync();
        return await queryable
            .Where(e => e.LessonId == lessonId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LessonEnrollment>> GetByStudentAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var queryable = await GetQueryableWithDetailsAsync();
        return await queryable
            .Where(e => e.StudentId == studentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LessonEnrollment>> GetByTeacherAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        var queryable = await GetQueryableWithDetailsAsync();
        return await queryable
            .Where(e => e.Lesson.TeacherId == teacherId)
            .ToListAsync(cancellationToken);
    }

    protected virtual async Task<IQueryable<LessonEnrollment>> GetQueryableWithDetailsAsync()
    {
        return (await GetQueryableAsync())
            .Include(e => e.Student)
            .Include(e => e.Lesson)
            .ThenInclude(l => l.Teacher);
    }
}
