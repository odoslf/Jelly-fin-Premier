# Guía de Instalación de JellyPremiere

Instrucciones detalladas para instalar y configurar **JellyPremiere** en un servidor **Jellyfin 10.10.7**.

---

## Método 1: Instalación Manual por Carpeta de Plugins

1. Detén el servicio o contenedor de Jellyfin.
2. Descarga o compila el archivo `JellyPremiere_1.0.0.0.zip` desde la carpeta `dist/`.
3. Extrae el contenido en el directorio de plugins de Jellyfin:
   - **Linux / Docker:** `/var/lib/jellyfin/plugins/JellyPremiere` o `<config_dir>/plugins/JellyPremiere`
   - **Windows:** `%ProgramData%\Jellyfin\Server\plugins\JellyPremiere`
4. Inicia el servidor Jellyfin.
5. Accede al **Panel de Control** -> **Plugins** y verifica que JellyPremiere aparece activo.

---

## Método 2: Instalación mediante Repositorio Manifest.json

1. Publica el archivo `dist/manifest.json` y `dist/JellyPremiere_1.0.0.0.zip` en un servidor web o servidor HTTP local.
2. En Jellyfin Web, ve a **Panel de Control** -> **Plugins** -> **Repositorios**.
3. Añade la URL de tu `manifest.json`.
4. Busca **JellyPremiere** en el catálogo de plugins e instálalo con un clic.
5. Reinicia el servidor Jellyfin.

---

## Uso de la Interfaz de Administración

1. Accede a Jellyfin con una cuenta de administrador.
2. Abre el **Panel de Control** del servidor.
3. En la barra lateral izquierda, selecciona la página de **JellyPremiere**.
4. Desde aquí podrás:
   - Crear banners de inicio o avisos importantes/obligatorios.
   - Seleccionar un elemento de la biblioteca e importar sus metadatos automáticamente.
   - Previsualizar cómo se verá el aviso antes de publicar.
   - Consultar las estadísticas de confirmación y lectura por usuario.
