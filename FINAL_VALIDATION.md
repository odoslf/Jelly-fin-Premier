# JellyPremiere final validation

Target: Jellyfin Server 10.10.7 / .NET 8.

## Client strategy

- Jellyfin Web and Android WebView keep the rich JellyPremiere web UI and authenticated API.
- Native clients receive an `IChannel` named **Estrenos**, registered through Jellyfin DI.
- The plugin does not patch Jellyfin Web or any official client binaries.

## CI gate

Every pull request to `main` must restore, compile, run tests and build the installable ZIP/manifest artifacts. A release is created only from a version tag.

## Runtime acceptance

1. Plugin loads without DI/type-load errors on Jellyfin 10.10.7.
2. `Estrenos` is discoverable through Jellyfin Channels.
3. Active announcements are exposed as native channel items.
4. Admin announcement endpoints remain administrator-only.
5. Web/WebView overlay remains optional through plugin configuration.

Native client placement is controlled by each official Jellyfin client; the plugin relies only on Jellyfin's supported server-side `IChannel` contract.
