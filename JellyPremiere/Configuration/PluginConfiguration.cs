using System;
using MediaBrowser.Model.Plugins;

namespace JellyPremiere.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public bool EnableClientInjection { get; set; } = true;
    }
}
