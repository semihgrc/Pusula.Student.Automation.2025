namespace Pusula.Student.Automation;

public static class AutomationDomainErrorCodes
{
    public const string TeacherEmailAlreadyExists = "Automation:01001";
    public const string TeacherNotFound = "Automation:01002";

    public const string StudentEmailAlreadyExists = "Automation:02001";
    public const string StudentNotFound = "Automation:02002";
    public const string StudentNumberAlreadyExists = "Automation:02003";

    public const string LessonNotFound = "Automation:03001";
    public const string LessonAlreadyHasStudent = "Automation:03002";
    public const string LessonEnrollmentNotFound = "Automation:03003";
    public const string LessonStudentNotFound = "Automation:03004";
    public const string LessonDailyReportAlreadyExists = "Automation:03005";
    public const string LessonDailyReportNotFound = "Automation:03006";
    public const string LessonDailyReportHasNoEntries = "Automation:03007";

    public const string IdentityUserNotFound = "Automation:04001";
    public const string IdentityOperationFailed = "Automation:04002";
}
