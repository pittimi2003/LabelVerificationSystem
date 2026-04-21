# Estándar reusable de vista administrativa tipo Grid

## Estado y alcance de este estándar

- Este documento define un **estándar reusable** para pedir y evaluar nuevas vistas administrativas tipo Grid en LabelVerificationSystem.
- Este documento **no introduce funcionalidad nueva** por sí mismo; solo formaliza patrón documental y reusable.
- Este documento se alinea al estado actual validado del proyecto.
- Contexto actual del proyecto: **Fase 4 sigue abierta** (informativo, no regla universal del estándar).
- Este estándar no mezcla alcance con Fase 5, NLog ni cambios backend no confirmados.

---

## 1) Objetivo de la vista

Una vista administrativa tipo Grid debe permitir:

- consultar información paginada desde backend,
- filtrar resultados de forma explícita y backend-driven,
- ejecutar acciones administrativas por fila,
- abrir formularios laterales (drawers) para operaciones create/edit/detail/reset cuando aplique,
- mantener consistencia visual con la shell existente.

La vista debe operar sobre contratos reales del backend, evitando suposiciones.

---

## 2) Composición mínima de la vista (obligatoria)

Toda vista Grid debe incluir, como bloques explícitos y distinguibles:

1. **Encabezado / toolbar**
   - título y subtítulo,
   - acción de recarga,
   - acción primaria de alta cuando backend soporte creación.
2. **Filtros**
   - bloque dedicado, no mezclado con toolbar,
   - controles alineados al contrato backend real.
3. **Resultados / grid**
   - tabla principal con columnas reales,
   - estado de carga y estado vacío visibles.
4. **Paginación**
   - metadatos (`Página X de Y`, `Total`),
   - cambio de página y tamaño.
5. **Drawers**
   - create/edit/detail/reset (solo si existe soporte real),
   - con cierre, validación y acciones claras.
6. **Feedback / errores**
   - mensajes para carga y operaciones,
   - sin fallos silenciosos.

No se deben introducir estructuras visuales paralelas que rompan este patrón.

---

## 3) Patrón de filtros obligatorio

El patrón UX validado y obligatorio para nuevas vistas Grid es:

- `SearchField`
- `SearchText`
- `StatusFilter` (ver regla de aplicabilidad)
- `Limpiar filtros`

Reglas de filtro:

- `SearchField` define **qué campo backend** se está filtrando.
- `SearchText` envía valor normalizado (trim, nulo si vacío).
- `StatusFilter` es **obligatorio** cuando el modelo tenga estado operativo real (activo/inactivo o equivalente confirmado).
- `StatusFilter` es **opcional/no aplicable** cuando ese concepto no exista en el modelo destino.
- `Limpiar filtros` debe restaurar valores iniciales y recargar la primera página.
- Cambiar cualquier filtro debe resetear la página actual a 1.

Prohibiciones explícitas:

- No inventar campos de búsqueda no existentes en contrato.
- No inventar catálogos de estado sin respaldo backend.
- No mantener filtros solo cliente si el patrón vigente es backend-driven.

---

## 4) Estados mínimos obligatorios de la vista

Toda vista Grid debe contemplar y documentar explícitamente estos estados:

1. **Carga inicial**
   - primer fetch de datos al abrir la vista.
2. **Carga al filtrar/paginar**
   - transición visible al cambiar filtros, página o tamaño.
3. **Estado vacío**
   - mensaje operativo cuando no hay resultados.
4. **Error de carga**
   - feedback claro si falla el listado.
5. **Guardado en curso**
   - bloqueo/previsión de doble submit en drawers o formularios.
6. **Éxito/error de operación**
   - feedback posterior a create/edit/toggle/reset/detail (según aplique).

Estos estados son de cumplimiento obligatorio en diseño, validación y documentación.

---

## 5) Grid y datos

El grid debe:

- reflejar columnas reales del contrato backend,
- mantener orden de columnas coherente con operación administrativa,
- mostrar placeholder explícito para opcionales (ej. `—`),
- presentar colecciones (roles/permisos/categorías) de forma legible (chips/lista), sin serialización cruda.

Regla multivalor obligatoria:

- Campos multivalor se capturan con **multiselección**.
- **No usar CSV** como mecanismo de edición multivalor.

---

## 6) Paginación

El patrón de paginación es obligatorio:

- metadatos visibles (`Página X de Y`, `Total backend`),
- selector de tamaño de página,
- control de navegación.

Reglas:

- `page` y `pageSize` se envían siempre a backend.
- Cambiar `pageSize` debe volver a página 1.
- `totalItems` y `totalPages` deben provenir de backend, no calcularse localmente.

---

## 7) Acciones por fila y drawers

Acciones por fila permitidas:

- solo las soportadas por endpoints/contratos confirmados,
- con affordance visual clara (icono + tooltip),
- con feedback de éxito/error.

Patrón de drawers:

- encabezado con título + cierre,
- cuerpo con controles alineados al DTO real,
- footer con `Cancelar` y `Guardar/Aplicar`,
- estado `isSaving` o equivalente para prevenir doble submit.

No se deben agregar acciones “previstas” pero no implementadas por backend.

---

## 8) Contrato mínimo frontend ↔ backend (obligatorio por vista nueva)

Para cada nueva vista Grid, se debe documentar explícitamente:

1. **Endpoint de listado** (ruta y método).
2. **Query params soportados** (filtros, `page`, `pageSize`, otros reales).
3. **Respuesta paginada** (estructura y metadatos).
4. **DTO/item real** (campos efectivamente usados en grid y formularios).
5. **Acciones por fila reales** (y su endpoint asociado).
6. **Autorización requerida** (policy/rol/permiso real, si aplica).
7. **Limitaciones abiertas** (faltantes o restricciones confirmadas).

Reglas duras:

- No inventar campos.
- No inventar filtros.
- No inventar catálogos.
- No inventar acciones no soportadas por backend.

Si falta capacidad backend, se declara como limitación abierta; no se improvisa en UI.

---

## 9) Contrato visual con la shell actual

Toda nueva vista Grid debe respetar:

- layout/shell vigente,
- tema visual vigente,
- navegación protegida vigente,
- espaciados y jerarquía visual consistentes.

No introducir un estilo aislado incompatible con el resto de la aplicación administrativa.

---

## 10) Prompt estándar reusable (endurecido)

Usar este prompt base para pedir una nueva vista administrativa tipo Grid:

```text
Necesito una vista administrativa tipo Grid para el modelo <MODELO_X> en LabelVerificationSystem.

Condiciones obligatorias:
- Reutilizar el estándar docs/frontend/grid-view-standard.md.
- No inventar campos, filtros, catálogos ni acciones no soportadas por backend.
- Mantener patrón UX validado: SearchField, SearchText, StatusFilter (si aplica), Limpiar filtros.
- Mantener filtros/paginación backend-driven cuando backend lo soporte.
- Resolver campos multivalor con multiselección (no CSV).
- Mantener contrato visual con la shell actual.
- Separar claramente:
  1) estándar reusable aplicado,
  2) decisiones específicas del módulo <MODELO_X>,
  3) limitaciones abiertas.
- No mezclar con Fase 5, NLog ni cambios backend fuera de alcance.

Entregables obligatorios:
1) Propuesta de columnas.
2) Propuesta de filtros.
3) Estados de la vista (carga inicial, carga en filtros/paginación, vacío, error, guardado, éxito/error de operación).
4) Acciones soportadas y no soportadas.
5) Mapeo contra backend real (endpoints, params, DTOs, paginación, autorización).
6) Limitaciones abiertas.
7) Documentación afectada y actualizada.
8) Checklist de cumplimiento del estándar.
```

---

## 11) Checklist rápido de revisión

- [ ] Composición mínima completa: toolbar, filtros, grid, paginación, drawers, feedback/errores.
- [ ] `StatusFilter` aplicado correctamente (obligatorio si existe estado operativo; no aplicable si no existe).
- [ ] Estados mínimos obligatorios documentados y visibles en UX.
- [ ] Backend-driven real (sin inventar contrato).
- [ ] Multivalor en multiselección (sin CSV).
- [ ] Acciones por fila confirmadas en backend.
- [ ] Contrato frontend↔backend documentado completo.
- [ ] Limitaciones abiertas declaradas explícitamente.
