using JellyPremiere.Channels;
using JellyPremiere.Services;
using JellyPremiere.WebIntegration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Plugins;
using Microsoft.AspNetCore.Hosting;
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

            serviceCollection.AddSingleton<IChannel, PremiereChannel>();
            serviceCollection.AddTransient<IStartupFilter, PremiereStartupFilter>();
        }
    }
}
