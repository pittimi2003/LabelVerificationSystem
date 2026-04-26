# Referencia concreta: LabelTypesAdmin.razor (`LabelType`)

## Estado y alcance
- Vista administrativa tipo grid para catálogo `LabelTypes`.
- Ruta frontend: `/label-types`.
- Contrato backend: `/api/label-types` + `/available-columns`.

## UX aplicada
- Toolbar con `Crear tipo` y `Actualizar`.
- Filtros: `SearchText`, `StatusFilter`, `Limpiar filtros`.
- Grid: nombre, reglas (`columna=valor`), estado y acciones.
- Drawer lateral para crear/editar reglas: seleccionar columna disponible y capturar valor esperado por columna.

## Autorización
- Policy robusta por acción:
  - `LabelTypesRead`
  - `LabelTypesCreate`
  - `LabelTypesEdit`
  - `LabelTypesActivateDeactivate`

## Reglas funcionales representadas
- No duplicar nombre.
- No guardar sin nombre o sin reglas.
- No permitir columnas duplicadas dentro del mismo tipo.
- No permitir valores esperados vacíos.
- No permitir duplicado activo por combinación exacta de columna+valor.
- Mostrar columnas disponibles desde backend (`GET /api/label-types/available-columns`).
- Mensaje UX para conflicto por regla duplicada: "Ya existe un tipo de etiqueta con la misma combinación de columnas y valores."
