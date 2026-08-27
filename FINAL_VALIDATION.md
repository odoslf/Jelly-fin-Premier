# JellyPremiere validation target

Target: **Jellyfin Server 10.10.7 / .NET 8**.

## Runtime acceptance gate

The release workflow is not considered successful unless it:

1. Restores and compiles the full solution with warnings treated as errors.
2. Passes all unit tests.
3. Reports no vulnerable NuGet dependency.
4. Packages only `JellyPremiere.dll`.
5. Starts the official `jellyfin/jellyfin:10.10.7` container with that exact package.
6. Completes Jellyfin setup and authenticates an administrator and a normal user.
7. Confirms automatic Web client injection and the real `/JellyPremiere/ClientScript.js` endpoint.
8. Confirms normal users cannot use administrator announcement endpoints.
9. Creates an announcement, retrieves it as active and acknowledges it as a normal user.
10. Confirms the native `Estrenos` channel is discoverable through Jellyfin Channels.
11. Confirms JellyPremiere emits no `[ERR]` entry in the runtime log.

## Native client model

`Estrenos` is a standard `IChannel`. Only announcements linked to directly playable video library items are exposed as media. Media sources come from Jellyfin's own `IMediaSourceManager`; informational notices stay in the Web/WebView overlay and are not presented as fake videos.

Client placement of Channels is controlled by each official Jellyfin client.
