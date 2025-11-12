using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

namespace Pusula.Student.Automation.Blazor.Client;

public class Program
{
    public async static Task Main(string[] args)
    {

		var builder = WebAssemblyHostBuilder.CreateDefault(args);
        var syncfusionLicenseKey = builder.Configuration["Syncfusion:LicenseKey"];
        if (!string.IsNullOrWhiteSpace(syncfusionLicenseKey))
        {
            SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);
        }

        var application = await builder.AddApplicationAsync<AutomationBlazorClientModule>(options =>
        {
            options.UseAutofac();
        });

		builder.Services.AddSyncfusionBlazor();
		var host = builder.Build();

        await application.InitializeApplicationAsync(host.Services);

        await host.RunAsync();
    }
}
