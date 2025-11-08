using Microsoft.EntityFrameworkCore;
using Pusula.Student.Automation;
using Pusula.Student.Automation.LessonDailyReports;
using Pusula.Student.Automation.LessonEnrollments;
using Pusula.Student.Automation.Lessons;
using Pusula.Student.Automation.Students;
using Pusula.Student.Automation.Teachers;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using StudentEntity = Pusula.Student.Automation.Students.Student;

namespace Pusula.Student.Automation.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class AutomationDbContext :
    AbpDbContext<AutomationDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<StudentEntity> Students { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<LessonEnrollment> LessonEnrollments { get; set; }
    public DbSet<LessonDailyReport> LessonDailyReports { get; set; }

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public AutomationDbContext(DbContextOptions<AutomationDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        builder.Entity<Teacher>(b =>
        {
            b.ToTable(AutomationConsts.DbTablePrefix + "Teachers", AutomationConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.IdentityUserId).IsRequired();
            b.Property(x => x.Name).IsRequired().HasMaxLength(TeacherConsts.MaxNameLength);
            b.Property(x => x.Surname).IsRequired().HasMaxLength(TeacherConsts.MaxSurnameLength);
            b.Property(x => x.Title).IsRequired().HasMaxLength(TeacherConsts.MaxTitleLength);
            b.Property(x => x.Email).IsRequired().HasMaxLength(TeacherConsts.MaxEmailLength);
            b.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(TeacherConsts.MaxPhoneNumberLength);
            b.HasIndex(x => x.Email).IsUnique();
            b.HasIndex(x => x.IdentityUserId).IsUnique();
        });

        builder.Entity<StudentEntity>(b =>
        {
            b.ToTable(AutomationConsts.DbTablePrefix + "Students", AutomationConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.IdentityUserId).IsRequired();
            b.Property(x => x.Name).IsRequired().HasMaxLength(StudentConsts.MaxNameLength);
            b.Property(x => x.Surname).IsRequired().HasMaxLength(StudentConsts.MaxSurnameLength);
            b.Property(x => x.Email).IsRequired().HasMaxLength(StudentConsts.MaxEmailLength);
            b.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(StudentConsts.MaxPhoneNumberLength);
            b.Property(x => x.StudentNumber).IsRequired().HasMaxLength(StudentConsts.MaxStudentNumberLength);
            b.HasIndex(x => x.Email).IsUnique();
            b.HasIndex(x => x.StudentNumber).IsUnique();
            b.HasIndex(x => x.IdentityUserId).IsUnique();
        });

        builder.Entity<Lesson>(b =>
        {
            b.ToTable(AutomationConsts.DbTablePrefix + "Lessons", AutomationConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(LessonConsts.MaxNameLength);
            b.Property(x => x.Description).HasMaxLength(LessonConsts.MaxDescriptionLength);
            b.Property(x => x.Status).IsRequired();

            b.HasOne(x => x.Teacher)
                .WithMany()
                .HasForeignKey(x => x.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<LessonEnrollment>(b =>
        {
            b.ToTable(AutomationConsts.DbTablePrefix + "LessonEnrollments", AutomationConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.TeacherComment).HasMaxLength(LessonEnrollmentConsts.MaxTeacherCommentLength);
            b.Property(x => x.Grade).HasPrecision(5, 2);
            b.Property(x => x.MidtermGrade).HasPrecision(5, 2);
            b.Property(x => x.FinalGrade).HasPrecision(5, 2);
            b.Property(x => x.AbsenceCount).IsRequired();

            b.HasOne(x => x.Lesson)
                .WithMany(l => l.Enrollments)
                .HasForeignKey(x => x.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.LessonId, x.StudentId }).IsUnique();
        });

        builder.Entity<LessonDailyReport>(b =>
        {
            b.ToTable(AutomationConsts.DbTablePrefix + "LessonDailyReports", AutomationConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Date).IsRequired();
            b.HasIndex(x => new { x.LessonId, x.Date }).IsUnique();
            b.HasMany(x => x.Entries)
                .WithOne()
                .HasForeignKey(entry => entry.LessonDailyReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LessonDailyReportEntry>(b =>
        {
            b.ToTable(AutomationConsts.DbTablePrefix + "LessonDailyReportEntries", AutomationConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.IsPresent).IsRequired();
            b.Property(x => x.DailyGrade).HasPrecision(5, 2);
            b.Property(x => x.DailyComment).HasMaxLength(LessonDailyReportConsts.MaxDailyCommentLength);
            b.HasIndex(x => new { x.LessonDailyReportId, x.StudentId }).IsUnique();
        });
    }
}
