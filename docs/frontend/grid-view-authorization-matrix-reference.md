# Referencia de vista administrativa: AuthorizationMatrixAdmin

## Estado
- Bloque B.
- **Fase 4 sigue abierta**.
- Documento de referencia del estado implementado; no cierra fase.

## Ruta
- Frontend: `/authorization-matrix`.

## Estructura UX aplicada
- Toolbar con título/subtítulo y acciones `Actualizar` + `Guardar cambios`.
- Selector de rol (catálogo explícito de backend).
- Grid por módulo con:
  - nombre/código de módulo,
  - `Module Authorized`,
  - listado de acciones hijas con `Action Authorized`.
- Estados implementados:
  - carga inicial,
  - error de carga,
  - estado sin rol/matriz,
  - guardado con feedback de éxito/error.

## Contrato frontend ↔ backend utilizado

| Elemento | Valor confirmado |
|---|---|
| Catálogo de roles | `GET /api/authorization-matrix/roles` |
| Lectura matriz por rol | `GET /api/authorization-matrix/roles/{roleCode}` |
| Guardado matriz por rol | `PUT /api/authorization-matrix/roles/{roleCode}` |
| DTO lectura | `RoleAuthorizationMatrixDto` |
| DTO guardado | `UpdateRoleAuthorizationMatrixRequestDto` |
| Autorización | Policy backend actual: `AuthorizationMatrixManage` |

## Limitaciones abiertas
- El acceso ya usa policy dedicada `AuthorizationMatrixManage`; la evolución pendiente es endurecer la gobernanza fina de quién puede delegar permisos por entorno.
- No se retiran aún `RolesJson`/`PermissionsJson`.
- No hay mezcla de este alcance con Fase 5 ni con NLog.
