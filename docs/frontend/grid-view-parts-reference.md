# Referencia concreta: PartsAdmin.razor

## Estándar reusable aplicado
- Toolbar con actualizar + alta.
- Bloque de filtros con `SearchField`, `SearchText` y `Limpiar filtros`.
- Grid administrativo backend-driven.
- Paginación backend-driven (`page`, `pageSize`, `totalItems`, `totalPages`).
- Drawers laterales para create/edit/detail.
- Estados visibles: loading inicial, loading al filtrar/paginar, vacío, error de carga, guardando, éxito/error de operación.

## Específico del modelo Part
- Columnas: `PartNumber`, `Model`, `MinghuaDescription`, `Caducidad`, `Cco`, `CertificationEac`, `FirstFourNumbers`, `CreatedByExcelUploadId`, `CreatedAtUtc`.
- Filtros soportados por backend: `partNumber`, `model`, `minghuaDescription`, `cco`.
- Acciones por fila soportadas: ver detalle, editar.
- Acción de toolbar soportada: crear.

## Limitaciones abiertas
- No existe estado operativo en `Part`; por estándar reusable no se renderiza `StatusFilter`.
- No existe contrato backend de activar/desactivar para `Part`.
