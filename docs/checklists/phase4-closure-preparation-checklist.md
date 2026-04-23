# Checklist de preparación para cierre técnico de Fase 4 (Bloque B)

> Estado de fase: **Fase 4 cerrada formalmente (2026-04-23 UTC)**.  
> Uso: registro histórico de criterios de preparación y su cumplimiento al cierre.

## 1) Cobertura funcional robusta (por módulo/perfil)

- [x] Inventario actualizado de módulos/acciones dentro del alcance de Fase 4.
- [x] Estado por item: `validado robust-only` / `en transición` / `pendiente`.
- [x] Evidencia por perfil crítico (`admin-001`, `manager-001`, `operator-001`) cuando aplica.

## 2) Dependencias legacy remanentes (fuera de cutover)

- [x] Lista explícita de puntos donde seguía vivo `RolesJson` durante preparación.
- [x] Lista explícita de puntos donde seguía vivo `PermissionsJson` durante preparación.
- [x] Lista explícita de puntos con fallback legacy por claims durante preparación.
- [x] Riesgo documentado por cada dependencia remanente detectada en preparación.

## 3) No regresión obligatoria

- [x] `POST /api/auth/login`
- [x] `POST /api/auth/refresh`
- [x] `GET /api/auth/me`
- [x] `/api/users`
- [x] `/api/authorization-matrix`
- [x] `/api/excel-uploads`

## 4) Evidencia E2E reproducible

- [x] Ejecución reproducible documentada de scripts vigentes de validación.
- [x] Registro de códigos esperados (`2xx`, `403`, `400 funcional`) por endpoint/perfil.
- [x] Registro explícito de denegaciones esperadas (deny-by-default).

## 5) Criterios de cierre (go/no-go)

- [x] Dependencia legacy no operativa en el alcance objetivo de cierre de Fase 4.
- [x] Contratos HTTP sin ruptura.
- [x] Documentación de estado consolidada y actualizada.
- [x] Decisión formal registrada: **cierre formal aprobado**.

## 6) Restricciones de esta checklist

- No mezclar actividades de Fase 5 ni NLog en el cierre de Fase 4.
- No reinterpretar evidencia fuera del estado validado.
- No extender alcance técnico sin validación explícita.

## 7) Referencias de consolidación de cierre

- `docs/reports/phase4-final-closure-evaluation-2026-04-23.md`
- `docs/reports/phase4-block-b-formal-closure-2026-04-23.md`
- `scripts/validation/robust_only_e2e_phase4_block_b_closure_eval.sh`
- `scripts/validation/robust_only_e2e_operator.sh`
