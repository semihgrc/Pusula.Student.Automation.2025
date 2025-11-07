using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Pusula.Student.Automation.Authorization;
using Pusula.Student.Automation.LessonEnrollments;
using Pusula.Student.Automation.Teachers;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Pusula.Student.Automation.Lessons;

[Authorize(Roles = AutomationRoleNames.Admin + "," + AutomationRoleNames.Teacher)]
public class LessonAppService : AutomationAppService, ILessonAppService
{
    private readonly ILessonRepository _lessonRepository;
    private readonly LessonManager _lessonManager;
    private readonly ILessonEnrollmentRepository _lessonEnrollmentRepository;
    private readonly ITeacherRepository _teacherRepository;

    public LessonAppService(
        ILessonRepository lessonRepository,
        LessonManager lessonManager,
        ILessonEnrollmentRepository lessonEnrollmentRepository,
        ITeacherRepository teacherRepository)
    {
        _lessonRepository = lessonRepository;
        _lessonManager = lessonManager;
        _lessonEnrollmentRepository = lessonEnrollmentRepository;
        _teacherRepository = teacherRepository;
    }

    public virtual async Task<LessonDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lesson = await _lessonRepository.GetAsync(id, includeDetails: true, cancellationToken: cancellationToken);
        return await MapLessonAsync(lesson, cancellationToken, lesson.Enrollments?.ToList());
    }

    public virtual async Task<PagedResultDto<LessonDto>> GetListAsync(LessonListRequestDto input, CancellationToken cancellationToken = default)
    {
        var totalCount = await _lessonRepository.GetCountAsync(
            input.TeacherId,
            input.Filter,
            input.Status,
            cancellationToken);

        var lessons = await _lessonRepository.GetListAsync(
            input.TeacherId,
            input.Filter,
            input.Status,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        var items = new List<LessonDto>();

        foreach (var lesson in lessons)
        {
            items.Add(await MapLessonAsync(lesson, cancellationToken, lesson.Enrollments?.ToList()));
        }

        return new PagedResultDto<LessonDto>(totalCount, items);
    }

    [Authorize(Roles = AutomationRoleNames.Admin)]
    public virtual async Task<LessonDto> CreateAsync(LessonCreateDto input, CancellationToken cancellationToken = default)
    {
        var lesson = await _lessonManager.CreateAsync(
            input.TeacherId,
            input.Name,
            input.Description,
            cancellationToken);

        return await MapLessonAsync(lesson, cancellationToken);
    }

    [Authorize(Roles = AutomationRoleNames.Admin)]
    public virtual async Task<LessonDto> UpdateAsync(Guid id, LessonUpdateDto input, CancellationToken cancellationToken = default)
    {
        var lesson = await _lessonManager.UpdateAsync(
            id,
            input.TeacherId,
            input.Name,
            input.Description,
            NormalizeConcurrencyStamp(input.ConcurrencyStamp),
            input.Status,
            cancellationToken);

        return await MapLessonAsync(lesson, cancellationToken);
    }

    [Authorize(Roles = AutomationRoleNames.Admin)]
    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _lessonManager.DeleteAsync(id, cancellationToken);
    }

    public virtual async Task ChangeStatusAsync(Guid id, LessonStatusUpdateDto input, CancellationToken cancellationToken = default)
    {
        await _lessonManager.ChangeStatusAsync(
            id,
            input.Status,
            NormalizeConcurrencyStamp(input.ConcurrencyStamp),
            cancellationToken);
    }

    public virtual async Task<LessonEnrollmentDto> AddStudentAsync(LessonEnrollmentCreateDto input, CancellationToken cancellationToken = default)
    {
        var enrollment = await _lessonManager.AddStudentAsync(input.LessonId, input.StudentId, cancellationToken);
        var detailedEnrollment = await _lessonEnrollmentRepository.GetAsync(
            enrollment.Id,
            includeDetails: true,
            cancellationToken: cancellationToken);

        return ObjectMapper.Map<LessonEnrollment, LessonEnrollmentDto>(detailedEnrollment);
    }

    public virtual async Task RemoveStudentAsync(Guid lessonId, Guid studentId, CancellationToken cancellationToken = default)
    {
        await _lessonManager.RemoveStudentAsync(lessonId, studentId, cancellationToken);
    }

    public virtual async Task<LessonEnrollmentDto> UpdateEnrollmentAsync(
        Guid lessonId,
        Guid studentId,
        LessonEnrollmentUpdateDto input,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await _lessonManager.UpdateEnrollmentAsync(
            lessonId,
            studentId,
            input.Grade,
            input.TeacherComment,
            input.AbsenceCount,
            NormalizeConcurrencyStamp(input.ConcurrencyStamp),
            cancellationToken);

        var detailedEnrollment = await _lessonEnrollmentRepository.GetAsync(
            enrollment.Id,
            includeDetails: true,
            cancellationToken: cancellationToken);

        return ObjectMapper.Map<LessonEnrollment, LessonEnrollmentDto>(detailedEnrollment);
    }

    public virtual async Task<List<LessonEnrollmentDto>> GetLessonStudentsAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        var enrollments = await _lessonEnrollmentRepository.GetByLessonAsync(lessonId, cancellationToken);
        return ObjectMapper.Map<List<LessonEnrollment>, List<LessonEnrollmentDto>>(enrollments);
    }

    [Authorize(Roles = AutomationRoleNames.Admin + "," + AutomationRoleNames.Teacher + "," + AutomationRoleNames.Student)]
    public virtual async Task<List<LessonEnrollmentDto>> GetStudentEnrollmentsAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var enrollments = await _lessonEnrollmentRepository.GetByStudentAsync(studentId, cancellationToken);
        return ObjectMapper.Map<List<LessonEnrollment>, List<LessonEnrollmentDto>>(enrollments);
    }

    private async Task<LessonDto> MapLessonAsync(
        Lesson lesson,
        CancellationToken cancellationToken,
        List<LessonEnrollment>? enrollments = null)
    {
        var dto = ObjectMapper.Map<Lesson, LessonDto>(lesson);

        var enrollmentList = enrollments ?? lesson.Enrollments?.ToList();

        if (enrollmentList == null || enrollmentList.Count == 0)
        {
            enrollmentList = await _lessonEnrollmentRepository.GetByLessonAsync(lesson.Id, cancellationToken);
        }

        dto.Enrollments = ObjectMapper.Map<List<LessonEnrollment>, List<LessonEnrollmentDto>>(enrollmentList);

        if (string.IsNullOrWhiteSpace(dto.TeacherName))
        {
            var teacher = await _teacherRepository.FindAsync(
                lesson.TeacherId,
                cancellationToken: cancellationToken);

            if (teacher != null)
            {
                dto.TeacherName = $"{teacher.Name} {teacher.Surname}";
            }
        }

        return dto;
    }
}
