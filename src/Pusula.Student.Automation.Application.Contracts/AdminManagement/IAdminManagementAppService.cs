using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Pusula.Student.Automation.AdminManagement;

public interface IAdminManagementAppService : IApplicationService
{
    Task<List<AdminUserSummaryDto>> SearchAsync(AdminUserSearchRequestDto input);

    Task<IRemoteStreamContent> ExportAsync(AdminUserExportRequestDto input);
}
