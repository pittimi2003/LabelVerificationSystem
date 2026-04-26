# Modelo de autorización robusto (Bloque B / Fase 4 cerrada)

> Estado de fase: **Fase 4 cerrada al 100%** (cierre formal: 2026-04-23 UTC, revalidación técnica: 2026-04-26 UTC).  
> Alcance: autorización robusta en runtime para autenticación/sesión, `/users`, `/roles`, `/parts`, `/authorization-matrix` y `/excel-uploads`.  
> Exclusiones explícitas: sin trabajo de Fase 5, sin NLog, sin cambios de arquitectura fuera del cierre.

## 1) Decisión final de modelo

1. La autorización operativa se resuelve por **matriz robusta persistida** (`RoleCatalog`, `ModuleCatalog`, `ModuleActionCatalog`, `RoleModuleAuthorization`, `RoleModuleActionAuthorization`, `SystemUserRole`).
2. Se mantiene la regla **deny-by-default**.
3. `Authentication:Users` permanece como bootstrap de identidad (usuarios iniciales), no como fuente paralela de autorización.
4. `RolesJson` y `PermissionsJson` quedaron retirados de persistencia y de uso operativo (migración `20260423110000_RemoveLegacyAuthorizationJson`).

## 2) Estado runtime validado

- `POST /api/auth/login` ✅
- `POST /api/auth/refresh` ✅
- `GET /api/auth/me` ✅
- `GET /api/users` ✅ (admin/manager) / 403 esperado (operator)
- `GET /api/roles` ✅ (admin) / 403 esperado (manager/operator)
- `GET /api/parts` ✅ (admin/manager) / 403 esperado (operator)
- `GET /api/authorization-matrix/roles` ✅ (admin) / 403 esperado (manager/operator)
- `GET /api/excel-uploads` ✅ (admin/manager/operator)
- `POST /api/excel-uploads` autorizado por perfil (400 funcional por archivo vacío en pruebas de smoke; 403 esperado donde aplica)

## 3) Evidencia reproducible usada en este cierre

- `bash scripts/validation/robust_only_e2e_phase4_block_b_closure_eval.sh`
- `bash scripts/validation/robust_only_e2e_operator.sh`
- Validación adicional de grids en runtime (`/users`, `/roles`, `/parts`, `/authorization-matrix`) por perfil.
- Validación de migración desde base limpia (creación y aplicación de 7 migraciones EF Core).

## 4) Confirmación explícita sobre legacy

- `RolesJson` y `PermissionsJson` **no son fuente operativa de autorización**.
- En el estado actual incluso fueron removidos del esquema `SystemUsers`; cualquier intento de depender de ellos falla por diseño.
- No existe fallback legacy operativo en el perímetro de cierre de Fase 4 validado.

## 5) Restricciones y continuidad

- Fase 5 permanece pendiente y fuera de este documento.
- No se añadió NLog.
- Los contratos API de Fase 4 se mantienen y su documentación fue alineada a estado de cierre.

## 6) Veredicto

**Fase 4 (Bloque B) cerrada al 100%.**

---

## 7) Estado histórico (referencia previa, no fuente operativa actual)

> Esta sección restaura contexto arquitectónico/técnico de la versión extensa previa del documento.  
> Clasificación: **Estado histórico** + **Referencia previa** + **No fuente operativa actual**.

### 7.1 Diseño técnico histórico que se mantiene como referencia

El diseño previo definía explícitamente (y se conserva como referencia histórica válida):

- Catálogos: `RoleCatalog`, `ModuleCatalog`, `ModuleActionCatalog`.
- Matriz de autorización: `RoleModuleAuthorization`, `RoleModuleActionAuthorization`.
- Relación usuario-rol: `SystemUserRole`.
- Reglas base: autorización por `módulo + acción`, precondición de módulo para habilitar acciones y `deny-by-default`.

Este diseño sigue siendo coherente con el runtime cerrado de Fase 4; lo que cambió fue el **estado de transición** (ya cerrada), no los fundamentos del modelo robusto.

### 7.2 Transición histórica `RolesJson` / `PermissionsJson` (ya cerrada)

En la versión extensa previa se documentaba el plan de transición en fases:

1. Expansión de esquema.
2. Seed de catálogos/matriz.
3. Backfill de usuarios.
4. Convivencia temporal.
5. Retiro progresivo de fallback legacy.

Ese contenido era útil como bitácora de migración, pero dejó de ser operativo al aplicar el retiro final de legacy en Fase 4.  
Por eso ahora se conserva como historia técnica y no como guía vigente de ejecución.

### 7.3 Línea temporal histórica restaurada (resumen)

- 2026-04-22: iteraciones de cutover robust-only por subconjuntos (`admin-001`, `manager-001`, `operator-001`) y expansión controlada por módulos/scopes.
- 2026-04-23: estabilización final, evaluación de cierre y cierre formal de Bloque B.
- 2026-04-26: revalidación técnica completa (build, migraciones desde base limpia, E2E robust-only, validación de grids), confirmando que Fase 4 permanece cerrada.

### 7.4 Alcance de uso de esta sección histórica

- Sirve para trazabilidad y auditoría técnica de decisiones.
- No reemplaza los contratos/documentos vigentes de operación.
- No habilita reintroducir fallback legacy ni `RolesJson`/`PermissionsJson` como fuente activa.
