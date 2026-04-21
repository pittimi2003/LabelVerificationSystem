# Estándar reusable de vista administrativa tipo Grid

## Estado y alcance de este estándar

- Este documento define un **estándar reusable** para pedir y evaluar nuevas vistas administrativas tipo Grid en LabelVerificationSystem.
- Este documento **no introduce funcionalidad nueva** por sí mismo; solo formaliza patrón documental y reusable.
- Este documento se alinea al estado actual validado del proyecto.
- **Fase 4 sigue abierta**; por tanto, este estándar no debe interpretarse como cierre de fase.
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

## 2) Estructura visual obligatoria

La estructura base obligatoria es:

1. **Contenedor principal tipo página administrativa** (`div` raíz del módulo).
2. **Card principal** (`MudPaper` o equivalente) con separación interna.
3. **Stack vertical** con bloques claramente diferenciados:
   - cabecera/toolbar,
   - bloque de filtros,
   - estado de carga o tabla,
   - bloque de paginación.

No se deben introducir estructuras visuales paralelas que rompan el patrón actual del módulo administrativo.

---

## 3) Toolbar / cabecera

La cabecera debe incluir como mínimo:

- título del módulo,
- subtítulo descriptivo breve,
- acción de recarga del listado (`Actualizar` o equivalente),
- acción primaria de alta (`Nuevo ...`) cuando el backend soporte creación.

Reglas:

- Los botones deben respetar estado de carga/guardado para evitar operaciones duplicadas.
- No exponer acciones que el backend no soporte.

---

## 4) Patrón de filtros obligatorio

El patrón UX validado y obligatorio para nuevas vistas Grid es:

- `SearchField`
- `SearchText`
- `StatusFilter`
- `Limpiar filtros`

Reglas de filtro:

- `SearchField` define **qué campo backend** se está filtrando.
- `SearchText` envía valor normalizado (trim, nulo si vacío).
- `StatusFilter` representa estado (`Todos` / activo / inactivo o equivalente real del módulo).
- `Limpiar filtros` debe restaurar valores iniciales y recargar la primera página.
- Cambiar cualquier filtro debe resetear la página actual a 1.

Prohibiciones explícitas:

- No inventar campos de búsqueda no existentes en contrato.
- No inventar catálogos de estado sin respaldo backend.
- No mantener filtros solo cliente si el patrón vigente es backend-driven.

---

## 5) Tabla / grid

El grid debe:

- reflejar columnas reales del contrato backend,
- mostrar estados vacíos claros cuando no hay resultados,
- mostrar estado de carga claro durante fetch,
- mantener orden de columnas coherente con uso administrativo.

Reglas de datos:

- Campos opcionales deben mostrar placeholder explícito (ej. `—`).
- Colecciones (roles/permisos/categorías) deben presentarse de forma legible (chips/lista), sin serializaciones crudas.

---

## 6) Paginación

El patrón de paginación es obligatorio:

- metadatos visibles (`Página X de Y`, `Total backend`),
- selector de tamaño de página,
- control de navegación por página.

Reglas:

- `page` y `pageSize` se envían siempre a backend.
- Cambiar `pageSize` debe volver a página 1.
- `totalItems` y `totalPages` deben provenir de backend, no calcularse localmente.

---

## 7) Acciones por fila

Acciones por fila permitidas:

- solo las soportadas por endpoints/contratos confirmados,
- con affordance visual clara (icono + tooltip),
- con feedback de éxito/error.

Ejemplos de tipo de acción (según módulo):

- editar,
- activar/desactivar,
- reset,
- detalle.

No se deben agregar acciones “previstas” pero no implementadas por backend.

---

## 8) Drawers o patrón de formularios (create/edit/detail/reset)

Patrón obligatorio:

- formularios laterales (`MudDrawer` o equivalente) para operaciones administrativas,
- encabezado con título y cierre explícito,
- cuerpo con controles alineados al contrato,
- acciones `Cancelar` y `Guardar/Aplicar` al pie.

Reglas de formularios:

- Validar campos requeridos antes de invocar backend.
- En modo edición, bloquear campos inmutables si así lo exige contrato.
- Cerrar drawer y refrescar grid tras operación exitosa.
- Mantener estado `isSaving` para prevenir doble submit.

Regla multivalor obligatoria:

- Campos multivalor se capturan con **multiselección**.
- **No usar CSV** como mecanismo de edición multivalor.

---

## 9) Reglas de integración con backend

Cada vista Grid debe mapear explícitamente:

- endpoint de listado paginado,
- endpoint(s) por acción (create/edit/toggle/reset/detail),
- parámetros de filtro reales,
- contrato de request/response.

Reglas duras:

- No inventar campos.
- No inventar filtros.
- No inventar catálogos.
- No inventar acciones no soportadas por backend.

Si falta un dato o capacidad en backend, debe declararse como limitación abierta, no improvisarse en UI.

---

## 10) Manejo de errores

Debe existir manejo de errores de usuario y operativo:

- feedback visible (snackbar/toast/mensaje inline),
- mensajes diferenciados para carga vs guardado,
- limpieza/control de estados de carga en `finally`.

Recomendaciones de consistencia:

- evitar mensajes genéricos vacíos,
- mantener texto entendible para operación administrativa,
- no ocultar fallo silenciosamente.

---

## 11) Contrato visual con la shell actual

Toda nueva vista Grid debe respetar:

- layout/shell vigente,
- tema visual vigente,
- navegación protegida vigente,
- espaciados y jerarquía visual consistentes.

No introducir un estilo aislado incompatible con el resto de la aplicación administrativa.

---

## 12) Criterios obligatorios para nuevas vistas

Una nueva vista “tipo grid del modelo X” se considera correcta solo si cumple:

1. Usa la estructura visual obligatoria.
2. Implementa el patrón de filtros (`SearchField`, `SearchText`, `StatusFilter`, `Limpiar filtros`).
3. Consume backend con filtros/paginación reales.
4. Muestra acciones por fila solo cuando existen en backend.
5. Implementa formularios laterales coherentes con contrato.
6. Usa multiselección en campos multivalor (sin CSV).
7. Expone manejo de errores visible y consistente.
8. Declara explícitamente limitaciones abiertas sin inventar soluciones.
9. Mantiene compatibilidad visual con shell actual.
10. Mantiene explícito que **Fase 4 sigue abierta** cuando aplique al bloque documental vigente.

---

## 13) Prompt estándar reutilizable

Usar este prompt base para pedir una nueva vista administrativa tipo Grid:

```text
Necesito una vista administrativa tipo Grid para el modelo <MODELO_X> en LabelVerificationSystem.

Condiciones obligatorias:
- Reutilizar el estándar docs/frontend/grid-view-standard.md.
- No inventar campos, filtros, catálogos ni acciones no soportadas por backend.
- Mantener patrón UX validado: SearchField, SearchText, StatusFilter, Limpiar filtros.
- Mantener filtros/paginación backend-driven.
- Resolver campos multivalor con multiselección (no CSV).
- Mantener contrato visual con la shell actual.
- Separar claramente:
  1) estándar reusable aplicado,
  2) decisiones específicas del módulo <MODELO_X>,
  3) limitaciones abiertas.
- No mezclar con Fase 5, NLog ni cambios backend fuera de alcance.
- Mantener explícito que Fase 4 sigue abierta si el trabajo corresponde a Bloque B en esa fase.

Entregables esperados:
1) Diseño de la vista (estructura, filtros, grid, paginación, acciones por fila, drawers).
2) Mapeo exacto contra contratos backend existentes.
3) Lista de limitaciones abiertas sin suposiciones.
4) Checklist de cumplimiento del estándar.
```

---

## Checklist rápido de revisión

- [ ] Estructura de card + toolbar + filtros + tabla + paginación.
- [ ] Filtros en patrón validado.
- [ ] Backend-driven real (sin inventar contrato).
- [ ] Multivalor en multiselección.
- [ ] Acciones por fila confirmadas en backend.
- [ ] Drawers coherentes por operación.
- [ ] Manejo de errores visible.
- [ ] Compatibilidad con shell.
- [ ] Limitaciones abiertas documentadas.
- [ ] Sin mezclar con Fase 5/NLog y con Fase 4 explícitamente abierta cuando corresponda.
