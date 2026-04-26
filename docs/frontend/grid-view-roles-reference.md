# Referencia concreta: RolesAdmin.razor (`RoleCatalog` / `Role`)

> Nota histórica: este archivo contiene entradas de bitácora previas al cierre formal de Fase 4 (2026-04-23).
> La verdad vigente del proyecto es **Fase 4 cerrada al 100%** y **Fase 5 pendiente**.


## Estado y alcance

- Vista administrativa tipo Grid para `RoleCatalog` sobre contrato HTTP real.
- Alcance limitado a Bloque B / Fase 4 abierta.
- Sin cambios de arquitectura backend, sin Fase 5, sin NLog.

---

## 1) Estándar reusable aplicado

Elementos del estándar `docs/frontend/grid-view-standard.md` reutilizados sin variación estructural:

- toolbar con título, subtítulo y acción de recarga,
- bloque de filtros separado,
- grid con estado de carga y estado vacío,
- paginación backend-driven (`page`, `pageSize`, `totalItems`, `totalPages`),
- drawer lateral para detalle,
- feedback de errores/éxitos por snackbar,
- patrón UX obligatorio: `SearchField`, `SearchText`, `StatusFilter`, `Limpiar filtros`.

---

## 2) Decisiones específicas del modelo `RoleCatalog`

### 2.1 Columnas propuestas (y usadas)

1. `RoleCode`
2. `RoleName`
3. `Estado` (`IsActive`)
4. `CreatedAtUtc`
5. `UpdatedAtUtc`
6. `Acciones`

### 2.2 Filtros propuestos (y usados)

- `SearchField`:
  - `Query` (global en backend)
  - `RoleCode`
  - `RoleName`
- `SearchText`
- `StatusFilter` (`Todos`, `Activos`, `Inactivos`) -> mapea a `isActive` nullable
- `Limpiar filtros`

### 2.3 Estados de vista

- carga inicial,
- carga por filtros/paginación,
- estado vacío,
- error de carga,
- operación en curso (toggle estado),
- éxito/error de operación.

### 2.4 Acciones soportadas y no soportadas

**Soportadas**

- crear rol (`POST /api/roles`) con drawer dedicado,
- editar rol (`PUT /api/roles/{roleCode}`) con `RoleCode` bloqueado (read-only),
- ver detalle (`GET /api/roles/{roleCode}`),
- activar/desactivar (`PATCH /api/roles/{roleCode}/activation`).

**No soportadas (declaradas explícitamente)**

- edición de `RoleCode` en update,
- eliminación de rol.

---

## 3) Diseño de página

### Toolbar
- título/subtítulo,
- botón `Actualizar`.

### Filtros
- `SearchField`,
- `SearchText`,
- `StatusFilter`,
- `Limpiar filtros`.

### Grid
- tabla principal con columnas del contrato backend,
- chips para estado.

### Paginación
- `Página X de Y`,
- `Total backend`,
- selector `Filas por página`,
- control de páginas.

### Drawers
- drawer de create,
- drawer de edit,
- drawer de detalle read-only.

---

## 4) Mapeo exacto contra contratos backend

| Elemento | Contrato real |
|---|---|
| Listado | `GET /api/roles` |
| Query params soportados | `query`, `code`, `name`, `isActive`, `page`, `pageSize` |
| Respuesta listado | `{ items, page, pageSize, totalItems, totalPages }` |
| Create | `POST /api/roles` body `{ roleCode, name, isActive }` |
| Update | `PUT /api/roles/{roleCode}` body `{ name, isActive }` |
| Detalle | `GET /api/roles/{roleCode}` |
| Toggle estado | `PATCH /api/roles/{roleCode}/activation` body `{ isActive }` |
| DTO item grid | `RoleCatalogListItemDto` |
| DTO detalle | `RoleCatalogDetailDto` |
| Policy | `RolesCatalog` por acción (`View`, `Create`, `Edit`, `ActivateDeactivate`) |

---

## 5) Limitaciones abiertas (sin suposiciones)

1. No existe endpoint para eliminación de rol.
2. `Description` no forma parte del modelo persistente actual de `RoleCatalog`; no se expone en UI ni API.
3. `RoleCode` es inmutable después de crear el rol.

---

## 6) Checklist de cumplimiento del estándar

- [x] Toolbar + filtros + grid + paginación + drawer + feedback.
- [x] `SearchField` + `SearchText` + `StatusFilter` + `Limpiar filtros`.
- [x] `StatusFilter` obligatorio aplicado (`RoleCatalog.IsActive`).
- [x] Paginación backend-driven.
- [x] Sin inventar campos/filtros/catálogos/acciones fuera del backend.
- [x] Contrato visual coherente con shell actual.
- [x] Autorización robusta por policy existente.


### Reutilización de estándar
- El estándar de grid aplicado en `RolesAdmin` se reutiliza también en `LabelTypesAdmin` (toolbar + filtros + grid + drawer + feedback).
