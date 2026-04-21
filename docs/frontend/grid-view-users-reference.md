# Referencia concreta: UsersAdmin.razor

## Propósito

Este documento evalúa `UsersAdmin.razor` como referencia actual del patrón de vista administrativa tipo Grid.

Objetivo:

- separar qué es reusable del patrón,
- identificar qué es específico del módulo de usuarios,
- evitar copiar esta implementación como plantilla universal literal.

Estado explícito:

- **Fase 4 sigue abierta**.
- Este documento es de referencia documental; no cierra fase ni agrega alcance funcional nuevo.

---

## 1) Qué partes del estándar ya cumple

`UsersAdmin.razor` ya cumple de forma consistente con el estándar reusable en estos puntos:

1. **Estructura visual base**
   - contenedor de página administrativa,
   - card principal,
   - bloques de toolbar, filtros, tabla y paginación.

2. **Toolbar/cabecera**
   - título + subtítulo,
   - acciones `Actualizar` y `Nuevo usuario`.

3. **Patrón de filtros validado**
   - `SearchField`, `SearchText`, `StatusFilter`, `Limpiar filtros`.

4. **Grid administrativo**
   - columnas alineadas al contrato real de usuarios,
   - chips para valores multivalor (`roles`, `permissions`),
   - estado vacío explícito cuando no hay resultados.

5. **Paginación backend-driven**
   - página actual, total de páginas y total backend,
   - selector de tamaño de página,
   - recarga en cambios de página/tamaño/filtros.

6. **Acciones por fila**
   - editar,
   - reset de contraseña,
   - activar/desactivar.

7. **Drawers de operación**
   - drawer create/edit,
   - drawer reset password,
   - validaciones mínimas antes de guardar.

8. **Manejo de errores**
   - mensajes visibles por snackbar en carga y acciones.

---

## 2) Decisiones actuales específicas del módulo de usuarios

Estas decisiones son de usuarios y **no deben promoverse automáticamente como estándar universal**:

- campos concretos de búsqueda: `userId`, `username`, `displayName`, `email`, `role`, `permission`;
- columnas específicas de identidad de usuario y timestamps;
- acción de reset password como operación por fila;
- semántica de activación/desactivación para estado de usuario;
- payloads específicos `CreateUserRequestDto` / `UpdateUserRequestDto`;
- mapeo de `StatusFilter` hacia `isActive` nullable.

---

## 3) Limitaciones abiertas que siguen vigentes

En el estado actual, siguen abiertas (no cerrar como estándar definitivo):

1. **Catálogo global de roles/permisos**
   - actualmente se detecta desde datos disponibles en respuestas cargadas,
   - no existe un catálogo independiente y completo garantizado para todo el universo de datos.

2. **Modelo final roles/permisos**
   - sigue abierto a cierre de fase (normalización final vs representación actual).

3. **Políticas finales de operación administrativa**
   - alcance vigente cubre lo implementado, pero no define por sí solo la estrategia final global para otros módulos.

Estas limitaciones deben declararse al replicar el patrón en otros módulos.

---

## 4) Qué NO debe copiarse ciegamente a otros módulos

No copiar de forma literal:

- nombres de columnas/labels propios de usuarios;
- filtros exactos de usuarios;
- acciones `reset password` o `activar/desactivar` si el backend del módulo destino no las soporta;
- reglas de validación puntuales de contraseñas/identidad;
- textos de UX específicos de usuarios;
- suposición de que cualquier módulo tiene `isActive`.

Tampoco copiar sin validar:

- estructura exacta de DTOs de usuarios;
- tooltips y semántica de iconos orientados a identidad/autenticación.

---

## 5) Qué piezas visuales/funcionales SÍ son reutilizables

Componentes/patrones reutilizables:

- layout administrativo en card principal;
- toolbar con acción de refresco + alta;
- patrón de filtros validado (`SearchField`, `SearchText`, `StatusFilter`, `Limpiar filtros`);
- recarga backend al cambiar filtros/paginación;
- tabla con chips para colecciones;
- bloque de paginación con metadatos backend;
- acciones por fila con iconografía/tooltip;
- drawers laterales para create/edit/detail/reset (según soporte real);
- validación mínima de formularios antes de invocar backend;
- feedback de errores y éxito con snackbar.

Además:

- el enfoque de multivalor con `MultiSelection=true` es reusable;
- el enfoque CSV para editar multivalor **no** debe reutilizarse.

---

## 6) Evaluación final de referencia

`UsersAdmin.razor` es una referencia útil y vigente para el patrón Grid administrativo del proyecto, pero **no es una plantilla literal universal**.

Uso recomendado:

- reutilizar su estructura y flujo UX,
- adaptar filtros/columnas/acciones al contrato real del módulo destino,
- declarar limitaciones abiertas cuando backend aún no cierra capacidades,
- mantener explícito que **Fase 4 sigue abierta** en el bloque documental correspondiente.
