using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace SmingCode.Utilities.Exceptions.ApiHandling;
using StartupProcesses;

internal class ExceptionHandlingInitialization : IServiceInitializer
{
    public Delegate ServiceInitializer => (WebApplication app) =>
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
    };
}