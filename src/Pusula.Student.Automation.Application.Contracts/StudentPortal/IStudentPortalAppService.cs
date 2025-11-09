using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Pusula.Student.Automation.StudentPortal;

public interface IStudentPortalAppService : IApplicationService
{
    Task<StudentDashboardDto> GetDashboardAsync();
}
