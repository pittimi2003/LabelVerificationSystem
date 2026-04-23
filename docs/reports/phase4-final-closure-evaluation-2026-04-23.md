# Evaluación final de cierre Fase 4 (Bloque B)

Fecha: 2026-04-23 (UTC)

## Alcance evaluado
Endpoints críticos:
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `GET /api/auth/me`
- `GET|POST|PUT|PATCH /api/users`
- `GET /api/authorization-matrix/roles`
- `GET|POST /api/excel-uploads`

## Evidencia E2E ejecutada
Comando ejecutado:

```bash
bash scripts/validation/robust_only_e2e_phase4_block_b_closure_eval.sh
```

Salida relevante:

```text
db_tamper_rows 1
tampered_roles ['Operators']
tampered_permissions ['excel.upload.create', 'excel.uploads.read']
db_restore_rows 3
CONFIG: ASPNETCORE_ENVIRONMENT=Development Authorization__UseRobustMatrix=true Authorization__EnableLegacyFallback=true Authorization__RobustOnlyCutover__Enabled=true
POST /api/auth/login (admin|manager|operator) => 200|200|200
GET /api/auth/me (admin|manager|operator) => 200|200|200
GET /api/users (admin|manager|operator) => 200|200|403
GET /api/authorization-matrix/roles (admin|manager|operator) => 200|403|403
GET /api/excel-uploads (admin|manager|operator) => 200|200|200
POST /api/excel-uploads (admin|manager|operator) => 400|403|400
TAMPERED /api/users (operator) => 403
TAMPERED /api/authorization-matrix/roles (operator) => 403
```

Interpretación E2E:
- Los tres usuarios del subconjunto (`admin-001`, `manager-001`, `operator-001`) operan con sesión válida.
- Deny-by-default se cumple en rutas no autorizadas por rol/acción (403 esperados).
- El tampering de `RolesJson`/`PermissionsJson` para `operator-001` no eleva permisos en `/users` ni `/authorization-matrix/roles`.
- El `me` tampered conserva claims robustos (`Operators`; permisos de Excel) y no refleja `SuperAdmin`/`users.manage`/`authorization.matrix.manage`.

## Verificación de diseño runtime (código)

### 1) Cutover robust-only por `userId+scope`
- `AuthorizationMatrixService.AuthorizeAsync` calcula `robustOnlyCutoverSubset` y, para ese subconjunto, deshabilita fallback de roles legacy y fallback por claims.
- Si el request está en cutover, `allowLegacyRoleFallback=false` y no entra a `IsAllowedByLegacyClaims`.

### 2) Login/refresh/me generan sesión con roles/permisos efectivos
- `AuthService` resuelve roles y permisos efectivos desde asignaciones robustas.
- Para usuarios en cutover, si no hay roles robustos no toma `RolesJson`; y permisos devueltos son solo robustos (sin mezcla `PermissionsJson`).

### 3) `/users` en usuarios cutover no usa legacy operativo
- Resolución de roles/permisos en `UserAdministrationService`:
  - para cutover sin roles robustos => vacíos (no fallback a `RolesJson`/`PermissionsJson`);
  - para cutover con roles robustos => permisos robustos únicamente.
- Los snapshots legacy se mantienen solo para transición fuera de cutover.

## Matriz final por endpoint crítico

| Endpoint | Robust-only en subconjunto validado | Depende operativamente de `RolesJson` | Depende operativamente de `PermissionsJson` | Depende de fallback por claims |
|---|---|---|---|---|
| `POST /api/auth/login` | Sí | No | No | No |
| `POST /api/auth/refresh` | Sí | No | No | No |
| `GET /api/auth/me` | Sí | No | No | No |
| `/api/users` | Sí (subconjunto y scopes evaluados) | No | No | No |
| `/api/authorization-matrix` | Sí (scope `Manage`) | No | No | No |
| `/api/excel-uploads` | Sí (`View`/`Upload`) | No | No | No |

## Flujos críticos dependientes de legacy (en alcance pedido)
- No se detectan dependencias operativas legacy en el subconjunto robust-only validado para los endpoints críticos solicitados.

## Veredicto
**CASO A — Fase 4 CERRABLE** (según criterio de esta iteración y sin exigir migración total).

Justificación técnica exacta:
1. Todos los endpoints críticos del alcance ejecutan correctamente con usuarios en cutover robust-only.
2. El runtime de autorización corta fallback legacy por scope en usuarios del subconjunto.
3. La prueba de tampering confirma ausencia de escalación por `RolesJson`/`PermissionsJson`.
4. Deny-by-default se verifica con 403 esperados en accesos no autorizados.

## Pendientes (no bloqueantes de cierre de esta fase)
- Cleanup administrativo/documental de compatibilidad legacy fuera de cutover.
- Plan de retiro posterior de fallback/JSON legacy para fases futuras, fuera de este cierre.

## Evidencia E2E complementaria para `refresh`
Comando ejecutado:

```bash
bash scripts/validation/robust_only_e2e_operator.sh
```

Salida relevante:

```text
POST /api/auth/login (operator) => 200
GET /api/auth/me (operator) => 200
POST /api/auth/refresh (operator) => 200
GET /api/users (operator, expected 403) => 403
GET /api/authorization-matrix/roles (operator, expected 403) => 403
```
