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

## Estructura del Repositorio

- `JellyPremiere/`: Código fuente del plugin (.NET 8).
- `JellyPremiere.Tests/`: Suite de pruebas unitarias (xUnit).
- `dist/`: Contiene el paquete release ZIP (`JellyPremiere_1.0.0.0.zip`) y el `manifest.json`.
- `build_release.sh`: Script de compilación y empaquetado de producción.

---

## Compilación y Ejecución de Pruebas

### Requisitos
- .NET 8 SDK

### Compilar y Ejecutar Tests
```bash
dotnet build JellyPremiere.sln
dotnet test JellyPremiere.sln
```

### Generar Paquete Instalable ZIP
```bash
./build_release.sh
```
El archivo comprimido y el `manifest.json` se generarán en la carpeta `dist/`.
