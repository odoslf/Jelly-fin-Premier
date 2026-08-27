using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace JellyPremiere.WebIntegration;

public sealed class PremiereStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        return app =>
        {
            app.UseMiddleware<PremiereWebInjectionMiddleware>();
            next(app);
        };
    }
}
