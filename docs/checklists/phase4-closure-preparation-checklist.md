# Checklist de preparación para cierre técnico de Fase 4 (Bloque B)

> Estado de fase: **Fase 4 abierta**.  
> Uso: preparar decisión futura de cierre sin cerrar fase en esta iteración.

## 1) Cobertura funcional robusta (por módulo/perfil)

- [ ] Inventario actualizado de módulos/acciones dentro del alcance de Fase 4.
- [ ] Estado por item: `validado robust-only` / `en transición` / `pendiente`.
- [ ] Evidencia por perfil crítico (`admin-001`, `manager-001`, `operator-001`) cuando aplique.

## 2) Dependencias legacy remanentes (fuera de cutover)

- [ ] Lista explícita de puntos donde sigue vivo `RolesJson`.
- [ ] Lista explícita de puntos donde sigue vivo `PermissionsJson`.
- [ ] Lista explícita de puntos con fallback legacy por claims.
- [ ] Riesgo documentado por cada dependencia remanente.

## 3) No regresión obligatoria

- [ ] `POST /api/auth/login`
- [ ] `POST /api/auth/refresh`
- [ ] `GET /api/auth/me`
- [ ] `/api/users`
- [ ] `/api/authorization-matrix`
- [ ] `/api/excel-uploads`

## 4) Evidencia E2E reproducible

- [ ] Ejecución reproducible documentada de scripts vigentes de validación.
- [ ] Registro de códigos esperados (`2xx`, `403`, `400 funcional`) por endpoint/perfil.
- [ ] Registro explícito de denegaciones esperadas (deny-by-default).

## 5) Criterios de cierre (go/no-go)

- [ ] Dependencia legacy no operativa en el alcance objetivo para cierre de Fase 4.
- [ ] Contratos HTTP sin ruptura.
- [ ] Documentación de estado consolidada y actualizada.
- [ ] Decisión formal: **cerrable** / **no cerrable aún**.

## 6) Restricciones de esta checklist

- No cerrar Fase 4 automáticamente por completar checklist.
- No ejecutar apagado global legacy sin iteración explícita y validada.
- No mezclar actividades de Fase 5 ni NLog.

## 7) Actualización de iteración — Bloque B expansión final y evaluación de cierre (2026-04-23)

### 7.1 Expansión aplicada en esta iteración

- **No se aplicó expansión adicional de cutover**.
- Motivo: en `appsettings.Development.json` ya está activo el subconjunto `admin-001`, `manager-001`, `operator-001` con scopes:
  - `UsersAdministration:View`
  - `UsersAdministration:Create`
  - `UsersAdministration:Edit`
  - `UsersAdministration:ActivateDeactivate`
  - `AuthorizationMatrixAdministration:Manage`
  - `ExcelUploads:View`
  - `ExcelUploads:Upload`
- No existe evidencia de perfiles adicionales robust-ready en configuración base (`Authentication:Users`) para extender sin riesgo de inventar comportamiento.

### 7.2 Perímetro robust-only actualizado (estado real)

- **Usuarios en cutover robust-only:** `admin-001`, `manager-001`, `operator-001`.
- **Scopes en cutover robust-only:** 7/7 del perímetro administrativo validado en esta iteración.
- **Endpoints cubiertos en validación E2E de cierre (scripts reproducibles):**
  - `POST /api/auth/login`
  - `POST /api/auth/refresh`
  - `GET /api/auth/me`
  - `GET /api/users`
  - `GET /api/authorization-matrix/roles`
  - `GET /api/excel-uploads`
  - `POST /api/excel-uploads` (400 funcional por archivo vacío cuando está autorizado)

### 7.3 Evidencia E2E consolidada

Script principal de evaluación de cierre:

- `scripts/validation/robust_only_e2e_phase4_block_b_closure_eval.sh`

Resultados esperados y verificados:

- `admin-001`
  - permit: `/me`, `/users`, `/authorization-matrix/roles`, `/excel-uploads`
  - permit funcional: `POST /excel-uploads => 400` por archivo vacío (autorización aprobada)
- `manager-001`
  - permit: `/me`, `/users`, `/excel-uploads`
  - deny: `/authorization-matrix/roles => 403`
  - deny: `POST /excel-uploads => 403`
- `operator-001`
  - permit: `/me`, `/excel-uploads`
  - permit funcional: `POST /excel-uploads => 400` por archivo vacío (autorización aprobada)
  - deny: `/users => 403`, `/authorization-matrix/roles => 403`

### 7.4 Verificación explícita de no uso legacy en el perímetro robust-only

Se ejecutó prueba de contaminación legacy controlada sobre `operator-001`:

1. Se forzó en DB: `RolesJson=["SuperAdmin"]` y `PermissionsJson=["users.manage","authorization.matrix.manage","excel.upload.create"]`.
2. Se renovó sesión y se revalidó:
   - `/api/auth/me` mantiene `roles=['Operators']`.
   - `permissions` no incorpora `users.manage` ni `authorization.matrix.manage`.
   - `/api/users` sigue `403`.
   - `/api/authorization-matrix/roles` sigue `403`.

Conclusión de evidencia: en el subconjunto robust-only validado, no hay dependencia operativa de `RolesJson`, `PermissionsJson` ni fallback por claims para conceder permisos.

### 7.5 Mapa actualizado de dependencias legacy remanentes

Dependencias que **siguen vivas** fuera del perímetro robust-only:

- `AuthorizationMatrixService`:
  - fallback a claims legacy cuando `EnableLegacyFallback=true` y request fuera de cutover.
  - fallback de roles desde `RolesJson` cuando faltan roles robustos y el request admite fallback.
- `AuthService` y `UserAdministrationService`:
  - convivencia transitoria con snapshots `RolesJson`/`PermissionsJson` para usuarios no cubiertos por cutover.

Riesgo remanente: usuarios/flows fuera de `RobustOnlyCutover` pueden seguir autorizándose por legacy en esta fase.

### 7.6 Evaluación global de cierre técnico de Fase 4

- **Estado:** `NO CERRABLE AÚN`.
- Justificación:
  - El perímetro administrativo objetivo para los 3 perfiles críticos está robust-only validado con evidencia E2E.
  - Pero el sistema global aún conserva fallback legacy activo fuera de cutover (`EnableLegacyFallback=true` + usuarios no incluidos en subset).
  - No hay evidencia completa de cobertura robust-only para el universo total de usuarios/escenarios de transición.

### 7.7 Qué falta exactamente para considerar cierre técnico

1. Inventario completo y verificable de usuarios/perfiles reales fuera de cutover.
2. Expandir cutover por lotes adicionales (siempre con E2E por lote) hasta cubrir perímetro objetivo de Fase 4.
3. Demostrar que las rutas en alcance de cierre no dependen operativamente de `RolesJson`/`PermissionsJson`/claims fallback.
4. Consolidar evidencia final para decidir apagado legacy en iteración separada (sin ejecutarlo en esta).
