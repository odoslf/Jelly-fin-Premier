using System;
using System.Collections.Generic;
using JellyPremiere.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace JellyPremiere
{
    public class JellyPremierePlugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public static JellyPremierePlugin? Instance { get; private set; }

        public override string Name => "JellyPremiere";

        public override Guid Id => Guid.Parse("e4138e6f-70db-40a2-9b21-171b3e839e99");

        public override string Description => "Sistema profesional de anuncios, estrenos y notificaciones obligatorias para Jellyfin.";

        public JellyPremierePlugin(
            IApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "jellypremiere",
                    EmbeddedResourcePath = GetType().Namespace + ".Web.jellypremiere.html"
                },
                new PluginPageInfo
                {
                    Name = "jellypremiere.js",
                    EmbeddedResourcePath = GetType().Namespace + ".Web.jellypremiere.js"
                }
            };
        }
    }
}
