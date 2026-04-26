# Referencia concreta: LabelTypesAdmin.razor (`LabelType`)

## Estado y alcance
- Vista administrativa tipo grid para catálogo `LabelTypes`.
- Ruta frontend: `/label-types`.
- Contrato backend: `/api/label-types` + `/available-columns`.

## UX aplicada
- Toolbar con `Crear tipo` y `Actualizar`.
- Filtros: `SearchText`, `StatusFilter`, `Limpiar filtros`.
- Grid: nombre, columnas (pipe), estado y acciones.
- Drawer lateral para crear/editar seleccionando columnas disponibles.

## Autorización
- Policy robusta por acción:
  - `LabelTypesRead`
  - `LabelTypesCreate`
  - `LabelTypesEdit`
  - `LabelTypesActivateDeactivate`

## Reglas funcionales representadas
- No duplicar nombre.
- No guardar sin nombre o sin columnas.
- No permitir duplicados de columnas.
- Mostrar columnas disponibles desde backend (`GET /api/label-types/available-columns`).
