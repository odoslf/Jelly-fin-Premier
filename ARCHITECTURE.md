# Documentación Técnica y Arquitectura de JellyPremiere

Este documento detalla la arquitectura técnica, las decisiones de diseño y las capacidades de compatibilidad con distintos clientes en Jellyfin 10.10.7.

---

## 1. Arquitectura General

JellyPremiere se compone de tres módulos principales:

### A. Capa de Dominio y Persistencia (Backend .NET 8)
- **Modelos:** `Announcement`, `MediaMetadata`, `UserAcknowledgment`, `AnnouncementStats`.
- **Persistencia Thread-Safe:** `JsonAnnouncementRepository` gestiona el almacenamiento en archivos JSON en la carpeta de configuración del plugin de Jellyfin (`IApplicationPaths.PluginConfigurationsPath`).
- **Escritura Atómica:** Las operaciones de guardado utilizan archivos temporales `.tmp` y reemplazo atómico para prevenir corrupción en reinicios o cortes de energía.

### B. Controladores REST API (`JellyPremiereController`)
- **Control de Acceso:** Protegido con atributos `[Authorize]` e inspección de permisos `PermissionKind.IsAdministrator` mediante Jellyfin User Claims e `IUserManager`.
- **Endpoints:**
  - `GET /JellyPremiere/Active`: Devuelve anuncios activos y no confirmados correspondientes al usuario autenticado.
  - `POST /JellyPremiere/Acknowledge/{id}`: Registra la confirmación de lectura individual de un aviso obligatorio.
  - `GET /JellyPremiere/Admin/Announcements`: Operaciones CRUD de administrador.
  - `GET /JellyPremiere/Admin/Stats/{id}`: Estadísticas detalladas de lectura por usuario real de Jellyfin.
  - `GET /JellyPremiere/Library/Item/{id}`: Extracción directa de metadatos desde `ILibraryManager`.

### C. Frontend e Interfaz de Usuario (`jellypremiere.html` / `jellypremiere.js`)
- **Página de Administración:** Integrada en la sección de administración de Jellyfin mediante `IHasWebPages`. Permite la gestión completa, previsualización en tiempo real e inspección de lecturas.
- **Overlay Inyectado:** Script liviano JavaScript en la Web App que dibuja banners en el Inicio y modales flotantes con soporte para control con mando (Android TV / TV Remote focus) y gestos táctiles en Android/Móvil.

---

## 2. Compatibilidad por Cliente y Limitaciones Técnicas en Jellyfin 10.10.7

### Jellyfin Web / WebView / Jellyfin Media Player (Escritorio, Android WebView)
- **Compatibilidad:** 100% Nativa e Integrada.
- **Visualización:** Muestra banners estilizados en la pantalla principal de Inicio y modales emergentes inmediatos con soporte para cierre o confirmación obligatoria.

### Aplicación Android (Móvil / Tablet)
- **Compatibilidad:** Alta (Mediante la integración WebView nativa de la app Android oficial de Jellyfin).
- **Diseño Responsive:** CSS adaptado para pantallas táctiles y móviles pequeños.

### Android TV / Fire TV
- **Compatibilidad UI:** La interfaz inyectada soporta navegación con D-pad (mando a distancia) mediante foco directo de botones HTML (`element.focus()`).
- **Limitación Técnica de Jellyfin 10.10.7:**
  - La aplicación nativa Android TV (`jellyfin-androidtv`) de Jellyfin está escrita en Kotlin/Java nativo usando Leanback UI y no utiliza WebView para el renderizado de la biblioteca principal.
  - Dado que el servidor Jellyfin 10.10.7 no proporciona una API de extensión oficial para modificar vistas nativas Leanback de Android TV desde plugins C#, la inyección visual directa de modales nativos en la app Android TV no es técnicamente posible sin modificar el código fuente Kotlin del cliente nativo Android TV.
  - **Estrategia Implementada:** En dispositivos que utilizan cliente Web/WebView en Android TV o clientes híbridos, JellyPremiere funciona perfectamente. Para clientes 100% nativos Leanback, los datos están completamente accesibles vía REST API de JellyPremiere para futuras extensiones nativas del cliente.
