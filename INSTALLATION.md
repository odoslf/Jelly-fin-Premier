# Guía de Instalación de JellyPremiere

Instrucciones detalladas para instalar y configurar **JellyPremiere** en un servidor **Jellyfin 10.10.7**.

---

## Método 1: Instalación Mediante Repositorio Manifest.json (Recomendado)

1. En Jellyfin Web, accede a **Panel de Control** -> **Plugins** -> **Repositorios**.
2. Haz clic en **+** para añadir un nuevo repositorio.
3. Rellena los datos:
   - **Nombre del repositorio:** `JellyPremiere Repository`
   - **URL del repositorio:** `https://raw.githubusercontent.com/odoslf/Jelly-fin-Premier/main/dist/manifest.json`
4. Guarda los cambios.
5. Ve a **Panel de Control** -> **Plugins** -> **Catálogo**.
6. Selecciona **JellyPremiere** e instálalo.
7. Reinicia el servidor Jellyfin.

---

## Método 2: Instalación Manual por Carpeta de Plugins

1. Detén el servicio o contenedor de Jellyfin.
2. Descarga el archivo `JellyPremiere_1.0.0.0.zip` desde la Release `v1.0.0.0` de GitHub:
   `https://github.com/odoslf/Jelly-fin-Premier/releases/download/v1.0.0.0/JellyPremiere_1.0.0.0.zip`
3. Extrae el contenido en el directorio de plugins de tu servidor Jellyfin:
   - **Linux / Docker:** `/var/lib/jellyfin/plugins/JellyPremiere` o `<config_dir>/plugins/JellyPremiere`
   - **Windows:** `%ProgramData%\Jellyfin\Server\plugins\JellyPremiere`
4. Inicia el servidor Jellyfin.
5. Accede a **Panel de Control** -> **Plugins** y verifica que JellyPremiere aparece activo.

---

## Uso del Panel de Administración

1. Accede a Jellyfin con una cuenta de usuario Administrador.
2. Abre el **Panel de Control** del servidor.
3. En la barra lateral izquierda, selecciona la página de **JellyPremiere**.
4. Desde aquí podrás:
   - Crear banners de inicio o avisos importantes/obligatorios.
   - Seleccionar un elemento de la biblioteca e importar sus metadatos automáticamente.
   - Previsualizar cómo se verá el aviso antes de publicar.
   - Consultar las estadísticas de confirmación y lectura por usuario real de Jellyfin.
