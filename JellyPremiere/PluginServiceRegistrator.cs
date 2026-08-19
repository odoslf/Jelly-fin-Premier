using JellyPremiere.Services;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace JellyPremiere
{
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            serviceCollection.AddSingleton<IAnnouncementRepository>(sp =>
            {
                var appPaths = sp.GetRequiredService<IApplicationPaths>();
                var storagePath = System.IO.Path.Combine(appPaths.PluginConfigurationsPath, "JellyPremiere");

                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<JsonAnnouncementRepository>>();
                return new JsonAnnouncementRepository(storagePath, logger);
            });
        }
    }
}
