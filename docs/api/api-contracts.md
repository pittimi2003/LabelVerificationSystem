# Contratos API

## Propósito de este documento

Este documento define la estructura inicial de contratos API del proyecto.

Su objetivo es:

- establecer la forma en que el frontend consumirá el backend
- alinear módulos funcionales, endpoints y responsabilidades
- evitar que se inventen contratos durante la implementación
- servir como base para refinamiento progresivo a medida que los módulos se construyan

Este documento no representa todavía el contrato HTTP definitivo de cada endpoint.

En esta etapa debe leerse como una **definición inicial de contratos API por módulo**.

---

## Estado actual de los contratos

**Estado:** Inicial

Todavía no existen contratos cerrados de implementación.

En esta etapa se documentan:

- módulos API esperados
- responsabilidades por módulo
- endpoints probables iniciales
- estructuras de request/response a nivel conceptual
- decisiones pendientes

La propuesta funcional inicial ya establece que la API debe cubrir al menos:

- carga y procesamiento de Excel
- CRUD de partes
- cálculo de configuraciones
- validación de escaneo 1 y 2
- gestión de packing lists
- auditoría
- exportación :contentReference[oaicite:1]{index=1}

---

## Principios de diseño de contratos

### 1. El frontend consume solo API
El frontend en Blazor WebAssembly no debe acceder directamente a dominio, persistencia ni infraestructura.  
Toda interacción funcional debe ocurrir a través de contratos HTTP.

### 2. Los contratos deben nacer desde el comportamiento real
No deben diseñarse endpoints decorativos o genéricos sin relación clara con flujos funcionales reales.

### 3. Los contratos deben crecer por módulo
Se documentarán y cerrarán progresivamente conforme avance la implementación real del proyecto.

### 4. La respuesta API debe ser consistente
Aunque los payloads finales aún no están cerrados, todos los módulos deben converger hacia una convención uniforme de respuestas, errores y validaciones.

### 5. Los contratos deben poder evolucionar
Mientras el proyecto esté en construcción inicial, los contratos podrán ajustarse, siempre que el cambio quede documentado.

---

## Convenciones iniciales

## Base path sugerido
Todos los endpoints del sistema deben exponerse bajo un prefijo común:

`/api`

## Formato de intercambio
- JSON para operaciones estándar
- `multipart/form-data` para carga de archivos cuando aplique

## Convención de nombres
- recursos en plural cuando representen colecciones
- rutas orientadas a intención funcional cuando el caso de uso lo requiera
- evitar nombres ambiguos o genéricos sin contexto de negocio

## Versionado
Pendiente de definición.

Mientras no se defina otra estrategia, se asume una única versión activa no versionada explícitamente.

## Autenticación
Definida a nivel de contrato para implementación posterior.

La propuesta contempla gestión de usuarios y roles y, desde este documento, se establece el contrato HTTP inicial para autenticación sin asumir endpoints ya implementados. :contentReference[oaicite:2]{index=2} :contentReference[oaicite:3]{index=3}

### Contrato propuesto de autenticación (.NET API + Blazor WebAssembly)

> Estado de implementación actual confirmado (fase backend auth 1): endpoints `POST /api/auth/login`, `POST /api/auth/refresh`, `POST /api/auth/logout`, `GET /api/auth/me` implementados.
> Estado frontend fase 1: login, bootstrap con `/api/auth/me`, refresh proactivo (3 minutos), restauración en recarga, single-flight y logout implementados en Blazor WASM.

#### Objetivos del contrato
- sesión robusta y estable para frontend Blazor WebAssembly
- access token de corta vida (20 minutos)
- renovación anticipada con refresh token desde 3 minutos antes del vencimiento
- soporte de modo configurable de bypass (acceso automático sin usuarios)
- contrato explícito para exponer identidad/claims del usuario autenticado a UI

#### Parámetros de sesión (normativos)
- `access_token_ttl`: **20 minutos** (`1200` segundos)
- ventana de renovación recomendada: iniciar refresh cuando `expiresAtUtc - nowUtc <= 3 minutos`
- `refresh_token_ttl`: configurable por `Authentication:RefreshToken:TtlMinutes` (valor operativo inicial: `1440` minutos)
- rotación de refresh token: **obligatoria** en cada renovación exitosa
- tolerancia de reloj (clock skew): hasta `60` segundos en validación de expiración

#### Endpoints propuestos

### `POST /api/auth/login`
Autentica credenciales y crea sesión inicial.

#### Request DTO
```json
{
  "usernameOrEmail": "string",
  "password": "string",
  "rememberMe": false,
  "clientInfo": {
    "app": "BlazorWasm",
    "deviceId": "string-opcional"
  }
}
```

#### Response DTO (200)
```json
{
  "accessToken": "jwt-string",
  "tokenType": "Bearer",
  "expiresAtUtc": "2026-04-16T18:25:00Z",
  "expiresInSeconds": 1200,
  "refreshToken": "opaque-token",
  "refreshExpiresAtUtc": "2026-04-17T18:25:00Z",
  "user": {
    "userId": "guid|string",
    "username": "string",
    "displayName": "string",
    "email": "string",
    "roles": ["Operator"],
    "permissions": ["excel.upload.create"]
  }
}
```

#### Códigos esperados
- `200 OK`: autenticación exitosa
- `400 Bad Request`: payload inválido
- `401 Unauthorized`: credenciales inválidas o usuario inactivo/bloqueado
- `429 Too Many Requests`: throttling por intentos fallidos

#### Reglas de validación
- `usernameOrEmail` requerido, trim, longitud `3..256`
- `password` requerido, longitud mínima `8`
- `clientInfo.app` opcional pero si se envía debe ser `BlazorWasm` en esta etapa

---

### `POST /api/auth/refresh`
Renueva sesión con refresh token (rotación obligatoria).

#### Request DTO
```json
{
  "refreshToken": "opaque-token"
}
```

#### Response DTO (200)
Misma estructura de `POST /api/auth/login`, con nuevos `accessToken` y `refreshToken`.

#### Códigos esperados
- `200 OK`: renovación exitosa
- `400 Bad Request`: payload inválido
- `401 Unauthorized`: refresh token inválido, revocado, expirado o no reconocido
- `409 Conflict`: token ya usado (detección de replay)

#### Reglas de validación
- `refreshToken` requerido, no vacío
- cada refresh token solo puede usarse una vez (one-time use)
- si se detecta replay, invalidar cadena de sesión asociada

---

### `POST /api/auth/logout`
Revoca la sesión activa (refresh token actual y/o cadena de sesión).

#### Request DTO
```json
{
  "refreshToken": "opaque-token-opcional"
}
```

#### Response DTO
Sin payload (`204`).

#### Códigos esperados
- `204 No Content`: cierre exitoso (idempotente)
- `401 Unauthorized`: access token ausente o inválido cuando la política requiera autenticación

---

### `GET /api/auth/me`
Devuelve información del usuario autenticado para bootstrap de UI y autorización cliente.

#### Request
Sin body. Requiere `Authorization: Bearer <access_token>`.

#### Response DTO (200)
```json
{
  "isAuthenticated": true,
  "authenticationMode": "User",
  "user": {
    "userId": "guid|string",
    "username": "string",
    "displayName": "string",
    "email": "string",
    "roles": ["Operator"],
    "permissions": ["excel.upload.create"]
  },
  "session": {
    "expiresAtUtc": "2026-04-16T18:25:00Z",
    "refreshRecommendedAtUtc": "2026-04-16T18:22:00Z",
    "serverUtcNow": "2026-04-16T18:05:00Z"
  }
}
```

#### Códigos esperados
- `200 OK`: token válido
- `401 Unauthorized`: token inválido/expirado/ausente

---

### `POST /api/auth/password/reset-request`
Solicita recuperación de contraseña (envío de token/código fuera de banda).

> Estado fase 1: **no implementado** (depende de decisiones aún abiertas de gestión formal de usuarios y canal de entrega).

#### Request DTO
```json
{
  "email": "user@example.com"
}
```

#### Response DTO (202)
```json
{
  "message": "If the account exists, reset instructions were sent."
}
```

#### Códigos esperados
- `202 Accepted`: respuesta neutral (no filtra existencia de usuario)
- `400 Bad Request`: formato inválido de email
- `429 Too Many Requests`: rate limit

---

### `POST /api/auth/password/reset-confirm`
Confirma cambio de contraseña con token de recuperación.

> Estado fase 1: **no implementado** por la misma dependencia funcional.

#### Request DTO
```json
{
  "resetToken": "string",
  "newPassword": "string",
  "confirmPassword": "string"
}
```

#### Response DTO
Sin payload (`204`).

#### Códigos esperados
- `204 No Content`: cambio exitoso
- `400 Bad Request`: payload inválido o contraseñas no coinciden
- `401 Unauthorized`: token inválido/expirado

#### Reglas de validación
- `newPassword` y `confirmPassword` requeridos y deben coincidir
- política mínima: longitud `>= 8`, al menos 1 letra y 1 número
- `resetToken` requerido, no vacío

#### Nota de alineación UI
Estos endpoints se alinean con la base de UI existente en `Pages/Authentication` (login/reset password) y permiten conectar esos flujos sin redefinir rutas al implementar backend.

---

#### Exposición de información del usuario autenticado
- En cada access token se recomienda incluir claims mínimos: `sub`, `name`, `email`, `role` (múltiple), `permission` (múltiple), `sid`, `jti`, `exp`.
- El endpoint `GET /api/auth/me` es la fuente canónica para bootstrap de sesión en Blazor WASM tras recarga de página.
- Implementación frontend fase 1: snapshot de sesión (incluyendo refresh token) persistido en `sessionStorage` y access token aplicado al cliente `BackendApi` por handler de autorización.
- Decisión de hardening pendiente: migrar refresh token a cookie `HttpOnly` (u opción equivalente) cuando infraestructura y estrategia CSRF estén cerradas.

#### Modo bypass configurable (acceso automático sin usuarios)
- Agregar flag de configuración de backend (nombre sugerido): `Authentication:Bypass:Enabled`.
- Cuando `Enabled=true`:
  - los endpoints protegidos aceptan identidad sintética de sistema (por ejemplo `username = "system-bypass"`).
  - `GET /api/auth/me` responde `authenticationMode = "Bypass"` e incluye usuario virtual y roles/permisos definidos por configuración.
  - Implementación backend fase 1: `POST /api/auth/login` y `POST /api/auth/refresh` responden `409 Conflict` indicando que bypass sustituye autenticación de usuario.
- Restricción obligatoria: bypass solo permitido en ambientes explícitamente autorizados por configuración (por ejemplo `Development`/`Local`), nunca habilitado por defecto.
- Auditoría: toda operación en bypass debe registrar marca explícita `authMode=Bypass`.

#### Requisitos de robustez de sesión
- renovación anticipada: frontend debe intentar refresh al entrar en la ventana de 3 minutos previos a expiración.
- single-flight refresh en frontend: evitar múltiples refresh concurrentes.
- retry acotado con backoff ante fallos transitorios de red.
- cierre de sesión automático al fallar refresh por `401`/`409`.
- revocación de cadena de refresh ante sospecha de replay.

### Diseño técnico del modelo de sesión por tokens (cierre de iteración)

> Alcance: diseño técnico y reglas de contrato. Esta sección **no implica implementación completa** en backend/frontend en esta iteración.

#### 1) Diseño técnico del access token
- Formato: JWT firmado por backend (algoritmo concreto pendiente de cierre de seguridad; no se fija proveedor de identidad en esta etapa).
- Vida útil: **20 minutos** (`1200s`) como regla cerrada.
- Claims mínimos requeridos por contrato:
  - `sub` (identidad única de usuario o identidad sintética de bypass)
  - `name`
  - `email` (si aplica)
  - `role` (múltiple)
  - `permission` (múltiple)
  - `sid` (identificador de sesión lógica)
  - `jti` (identificador único del token)
  - `iat`, `nbf`, `exp`
- Uso: autorización de endpoints API y bootstrap de estado autenticado en UI vía `GET /api/auth/me`.

#### 2) Diseño técnico del refresh token
- Formato: token opaco, aleatorio, de alta entropía.
- Persistencia: solo hash del token en backend; el valor plano no se persiste.
- Semántica: token de un solo uso (one-time use) para habilitar rotación segura.
- TTL: **pendiente de cierre** (decisión abierta). Debe ser mayor que 20 minutos y definido formalmente en implementación.
- Asociación: cada refresh token pertenece a una sesión (`sid`) y a un usuario (o identidad de bypass si aplica por política de entorno).

#### 3) Estrategia de expiración y refresh
- Refresh proactivo obligatorio: iniciar renovación cuando resten **3 minutos o menos** para `exp`.
- Clock skew máximo: 60 segundos.
- Backend valida expiración de refresh token y estado de revocación.
- Si refresh falla por `401` o `409`, frontend debe cerrar sesión local y requerir nueva autenticación (o reentrada por bypass, según configuración activa).

#### 4) Estrategia de rotación y revocación
- Rotación obligatoria por cada `POST /api/auth/refresh` exitoso:
  - refresh token anterior queda consumido/inválido
  - se emite nuevo refresh token
  - se mantiene/actualiza cadena de sesión (`sid`) según diseño de implementación
- Revocación explícita:
  - `POST /api/auth/logout` revoca sesión actual (idempotente)
  - detección de replay/reuse revoca cadena de sesión asociada por seguridad
- Revocación administrativa global de usuario: **pendiente de formalización** (fuera de esta iteración).

#### 5) Estrategia de detección de replay/reuse
- Cada refresh token tiene estado de consumo (`usedAtUtc`) y revocación (`revokedAtUtc`, razón).
- Si llega un refresh token ya consumido, backend responde `409 Conflict` (reuse detectado).
- Al detectar reuse, backend marca comprometida la cadena de sesión y revoca tokens activos asociados a `sid`.
- El evento debe quedar en auditoría con marca de seguridad.

#### 6) Restauración de sesión al recargar aplicación (Blazor WASM)
- Fuente canónica de identidad: `GET /api/auth/me` con access token vigente.
- Flujo recomendado:
  1. app carga estado local de credenciales (según almacenamiento definido)
  2. valida ventana de expiración del access token
  3. si está por vencer (<= 3 minutos), ejecuta refresh antes de hidratar sesión
  4. consulta `/api/auth/me` para reconstruir usuario/claims en UI
- Si no hay tokens válidos, la app queda en estado anónimo o modo bypass (si está habilitado y permitido por entorno).

#### 7) Política single-flight para evitar refresh concurrentes
- Frontend debe usar una única operación de refresh en curso compartida (mutex/promise compartida).
- Requests concurrentes que detecten necesidad de refresh deben esperar el mismo resultado.
- Si el refresh único falla, todas las solicitudes pendientes heredan el fallo y se ejecuta logout controlado.

#### 8) Retry acotado ante fallos transitorios
- Sólo para fallos transitorios de red/timeouts/5xx durante refresh.
- Política propuesta (cerrada para contrato cliente): máximo **2 reintentos** además del intento inicial.
- Backoff exponencial corto sugerido: `300ms`, `900ms` (con jitter opcional).
- No reintentar en `400/401/409`.

#### Decisiones abiertas explícitas (no cerradas aún)
- TTL exacto de refresh token.
- Algoritmo/firma final de JWT y gestión de llaves.
- Ubicación final de almacenamiento del refresh token en cliente (cookie HttpOnly si infraestructura final lo soporta vs almacenamiento protegido alterno).
- Política de revocación masiva por usuario/rol desde administración.

---

## Convención inicial de respuestas

## Respuesta exitosa conceptual
Las respuestas exitosas deberían contener, según el caso:

- resultado principal
- mensajes relevantes para UI
- datos derivados necesarios para el flujo
- metadatos mínimos si son necesarios

## Respuesta de error conceptual
Las respuestas de error deberían poder comunicar, según el caso:

- tipo de error
- mensaje principal
- detalle validable por UI
- errores por campo o por fila, si aplica
- código funcional o técnico, si se define

## Decisión pendiente
Debe definirse si existirá:

- una envoltura uniforme tipo `ApiResponse<T>`
- respuestas directas por endpoint
- una combinación según el caso de uso

---

# 1. Módulo API de Carga de Excel

## Estado
**Primer módulo real a construir**

## Responsabilidad
Recibir un archivo Excel, validar su formato y estructura, procesar su contenido, calcular información derivada y registrar el resultado de la carga. La propuesta funcional inicial incluye validación de formato y estructura, cálculo automático del tipo de etiqueta y cálculo automático de configuración de lectura. :contentReference[oaicite:4]{index=4} :contentReference[oaicite:5]{index=5}

## Endpoints iniciales propuestos

### `POST /api/excel-uploads`
Endpoint principal para registrar una nueva carga de archivo Excel.

#### Request conceptual
- archivo Excel
- datos contextuales mínimos si se requieren
- usuario autenticado implícito por contexto de seguridad

#### Response conceptual
- identificador de carga
- estado de procesamiento
- resumen del resultado
- métricas generales
- lista o referencia de errores si aplica

### `GET /api/excel-uploads`
Consulta de historial o listado de cargas.

#### Response conceptual
- colección de cargas
- estado
- fecha
- usuario
- resumen de resultado

### `GET /api/excel-uploads/{uploadId}`
Consulta de una carga específica.

#### Response conceptual
- detalle de la carga
- métricas
- errores o advertencias
- información de auditoría asociada si aplica

## Implementación cerrada para Excel Upload v1
- Endpoints implementados:
  - `POST /api/excel-uploads`
  - `GET /api/excel-uploads`
  - `GET /api/excel-uploads/{id}`
- Tipo de request: `multipart/form-data` con campo `file`.
- El procesamiento es en línea (sin background).
- Se procesa una sola hoja, sin depender del nombre de la hoja.
- Si faltan columnas mínimas obligatorias en el encabezado, la carga se rechaza como archivo inválido (HTTP 400).
- La respuesta del `POST` devuelve resultado final de la carga con resumen y errores por fila.

### Response de `GET /api/excel-uploads` y `GET /api/excel-uploads/{id}` (v1)
- `uploadId`: identificador de la carga.
- `originalFileName`: nombre original del archivo cargado.
- `uploadedAtUtc`: fecha/hora UTC de registro de la carga.
- `status`: estado básico de la carga.
- `totalRows`: filas leídas (sin encabezado).
- `insertedRows`: filas insertadas como nuevas partes.
- `rejectedRows`: filas rechazadas.

### Status codes de historial (v1)
- `GET /api/excel-uploads`: `200 OK`.
- `GET /api/excel-uploads/{id}`:
  - `200 OK` cuando la carga existe.
  - `404 Not Found` cuando el id no existe.

### `GET /api/excel-uploads/{id}/details`
Consulta detallada de una carga específica para UX de inspección histórica.

#### Response (v1.2)
- resumen de carga: `uploadId`, `originalFileName`, `uploadedAtUtc`, `status`, `totalRows`, `insertedRows`, `rejectedRows`
- `rows`: detalle por fila persistida con:
  - `rowNumber`
  - `partNumber`
  - `model`
  - `status` (`Inserted` o `Rejected`)
  - `errorCode`
  - `errorMessage`

### Status codes de detalle (v1.2)
- `GET /api/excel-uploads/{id}/details`:
  - `200 OK` cuando la carga existe.
  - `404 Not Found` cuando el id no existe.

### Response de `POST /api/excel-uploads` (v1)
- `uploadId`: identificador de la carga.
- `fileName`: nombre original del archivo.
- `totalRows`: filas leídas (sin encabezado).
- `insertedRows`: filas insertadas como nuevas partes.
- `rejectedRows`: filas rechazadas.
- `rowErrors`: lista de errores por fila con:
  - `rowNumber`
  - `partNumber`
  - `error`

### Reglas funcionales cerradas reflejadas en el contrato
- Solo inserta nuevas partes.
- No actualiza partes existentes.
- Duplicado contra sistema: mismo `Part Number`.
- Carga parcial: las filas válidas se insertan y las inválidas/duplicadas se rechazan sin detener toda la carga.
- El archivo original se conserva.
- Se registra historial básico de carga desde v1.

## Decisiones pendientes
- versionado formal del endpoint

---

# 2. Módulo API de Partes

## Responsabilidad
Administrar el catálogo oficial de partes del sistema.

## Endpoints iniciales propuestos

### `GET /api/parts`
Obtiene el catálogo de partes.

### `GET /api/parts/{partId}`
Obtiene el detalle de una parte específica.

### `POST /api/parts`
Registra una nueva parte.

### `PUT /api/parts/{partId}`
Actualiza una parte existente.

### `DELETE /api/parts/{partId}`
Elimina o desactiva una parte, según la estrategia que se defina.

## Request conceptual
Todavía pendiente de definición exacta, pero deberá incluir los datos oficiales necesarios para representar una parte y sus atributos de validación.

## Response conceptual
- identificador
- número de parte
- datos oficiales
- tipo de etiqueta
- información de configuración asociada si aplica

## Decisiones pendientes
- atributos exactos de la parte
- política de borrado
- criterios de unicidad
- si las configuraciones viajan embebidas o relacionadas
- si existe búsqueda paginada o filtrada desde la primera versión

---

# 3. Módulo API de Configuraciones

## Responsabilidad
Exponer o resolver configuraciones de lectura asociadas a partes o procesos de validación.

## Justificación
La propuesta funcional contempla cálculo automático de configuraciones y uso de configuraciones durante el flujo de verificación. :contentReference[oaicite:6]{index=6} :contentReference[oaicite:7]{index=7}

## Endpoints iniciales propuestos

### `GET /api/configurations/{configurationId}`
Obtiene una configuración específica.

### `GET /api/parts/{partId}/configuration`
Obtiene la configuración asociada a una parte.

## Alternativa posible
Si la configuración no se administra como recurso independiente, parte de esta información podrá venir incluida en respuestas del módulo de partes o del módulo de verificación.

## Decisiones pendientes
- si Configuration será un recurso API de primer nivel
- si habrá CRUD de configuraciones
- si la configuración se calcula siempre en backend
- si la configuración se expone completa o resumida hacia el frontend

---

# 4. Módulo API de Verificación

## Responsabilidad
Soportar el flujo de verificación de etiquetas mediante dos escaneos.

## Justificación
La propuesta define validación de escaneo 1 y 2, así como servicios internos asociados a verificación. :contentReference[oaicite:8]{index=8} :contentReference[oaicite:9]{index=9}

## Endpoints iniciales propuestos

### `POST /api/verifications/scan-1`
Procesa el primer escaneo.

#### Request conceptual
- valor escaneado
- contexto del operador
- contexto operativo si aplica

#### Response conceptual
- parte identificada
- configuración requerida
- estado del flujo
- mensaje al operador
- error si no se encontró la parte

### `POST /api/verifications/scan-2`
Procesa el segundo escaneo.

#### Request conceptual
- referencia al contexto del primer escaneo o datos equivalentes
- valor de etiqueta completa escaneada
- contexto del operador

#### Response conceptual
- resultado de verificación
- detalle de coincidencia o discrepancia
- estado final del flujo
- instrucciones operativas si aplica

### `GET /api/verifications/{verificationId}`
Consulta una verificación específica, si se decide persistirla como recurso consultable.

## Decisiones pendientes
- cómo se vinculan scan 1 y scan 2
- si el contexto del primer escaneo se guarda en backend, frontend o ambos
- si el backend devuelve discrepancias por campo
- si existe endpoint de reinicio o cancelación
- si se exponen verificaciones históricas desde primera versión

---

# 5. Módulo API de Packing Lists

## Responsabilidad
Permitir creación, consulta, operación colaborativa, cierre, reapertura y exportación de packing lists.

## Justificación
La propuesta funcional inicial define creación o unión por número, registro de líneas a partir de verificaciones correctas, cierre, monitoreo, exportación y posible reapertura. :contentReference[oaicite:10]{index=10} :contentReference[oaicite:11]{index=11}

## Endpoints iniciales propuestos

### `POST /api/packing-lists/open-or-join`
Crea un packing list o une al operador a uno existente a partir del número ingresado.

#### Request conceptual
- número de packing list

#### Response conceptual
- packing list resultante
- estado actual
- información operativa mínima
- indicador de si fue creado o reutilizado

### `GET /api/packing-lists/{packingListId}`
Obtiene detalle de un packing list.

### `GET /api/packing-lists/{packingListId}/lines`
Obtiene líneas asociadas al packing list.

### `POST /api/packing-lists/{packingListId}/lines`
Agrega una línea al packing list.

#### Observación
La línea probablemente se origine a partir de una verificación correcta, pero esa relación exacta aún debe definirse.

### `DELETE /api/packing-lists/{packingListId}/lines/{lineId}`
Elimina una línea registrada.

### `POST /api/packing-lists/{packingListId}/close`
Cierra un packing list.

### `POST /api/packing-lists/{packingListId}/reopen`
Reabre un packing list, si la funcionalidad se implementa.

### `GET /api/packing-lists/{packingListId}/operators`
Obtiene operadores activos, si se materializa como endpoint separado.

### `GET /api/packing-lists/{packingListId}/export`
Exporta el packing list.

## Decisiones pendientes
- si la apertura/unión debe resolverse por número o por id interno
- si las líneas se agregan por verificación o por payload directo
- cómo manejar concurrencia
- si el monitoreo en tiempo real se resuelve por polling o tiempo real real
- formato exacto de exportación

---

# 6. Módulo API de Usuarios y Roles

## Responsabilidad
Gestionar usuarios del sistema y la asignación de roles.

## Justificación
La propuesta funcional inicial contempla gestión de usuarios y roles Operador, Supervisor y Administrador. :contentReference[oaicite:12]{index=12}

## Endpoints iniciales propuestos

### `GET /api/users`
Lista usuarios.

### `GET /api/users/{userId}`
Obtiene detalle de usuario.

### `POST /api/users`
Crea usuario.

### `PUT /api/users/{userId}`
Actualiza usuario.

### `DELETE /api/users/{userId}`
Desactiva o elimina usuario según la estrategia que se defina.

### `GET /api/roles`
Obtiene catálogo de roles disponibles.

## Decisiones pendientes
- si el alta de usuario incluye credenciales
- estrategia de autenticación
- si los roles son catálogo fijo o administrable
- si existirá endpoint de login separado
- política de bloqueo o desactivación

---

# 7. Módulo API de Auditoría

## Responsabilidad
Exponer información auditable para consulta administrativa o trazabilidad operativa.

## Justificación
La propuesta inicial menciona auditoría e historial como capacidades del sistema. :contentReference[oaicite:13]{index=13} :contentReference[oaicite:14]{index=14} :contentReference[oaicite:15]{index=15}

## Endpoints iniciales propuestos

### `GET /api/audit-logs`
Lista eventos auditables.

### `GET /api/audit-logs/{auditLogId}`
Obtiene detalle de un evento auditable.

### `GET /api/excel-uploads/{uploadId}/audit`
Consulta auditoría relacionada con una carga, si se decide exponer por subrecurso (no implementado actualmente).

## Decisiones pendientes
- eventos exactos auditables
- filtros por fecha, usuario, entidad y tipo de evento
- nivel de detalle de cada evento
- retención de información

---

# 8. Módulo API de Configuración General

## Responsabilidad
Permitir consulta y actualización de parámetros generales del sistema.

## Justificación
La propuesta inicial contempla configuración general del sistema. :contentReference[oaicite:16]{index=16}

## Endpoints iniciales propuestos

### `GET /api/system-configuration`
Obtiene configuración general.

### `PUT /api/system-configuration`
Actualiza configuración general.

## Decisiones pendientes
- qué parámetros existirán
- si la configuración será única o versionada
- si se expondrá completa o por secciones
- qué roles pueden modificarla

---

## Contratos transversales probables

Estos contratos todavía no están formalmente definidos, pero son altamente probables:

### Contrato de error de validación
Especialmente necesario para:
- carga de Excel
- alta y edición de partes
- procesos de verificación

### Contrato de error funcional
Necesario para expresar errores de negocio controlados:
- parte no encontrada
- packing list cerrado
- usuario sin permisos
- línea no eliminable
- estructura de archivo inválida

### Contrato de paginación o consulta
Probable para:
- catálogo de partes
- usuarios
- historial de cargas
- auditoría

### Contrato de exportación
Necesario si el sistema expone exportación de packing lists o reportes.

---

## Prioridad de formalización de contratos

La prioridad actual recomendada es:

### Prioridad 1
- `POST /api/excel-uploads`
- `GET /api/excel-uploads`
- `GET /api/excel-uploads/{uploadId}`
- `GET /api/excel-uploads/{uploadId}/details`

### Prioridad 2
- `GET /api/parts`
- `POST /api/parts`
- `PUT /api/parts/{partId}`

### Prioridad 3
- `POST /api/verifications/scan-1`
- `POST /api/verifications/scan-2`

### Prioridad 4
- endpoints de packing lists

### Prioridad 5
- usuarios, auditoría y configuración general

---

## Reglas aún pendientes

Las siguientes decisiones siguen abiertas y afectarán la forma final de los contratos:

- autenticación y autorización
- convención final de respuestas
- convención final de errores
- versionado de API
- estrategia de ids públicos vs internos
- paginación y filtros
- granularidad de resultados de carga
- estrategia de contexto entre escaneo 1 y escaneo 2
- estrategia de concurrencia para packing lists
- estrategia de exportación

---

## Regla de evolución

Este documento debe actualizarse cuando ocurra cualquiera de estas situaciones:

- se cierre el contrato real de un endpoint
- se implemente un módulo y se necesiten payloads definitivos
- se defina la convención de errores
- se adopte autenticación formal
- se introduzca versionado
- se elimine, cambie o consolide un endpoint inicialmente propuesto

---

## Historial

### Versión inicial
- Se documenta la estructura inicial de contratos API por módulo.
- Se fija Carga de Excel como primer módulo con prioridad de formalización.
- Se definen endpoints iniciales propuestos sin fijar aún payloads definitivos.
- Se deja explícito qué aspectos del contrato siguen pendientes de decisión.
