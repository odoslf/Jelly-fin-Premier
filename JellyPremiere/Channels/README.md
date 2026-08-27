# Native channel

`PremiereChannel` exposes active JellyPremiere announcements through Jellyfin's `IChannel` extension point so clients that do not execute the plugin web UI still receive a native `Estrenos` surface.

Announcements are informational entries. They intentionally do not expose a synthetic media stream. Playback/navigation to a library item continues to be handled by the rich JellyPremiere UI when an announcement is linked to Jellyfin library metadata.
