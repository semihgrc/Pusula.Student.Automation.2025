using AutoMapper;
using Pusula.Student.Automation.LessonEnrollments;
using Pusula.Student.Automation.Lessons;
using Pusula.Student.Automation.Students;
using Pusula.Student.Automation.Teachers;
using StudentEntity = Pusula.Student.Automation.Students.Student;

namespace Pusula.Student.Automation;

public class AutomationApplicationAutoMapperProfile : Profile
{
    public AutomationApplicationAutoMapperProfile()
    {
        CreateMap<Teacher, TeacherDto>();

        CreateMap<StudentEntity, StudentDto>()
            .ForMember(dest => dest.Lessons, opt => opt.Ignore())
            .ForMember(dest => dest.AverageGrade, opt => opt.Ignore());

        CreateMap<Lesson, LessonDto>()
            .ForMember(dest => dest.TeacherName,
                opt => opt.MapFrom(src =>
                    src.Teacher == null
                        ? null
                        : src.Teacher.Name + " " + src.Teacher.Surname))
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore());

        CreateMap<LessonEnrollment, LessonEnrollmentDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src =>
                    src.Student == null
                        ? string.Empty
                        : src.Student.Name + " " + src.Student.Surname))
            .ForMember(dest => dest.StudentNumber,
                opt => opt.MapFrom(src => src.Student == null ? string.Empty : src.Student.StudentNumber));
    }
}
