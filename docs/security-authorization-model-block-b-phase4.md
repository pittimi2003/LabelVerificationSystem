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
