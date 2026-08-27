# Instalación de JellyPremiere 1.0.1.0

## Requisitos

- Jellyfin Server **10.10.7**.
- Runtime **.NET 8** del propio servidor.
- Reinicio de Jellyfin después de instalar o actualizar el plugin.

## Repositorio Jellyfin recomendado

Añade una sola URL en **Panel de Control → Plugins → Repositorios**:

```text
https://raw.githubusercontent.com/odoslf/Repositorio-plugin-Jelly-fin-odos3d.lab/main/manifest.json
```

Ese catálogo unifica JellyPremiere, Community y JellyLiveNow y se sincroniza automáticamente con sus manifests oficiales.

Después instala **JellyPremiere** desde el catálogo y reinicia Jellyfin.

## Instalación manual

1. Descarga `JellyPremiere_1.0.1.0.zip` desde la release `v1.0.1.0`.
2. Crea una carpeta `JellyPremiere` dentro del directorio de plugins de Jellyfin.
3. Extrae `JellyPremiere.dll` dentro de esa carpeta.
4. Elimina DLL antiguas de JellyPremiere de esa misma carpeta.
5. Reinicia Jellyfin.

El ZIP oficial contiene únicamente la DLL del plugin. Las DLL de Jellyfin no se incluyen porque las proporciona el servidor 10.10.7.

## Comprobación

Tras reiniciar:

- Panel de Control → Plugins debe mostrar JellyPremiere 1.0.1.0.
- Jellyfin Web debe cargar automáticamente `/JellyPremiere/ClientScript.js` si la inyección está activada.
- Los usuarios normales deben recibir avisos activos destinados a ellos.
- La administración de anuncios debe seguir restringida a administradores.
- En clientes que exponen Channels debe aparecer `Estrenos`; solo contendrá películas o episodios vinculados y reproducibles.

Si también está instalado Community, JellyPremiere utiliza su bootstrap Web para que ambos plugins funcionen juntos sin modificar los archivos físicos de Jellyfin Web.
