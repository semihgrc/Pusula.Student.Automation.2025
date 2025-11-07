using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Pusula.Student.Automation.GlobalExceptions;

public class AutomationException : IAutomationException, ISingletonDependency
{
    public void Throw(string code, string message, IDictionary<string, object>? data = null)
    {
        throw CreateException(code, message, data);
    }

    public void ThrowIf(bool condition, string code, string message, IDictionary<string, object>? data = null)
    {
        if (condition)
        {
            Throw(code, message, data);
        }
    }

    private static BusinessException CreateException(string code, string message, IDictionary<string, object>? data)
    {
        var exception = new BusinessException(code, message);

        if (data != null)
        {
            foreach (var pair in data)
            {
                exception.WithData(pair.Key, pair.Value);
            }
        }

        return exception;
    }
}
