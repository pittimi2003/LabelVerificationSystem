# Modelo de autorización robusto (Bloque B / Fase 4 abierta)

> Estado de fase: **Fase 4 continúa abierta**.  
> Alcance: robustecimiento de roles/permisos en **Bloque B**.  
> Exclusiones explícitas: este documento no cierra Fase 4, no mezcla alcance con Fase 5 y no incorpora NLog.

## Referencias usadas en esta definición

Esta propuesta se consolida usando únicamente referencias confirmadas del repositorio:

- `docs/Permissions.xml` como referencia **estructural histórica** del árbol de permisos.
- `docs/Managment.html` como referencia **conceptual UX** para administrar permisos.
- `docs/frontend/grid-view-standard.md` como marco UX/documental reusable para vistas administrativas.
- `docs/frontend/grid-view-users-reference.md` como referencia de límites actuales del patrón de usuarios.

## 1) Propuesta de modelo conceptual completo

El sistema evoluciona desde un esquema de listas serializadas (`roles`/`permissions`) hacia un modelo explícito de autorización por catálogo:

- **Catálogo de roles** (cerrado en esta iteración):
  - `SuperAdmin`
  - `Operators`
  - `Managers`
- **Catálogo de módulos** del sistema.
- **Catálogo de acciones** por módulo.
- **Autorización por módulo** (`Module Authorized`): habilita o niega acceso al módulo.
- **Autorización por acción** (`Action Authorized`): habilita o niega la capacidad operativa concreta.

Semántica funcional base confirmada:

- `Module Authorized = true`: el rol puede entrar/usar el módulo.
- `Module Authorized = false`: el rol no puede usar el módulo.
- `Action Authorized = true`: el rol puede ejecutar la acción.
- `Action Authorized = false`: el rol no puede ejecutar la acción.

## 2) Entidades y relaciones necesarias

### Catálogos

1. **RoleCatalog**
   - Identificador técnico.
   - `Code` único (`SuperAdmin`, `Operators`, `Managers`).
   - Nombre de presentación.
   - Estado (`IsActive`) para evolución controlada.

2. **ModuleCatalog**
   - Identificador técnico.
   - `Code` único (clave estable para backend/frontend).
   - Nombre de presentación.
   - Orden para UI.
   - Estado (`IsActive`).

3. **ModuleActionCatalog**
   - Identificador técnico.
   - `ModuleId` (FK a `ModuleCatalog`).
   - `Code` único por módulo.
   - Nombre de presentación.
   - Orden para UI.
   - Estado (`IsActive`).

### Matriz de autorización

4. **RoleModuleAuthorization**
   - `RoleId` (FK).
   - `ModuleId` (FK).
   - `Authorized` (`bool`).
   - Único por (`RoleId`, `ModuleId`).

5. **RoleModuleActionAuthorization**
   - `RoleId` (FK).
   - `ModuleActionId` (FK).
   - `Authorized` (`bool`).
   - Único por (`RoleId`, `ModuleActionId`).

### Relación con usuarios

6. **SystemUserRole**
   - `SystemUserId` (FK).
   - `RoleId` (FK).
   - Único por (`SystemUserId`, `RoleId`).

> Nota de diseño: para simplificar gobernanza inicial, se recomienda **1 rol principal por usuario** en operación diaria; la estructura N:M se conserva para crecimiento controlado.

## 3) Catálogo base propuesto

### Roles (cerrado)
- `SuperAdmin`
- `Operators`
- `Managers`

### Módulos base (propuesta inicial a confirmar contra contratos activos)
- `UsersAdministration`
- `ExcelUploads`
- `PartsCatalog`
- `LabelVerification`
- `PackingLists`
- `AuditTrail`
- `SystemConfiguration`

### Acciones base por módulo (propuesta inicial)

- `UsersAdministration`: `View`, `Create`, `Edit`, `ActivateDeactivate`, `ResetPassword`.
- `ExcelUploads`: `View`, `Upload`.
- `PartsCatalog`: `View`, `Create`, `Edit`.
- `LabelVerification`: `View`, `Execute`.
- `PackingLists`: `View`, `Create`, `Edit`, `Close`.
- `AuditTrail`: `View`.
- `SystemConfiguration`: `View`, `Edit`.

> Estos módulos/acciones son punto de partida operativo y no deben considerarse catálogo definitivo hasta su cierre contractual por módulo.

## 4) Qué debe persistirse y qué no

### Persistir (fuente de verdad)
- Catálogo de roles.
- Catálogo de módulos.
- Catálogo de acciones por módulo.
- Autorización por rol-módulo (`Authorized`).
- Autorización por rol-acción (`Authorized`).
- Asignación usuario-rol.

### No persistir como fuente canónica
- Listas libres de permisos en texto/JSON sin normalizar.
- Inferencias dinámicas de catálogos a partir de datos de grilla.
- Reglas de autorización embebidas únicamente en frontend.

## 5) Cómo se relaciona con usuarios

- `SystemUser` deja de depender de `RolesJson`/`PermissionsJson` como modelo final.
- Cada usuario se vincula a uno o más roles del catálogo explícito.
- La autorización efectiva del usuario se calcula por la matriz:
  - Roles del usuario
  - `RoleModuleAuthorization`
  - `RoleModuleActionAuthorization`

Regla de resolución recomendada en backend:
- **Deny por defecto**.
- Si un usuario no tiene asignación explícita para módulo/acción, la respuesta es no autorizado.

## 6) Traducción a autorización backend

1. **Claims de sesión**
   - Incluir `role` desde catálogo.
   - Incluir claims técnicos para módulo/acción cuando aplique (o resolverlos por consulta en caché).

2. **Policies por acción backend**
   - Reemplazar políticas genéricas por políticas que validen:
     - acceso al módulo,
     - permiso de acción.

3. **Punto de decisión unificado**
   - Definir un servicio central (ej. `IAuthorizationMatrixService`) para evitar lógica duplicada en controladores/endpoints.

4. **Errores esperados**
   - `401`: sin identidad autenticada.
   - `403`: identidad válida pero sin autorización de módulo/acción.

## 7) XML (`docs/Permissions.xml`): qué sí sirve y qué no copiar tal cual

### Útil como referencia estructural
- Jerarquía `UserGroup -> Modules -> Module -> Actions -> Action`.
- `Authorized` a nivel módulo.
- `Authorized` a nivel acción.
- Distinción explícita entre acceso de módulo y ejecución por acción.

### No copiar tal cual al modelo final
- `UserGroup` textual libre como fuente principal (se sustituye por catálogo de roles explícito).
- Atributo `Permissions` como eje del diseño final (queda como referencia histórica solamente).
- Nombres legacy de módulos/acciones que no correspondan al dominio actual de LabelVerificationSystem.

## 8) HTML (`docs/Managment.html`): qué sí sirve y qué no copiar tal cual

### Útil como referencia UX
- Selector de grupo/rol para administrar permisos.
- Vista tabular por módulos.
- Toggle de `Authorized` en módulo.
- Acciones hijas dentro del módulo con toggles por acción.
- Flujo de edición operativa centralizado (`leer -> editar -> guardar`).

### No copiar tal cual
- Dependencia de archivo XML como contrato operativo final.
- Restricciones hardcodeadas del ejemplo heredado.
- Reglas de negocio resueltas solo en JavaScript sin backend autoritativo.

## 9) Decisiones abiertas pendientes

1. Confirmar catálogo definitivo de módulos/acciones contra endpoints activos por módulo.
2. Definir plan de migración de `RolesJson`/`PermissionsJson` a tablas normalizadas.
3. Decidir si la UX inicial permitirá multi-rol o solo rol único operativo.
4. Definir estrategia de caché e invalidación de matriz de autorización en backend.
5. Definir trazabilidad/auditoría de cambios de permisos (quién cambió qué, cuándo y por qué).
6. Confirmar contrato API de administración de permisos (consultar/editar) por rol, módulo y acción.

## 10) Impacto en backend, frontend y documentación

### Backend
- Nuevas entidades/tablas para catálogos y matriz de autorización.
- Refactor de policies para validación explícita módulo+acción.
- Ajustes de emisión de claims y/o consulta de matriz autorizada.
- Migración progresiva desde estado serializado actual.

### Frontend
- Pantalla de administración de permisos alineada al patrón Grid/documental vigente.
- Gestión por rol explícito + módulos + acciones (sin dependencia de listas ad-hoc).
- Mensajería de autorización consistente con backend (`401`/`403`).
- Mantener criterios de UX reusable documentados en `grid-view-standard.md` y límites de reutilización ya identificados en `grid-view-users-reference.md`.

### Documentación
- Mantener este documento como definición viva de Bloque B / Fase 4 abierta.
- Actualizar contratos API y modelo de datos cuando se cierre la implementación de catálogos/matriz.
- Mantener trazabilidad explícita de decisiones abiertas para evitar cierres implícitos no confirmados.
