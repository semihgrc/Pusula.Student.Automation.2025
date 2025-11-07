using System;
using Microsoft.Extensions.DependencyInjection;
using Pusula.Student.Automation.EntityFrameworkCore.LessonEnrollments;
using Pusula.Student.Automation.EntityFrameworkCore.Lessons;
using Pusula.Student.Automation.EntityFrameworkCore.Students;
using Pusula.Student.Automation.EntityFrameworkCore.Teachers;
using Pusula.Student.Automation.LessonEnrollments;
using Pusula.Student.Automation.Lessons;
using Pusula.Student.Automation.Students;
using Pusula.Student.Automation.Teachers;
using Volo.Abp.Uow;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using StudentEntity = Pusula.Student.Automation.Students.Student;

namespace Pusula.Student.Automation.EntityFrameworkCore;

[DependsOn(
    typeof(AutomationDomainModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule)
    )]
public class AutomationEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        AutomationEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<AutomationDbContext>(options =>
        {
                /* Remove "includeAllEntities: true" to create
                 * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Teacher, EfCoreTeacherRepository>();
            options.AddRepository<StudentEntity, EfCoreStudentRepository>();
            options.AddRepository<Lesson, EfCoreLessonRepository>();
            options.AddRepository<LessonEnrollment, EfCoreLessonEnrollmentRepository>();
        });

        Configure<AbpDbContextOptions>(options =>
        {
                /* The main point to change your DBMS.
                 * See also AutomationMigrationsDbContextFactory for EF Core tooling. */
            options.UseNpgsql();
        });

    }
}
