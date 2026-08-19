# JellyPremiere

**JellyPremiere** es un plugin profesional de gestión y visualización de anuncios, estrenos de la biblioteca y notificaciones para servidores Jellyfin (10.10.7 / .NET 8).

---

## Características Principales

1. **Tipos de Anuncio:**
   - **Banner Visual (Inicio):** Banner estilo plataforma de streaming con imágenes de backdrop, degradado oscuro para legibilidad y botón de acción.
   - **Aviso Importante:** Modal emergente informativo con botón para cerrar.
   - **Aviso Obligatorio:** Modal emergente que requiere confirmación explícita ("Entendido") y registra individualmente qué usuario ha confirmado el aviso. No vuelve a mostrarse tras la confirmación.

2. **Integración con Biblioteca:**
   - Permite vincular cualquier película o serie existente en Jellyfin.
   - Extrae automáticamente título, póster, fondo/backdrop y sinopsis, generando anuncios de estreno sin duplicar información.

3. **Programación y Control:**
   - Publicación inmediata o programada (fecha/hora inicio y fecha/hora fin).
   - Ocultamiento automático tras caducidad.
   - Activación / desactivación manual por el administrador.

4. **Panel de Administración:**
   - Sección nativa integrada en la interfaz de Jellyfin.
   - Panel de control completo (Crear, Editar, Eliminar, Previsualizar, Programar).
   - Tabla de estadísticas de lectura individualizadas por usuario real de Jellyfin.

5. **Seguridad y Permisos:**
   - Endpoints REST API protegidos en servidor con autenticación y permisos de usuario.
   - Identificación de usuario mediante la identidad de Jellyfin (GUID) en lugar de direcciones IP.

---

## Instalación en Jellyfin

Añade la URL del repositorio oficial a tu servidor Jellyfin en **Panel de Control -> Plugins -> Repositorios**:

```text
https://raw.githubusercontent.com/odoslf/Jelly-fin-Premier/main/dist/manifest.json
```

Consulta [INSTALLATION.md](INSTALLATION.md) para instrucciones detalladas.

---

## Compilación y Pruebas (Desarrollo)

### Requisitos
- .NET 8 SDK

### Compilar y Ejecutar Pruebas Unitarias
```bash
dotnet restore JellyPremiere.sln
dotnet build JellyPremiere.sln -c Release
dotnet test JellyPremiere.sln -c Release
```

### Generar Paquete Instalable ZIP y Manifest
```bash
./build_release.sh
```
El archivo comprimido `JellyPremiere_1.0.0.0.zip` y el `manifest.json` se generarán en la carpeta `dist/`.
