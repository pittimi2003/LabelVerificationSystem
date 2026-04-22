# Modelo de autorización robusto (Bloque B / Fase 4 abierta)

> Estado de fase: **Fase 4 continúa abierta**.  
> Alcance: robustecimiento de roles/permisos en **Bloque B**.  
> Exclusiones explícitas: este documento no cierra Fase 4, no mezcla alcance con Fase 5 y no incorpora NLog.

## Referencias usadas en esta definición

Esta propuesta se consolida usando únicamente referencias confirmadas del repositorio:

- `docs/Permissions.xml` como referencia **estructural histórica** del árbol `grupo -> módulo -> acción`.
- `docs/Managment.html` como referencia **conceptual UX** para edición centralizada por rol.
- `docs/frontend/grid-view-standard.md` como marco UX/documental reusable.
- `docs/frontend/grid-view-users-reference.md` como límites reales del patrón vigente en UI.

---

## 1) Resumen técnico implementable (objetivo de esta iteración)

Esta iteración transforma la propuesta conceptual de Bloque B en un diseño **implementable y migrable** sin ejecutar aún el reemplazo completo.

### Decisiones funcionales cerradas (base obligatoria)

1. Roles como catálogo explícito.
2. Catálogo inicial de roles:
   - `SuperAdmin`
   - `Operators`
   - `Managers`
3. Catálogo explícito de módulos.
4. Catálogo explícito de acciones por módulo.
5. Validación de autorización por **módulo + acción** (no solo por ruta).
6. Semántica base:
   - `Module Authorized` = acceso al módulo.
   - `Action Authorized` = capacidad operativa sobre acción concreta.
   - `Action Authorized = true` => ejecuta acción.
   - `Action Authorized = false` => no ejecuta acción.
7. Atributo `Permissions` del XML queda como referencia histórica/estructural, no como núcleo del modelo final.

---

## 2) Diseño técnico de entidades/tablas/relaciones

> Nota: nombres en PascalCase alineados a nomenclatura ya usada en backend .NET. Se pueden mapear a snake_case en físico si se decide más adelante.

### 2.1 Catálogos

#### `RoleCatalog`
- **PK**: `Id` (Guid)
- `Code` (string, requerido, único global)
- `Name` (string, requerido)
- `IsActive` (bool, requerido, default true)
- `CreatedAtUtc` (datetime)
- `UpdatedAtUtc` (datetime)

**Reglas**
- `Code` debe ser estable y case-insensitive único.
- No eliminación física en operación normal (soft lifecycle por `IsActive`).

#### `ModuleCatalog`
- **PK**: `Id` (Guid)
- `Code` (string, requerido, único global)
- `Name` (string, requerido)
- `Description` (string, opcional)
- `DisplayOrder` (int, requerido)
- `IsActive` (bool, requerido, default true)
- `CreatedAtUtc` / `UpdatedAtUtc`

**Reglas**
- `Code` estable para policy mapping backend/frontend.
- `DisplayOrder` único opcional (recomendado) para evitar empates en UX administrativa.

#### `ModuleActionCatalog`
- **PK**: `Id` (Guid)
- **FK**: `ModuleId` -> `ModuleCatalog.Id`
- `Code` (string, requerido)
- `Name` (string, requerido)
- `Description` (string, opcional)
- `DisplayOrder` (int, requerido)
- `IsActive` (bool, requerido, default true)
- `CreatedAtUtc` / `UpdatedAtUtc`

**Reglas**
- Unicidad compuesta: (`ModuleId`, `Code`).
- Unicidad recomendada de orden por módulo: (`ModuleId`, `DisplayOrder`).

### 2.2 Matriz de autorización

#### `RoleModuleAuthorization`
- **PK**: `Id` (Guid) *(alternativa: PK compuesta; se mantiene surrogate para auditoría futura)*
- **FK**: `RoleId` -> `RoleCatalog.Id`
- **FK**: `ModuleId` -> `ModuleCatalog.Id`
- `Authorized` (bool, requerido)
- `CreatedAtUtc` / `UpdatedAtUtc`

**Reglas**
- Unicidad compuesta obligatoria: (`RoleId`, `ModuleId`).
- `Authorized` explícito (sin null).

#### `RoleModuleActionAuthorization`
- **PK**: `Id` (Guid)
- **FK**: `RoleId` -> `RoleCatalog.Id`
- **FK**: `ModuleActionId` -> `ModuleActionCatalog.Id`
- `Authorized` (bool, requerido)
- `CreatedAtUtc` / `UpdatedAtUtc`

**Reglas**
- Unicidad compuesta obligatoria: (`RoleId`, `ModuleActionId`).
- `Authorized` explícito (sin null).

### 2.3 Relación usuario-rol

#### `SystemUserRole`
- **PK**: `Id` (Guid)
- **FK**: `SystemUserId` -> `SystemUser.Id`
- **FK**: `RoleId` -> `RoleCatalog.Id`
- `IsPrimary` (bool, requerido, default false)
- `AssignedAtUtc` (datetime)
- `AssignedByUserId` (Guid, opcional, FK a `SystemUser.Id`)

**Reglas**
- Unicidad compuesta obligatoria: (`SystemUserId`, `RoleId`).
- Restricción recomendada: máximo un registro `IsPrimary=true` por usuario.

---

## 3) Reglas de integridad y consistencia (backend)

1. **Deny by default**: ausencia de fila en matriz => no autorizado.
2. **Precondición de módulo para acción**: si `RoleModuleAuthorization.Authorized = false`, cualquier acción del módulo se resuelve no autorizada aunque exista acción en true.
3. **Acciones huérfanas prohibidas lógicamente**:
   - al guardar acción en `true`, el backend debe validar que el módulo esté en `true` para el mismo rol (o elevar módulo automáticamente y registrar evento, decisión pendiente).
4. **No cascada destructiva** en eliminaciones:
   - para evitar pérdida histórica accidental, preferir `IsActive=false` en catálogos.
5. **Normalización de códigos**:
   - almacenar `Code` canonicalizado (ej. trim + upper/lower invariant) para comparar sin ambigüedad.
6. **Concurrencia**:
   - usar token de concurrencia (rowversion/timestamp o `UpdatedAtUtc` validado) en edición de matriz.

---

## 4) Catálogos iniciales y seed base

## 4.1 Seed de roles (cerrado)
- `SuperAdmin`
- `Operators`
- `Managers`

## 4.2 Seed de módulos (baseline operativo)
- `UsersAdministration`
- `ExcelUploads`
- `PartsCatalog`
- `LabelVerification`
- `PackingLists`
- `AuditTrail`
- `SystemConfiguration`

## 4.3 Seed de acciones por módulo (baseline operativo)
- `UsersAdministration`: `View`, `Create`, `Edit`, `ActivateDeactivate`, `ResetPassword`.
- `ExcelUploads`: `View`, `Upload`.
- `PartsCatalog`: `View`, `Create`, `Edit`.
- `LabelVerification`: `View`, `Execute`.
- `PackingLists`: `View`, `Create`, `Edit`, `Close`.
- `AuditTrail`: `View`.
- `SystemConfiguration`: `View`, `Edit`.

## 4.4 Seed de matriz base de autorización (propuesta migrable)

### `SuperAdmin`
- Todos los módulos `Authorized=true`.
- Todas las acciones `Authorized=true`.

### `Operators`
- Módulos `true`: `ExcelUploads`, `LabelVerification`, `PackingLists`.
- Módulos `false`: `UsersAdministration`, `PartsCatalog`, `AuditTrail`, `SystemConfiguration`.
- Acciones `true` sugeridas:
  - `ExcelUploads`: `View`, `Upload`
  - `LabelVerification`: `View`, `Execute`
  - `PackingLists`: `View`, `Create`, `Edit`
- Resto de acciones: `false`.

### `Managers`
- Módulos `true`: todos excepto los que se restrinjan explícitamente por decisión funcional.
- Acciones `true` sugeridas:
  - `UsersAdministration`: `View`
  - `ExcelUploads`: `View`
  - `PartsCatalog`: `View`, `Edit`
  - `LabelVerification`: `View`
  - `PackingLists`: `View`, `Close`
  - `AuditTrail`: `View`
  - `SystemConfiguration`: `View`
- Acciones de alto impacto administrativo (ej. `ResetPassword`, `SystemConfiguration.Edit`) en `false` salvo cierre explícito.

> Esta matriz es baseline para arranque técnico. No debe tratarse como política final de negocio hasta validación de dueños funcionales.

---

## 5) Estrategia de transición desde estado transitorio (`RolesJson` / `PermissionsJson`)

## 5.1 Principio
Transición en **modo convivencia**, sin corte big-bang.

## 5.2 Fases de transición de datos

1. **Expandir esquema**
   - Crear nuevas tablas de catálogo, matriz y asignación usuario-rol.
   - Sin remover `RolesJson`/`PermissionsJson`.

2. **Seed controlado**
   - Insertar roles, módulos, acciones y matriz base.

3. **Backfill usuarios existentes**
   - Mapear `RolesJson` -> `SystemUserRole`.
   - Si un usuario no tiene rol reconocible, asignar rol seguro por defecto (`Operators` o rol mínimo definido por negocio) y registrar incidencia para revisión manual.

4. **Backfill permisos legacy**
   - Interpretar `PermissionsJson` solo como ayuda de migración.
   - Traducir a filas en `RoleModuleAuthorization`/`RoleModuleActionAuthorization` cuando exista equivalencia conocida.
   - Si no hay equivalencia directa, registrar hallazgo en reporte de migración y aplicar deny explícito.

5. **Dual-read (temporal)**
   - Resolver autorización primero desde nuevo modelo.
   - Si falta dato migrado, fallback controlado a legacy por ventana acotada de transición.

6. **Dual-write (solo si imprescindible)**
   - Durante breve ventana, cambios administrativos pueden actualizar ambos modelos para evitar divergencia.
   - Desactivar dual-write al cerrar validación de transición.

7. **Retiro gradual legacy**
   - Congelar escritura de `RolesJson`/`PermissionsJson`.
   - Retirar fallback.
   - Planificar eliminación física en migración posterior, no en esta iteración documental.

## 5.3 Reglas de migración para usuarios existentes

- Priorizar continuidad de acceso mínimo operativo (evitar lockout masivo).
- Cualquier ambigüedad -> deny por defecto + cola de remediación.
- Emitir reporte de migración con:
  - usuarios mapeados automáticamente,
  - usuarios con conflictos,
  - permisos no traducibles.

---

## 6) Convivencia temporal con autenticación actual

1. **Autenticación no cambia en esta iteración**
   - login, emisión de token, refresh y sesión continúan con arquitectura vigente.

2. **Autorización evoluciona por capas**
   - Nuevas verificaciones módulo/acción se agregan en backend sin romper pipeline de autenticación.

3. **Compatibilidad temporal**
   - Policies nuevas deben convivir con claims/roles actuales hasta completar backfill.

4. **Feature flags recomendados**
   - `Authorization:UseRobustMatrix` (on/off por entorno).
   - `Authorization:EnableLegacyFallback` (solo transición).

---

## 7) Token/claims vs resolución backend

## 7.1 Debe mantenerse en token/claims
- `sub` (usuario).
- identificadores de sesión (`sid` o equivalente).
- roles resumidos (`role`) para compatibilidad y UI básica.
- metadata mínima de seguridad (issuer, exp, etc.).

## 7.2 Debe resolverse desde backend (fuente de verdad)
- autorización efectiva módulo/acción.
- validación de precondición módulo->acción.
- invalidación inmediata por cambios de permisos (sin esperar expiración larga del token).

## 7.3 Recomendación operativa
- No embutir matriz completa en claims.
- Usar servicio central `IAuthorizationMatrixService` + caché corta por usuario/rol con invalidación por versión.

---

## 8) Riesgos técnicos y de migración

1. **Deriva entre legacy y nuevo modelo** en ventana dual-write.
2. **Mapeos incompletos** de `PermissionsJson` a catálogo nuevo.
3. **Incidencias de performance** si cada request consulta matriz sin caché.
4. **Stale authorization** por caché no invalidada tras cambios administrativos.
5. **Riesgo de sobreasignación** en seeds iniciales si no se valida matriz por negocio.
6. **Impacto UX** si frontend asume autorizaciones locales y backend deniega.

Mitigaciones mínimas:
- reporte de migración,
- invalidación de caché por versión,
- pruebas de regresión 401/403,
- rollout por feature flags.

---

## 9) Orden recomendado de implementación real (sin ejecutar aún reemplazo total)

1. **DB**: crear tablas nuevas + constraints + índices.
2. **Seed**: roles/módulos/acciones/matriz base.
3. **Backfill**: mapear usuarios/legacy a nuevo modelo.
4. **Backend service**: implementar `IAuthorizationMatrixService` con deny by default.
5. **Policies**: migrar endpoints críticos a verificación módulo+acción.
6. **Auth pipeline**: activar dual-read/fallback temporal.
7. **Frontend incremental**:
   - primero consumo de capacidades efectivas (solo lectura),
   - después herramientas mínimas operativas de administración,
   - **sin implementar aún UI administrativa completa de permisos**.
8. **Observabilidad y auditoría básica** de cambios de autorización.
9. **Cierre de transición** y retiro legacy en iteración posterior.

---

## 10) Impacto por capa

### Backend
- Nuevas entidades EF y migraciones.
- Servicio central de autorización módulo/acción.
- Policies específicas por caso de uso.
- Endpoints de consulta de matriz/capacidades (si se habilitan en fase siguiente).

### Frontend
- Mantener patrón grid/documental vigente.
- Adaptar visibilidad/acciones a capacidades resueltas por backend.
- Evitar decisiones de seguridad solo cliente.

### Auth
- Sin rediseño del flujo de autenticación en esta iteración.
- Ajuste de claims mínimos + resolución server-side.

### Documentación
- Mantener trazabilidad explícita de Fase 4 abierta.
- Actualizar `docs/database/data-model.md` con el esquema lógico de autorización.
- Actualizar estado del proyecto para registrar que el diseño técnico quedó definido pero no implementado completamente.

---

## 11) Decisiones abiertas que deben cerrarse antes de implementación completa

1. Política oficial de multi-rol (permitido en runtime o restringido a rol único).
2. Regla exacta cuando acción=true y módulo=false (rechazo estricto vs autocorrección).
3. Matriz final de autorización por rol validada por negocio (no solo baseline técnico).
4. Estrategia formal de auditoría de cambios de permisos (tabla/evento y retención).
5. Contrato API definitivo para administración de permisos por rol.
6. Política de caché/invalidez distribuida para autorización en entornos con múltiples instancias.
7. Fecha de retiro final de `RolesJson`/`PermissionsJson`.

---

## 12) Qué sigue explícitamente fuera de alcance en esta iteración

- No implementar UI administrativa completa de permisos.
- No ejecutar reemplazo total del modelo actual sin cerrar transición.
- No cerrar Fase 4.
- No incorporar Fase 5 ni NLog en este flujo.

---

## 7) Estado de implementación backend (esta iteración)

> **Fase 4 permanece abierta**. Este avance no cierra Fase 4 y se mantiene en Bloque B.

Implementado en backend:

- Persistencia física de entidades del modelo robusto:
  - `RoleCatalog`
  - `ModuleCatalog`
  - `ModuleActionCatalog`
  - `RoleModuleAuthorization`
  - `RoleModuleActionAuthorization`
  - `SystemUserRole`
- Configuración EF Core con:
  - claves primarias,
  - unicidades,
  - índices,
  - relaciones y FKs restrictivas,
  - colación `NOCASE` para `Code` en catálogos.
- Migración con seed inicial de:
  - roles (`SuperAdmin`, `Operators`, `Managers`),
  - módulos,
  - acciones por módulo,
  - matriz base rol/módulo y rol/acción.
- Backfill inicial de usuarios existentes (`SystemUsers`) hacia `SystemUserRole` usando `RolesJson` cuando es traducible, con fallback de rol `Operators` si no hay rol reconocido.
- Convivencia transitoria explícita mantenida:
  - `RolesJson` y `PermissionsJson` **no se eliminan**,
  - login/auth actual se conserva,
  - escritura administrativa sigue manteniendo campos legacy y agrega sincronización hacia `SystemUserRole`,
  - `RolesJson` se mantiene como espejo transitorio de roles efectivamente sincronizados en catálogo (sin arrastrar roles no catalogados en nuevas altas/ediciones).

No implementado en esta iteración:

- Reemplazo completo del motor de autorización legacy en runtime.
- UI administrativa completa del nuevo modelo.
- Retiro físico de `RolesJson` / `PermissionsJson`.

## 13) Estado de implementación runtime (iteración actual, Bloque B / Fase 4 abierta)

> **Fase 4 sigue abierta**. Esta implementación no cierra Fase 4.

Implementado en backend para resolución efectiva en runtime:

- Servicio central `AuthorizationMatrixService` (infra) + contrato `IAuthorizationMatrixService` (application) para evaluar autorización efectiva por:
  - módulo (`ModuleCode`)
  - acción dentro de módulo (`ActionCode`).
- Estrategia runtime activa:
  1. intenta resolver con modelo robusto (`RoleCatalog` + `SystemUserRole` + `RoleModuleAuthorization` + `RoleModuleActionAuthorization`);
  2. aplica precondición de módulo antes de acción;
  3. si no puede resolver robusto (usuario sin datos migrados / bypass / transición), aplica fallback legacy controlado por flag.
- Policies de `/api/users` migradas internamente a requirement/handler basado en módulo+acción:
  - `UsersRead` => `UsersAdministration.View`
  - `UsersCreate` => `UsersAdministration.Create`
  - `UsersEdit` => `UsersAdministration.Edit`
  - `UsersActivateDeactivate` => `UsersAdministration.ActivateDeactivate`
- Nueva policy dedicada para matriz de autorizaciones:
  - `AuthorizationMatrixManage` => `AuthorizationMatrixAdministration.Manage`
- Compatibilidad transitoria mantenida:
  - fallback legacy sigue reconociendo `role=Administrator` y claims `users.read` / `users.manage` / `authorization.matrix.manage`;
  - usuarios legacy y bypass continúan operativos mientras se completa transición.
- Bypass ajustado para convivencia:
  - se mantiene flujo actual de bypass;
  - al no existir `SystemUser` real, la autorización se resuelve por fallback legacy de claims;
  - no se rompe `login`/`refresh`/`/me` por este cambio.
- Configuración de rollout:
  - `Authorization:UseRobustMatrix` (default `true`)
  - `Authorization:EnableLegacyFallback` (default `true`)

No implementado en esta iteración runtime:

- migración de todos los endpoints/policies al modelo robusto (en este corte se migran `/api/users` y `/api/authorization-matrix`);
- invalidación distribuida/caché avanzada de matriz;
- retiro de `RolesJson`/`PermissionsJson`.

## 14) Avance de consolidación final (Bloque B, iteración actual; Fase 4 abierta)

> **Fase 4 permanece abierta**. No hay cierre de fase ni retiro total legacy en esta iteración.

Implementado en esta iteración:

- Reducción incremental de escritura legacy en `/api/users`:
  - al crear/editar usuarios, `SystemUserRole` sigue siendo la fuente robusta principal;
  - `RolesJson` se persiste como snapshot alineado a roles robustos sincronizados, evitando propagar valores legacy no presentes en `RoleCatalog`.
- Se mantiene compatibilidad:
  - lectura de roles efectivos continúa priorizando `SystemUserRole` con fallback a `RolesJson` para usuarios todavía no migrados;
  - no se altera el fallback legacy de claims en runtime para no romper bypass/transición.

Estado transitorio explícito después del ajuste:
- Sigue vivo: `PermissionsJson`, fallback legacy de claims, fallback por `RolesJson` cuando no hay asignación robusta.
- Reducido: escritura de `RolesJson` desde `/api/users` (ya no conserva roles fuera de catálogo en nuevas operaciones administrativas).


## 10) Avance implementado en esta iteración (Bloque B / integración users)

> Estado reiterado: **Fase 4 permanece abierta**.

Se implementó integración del módulo de usuarios con el modelo robusto de roles:

- `/api/users` ahora resuelve roles efectivos priorizando `SystemUserRole` + `RoleCatalog` (fallback a `RolesJson` solo si aún no hay asignación robusta).
- alta/edición de usuario sincroniza asignaciones robustas en `SystemUserRole`.
- se añadió endpoint de catálogo real para users: `GET /api/users/roles`.
- UI `/users` consume ese catálogo para asignación de roles (sin depender principalmente de inferencias del grid).
- `RolesJson`/`PermissionsJson` se mantienen de forma transitoria explícita para compatibilidad durante la ventana de migración.

Fuera de alcance de esta iteración:
- cierre de Fase 4,
- cambios de Fase 5,
- incorporación de NLog.

## 15) Avance de iteración: reducción incremental de lecturas legacy (Bloque B / Fase 4 abierta)

> **Fase 4 permanece abierta**. No se realizó retiro total de legacy en este corte.

### 15.1) Avance de iteración: cutover controlado en sesión auth (subconjunto robust-ready)

> **Fase 4 permanece abierta**.  
> Sin apagado global de legacy y sin mezcla con Fase 5/NLog.

Se aplicó reducción adicional de dependencia legacy en el subconjunto robust-ready ya validado en Development:

- subconjunto: `Authorization:RobustOnlyCutover.Enabled=true`, `UserIds=[admin-001]`, `Scopes=[UsersAdministration:View, AuthorizationMatrixAdministration:Manage]`.
- en `AuthService`, para usuarios dentro de ese subconjunto:
  - no hay fallback de roles a `RolesJson` cuando faltan asignaciones robustas;
  - permisos efectivos de sesión se derivan únicamente de matriz robusta y no se fusionan con `PermissionsJson`.

Se mantiene transición para lo no robust-ready:

- usuarios fuera del subconjunto continúan con fallback actual (`RolesJson`/`PermissionsJson`);
- `EnableLegacyFallback` sigue disponible para claims legacy en runtime de autorización;
- no se ejecuta retiro global de campos legacy en persistencia.

Implementado en esta iteración:

- Prioridad robusta también en lectura de permisos de sesión (`AuthService`):
  - se derivan permisos efectivos conocidos (`users.read`, `users.manage`, `authorization.matrix.manage`) desde roles robustos (`SystemUserRole`) y matriz robusta (`RoleModuleAuthorization` + `RoleModuleActionAuthorization`);
  - `PermissionsJson` permanece como compatibilidad transitoria, combinándose con el resultado robusto para no romper transición.
- Reducción incremental en filtros de `/api/users` (`UserAdministrationService.ListAsync`):
  - el filtro por `permission` ya no depende únicamente de `PermissionsJson`;
  - incorpora resolución robusta por rol/matriz para permisos conocidos;
  - mantiene fallback legacy por `PermissionsJson` para usuarios aún no migrados o permisos no mapeados.

Dependencias legacy que permanecen explícitamente activas:
- `PermissionsJson` (lectura y persistencia transitoria).
- fallback por claims legacy en `AuthorizationMatrixService` controlado por `Authorization:EnableLegacyFallback`.
- fallback a `RolesJson` cuando no hay roles robustos efectivos del usuario.

Camino verificable a modo robust-only en entorno controlado:
- ya es factible validar `Authorization:EnableLegacyFallback=false` para usuarios totalmente migrados (con `SystemUserRole` y matriz robusta completa);
- no es seguro aún para bypass ni para usuarios sin migración completa, por lo que se mantiene transición dual.

## 16) Validación controlada robust-only (Bloque B / Fase 4 abierta, 2026-04-22)

> **Fase 4 permanece abierta**. Esta validación no implica cierre de fase ni retiro global de legacy.

### Perímetro validado en esta iteración

Validación ejecutada en entorno local controlado, con API levantada sobre SQLite local y modo **robust-only** explícito:

- `Authorization:UseRobustMatrix=true`
- `Authorization:EnableLegacyFallback=false`
- `Authentication:ConfiguredUsersRobustBridge:Enabled=true`
- `ASPNETCORE_ENVIRONMENT=Development` (entorno permitido por `AllowedEnvironments`)

Perímetro funcional verificado:

- `POST /api/auth/login`
- `GET /api/auth/me`
- `GET /api/users`
- `GET /api/authorization-matrix/roles`

Se trabajó con el usuario `admin` configurado en `Authentication:Users` (caso local), **sin** retiro global de legacy para el resto de escenarios.

### Resultado real por flujo

En ejecución reproducible del **2026-04-22** (esta sesión), con script de validación dedicado:

- `POST /api/auth/login` => **200**
- `GET /api/auth/me` => **200**
- `GET /api/users` => **200**
- `GET /api/authorization-matrix/roles` => **200**

Conclusión validada: para `admin` local/configurado, el camino **robust-only + bridge** ya opera end-to-end en los endpoints críticos del alcance (Bloque B / Fase 4 abierta).

### Dependencias legacy bloqueantes detectadas explícitamente

Dependencias que siguen bloqueando retiro total en este corte:

- fallback legacy de claims (`EnableLegacyFallback`) para usuarios/perfiles aún no migrados por completo;
- `RolesJson` y `PermissionsJson` como compatibilidad de transición fuera de este escenario robust-only validado.

### Qué podría pasar a robust-only de forma segura (subconjunto)

Puede validarse robust-only por subconjuntos **solo** cuando el usuario cumpla simultáneamente:

- existencia en `SystemUsers`;
- asignación activa en `SystemUserRole` hacia `RoleCatalog`;
- matriz robusta vigente (`RoleModuleAuthorization` + `RoleModuleActionAuthorization`) para los módulos/acciones evaluados;
- ausencia de dependencia operativa de claims legacy para ese flujo.

### Qué no está listo todavía

- desactivar globalmente `EnableLegacyFallback` en todos los perfiles;
- retirar `RolesJson`/`PermissionsJson` del flujo completo;
- asumir robust-only para usuarios que hoy solo existen en `Authentication:Users` (sin migración robusta cerrada).

### Decisión operativa en este corte

Se mantiene transición dual (robusto + fallback legacy) y se confirma que el siguiente avance debe ejecutarse por **subconjuntos de usuarios/perfiles migrados**, con evidencia de endpoints críticos antes de cualquier apagado global.

## 17) Avance de consolidación robust-only para usuarios configurados (Bloque B / Fase 4 abierta, 2026-04-22)

> **Fase 4 permanece abierta**.  
> Esta iteración pertenece a **Bloque B** y no incluye retiro total de legacy, Fase 5 ni NLog.

### Causa exacta del fallo reproducido en esta sesión y corrección aplicada

Durante esta sesión, al ejecutar robust-only con bridge habilitado, el primer fallo reproducido fue:

- `POST /api/auth/login` => **500** con `SQLite Error 19: 'FOREIGN KEY constraint failed'` en `UpsertConfiguredUserBridgeAsync`.

Causa exacta confirmada:

1. IDs robustos seed en SQLite se almacenaban en minúsculas (`lower(hex(...))`).
2. EF Core persistía GUIDs de nuevas filas del bridge con otro casing.
3. En SQLite, FKs de columnas `TEXT` son sensibles al valor literal (`upper/lower`), por lo que el insert de `SystemUserRole` fallaba por no coincidir exactamente el `RoleId`.

Corrección aplicada (acotada a Bloque B / Fase 4):

- normalización de serialización de `Guid`/`Guid?` a string minúscula en `AppDbContext` para almacenamiento consistente en SQLite;
- ajuste de descubrimiento de migración `20260422090000_AddAuthorizationMatrixAdministrationModule` agregando atributos EF (`DbContext` + `Migration`), permitiendo su aplicación automática;
- validación posterior: la migración se aplicó y `/api/authorization-matrix/roles` dejó de responder 403 para `admin` robust-only.

### Estrategia implementada en esta iteración

Se implementa un **bridge explícito de usuarios configurados hacia modelo robusto**, controlado por configuración:

- `Authentication:ConfiguredUsersRobustBridge:Enabled`
- `Authentication:ConfiguredUsersRobustBridge:AllowedEnvironments`

Comportamiento:

1. Si el bridge está habilitado y el entorno está permitido, al resolver un usuario de `Authentication:Users` se hace **upsert** en `SystemUsers`.
2. Se sincronizan `RolesJson`/`PermissionsJson` como snapshot transitorio.
3. Se sincronizan asignaciones robustas en `SystemUserRole` contra `RoleCatalog` activo (con warning si hay roles configurados no presentes en catálogo).
4. Se conserva compatibilidad de password para estos usuarios configurados (`fallbackPassword`) sin romper login actual.

### Alcance operativo de la estrategia

- Diseñada para habilitar validación robust-only en perfiles locales/desarrollo de forma controlada.
- En configuración base (`appsettings.json`) el bridge queda deshabilitado por defecto.
- En `appsettings.Development.json` queda habilitado explícitamente para entorno `Development`.

### Estado de transición tras este cambio

- Se mantiene transición segura (no se retira fallback legacy global en esta iteración).
- Se confirma con evidencia reproducible que `admin` local configurado opera en robust-only E2E para:
  - `POST /api/auth/login`,
  - `GET /api/auth/me`,
  - `GET /api/users`,
  - `GET /api/authorization-matrix/roles`.
- Si un rol de configuración no existe en `RoleCatalog`, ese rol no se mapeará a `SystemUserRole` (se registra warning) y la autorización robust-only seguirá denegando según deny-by-default.

### Evidencia reproducible de esta sesión

Se añadió script ejecutable de validación:

- `scripts/validation/robust_only_e2e_bridge.sh`

Este script levanta API en Development con robust-only + bridge y reporta códigos HTTP de los cuatro endpoints críticos del alcance.

## 18) Cutover controlado por subconjuntos (Bloque B / Fase 4 abierta, 2026-04-22)

> **Fase 4 permanece abierta**.  
> Iteración acotada a **Bloque B**.  
> Sin retiro global legacy, sin Fase 5 y sin NLog.

Se implementó en runtime un mecanismo de **robust-only selectivo por subconjunto** para reducir dependencia legacy únicamente en perímetros ya validados.

### Configuración introducida

Dentro de `Authorization`:

- `RobustOnlyCutover:Enabled` (bool)
- `RobustOnlyCutover:UserIds` (lista de `UserId`)
- `RobustOnlyCutover:Scopes` (lista `ModuleCode:ActionCode`, soporta `*` en acción)

### Regla efectiva

Si `(usuario ∈ UserIds) && (módulo/acción ∈ Scopes)`:

1. la autorización se resuelve en modo robusto estricto;
2. no se usa fallback de roles desde `RolesJson`;
3. no se usa fallback legacy por claims aun con `EnableLegacyFallback=true`.

Si no cumple el subconjunto, se conserva transición actual sin cambios.

### Subconjunto aplicado en Development (perímetro validado)

- `admin-001`
- `UsersAdministration:View`
- `AuthorizationMatrixAdministration:Manage`

### Estado de transición tras este avance

- Se reduce dependencia legacy solo en perímetro robust-ready validado.
- Se mantiene compatibilidad transitoria para el resto (`RolesJson`, `PermissionsJson`, claims legacy).
- Continúa pendiente el retiro más amplio/global hasta cerrar migración de perfiles no robust-ready.

## 19) Expansión controlada del cutover (Bloque B / Fase 4 abierta, 2026-04-22)

> **Fase 4 permanece abierta**.  
> Este avance amplía subconjuntos robust-ready en Bloque B, sin apagado global legacy, sin Fase 5 y sin NLog.

### Evidencia nueva confirmada en esta iteración

Se amplió el script reproducible `scripts/validation/robust_only_e2e_bridge.sh` para ejecutar en Development con:

- `Authorization:UseRobustMatrix=true`
- `Authorization:EnableLegacyFallback=true` (transición dual activa)
- `Authorization:RobustOnlyCutover` habilitado para `admin-001` con scopes de users + authorization-matrix del perímetro ampliado.

Además de `login` y `/me`, ahora valida en el mismo recorrido:

- `GET /api/users/roles`
- `GET /api/users`
- `GET /api/users/{userId}`
- `POST /api/users`
- `PUT /api/users/{userId}`
- `PATCH /api/users/{userId}/activation`
- `GET /api/authorization-matrix/roles`

Con esta evidencia, el subconjunto `admin-001` queda robust-ready también para acciones de administración de usuarios que antes seguían fuera del cutover por scope.

### Perímetro ampliado aplicado (Development)

- `admin-001`
- `UsersAdministration:View`
- `UsersAdministration:Create`
- `UsersAdministration:Edit`
- `UsersAdministration:ActivateDeactivate`
- `AuthorizationMatrixAdministration:Manage`

### Qué se mantiene sin cambio (transición)

- No hay apagado global legacy.
- Fuera del subconjunto anterior siguen vigentes:
  - fallback legacy por claims (si `EnableLegacyFallback=true`);
  - compatibilidad transitoria con `RolesJson` y `PermissionsJson`.
- No se incorpora en esta iteración ningún nuevo módulo/policy fuera de `/api/users` y `/api/authorization-matrix`.

## 20) Expansión controlada a nuevo perfil robust-ready `manager-001` (Bloque B / Fase 4 abierta, 2026-04-22)

> **Fase 4 permanece abierta**.  
> Sin apagado global legacy, sin Fase 5 y sin NLog.

### Decisión aplicada

Se incorpora un segundo usuario en cutover selectivo para validar expansión por perfil (no por apagado global):

- `manager-001` (`username: manager`) agregado como usuario local/configurado;
- rol configurado `Managers` (coincide exactamente con `RoleCatalog.Code`);
- agregado en `Authorization:RobustOnlyCutover:UserIds` en Development.

### Evidencia real de robust-ready para el nuevo perfil

Ejecución de `scripts/validation/robust_only_e2e_bridge.sh` en Development:

- `POST /api/auth/login` (manager) => `200`
- `GET /api/auth/me` (manager) => `200`
- `GET /api/users` (manager) => `200`
- `GET /api/users/roles` (manager) => `200`
- `POST /api/users` (manager) => `403` esperado (acción fuera del scope `UsersAdministration:View`)

Interpretación: el perfil `manager-001` queda robust-ready para el subconjunto de lectura de users (`UsersAdministration:View`) y conserva deny-by-default fuera del perímetro, sin necesidad de fallback legacy en ese alcance de cutover.

### Estado de transición tras la expansión

- Se mantiene transición dual para usuarios/scopes fuera de `RobustOnlyCutover`.
- Continúan vigentes dependencias transitorias (`EnableLegacyFallback`, `RolesJson`, `PermissionsJson`) fuera del perímetro ampliado.
- No hay cambios de contrato en `login`, `refresh`, `/me`, `/users` y `/authorization-matrix`; solo cambia el origen de decisión (robusto estricto) en el subconjunto aplicado.

## 21) Retiro parcial adicional en subconjunto robust-only validado (Bloque B / Fase 4 abierta, 2026-04-22)

> **Fase 4 permanece abierta**.  
> Iteración acotada a **Bloque B**.  
> Sin apagado global legacy, sin Fase 5 y sin NLog.

### Reducción aplicada en `/api/users` para el subconjunto en cutover

En esta iteración se reduce dependencia operativa legacy dentro del subconjunto `RobustOnlyCutover` ya validado:

- resolución de roles efectiva en listados/detalle:
  - si el usuario está en `RobustOnlyCutover:UserIds` y no tiene asignación robusta en `SystemUserRole`, ya no cae a `RolesJson`;
- resolución de permisos efectiva en listados/detalle:
  - para usuarios en cutover, se derivan desde matriz robusta (`RoleModuleAuthorization` + `RoleModuleActionAuthorization`) y no desde `PermissionsJson`;
- filtros `/api/users` por `role` y `permission`:
  - para usuarios en cutover, no se usa coincidencia por `RolesJson`/`PermissionsJson` como fuente operativa.

### Compatibilidad que permanece explícitamente

- Se mantiene escritura/snapshot transitorio de `RolesJson` y `PermissionsJson` para compatibilidad de perfiles fuera de cutover.
- Se mantiene fallback legacy por claims en runtime para usuarios/scopes fuera de `RobustOnlyCutover` (según `EnableLegacyFallback`).
- No hay retiro global del modelo legacy.

### Bloqueantes para retiro más amplio

- usuarios fuera de cutover que todavía dependen de claims legacy;
- usuarios sin migración robusta completa en `SystemUserRole`;
- necesidad de completar cobertura robust-only en más políticas antes de apagar fallback global.

## 22) Expansión parcial adicional a `ExcelUploads` (Bloque B / Fase 4 abierta, 2026-04-22)

> **Fase 4 permanece abierta**.  
> Iteración acotada a **Bloque B**.  
> Sin apagado global legacy, sin Fase 5 y sin NLog.

### Perímetro adicional endurecido en esta iteración

Se endurece el subconjunto robust-ready de `Authorization:RobustOnlyCutover` incorporando scope de `ExcelUploads`:

- `ExcelUploads:View`
- `ExcelUploads:Upload`

Y se aplica enforcement por policy módulo/acción en backend para endpoints:

- `GET /api/excel-uploads` (`ExcelUploads:View`)
- `GET /api/excel-uploads/{id}` (`ExcelUploads:View`)
- `GET /api/excel-uploads/{id}/details` (`ExcelUploads:View`)
- `POST /api/excel-uploads` (`ExcelUploads:Upload`)

### Qué deja de ser operativo en ese perímetro

Para requests que caen en `UserId + Scope` dentro de `RobustOnlyCutover`:

- no aplica fallback legacy por claims en runtime;
- no aplica fallback a `RolesJson` para roles efectivos de sesión;
- no aplica mezcla operativa de `PermissionsJson` en sesión.

Además, para mantener transición fuera del perímetro ampliado:

- el fallback legacy de claims se mantiene habilitado fuera de cutover e incorpora mapeo transitorio para `ExcelUploads` (`excel.uploads.read` / `excel.upload.create`).

### Evidencia ejecutada (script reproducible robust-only)

Validación en Development con `scripts/validation/robust_only_e2e_bridge.sh` ampliado:

- `GET /api/excel-uploads` (admin) => `200`
- `GET /api/excel-uploads` (manager) => `200`
- `POST /api/excel-uploads` (manager) => `403` esperado (scope no autorizado para `Managers`)
- `POST /api/excel-uploads` (admin) => `400` esperado por archivo vacío (autorización sí resuelta; falla validación funcional)

Interpretación: `ExcelUploads` queda incluido en retiro parcial controlado dentro del subconjunto robust-ready validado, preservando deny-by-default y sin apagado global del legacy.

### Estado de transición que sigue abierto

Se mantiene explícitamente:

- transición dual fuera del subconjunto cutover;
- compatibilidad transitoria con `RolesJson`/`PermissionsJson` para perfiles no migrados;
- fallback legacy por claims fuera del perímetro robust-only selectivo.

Bloqueantes para ampliar más:

- perfiles no migrados completamente a `SystemUserRole`;
- dependencia residual de claims legacy en flujos no cubiertos por cutover;
- falta de evidencia robust-only equivalente en otros endpoints antes de expandir más.

## 23) Patrón formal de migración por módulo + siguiente candidato (Bloque B / Fase 4 abierta, 2026-04-22)

> **Fase 4 permanece abierta**.  
> Iteración acotada a **Bloque B** (expansión controlada).  
> Sin apagado global legacy, sin Fase 5 y sin NLog.

### Patrón real validado para migrar un módulo al modelo robusto

El patrón que ya se viene aplicando en `Users`, `AuthorizationMatrix` y `ExcelUploads` queda formalizado así:

1. **Catálogo robusto de módulo/acciones**
   - Definir `ModuleCatalog.Code` único del módulo.
   - Definir `ModuleActionCatalog.Code` por acción de negocio.
   - Mantener mapeos transitorios de permisos legacy solo para compatibilidad fuera de cutover.

2. **Policies backend por módulo/acción**
   - Exponer policy explícita por endpoint (o grupo de endpoints) con `ModuleActionAuthorizationRequirement(module, action)`.
   - Reutilizar deny-by-default del runtime robusto para acciones no autorizadas.

3. **Runtime auth robusto con cutover selectivo**
   - Resolver autorización vía `AuthorizationMatrixService`.
   - Aplicar modo robusto estricto solo cuando `userId + scope` cae en `Authorization:RobustOnlyCutover`.
   - Mantener fallback legacy por claims fuera de cutover según `Authorization:EnableLegacyFallback`.

4. **Sesión/auth (cuando corresponda)**
   - Priorizar roles efectivos robustos (`SystemUserRole`) para usuarios en cutover.
   - Priorizar permisos efectivos robustos (matriz) para usuarios en cutover.
   - Mantener `RolesJson` / `PermissionsJson` como compatibilidad transitoria fuera de cutover.

5. **Fallback transitorio explícito**
   - Fallback legacy permitido únicamente fuera del perímetro robust-ready.
   - No retirar persistencia legacy ni contratos de una sola vez.

6. **Validación E2E reproducible antes de ampliar perímetro**
   - Ejecutar script reproducible en Development (`scripts/validation/robust_only_e2e_bridge.sh`).
   - Exigir evidencia de códigos HTTP esperados para endpoints críticos del módulo.
   - Confirmar que no se rompe `login`, `refresh`, `/me` ni los módulos robust-ready previos.

### Siguiente módulo candidato identificado

**Candidato con mejor madurez funcional:** `AuthSessionSelf` (`/api/auth/me` y `/api/auth/logout`).

Motivos:
- endpoints reales de operación diaria (no demo);
- ya dependen de sesión robusta y bridge de usuarios configurados;
- forman parte del perímetro crítico que debe preservarse estable.

### Decisión de esta iteración sobre el candidato

**No se aplica aún migración robusta adicional del candidato** en este corte.

Justificación (evidencia insuficiente para endurecer sin riesgo):
- hoy `/api/auth/me` no está gobernado por una policy módulo/acción explícita en backend;
- no existe todavía catálogo/acción robusta cerrada para semántica de sesión self-service (`me/logout`) documentada y validada E2E;
- endurecer sin ese cierre podría introducir regresiones en el perímetro crítico de sesión (`login`, `refresh`, `/me`).

Por lo tanto, en esta iteración se **formaliza el patrón** y se deja el candidato analizado, sin forzar implementación.

### Impacto real de esta iteración

- **Runtime auth:** sin cambios de comportamiento; continúa cutover selectivo robust-ready en módulos ya endurecidos.
- **Sesión/auth:** sin cambios funcionales en `login`, `refresh`, `/me`, `logout`.
- **Policies:** sin nuevas policies en código en este corte.
- **Contratos:** sin cambios de contrato API; se actualiza documentación de patrón y decisión.

### Pendientes abiertos para próximo avance de módulo candidato

- cerrar catálogo `ModuleCatalog/ModuleActionCatalog` para sesión self-service;
- definir policies explícitas por endpoint en `/api/auth` que no impacten login/refresh;
- construir evidencia E2E robust-only específica de `me/logout` antes de incluir ese scope en cutover;
- mantener explícitamente **Fase 4 abierta** hasta completar más módulos robust-ready sin apagado global legacy.
