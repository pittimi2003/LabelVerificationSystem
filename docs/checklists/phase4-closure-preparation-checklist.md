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
