using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Pusula.Student.Automation.Authorization;
using Pusula.Student.Automation.Enums;
using Pusula.Student.Automation.LessonEnrollments;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace Pusula.Student.Automation.Students;

[Authorize(Roles = AutomationRoleNames.Admin)]
public class StudentAppService : AutomationAppService, IStudentAppService
{
    private readonly IStudentRepository _studentRepository;
    private readonly StudentManager _studentManager;
    private readonly ILessonEnrollmentRepository _lessonEnrollmentRepository;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IGuidGenerator _guidGenerator;

    public StudentAppService(
        IStudentRepository studentRepository,
        StudentManager studentManager,
        ILessonEnrollmentRepository lessonEnrollmentRepository,
        IdentityUserManager identityUserManager,
        IGuidGenerator guidGenerator)
    {
        _studentRepository = studentRepository;
        _studentManager = studentManager;
        _lessonEnrollmentRepository = lessonEnrollmentRepository;
        _identityUserManager = identityUserManager;
        _guidGenerator = guidGenerator;
    }

    [Authorize(Roles = AutomationRoleNames.Admin + "," + AutomationRoleNames.Teacher)]
    public virtual async Task<StudentDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetAsync(id, cancellationToken: cancellationToken);
        var dto = ObjectMapper.Map<Student, StudentDto>(student);
        await PopulateLessonsAsync(dto, cancellationToken);
        return dto;
    }

    [Authorize(Roles = AutomationRoleNames.Admin + "," + AutomationRoleNames.Teacher)]
    public virtual async Task<PagedResultDto<StudentDto>> GetListAsync(StudentListRequestDto input, CancellationToken cancellationToken = default)
    {
        var totalCount = await _studentRepository.GetCountAsync(input.Filter, cancellationToken);
        var students = await _studentRepository.GetListAsync(
            input.Filter,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        var items = ObjectMapper.Map<List<Student>, List<StudentDto>>(students);
        return new PagedResultDto<StudentDto>(totalCount, items);
    }

    public virtual async Task<StudentDto> CreateAsync(StudentCreateDto input, CancellationToken cancellationToken = default)
    {
        await EnsureIdentityUserRoleAsync(input.IdentityUserId, AutomationRoleNames.Student);

        var student = await _studentManager.CreateAsync(
            input.IdentityUserId,
            input.Name,
            input.Surname,
            input.Gender,
            input.Email,
            input.PhoneNumber,
            input.StudentNumber,
            cancellationToken);

        return ObjectMapper.Map<Student, StudentDto>(student);
    }

    public virtual async Task<StudentDto> CreateWithIdentityAsync(StudentCreateWithIdentityDto input, CancellationToken cancellationToken = default)
    {
        var identityUserId = _guidGenerator.Create();
        var user = new IdentityUser(
            identityUserId,
            input.UserName,
            input.Email,
            CurrentTenant.Id)
        {
            Name = input.Name,
            Surname = input.Surname
        };

        if (!input.PhoneNumber.IsNullOrWhiteSpace())
        {
            user.SetPhoneNumber(input.PhoneNumber, false);
        }

        ThrowIdentityErrors(await _identityUserManager.CreateAsync(user, input.Password));

        await EnsureIdentityUserRoleAsync(identityUserId, AutomationRoleNames.Student);

        var student = await _studentManager.CreateAsync(
            identityUserId,
            input.Name,
            input.Surname,
            input.Gender,
            input.Email,
            input.PhoneNumber,
            input.StudentNumber,
            cancellationToken);

        return ObjectMapper.Map<Student, StudentDto>(student);
    }

    public virtual async Task<StudentDto> UpdateAsync(Guid id, StudentUpdateDto input, CancellationToken cancellationToken = default)
    {
        await EnsureIdentityUserRoleAsync(input.IdentityUserId, AutomationRoleNames.Student);

        var student = await _studentManager.UpdateAsync(
            id,
            input.IdentityUserId,
            input.Name,
            input.Surname,
            input.Gender,
            input.Email,
            input.PhoneNumber,
            input.StudentNumber,
            NormalizeConcurrencyStamp(input.ConcurrencyStamp),
            cancellationToken);

        return ObjectMapper.Map<Student, StudentDto>(student);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _studentManager.DeleteAsync(id, cancellationToken);
    }

    private async Task EnsureIdentityUserRoleAsync(Guid identityUserId, string roleName)
    {
        var user = await _identityUserManager.FindByIdAsync(identityUserId.ToString());
        if (user == null)
        {
            throw new BusinessException(AutomationDomainErrorCodes.IdentityUserNotFound)
                .WithData(nameof(identityUserId), identityUserId);
        }

        if (!await _identityUserManager.IsInRoleAsync(user, roleName))
        {
            await _identityUserManager.AddToRoleAsync(user, roleName);
        }
    }

    private async Task PopulateLessonsAsync(StudentDto studentDto, CancellationToken cancellationToken)
    {
        var enrollments = await _lessonEnrollmentRepository.GetByStudentAsync(studentDto.Id, cancellationToken);
        var lessons = new List<StudentLessonDto>();

        foreach (var enrollment in enrollments)
        {
            var lesson = enrollment.Lesson;

            lessons.Add(new StudentLessonDto
            {
                LessonId = enrollment.LessonId,
                LessonName = lesson?.Name ?? string.Empty,
                Status = lesson?.Status ?? LessonStatus.Planned,
                Grade = enrollment.Grade,
                TeacherComment = enrollment.TeacherComment,
                AbsenceCount = enrollment.AbsenceCount,
                TeacherId = lesson?.TeacherId ?? Guid.Empty,
                TeacherName = lesson?.Teacher != null
                    ? $"{lesson.Teacher.Name} {lesson.Teacher.Surname}"
                    : string.Empty
            });
        }

        studentDto.Lessons = lessons;

        var grades = lessons
            .Where(l => l.Grade.HasValue)
            .Select(l => l.Grade!.Value)
            .ToList();

        studentDto.AverageGrade = grades.Count > 0
            ? Math.Round(grades.Average(), 2)
            : null;
    }

    private static void ThrowIdentityErrors(IdentityResult identityResult)
    {
        if (identityResult.Succeeded)
        {
            return;
        }

        var message = string.Join(" ", identityResult.Errors.Select(e => e.Description));
        throw new BusinessException(AutomationDomainErrorCodes.IdentityOperationFailed, message);
    }
}
