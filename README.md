# JellyPremiere

Plugin de anuncios y estrenos para **Jellyfin 10.10.7 / .NET 8**.

## Qué hace

- Banners, avisos importantes y avisos obligatorios para usuarios autenticados.
- Fechas de inicio/fin, destinatarios concretos y confirmación por usuario.
- Selección de elementos de la biblioteca desde el panel de administración.
- Carga automática del cliente visual en Jellyfin Web y WebView cuando `EnableClientInjection` está activado.
- Compatibilidad con **Jellyfin Community**: si ambos plugins están instalados, JellyPremiere se engancha al bootstrap de Community sin competir por `index.html`.
- Canal nativo **Estrenos** mediante `IChannel` para clientes que muestran Channels.
- El canal nativo solo publica anuncios vinculados a películas/episodios realmente reproducibles; la reproducción usa `IMediaSourceManager.GetPlaybackMediaSources` del propio Jellyfin. Los avisos de texto nunca se hacen pasar por vídeos.

## Compatibilidad de clientes

- **Jellyfin Web / clientes WebView:** experiencia completa de banners, modales y confirmaciones.
- **Android / Android TV nativos:** pueden mostrar el canal estándar `Estrenos` si el cliente expone Channels. La ubicación exacta depende del cliente oficial.
- Un plugin de servidor no puede convertir arbitrariamente una página HTML en una pantalla nativa de Android TV; por eso la parte nativa se implementa con contratos oficiales de Jellyfin.

## Instalación recomendada

En **Panel de Control → Plugins → Repositorios**, añade solo el repositorio unificado ODOS3D:

```text
https://raw.githubusercontent.com/odoslf/Repositorio-plugin-Jelly-fin-odos3d.lab/main/manifest.json
```

Instala **JellyPremiere** y reinicia Jellyfin.

## Seguridad

- Administración de anuncios: solo administradores.
- Listado de usuarios y estadísticas: solo administradores.
- Estado activo y confirmación: requieren sesión Jellyfin.
- El script cliente servido por el plugin no contiene tokens; usa la sesión que ya mantiene Jellyfin Web.

## Validación

La versión **1.0.1.0** se construye contra Jellyfin 10.10.7. GitHub Actions compila con warnings como errores, ejecuta tests, audita dependencias, genera un ZIP que contiene únicamente `JellyPremiere.dll`, arranca el contenedor oficial `jellyfin/jellyfin:10.10.7` e inspecciona en runtime:

- carga del plugin;
- inyección automática del cliente Web;
- API con administrador y usuario normal;
- permisos de administración;
- creación/lectura/confirmación de avisos;
- aparición del canal nativo `Estrenos`;
- ausencia de errores JellyPremiere en el log.

## Desarrollo

```bash
dotnet restore JellyPremiere.sln
dotnet build JellyPremiere.sln -c Release
dotnet test JellyPremiere.sln -c Release
./build_release.sh
```
