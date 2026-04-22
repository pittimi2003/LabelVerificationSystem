# Estado Actual del Proyecto

## Propósito de este documento

Este documento es la referencia del estado actual del proyecto.

Su propósito es mantener un registro explícito y continuamente actualizado de:

- en qué punto se encuentra el proyecto
- qué decisiones ya están fijadas
- qué se está usando como base activa
- qué todavía no existe
- qué se ha añadido o cambiado con el tiempo

Este documento debe actualizarse cada vez que se introduzca un cambio relevante a nivel arquitectónico, funcional, estructural o de implementación.

El objetivo es conservar el contexto del proyecto en todo momento y evitar suposiciones, desviaciones no documentadas o interpretaciones inventadas por cualquier persona o agente que trabaje sobre la solución.

---

## Estado actual

**Estado del proyecto:** Construcción inicial

El proyecto se considera oficialmente iniciado.

La solución actual es la base activa de trabajo.  
La UI existente se utilizará como template y shell visual.  
La estructura arquitectónica del backend se mantiene y no será modificada como decisión fundacional.

La funcionalidad real de negocio comienza a construirse a partir de esta base.

---

## Línea base funcional de referencia

La referencia funcional inicial del sistema es el documento de propuesta del proyecto.

Dicha propuesta define el alcance inicial esperado de la solución, incluyendo:

- verificación de etiquetas mediante un proceso de dos escaneos
- gestión del catálogo de partes
- carga y procesamiento de Excel
- gestión colaborativa de packing lists
- administración de usuarios y roles
- capacidades de auditoría
- operación en planta mediante un sistema web
- frontend basado en Blazor WebAssembly
- backend basado en API .NET
- uso inicial de SQLite
- despliegue sobre infraestructura IIS local :contentReference[oaicite:0]{index=0} :contentReference[oaicite:1]{index=1} :contentReference[oaicite:2]{index=2}

La propuesta es la **línea base funcional inicial**, pero no es inmutable.  
Podrá refinarse, aclararse, ampliarse o ajustarse durante la implementación, siempre que dichos cambios queden documentados de forma explícita.

---

## Enfoque activo de construcción

El proyecto avanzará con el siguiente enfoque:

### Frontend
La UI actual se utiliza como:

- template visual
- shell de navegación
- base de layout
- contenedor de renderizado para la nueva funcionalidad

Los nuevos módulos de negocio se renderizarán dentro de la estructura existente de layout y cuerpo de aplicación.

La base actual de frontend podrá ser **refactorizada o reemplazada parcialmente** si alguna de sus partes se convierte en un obstáculo para la implementación real.

### Backend
La estructura arquitectónica del backend ya presente en la solución se conserva como base de trabajo.

La arquitectura en capas no debe reestructurarse como parte del trabajo normal de implementación.

La funcionalidad de negocio se añadirá progresivamente dentro de esa base arquitectónica existente.

---

## Qué existe hoy

En el momento de redactar este documento, el proyecto cuenta con:

- una estructura de solución existente que servirá como base activa
- un shell UI frontend que se utilizará como template
- una dirección establecida hacia Blazor WebAssembly en frontend
- una estructura arquitectónica backend preservada
- una propuesta funcional inicial que define el problema a resolver y el alcance esperado de la solución :contentReference[oaicite:3]{index=3} :contentReference[oaicite:4]{index=4}

Lo que existe actualmente debe entenderse principalmente como **base, estructura y punto de partida**, no como una implementación de negocio ya completada.

---

## Qué no existe todavía

Los siguientes artefactos **todavía no están formalmente definidos** en esta etapa:

- contratos API
- flujos funcionales detallados
- modelo de datos formal
- reglas de negocio finalizadas a nivel de detalle de implementación
- contratos definitivos de integración entre UI y backend
- glosario de dominio finalizado

Estos artefactos se crearán como parte del proceso real de construcción del proyecto.

No faltan por descuido; forman parte del trabajo que comienza ahora.

---

## Módulos iniciales oficiales

Con base en la propuesta, el proyecto parte con los siguientes módulos funcionales esperados:

- carga y procesamiento de Excel
- gestión de partes
- verificación de etiquetas
- packing lists
- administración de usuarios y roles
- auditoría / historial
- configuración general del sistema :contentReference[oaicite:5]{index=5} :contentReference[oaicite:6]{index=6}

Estos módulos podrán refinarse posteriormente en submódulos o fases de implementación.

---

## Primer objetivo de implementación

El primer módulo real que se construirá es:

**Carga de Excel**

Este es el primer objetivo funcional de construcción del proyecto.

La decisión es intencional: los contratos, flujos y modelo de datos empezarán a formalizarse durante la implementación real, comenzando por la capacidad de carga de Excel.

---

## Línea base de infraestructura

La línea base de infraestructura actualmente prevista es:

- Frontend: Blazor WebAssembly
- Backend: API .NET
- Base de datos: inicialmente SQLite
- Hosting: infraestructura local sobre IIS :contentReference[oaicite:7]{index=7}

Esta línea base se mantiene activa salvo que sea modificada y documentada explícitamente.

---

## Riesgos y áreas abiertas en esta etapa

En esta etapa, las siguientes áreas permanecen abiertas y deberán aclararse progresivamente durante la implementación:

- definición exacta de contratos API
- definición formal del modelo de datos
- definición detallada de flujos funcionales
- reglas exactas de negocio por módulo
- detalles de integración con lectoras / escáneres
- manejo de concurrencia para trabajo colaborativo en packing lists
- reglas de validación de estructura y contenido para la carga de Excel :contentReference[oaicite:8]{index=8}

Estas áreas abiertas son esperables para la fase actual del proyecto.

## Decisión documentada en esta iteración: modelo técnico de sesión auth

Se cerró a nivel de diseño documental el modelo técnico de sesión para autenticación por tokens, manteniendo la arquitectura backend actual y la UI existente como shell.

### Confirmado
- Se mantiene contrato de autenticación en `docs/api/api-contracts.md` como base de trabajo.
- Access token con TTL de 20 minutos.
- Refresh proactivo 3 minutos antes de expiración.
- Rotación obligatoria de refresh token.
- Detección de replay/reuse con invalidación de cadena de sesión.
- Requisito de single-flight en frontend para evitar refresh concurrente.
- Modo bypass configurable (`Authentication:Bypass:Enabled`) con restricción de entorno autorizado.

### Estado de implementación (actualizado)
- Backend de autenticación fase 1 implementado.
- Frontend de sesión auth fase 1 implementado (login, `/me`, refresh proactivo, restauración y logout).
- Persistencia física de entidades de sesión implementada en base de datos (`AuthSession`, `RefreshToken`).

### Decisiones abiertas explícitas
- TTL exacto del refresh token.
- Algoritmo/firma final de JWT y política de llaves.
- Estrategia final de almacenamiento cliente para refresh token.
- Política de sesiones simultáneas por usuario.

---

## Regla de documentación

Este documento no es estático.

Debe actualizarse siempre que ocurra una o más de las siguientes situaciones:

- se inicia la implementación de un módulo
- se toma una decisión arquitectónica relevante
- un flujo antes indefinido pasa a estar definido
- se crea o modifica un contrato
- se introduce o modifica un modelo de datos
- una suposición funcional pasa a ser una regla confirmada
- parte del shell UI heredado es reemplazado, refactorizado o descartado
- un hito cambia el estado práctico del proyecto

Este documento funciona como un **registro del estado actual con conciencia de línea temporal**, para que el equipo pueda entender siempre:

- qué tenía el proyecto en un momento dado
- qué cambió
- cómo evolucionó
- cuál es la verdad vigente

---


## Avance implementado: Carga de Excel v1 (backend mínimo)

Se implementó el primer corte real del backend del módulo **Carga de Excel** con alcance mínimo funcional:

- endpoint `POST /api/excel-uploads`
- endpoint `GET /api/excel-uploads`
- endpoint `GET /api/excel-uploads/{id}`
- lectura de una sola hoja de Excel
- detección de la fila real de encabezados por mejor coincidencia con columnas obligatorias normalizadas
- validación de encabezados mínimos obligatorios con normalización robusta de texto
- procesamiento fila por fila con carga parcial
- inserción de nuevas partes únicamente
- rechazo de filas inválidas o duplicadas por `Part Number`
- respuesta con resumen y errores por fila
- almacenamiento del archivo original
- registro de historial básico de carga (`ExcelUpload`)
- estrategia técnica provisional de inicialización de base de datos con `EnsureCreated()` (pendiente migrar a esquema formal de migraciones)

No se implementó en esta versión:

- actualización de partes existentes
- cálculo de tipo de etiqueta
- cálculo de configuración de lectura
- procesamiento en background




## Avance implementado: Carga de Excel v1.1 (trazabilidad persistente por fila)

Se implementó una iteración incremental del backend de **Carga de Excel** centrada en trazabilidad real y mejora del modelo de persistencia:

- nueva entidad persistente `ExcelUploadRowResult` para registrar resultado por fila (`Inserted`/`Rejected`)
- persistencia de `ErrorCode` y `ErrorMessage` por fila rechazada
- vínculo explícito `Part.CreatedByExcelUploadId` para trazar qué carga creó cada parte
- persistencia de relación `ExcelUpload -> RowResults` y `ExcelUpload -> CreatedParts`
- corrección de tipos de `Part`:
  - `Caducidad` de texto a `int?`
  - `CertificationEac` de texto a `bool?`
  - `FirstFourNumbers` de texto a `int`
- ajuste de parseo por fila:

---

## Actualización 2026-04-22: Bloque B / consolidación robust-only para usuarios configurados (Fase 4 abierta)

- Se confirma que **Fase 4 sigue abierta**.
- Se implementó bridge controlado para `Authentication:Users` hacia identidad robusta persistida (`SystemUsers` + `SystemUserRole`) con feature flag de entorno.
- Objetivo del bridge: evitar bloqueo de validaciones robust-only en perfiles admin locales/desarrollo sin retirar aún fallback legacy global.
- Se mantiene transición dual segura; no se ejecuta retiro total del legacy en esta iteración.
  - `Caducidad`: `NA`/vacío => `null`; entero válido => `int`; otro valor => fila rechazada
  - `Certification EAC`: `YES` => `true`; `NO` => `false`; `NA`/vacío => `null`; otro valor => fila rechazada
  - `4 FIRST NUMERS`: obligatorio y entero; si falla parseo => fila rechazada
- se mantiene `EnsureCreated()` como estrategia técnica provisional

Para bases SQLite locales ya existentes, los cambios de esquema **no se aplican automáticamente** con `EnsureCreated()`.
Se requiere recrear la base local (o eliminar el archivo SQLite actual) y volver a iniciar la API para que se cree el nuevo esquema completo.

## Avance implementado: Carga de Excel v1 (frontend mínimo)

Se implementó una pantalla frontend mínima para operar el módulo **Carga de Excel v1** sobre la API ya existente, manteniendo el shell actual de la aplicación:

- página de carga en `/excel-uploads`
- selección de archivo y envío por `POST /api/excel-uploads`
- indicador de envío en progreso y mensajes de error/estado
- visualización de resumen de resultado de carga
- visualización de errores por fila (`rowNumber`, `partNumber`, `error`)
- historial básico consumiendo `GET /api/excel-uploads`
- acceso del módulo desde el menú actual


## Historial de cambios

### Versión inicial
- Se establece el estado del proyecto como **Construcción inicial**
- Se define la propuesta como línea base funcional inicial
- Se establece la solución actual como base activa de trabajo
- Se define la UI existente como template / shell para la nueva funcionalidad
- Se declara estable la estructura arquitectónica del backend como base de implementación
- Se confirma que todavía no existen formalmente contratos, flujos ni modelo de datos
- Se confirma que dichos artefactos se crearán durante la implementación
- Se fija como primer objetivo de implementación el módulo de **Carga de Excel**

## Avance implementado: Carga de Excel v1.2 (iteración UX/UI frontend)

Se implementó una iteración de mejora de experiencia de usuario en el frontend del módulo `/excel-uploads`:

- control de selección de archivo migrado a `MudFileUpload`
- feedback de operación migrado a `Snackbar`
- limpieza de estado post-carga para permitir cargas sucesivas sin fricción
- refresco automático del historial tras cada `POST /api/excel-uploads`
- acción de detalle en la primera columna del grid de historial
- panel lateral derecho para inspección de detalle de una carga
- alternancia entre vista general y vista por fila dentro del detalle

Para habilitar inspección histórica por fila se agregó un endpoint mínimo de detalle:

- `GET /api/excel-uploads/{id}/details`

Este cambio no modifica reglas funcionales de negocio de Carga de Excel v1/v1.1; sólo amplía consulta y experiencia de visualización.

## Avance implementado: Carga de Excel v1.3 (refinamiento UX/UI y limpieza técnica)

Se implementó una pasada fina sobre el bloque de carga de `/excel-uploads`, manteniendo la misma funcionalidad de negocio:

- `MudFileUpload` como dropzone principal de ancho completo (drag & drop + clic para seleccionar)
- ocultamiento del input nativo visible para una experiencia integrada con MudBlazor
- estado visual del archivo seleccionado (nombre, tamaño y estado "listo para cargar")
- acción clara para quitar archivo y preparar reemplazo desde la misma dropzone
- botón `Cargar` mantenido como acción final del bloque
- se mantiene limpieza post-carga y refresco automático de historial
- limpieza técnica en el componente para evitar referencias residuales de refactor

Este cambio no modifica contratos API ni reglas de procesamiento; sólo refina UX/UI y consistencia técnica del módulo.

## Avance implementado: Carga de Excel v1.4 (filtros y paginación en detalle por fila)

Se implementó una mejora incremental en el drawer de detalle de `/excel-uploads`, enfocada en navegación de cargas grandes sin modificar reglas de negocio:

- filtro por texto con selector de campo (`Part Number` o `Model`)
- filtro por `Status` de fila (`Inserted`/`Rejected`)
- acción `Limpiar filtros` para volver al estado inicial
- paginación local en la tabla de vista por fila con tamaños 5, 10 y 20
- chips de `Status` por fila con estilo consistente
- mantenimiento de vista general y vista por fila dentro del mismo drawer

Este cambio se apoya en el endpoint ya implementado `GET /api/excel-uploads/{id}/details` y no introduce contratos nuevos.


## Avance implementado: Autenticación frontend fase 1 (Blazor WASM)

Se implementó la gestión robusta de sesión en frontend, alineada con contratos backend ya activos:

- servicio central de sesión/auth con estado en memoria + persistencia en `sessionStorage`
- login real contra `POST /api/auth/login`
- carga de identidad canónica contra `GET /api/auth/me`
- refresh automático proactivo en ventana de 3 minutos previos al vencimiento
- restauración de sesión al recargar aplicación
- política single-flight para evitar refresh concurrentes
- integración del cliente nombrado `BackendApi` con handler de autorización
- logout limpio contra `POST /api/auth/logout`
- manejo consistente de `401/403` con redirección controlada
- integración de bypass desde frontend mediante bootstrap de `/api/auth/me` (sin inventar endpoint adicional)

### Decisión explícita de almacenamiento cliente en esta fase
- Se usa `sessionStorage` para persistir snapshot de sesión durante la vida de la pestaña.
- Límite conocido: la sesión no se restaura al cerrar completamente la pestaña/navegador.
- Decisión final de endurecimiento (cookie HttpOnly u otra estrategia) permanece abierta para cierre de seguridad de producción.

## Avance implementado: Autenticación backend fase 1

Se implementó el primer corte real del backend de autenticación en API .NET, manteniendo arquitectura vigente:

- entidades persistentes `AuthSession` y `RefreshToken`
- migración EF Core para esquema auth
- emisión de access token JWT (TTL 20 minutos)
- refresh token opaco con hash persistido, rotación obligatoria y detección de reuse
- endpoints `POST /api/auth/login`, `POST /api/auth/refresh`, `POST /api/auth/logout`, `GET /api/auth/me`
- soporte de bypass configurable (`Authentication:Bypass:Enabled`) restringido por entorno
- estrategia de inicialización DB migrada de `EnsureCreated()` a `Database.Migrate()`

### Explícitamente fuera de esta fase

- integración del usuario autenticado en flujo `ExcelUpload`
- autorización avanzada y administración completa de usuarios

### Decisiones abiertas que permanecen

- política final de sesiones simultáneas por usuario
- estrategia final de almacenamiento cliente de refresh token para endurecimiento productivo
- política de purga de sesiones/tokens expirados o revocados

## Avance implementado: Bloque A en Fase 4 (abierta) — Reset real de contraseña

Se implementó el flujo real end-to-end de reset password, alineado con el modelo de autenticación existente y sin mover la arquitectura backend.

### Implementado en backend
- endpoint `POST /api/auth/password/reset-request` activo.
- endpoint `POST /api/auth/password/reset-confirm` activo.
- nueva persistencia `PasswordResetToken` (token opaco hash + expiración + uso/revocación).
- nueva persistencia `UserPasswordCredential` para credencial efectiva mutable (PBKDF2 + salt), manteniendo fallback compatible con `Authentication:Users`.
- invalidación de token por uso único y revocación de tokens de reset previos/pendientes.
- manejo neutral para usuario inexistente (`202` idéntico) sin filtrar existencia de cuenta.
- política vigente tras cambio de contraseña: revocar todas las sesiones activas y refresh tokens del usuario (`Authentication:PasswordReset:RevokeAllSessionsOnPasswordReset=true`).

### Implementado en frontend
- integración real de la pantalla `/reset-password` con backend (`reset-request` + `reset-confirm`).
- captura de `usernameOrEmail`, token de reset y nueva contraseña con confirmación.
- mensajes de estado/errores alineados con respuestas API.

### Estrategia explícita de fase para entrega de token
- No se integró proveedor externo de email en este bloque.
- La entrega del token se resuelve temporalmente fuera de banda por operación/logs (registro operativo del token emitido), y queda documentada como decisión de fase abierta.

### Decisiones abiertas que continúan en Fase 4
- canal definitivo de entrega de token (email/SMTP u otro) para producción.
- endurecimiento adicional de operación (rate limiting dedicado, antifraude y auditoría extendida).
- política final de purga de tokens de reset expirados/revocados.



## Avance implementado: Bloque B en Fase 4 (abierta) — Base backend de administración de usuarios

Se implementó la base backend del módulo administrativo de usuarios, manteniendo intacta la arquitectura actual y sin mezclar alcance con Fase 5.

### Implementado en backend
- nueva entidad persistente `SystemUser` para administración de cuentas internas (`userId`, `username`, `displayName`, `email`, `isActive`, `roles`, `permissions`, timestamps).
- nueva migración de base de datos para tabla `SystemUsers` con índices para `userId`, `username`, `email` y `isActive`.
- servicio de administración de usuarios con casos base:
  - listado paginado y filtrable
  - detalle por `userId`
  - alta con credencial inicial
  - edición de datos base + cambio opcional de contraseña
  - activación/desactivación explícita
- endpoints protegidos (`/api/users`) para exponer estas capacidades al frontend.
- ajuste de CORS backend para permitir métodos `PUT` y `PATCH` en consumo SPA local.

### Integración aplicada con autenticación existente
- autenticación resuelve usuarios prioritariamente desde persistencia (`SystemUsers`) y mantiene fallback compatible con `Authentication:Users`.
- refresh, `/me` y reset de contraseña utilizan resolución por `userId` con la misma compatibilidad.
- la credencial persistida `UserPasswordCredential` sigue siendo la fuente efectiva para contraseña cuando existe.

### Preparación explícita para frontend administrativo (grid)
- contrato de listado incluye `query`, `isActive`, `page`, `pageSize` y metadatos (`totalItems`, `totalPages`) para soportar grilla con filtros/paginación.
- contrato de detalle y edición deja payload consistente para formularios de administración.

### Decisiones abiertas que continúan en Fase 4 (Bloque B)
- definición final del modelo de roles/permisos (normalizado vs serializado).
- política final de baja lógica/física (por ahora se opera con activación/desactivación).
- estrategia final de normalización/colación case-insensitive para unicidad robusta cross-DB.

## Avance implementado: Bloque B en Fase 4 (abierta) — Vista frontend administrativa de usuarios (grid)

Se implementó la vista principal del módulo de usuarios en Blazor WebAssembly, reutilizando la shell actual y consumiendo exclusivamente el backend ya implementado para administración de usuarios.

### Implementado en frontend
- nueva página protegida `/users` con patrón de grid administrativo en card principal.
- cabecera de módulo con acciones visibles (`Actualizar`, `Nuevo usuario`).
- tabla administrativa con columnas alineadas al contrato real (`userId`, `username`, `displayName`, `email`, `roles`, `permissions`, `isActive`, timestamps).
- badges visuales para estado de usuario (`Activo` / `Inactivo`) y chips para roles/permisos.
- filtros por columna visibles en la parte superior del grid.
- paginación visible con selector de tamaño de página y navegación por página.
- acciones CRUD por fila:
  - editar usuario
  - activar/desactivar usuario
  - reset password vía `newPassword` en `PUT /api/users/{userId}` (capacidad existente en backend)
- alta de usuario mediante formulario lateral conectado a `POST /api/users`.

### Integración con navegación existente
- se agregó entrada de menú `Usuarios (Admin)` sin romper la navegación actual de la shell.
- la ruta permanece bajo las reglas actuales de protección de rutas autenticadas.

### Decisiones abiertas que continúan en Fase 4 (Bloque B)
- el filtrado por `roles` y `permissions` se soporta sobre JSON serializado actual (`RolesJson`/`PermissionsJson`) y queda pendiente una estrategia definitiva cuando se cierre el modelo final de roles/permisos.
- no se implementa eliminación física/lógica adicional porque el backend actual expone activación/desactivación como operación vigente.

## Avance implementado: Bloque B en Fase 4 (abierta) — Cierre de filtrado/paginación backend-driven en `/users`

Se cerró la consistencia de filtrado y paginación del grid administrativo de usuarios sin mezclar alcance con Fase 5 ni incorporar NLog.

### Implementado en backend
- se extendió el contrato de `GET /api/users` con filtros por columna opcionales (`userId`, `username`, `displayName`, `email`, `role`, `permission`) además de `query`, `isActive`, `page` y `pageSize`.
- la paginación se aplica sobre el conjunto ya filtrado en backend, manteniendo metadatos (`totalItems`, `totalPages`) consistentes con los filtros activos.
- se conservó el alcance funcional vigente: listado, detalle, alta, edición, reset password (vía `newPassword`) y activación/desactivación.

### Implementado en frontend
- la página `/users` mantiene la UX actual (grid, drawers y acciones por fila), pero los filtros visibles ahora disparan recarga backend y dejan de filtrar únicamente la página cargada en cliente.
- el cliente `UserAdministrationApiClient` envía los filtros de columna y estado como query params reales al backend.

### Estado explícito de fase
- **Fase 4 continúa abierta**.
- este avance corresponde únicamente al **Bloque B**.

## Avance implementado: Bloque B en Fase 4 (abierta) — Refinamiento UX/UI de `/users` sin cambio de alcance funcional

Se refinó la experiencia visual y de uso del módulo `/users`, manteniendo intactas las capacidades funcionales ya implementadas (create, edit, reset password, activación/desactivación, autorización y paginación backend-driven) y sin mezclar alcance con Fase 5 ni con NLog.

### Implementado en frontend (UX/UI)
- se reemplazó la edición CSV de `roles` y `permissions` por selectores multiselección en create/edit.
- los valores de multiselección se alimentan solo con roles/permisos reales detectados desde respuestas existentes del backend (`GET /api/users` y `GET /api/users/{userId}`), sin catálogo inventado.
- la zona de filtros del grid se simplificó al patrón:
  - `SearchField` (selector de campo)
  - `SearchText`
  - `StatusFilter`
  - `Limpiar filtros`
- se conserva el envío de filtros al backend y la paginación server-side; solo cambió la UX del filtro visible.
- se ajustaron estilos del módulo para coexistir con tema actual (incluyendo light theme), eliminando colores oscuros hardcodeados en card/filtros/tabla/drawers.

### Estado explícito de fase
- **Fase 4 continúa abierta**.
- Este avance corresponde únicamente al **Bloque B**.

### Decisiones abiertas que continúan en Fase 4 (Bloque B)
- definir estrategia definitiva para descubrir catálogo global de roles/permisos cuando existan más páginas que las cargadas por el grid actual.

## Avance implementado: Control total de entrada y navegación auth (frontend fase 1.1)

Se cerró el comportamiento de acceso inicial y protección de navegación para que la app no renderice contenido protegido sin sesión activa o recuperable.

### Implementado en esta iteración
- Bootstrap de sesión bloqueante en `App.razor` antes de habilitar el router.
- Guard de navegación por política de rutas (públicas vs protegidas) con redirección a `/signin` cuando no hay sesión válida.
- Entrada normal cuando la sesión es válida, restaurada por refresh o en modo bypass.
- Conservación de la regla de backend como autoridad real para validar tokens en llamadas API (`401/403` siguen disparando limpieza/redirección desde handler HTTP).
- Redirección desde `/signin` a `/` cuando ya existe sesión autenticada.

### Rutas oficiales en esta etapa
- Públicas: `/signin`, `/signin-basic`, `/signup`, `/reset-password`, `/error`, `/error401`.
- Protegidas: toda ruta no listada como pública (incluye `/`, `/index`, `/excel-uploads`, `/counter`, `/weather`, `/logout` y nuevas rutas futuras por defecto).

### Configuración relevante (vigente)
- Backend: `Authentication:Jwt:AccessTokenTtlMinutes`, `Authentication:Jwt:RefreshProactiveWindowMinutes`, `Authentication:RefreshToken:TtlMinutes`, `Authentication:Bypass:Enabled`, `Authentication:Bypass:AllowedEnvironments`.
- Frontend: `Api:BaseUrl` y persistencia en `sessionStorage` (`AuthSessionV1`).

### Decisiones abiertas que continúan
- Estrategia final endurecida de almacenamiento del refresh token en cliente para producción.
- Política final de sesiones simultáneas por usuario.
- Definición final de comportamiento de bypass para operaciones protegidas fuera de `/api/auth/me` en fases posteriores.

## Avance implementado: Integración visible de usuario autenticado (frontend auth fase 1.2)

Se completó la consolidación del perfil autenticado en UI, manteniendo la shell actual y reutilizando la sesión existente sin duplicar lógica de autenticación/logout:

- nueva vista protegida `/profile-settings` para `Account Settings`
- visualización de datos reales de sesión (`displayName`, `username`, `email`, `userId`, `roles`, `permissions`, `authenticationMode`)
- mensaje explícito cuando un dato no está disponible en la sesión actual (por ejemplo, `email`)
- menú de perfil del header actualizado para mostrar usuario autenticado real en lugar de datos estáticos
- `Account Settings` y `View Profile` ahora navegan a `/profile-settings`
- `Log Out` del menú de perfil usa `AuthSessionService.LogoutAsync()` (flujo real existente)

### Restricciones respetadas en esta iteración
- No se agregaron endpoints backend nuevos ni se alteró arquitectura backend.
- No se implementó edición de perfil ni gestión avanzada de cuenta.
- Se usó únicamente información ya disponible en snapshot de sesión y `GET /api/auth/me`.

## Avance implementado: Bloque B en Fase 4 (abierta) — Ajuste de autorización real para `/api/users` (401 vs 403)

Se corrigió el bloqueo real del módulo de usuarios sin mezclar alcance con Fase 5 ni con NLog.

### Diagnóstico confirmado en esta iteración
- Con `Authentication:Bypass:Enabled=true` (entorno permitido), `GET /api/auth/me` respondía autenticado en modo `Bypass`.
- En ese mismo estado, `GET /api/users` devolvía `401 Unauthorized` porque el endpoint requería `[Authorize]` JWT pero no existía identidad autenticada para requests sin bearer token.
- El problema reproducido no era parseo JSON (ya corregido), sino autenticación/autorización efectiva del request a `/api/users`.

### Corrección aplicada
- Se incorporó esquema de autenticación bypass para requests sin bearer token cuando bypass está habilitado y el entorno está permitido.
- Se introdujeron políticas explícitas para módulo usuarios:
  - `UsersRead`: `Administrator` o claim `permission=users.read|users.manage`.
  - `UsersManage`: `Administrator` o claim `permission=users.manage`.
- `UsersController` ahora aplica `UsersRead` en `GET` y `UsersManage` en `POST/PUT/PATCH`.
- Se actualizó el cliente frontend de usuarios para distinguir mensaje de error por `401` vs `403`.

### Impacto funcional confirmado
- En bypass válido, `/api/users` deja de fallar por ausencia de identidad y respeta políticas de permiso/rol.
- En usuario autenticado sin permisos administrativos, el resultado esperado para `/api/users` pasa a ser `403` (sesión válida pero sin autorización).
- Se mantiene intacto el alcance funcional del módulo (`grid`, filtros/paginación, create/edit/reset password/activación-desactivación).

### Estado explícito de fase
- **Fase 4 continúa abierta**.
- Este avance corresponde únicamente al **Bloque B**.

### Decisiones abiertas que continúan en Fase 4 (Bloque B)
- Catálogo/normalización definitiva de permisos y roles (actualmente lista serializada).
- Política final de asignación mínima de permisos administrativos por entorno (usuarios reales vs bypass).

## Avance implementado: Bloque B en Fase 4 (abierta) — Diagnóstico/corrección de autenticación HTTP en frontend `/users`

Se atendió el caso reproducido con `Authentication:Bypass:Enabled=false` y login real (`admin`) donde `/api/auth/me` validaba sesión de usuario, pero `GET /api/users` devolvía `401`.

### Diagnóstico confirmado en esta iteración
- El problema confirmado está en la autenticación de la request HTTP del módulo `/users`, no en políticas `UsersRead`/`UsersManage`.
- En el estado reproducido, la llamada de frontend a `GET /api/users` podía salir sin `Authorization: Bearer` al depender del pipeline genérico de `HttpClient` para adjuntar token.
- El `401` observado corresponde a ausencia de identidad autenticada efectiva en la request de `/api/users` (no a falta de permisos, que sería `403`).

## Avance implementado: Bloque B / Fase 4 (abierta) — validación E2E robust-only reproducible para bridge local (2026-04-22)

Se ejecutó validación E2E controlada para usuarios configurados/locales bridgeados en Development, con alcance exclusivo de Bloque B y manteniendo **Fase 4 abierta**.

Configuración usada en ejecución:

- `ASPNETCORE_ENVIRONMENT=Development`
- `Authorization:UseRobustMatrix=true`
- `Authorization:EnableLegacyFallback=false`
- `Authentication:ConfiguredUsersRobustBridge:Enabled=true`

Resultado final validado para usuario `admin` de `Authentication:Users`:

- `POST /api/auth/login` => `200`
- `GET /api/auth/me` => `200`
- `GET /api/users` => `200`
- `GET /api/authorization-matrix/roles` => `200`

Correcciones acotadas aplicadas para lograrlo:

- normalización consistente de almacenamiento Guid en SQLite (string minúscula) para eliminar fallo FK en bridge robusto;
- habilitación efectiva de descubrimiento de migración `20260422090000_AddAuthorizationMatrixAdministrationModule` (atributos EF), permitiendo aplicar el seed robusto faltante para políticas de `AuthorizationMatrixAdministration`;
- script reproducible agregado: `scripts/validation/robust_only_e2e_bridge.sh`.

Decisión de estado: no se retira legacy global; la validación cerrada aplica al escenario robust-only + bridge local validado en esta sesión.

### Corrección aplicada
- `UserAdministrationApiClient` ahora adjunta explícitamente el bearer token de la sesión real (`AuthSessionService`) antes de enviar cada request del módulo (`GET/POST/PUT/PATCH`).
- El cliente de `/users` se registra usando `BackendApiRaw` + `AuthSessionService` para que el módulo utilice de forma determinística la misma sesión/token vigente del frontend.
- Se mantiene intacto el resto del comportamiento funcional del módulo (`grid`, filtros/paginación, create/edit/reset password/activación-desactivación).

### Estado explícito de fase
- **Fase 4 continúa abierta**.
- Este avance corresponde únicamente al **Bloque B**.

## Avance documental: Bloque B en Fase 4 (abierta) — Estándar reusable para nuevas vistas Grid administrativas

Se formalizó una base documental reusable para pedir nuevas vistas administrativas tipo Grid, separando estándar general y referencia concreta del módulo de usuarios.

### Documentos incorporados
- `docs/frontend/grid-view-standard.md`
- `docs/frontend/grid-view-users-reference.md`

### Alcance explícito
- Trabajo estrictamente documental reusable; no agrega implementación funcional nueva.
- No mezcla alcance con Fase 5, NLog ni cambios backend no necesarios.
- Se mantiene regla de no inventar campos/filtros/catálogos/acciones no soportadas por backend.
- Se formaliza como patrón UX validado: `SearchField`, `SearchText`, `StatusFilter`, `Limpiar filtros`.
- Se formaliza que campos multivalor deben resolverse con multiselección (no CSV).

### Estado explícito de fase
- **Fase 4 continúa abierta**.

## Avance documental: Bloque B en Fase 4 (abierta) — Propuesta robusta de modelo de roles/módulos/acciones

Se documentó la propuesta completa para robustecer autorización en Bloque B, tomando como referencia estructural `docs/Permissions.xml` y referencia conceptual UX `docs/Managment.html`, sin copiar esos artefactos como modelo final.

### Decisiones funcionales cerradas en esta iteración
- roles como catálogo explícito (no inferido/libre): `SuperAdmin`, `Operators`, `Managers`.
- autorización separada por módulo y por acción.
- semántica base confirmada:
  - `Module Authorized` controla acceso al módulo.
  - `Action Authorized` controla ejecución de la acción.
- el atributo `Permissions` del XML no se usa como centro del modelo final.

### Entregable documental incorporado
- `docs/security-authorization-model-block-b-phase4.md` con:
  - propuesta conceptual integral,
  - entidades/relaciones,
  - catálogo base,
  - persistencia recomendada,
  - traducción a políticas backend,
  - uso correcto de referencias XML/HTML,
  - decisiones abiertas e impacto por capa.

### Estado explícito de fase
- **Fase 4 continúa abierta**.
- Este avance corresponde únicamente al **Bloque B**.
- No mezcla alcance con Fase 5 ni con NLog.

### Decisiones abiertas que continúan en Fase 4 (Bloque B)
- cierre definitivo de catálogo de módulos/acciones contra endpoints finales por módulo.
- estrategia de migración de almacenamiento serializado actual (`RolesJson`/`PermissionsJson`) a tablas normalizadas.
- trazabilidad/auditoría de cambios de autorización a nivel rol/módulo/acción.

## Avance documental vigente: robustecimiento de autorización (Bloque B / Fase 4 abierta)

Se consolidó la propuesta documental del modelo robusto de autorización para Bloque B con estas definiciones confirmadas:

- catálogo explícito de roles (`SuperAdmin`, `Operators`, `Managers`),
- catálogo de módulos,
- catálogo de acciones por módulo,
- autorización separada por módulo y por acción,
- lineamiento de traslado de protección por ruta a validación explícita módulo+acción.

Este avance se mantiene en **Fase 4 abierta** y no implica cierre de fase.
También se mantiene fuera de alcance cualquier mezcla con Fase 5 o NLog.

Referencias de trabajo utilizadas y trazadas en documentación:

- `docs/Permissions.xml` (estructura histórica de árbol de permisos),
- `docs/Managment.html` (referencia conceptual de UX de administración),
- `docs/frontend/grid-view-standard.md`,
- `docs/frontend/grid-view-users-reference.md`.

## Avance documental: diseño técnico implementable de autorización robusta (Bloque B / Fase 4 abierta)

Se consolidó la propuesta técnica implementable y migrable del modelo de autorización robusta para Bloque B, manteniendo explícitamente que **Fase 4 sigue abierta**.

### Alcance confirmado en esta iteración
- diseño de entidades/relaciones para catálogos de roles, módulos y acciones;
- diseño de matriz de autorización por rol-módulo y rol-acción;
- definición de reglas de integridad y unicidad;
- estrategia de seed inicial (roles, módulos, acciones y matriz base);
- estrategia de transición desde `RolesJson`/`PermissionsJson` y usuarios existentes;
- definición de convivencia temporal con autenticación actual;
- separación de qué permanece en token/claims y qué se resuelve en backend.

### Exclusiones explícitas
- no se implementa todavía UI administrativa completa de permisos;
- no se ejecuta todavía reemplazo completo del modelo legacy sin cierre de transición;
- no se mezcla este avance con Fase 5 ni NLog.

## Avance implementado: Bloque B autorización robusta (Fase 4 abierta)

Se implementó en backend la persistencia inicial del modelo robusto de autorización definido para Bloque B, manteniendo transición con el modelo legacy.

### Implementado
- Nuevas tablas/entidades de autorización robusta: `RoleCatalog`, `ModuleCatalog`, `ModuleActionCatalog`, `RoleModuleAuthorization`, `RoleModuleActionAuthorization`, `SystemUserRole`.
- Seed inicial de catálogos y matriz base de autorización (roles/módulos/acciones).
- Backfill de `SystemUsers` a `SystemUserRole` basado en `RolesJson` cuando existe equivalencia.
- Fallback de asignación de rol `Operators` para usuarios sin rol legacy reconocible.
- `UserAdministrationService` sincroniza en alta/edición tanto el modelo legacy (`RolesJson`) como `SystemUserRole`.

### No implementado en este corte
- Reemplazo total del runtime de autorización hacia la nueva matriz.
- Eliminación de `RolesJson` y `PermissionsJson`.
- UI administrativa completa de permisos.

### Estado de fase
- **Fase 4 sigue abierta**.

## Avance implementado: resolución runtime de autorización robusta (Bloque B / Fase 4 abierta)

Se implementó la resolución efectiva de autorización en backend usando el modelo robusto, con transición segura y compatibilidad legacy.

### Implementado en backend
- Nuevo servicio de runtime `AuthorizationMatrixService` con contrato `IAuthorizationMatrixService` para resolver autorización por módulo y acción.
- Nuevo handler/policies sobre ASP.NET Authorization:
  - `UsersRead` ahora evalúa `UsersAdministration.View`.
  - `UsersManage` ahora evalúa `UsersAdministration.Edit`.
- Evaluación runtime aplicada:
  1. intenta matriz robusta (`SystemUserRole` + `RoleModuleAuthorization` + `RoleModuleActionAuthorization`);
  2. exige precondición de módulo para validar acción;
  3. si no hay resolución robusta suficiente, ejecuta fallback legacy controlado.
- Compatibilidad temporal activa:
  - convivencia con `RolesJson` y `PermissionsJson` sin remoción,
  - convivencia con policies legacy (mismo nombre externo),
  - continuidad para usuarios existentes.
- Bypass ajustado para coexistencia:
  - bypass sigue disponible sin romper auth actual,
  - cuando no hay `SystemUser` real (bypass), resuelve por fallback legacy de roles/permisos en claims.
- Ajuste en emisión de roles de usuario autenticado:
  - token prioriza roles de `SystemUserRole/RoleCatalog` si existen;
  - fallback a `RolesJson` cuando aún no hay datos robustos del usuario.
- Configuración agregada:
  - `Authorization:UseRobustMatrix`
  - `Authorization:EnableLegacyFallback`

### Alcance de migración en este corte
- Migración aplicada explícitamente a políticas del módulo `/api/users`.
- No se migra todavía el resto de endpoints/policies (se mantiene transición incremental).

### Estado de fase
- **Fase 4 sigue abierta**.

## Avance implementado: primera vista administrativa de matriz por rol (Bloque B / Fase 4 abierta)

Se implementó la primera pantalla administrativa para gestión de permisos por rol sobre módulos y acciones, adaptada al shell actual y conectada a backend real.

### Implementado en backend
- Nuevo servicio de aplicación/infra para administración de matriz: `IAuthorizationAdministrationService` + `AuthorizationAdministrationService`.
- Nuevos endpoints:
  - `GET /api/authorization-matrix/roles`
  - `GET /api/authorization-matrix/roles/{roleCode}`
  - `PUT /api/authorization-matrix/roles/{roleCode}`
- Los endpoints operan sobre catálogos y matriz robusta ya existente (`RoleCatalog`, `ModuleCatalog`, `ModuleActionCatalog`, `RoleModuleAuthorization`, `RoleModuleActionAuthorization`), sin retirar `RolesJson`/`PermissionsJson`.

### Implementado en frontend
- Nueva ruta `/authorization-matrix` para administrar autorización por rol.
- UX aplicada:
  - selector de rol,
  - listado de módulos,
  - switch `Module Authorized`,
  - switches de acciones hijas por módulo,
  - guardado explícito de cambios.
- La implementación toma como referencia conceptual `docs/Managment.html`, sin copiarla literal, y mantiene patrón visual del shell actual.

### Estado de fase
- **Fase 4 sigue abierta**.
- Este avance no se mezcla con Fase 5 ni con NLog.


## Avance reciente: Bloque B / Fase 4 abierta (integración users con catálogo robusto de roles)

- Estado de fase: **Fase 4 continúa abierta** (no cerrada en esta iteración).
- El módulo `/users` quedó integrado para priorizar asignación de roles sobre `RoleCatalog` + `SystemUserRole`.
- Se mantiene convivencia transitoria explícita con `RolesJson` / `PermissionsJson` para compatibilidad de transición.
- En altas/ediciones de `/users`, `RolesJson` se conserva solo como espejo transitorio de roles ya sincronizados en `SystemUserRole` (sin arrastrar nuevos roles legacy fuera de catálogo).
- No se incorporaron cambios de Fase 5 ni NLog en este avance.

## Avance reciente: Bloque B / Fase 4 abierta (consolidación runtime por policies)

- Estado de fase: **Fase 4 continúa abierta** (no cerrada en esta iteración).
- Se separó la autorización de `/api/authorization-matrix` del módulo de usuarios:
  - nueva policy `AuthorizationMatrixManage`,
  - nueva resolución robusta `AuthorizationMatrixAdministration.Manage`,
  - fallback legacy transitorio compatible con `Administrator` y permisos `authorization.matrix.manage` / `users.manage`.
- Se migró granularmente `/api/users` a policies robustas por acción:
  - `UsersRead` => `UsersAdministration.View`,
  - `UsersCreate` => `UsersAdministration.Create`,
  - `UsersEdit` => `UsersAdministration.Edit`,
  - `UsersActivateDeactivate` => `UsersAdministration.ActivateDeactivate`.
- Se agregó seed incremental para catálogo robusto de autorización de matriz:
  - módulo `AuthorizationMatrixAdministration`,
  - acción `Manage`,
  - autorización inicial para `SuperAdmin`.
- Se mantiene convivencia transitoria explícita con `RolesJson` / `PermissionsJson`; no hay retiro legacy en este corte.
- No se incorporaron cambios de Fase 5 ni NLog en este avance.

## Avance reciente: Bloque B / Fase 4 abierta (reducción incremental de lecturas legacy en runtime y `/users`)

- Estado de fase: **Fase 4 continúa abierta** (no cerrada en esta iteración).
- Esta iteración se limitó a reducción incremental en backend (sin mezclar con Fase 5 ni con NLog, y sin retiro total legacy).

### Implementado
- `AuthService` ahora prioriza también en lectura de permisos el modelo robusto:
  - deriva permisos efectivos (`users.read`, `users.manage`, `authorization.matrix.manage`) desde `SystemUserRole` + matriz robusta,
  - conserva `PermissionsJson` como complemento/fallback transitorio (unión sin pérdida) durante la transición.
- `UserAdministrationService.ListAsync` reduce dependencia de `PermissionsJson` para filtros:
  - el filtro `permission` incorpora resolución robusta por roles y matriz para permisos conocidos,
  - mantiene evaluación legacy sobre `PermissionsJson` para compatibilidad de usuarios/claims todavía no migrados.

### Sigue vivo en transición
- `PermissionsJson` continúa activo para compatibilidad.
- fallback legacy por claims sigue activo bajo `Authorization:EnableLegacyFallback`.
- fallback de lectura por `RolesJson` se mantiene cuando usuario no tiene asignaciones robustas en `SystemUserRole`.

### Validación de camino controlado a robust-only
- Con `Authorization:EnableLegacyFallback=false`, ya es validable en entorno controlado el runtime robusto para usuarios con:
  - `SystemUserRole` correctamente asignado,
  - matriz robusta (`RoleModuleAuthorization` + `RoleModuleActionAuthorization`) completa para los módulos/acciones requeridos.
- Bypass y usuarios no migrados continúan dependiendo de fallback legacy; por eso no se ejecuta corte total en esta iteración.

## Avance reciente: Bloque B / Fase 4 abierta (validación controlada de modo robust-only)

- Estado de fase: **Fase 4 continúa abierta** (sin cierre de fase en este corte).
- Alcance acotado a Bloque B: validación controlada de autorización con `EnableLegacyFallback=false`, sin mezclar con Fase 5 ni con NLog.

### Validación ejecutada (2026-04-22)

Se comparó comportamiento real en entorno local controlado:

- escenario transición (`EnableLegacyFallback=true`), y
- escenario robust-only controlado (`EnableLegacyFallback=false`).

Flujos/endpoints verificados:

- `POST /api/auth/login`
- `GET /api/auth/me`
- `GET /api/users`
- `GET /api/authorization-matrix/roles`

### Resultado resumido

- Con fallback habilitado: login, `/me`, `/users` y `/authorization-matrix/roles` operan correctamente.
- Con fallback deshabilitado (mismo usuario configurado en `Authentication:Users`): login y `/me` operan, pero `/users` y `/authorization-matrix/roles` responden `403`.

### Lectura del resultado

- El sistema ya soporta validaciones robust-only por subconjuntos, pero **no** está listo para apagado global del legacy.
- Persisten dependencias de transición para perfiles no migrados completamente a `SystemUsers` + `SystemUserRole`.

### Dependencias legacy que siguen activas/bloqueantes

- fallback legacy por claims (`EnableLegacyFallback`),
- `RolesJson` en trayectorias de transición,
- `PermissionsJson` en resolución efectiva de permisos de sesión.

### Decisiones abiertas que continúan

- definir y ejecutar plan por subconjuntos/perfiles robust-ready antes de cualquier corte global;
- cerrar migración operativa de usuarios que hoy viven solo en `Authentication:Users` hacia identidad/roles robustos persistidos;
- mantener Fase 4 abierta hasta cerrar validación robust-only de endpoints críticos sin fallback.
