using System.Collections.Generic;
using System.Threading.Tasks;
using Pusula.Student.Automation.Students;
using Volo.Abp.Application.Services;

namespace Pusula.Student.Automation.TeacherPortal;

public interface ITeacherPortalAppService : IApplicationService
{
    Task<TeacherPortalProfileDto> GetProfileAsync();
    Task<StudentDto> CreateStudentAsync(StudentCreateWithIdentityDto input);
    Task<List<StudentLookupDto>> SearchStudentsAsync(string? filter);
}
