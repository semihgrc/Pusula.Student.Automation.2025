using System.Collections.Generic;

namespace Pusula.Student.Automation.GlobalExceptions;

public interface IAutomationException
{
    void Throw(string code, string message, IDictionary<string, object>? data = null);

    void ThrowIf(bool condition, string code, string message, IDictionary<string, object>? data = null);
}
