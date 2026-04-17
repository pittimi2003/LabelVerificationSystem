# Flujos Funcionales

## Propósito de este documento

Este documento describe los flujos funcionales del sistema con el nivel de definición actualmente disponible.

Su objetivo es:

- dejar explícito cómo se espera que opere el sistema
- alinear frontend, backend y dominio sobre el mismo comportamiento
- evitar interpretaciones inventadas o contradictorias
- servir como base para futuros contratos API, modelo de datos, validaciones y pruebas

Este documento es evolutivo.  
Debe actualizarse a medida que cada módulo gane precisión funcional.

---

## Regla de lectura

Los flujos aquí descritos deben interpretarse en tres niveles:

### 1. Flujo base confirmado
Comportamiento respaldado por la propuesta funcional inicial.

### 2. Decisión actual del proyecto
Comportamiento ya decidido durante la construcción del proyecto.

### 3. Pendiente de formalización
Puntos que todavía no tienen suficiente definición y deberán cerrarse después.

No debe inventarse comportamiento fuera de estos tres niveles.

---

## Módulos con flujo funcional identificado

A la fecha, el sistema tiene identificados estos módulos funcionales:

- Carga y procesamiento de Excel
- Gestión de partes
- Verificación de etiquetas
- Packing Lists
- Administración de usuarios y roles
- Auditoría / historial
- Configuración general del sistema :contentReference[oaicite:2]{index=2}

---

# 1. Flujo de Carga de Excel

## Estado del flujo
**Primer módulo real a construir**

## Objetivo
Permitir la carga de un archivo Excel con matriz de partes para validar su estructura, procesar su contenido, calcular información derivada y registrar la información oficial en el sistema. La propuesta inicial define que este módulo incluye validación de formato y estructura, cálculo automático del tipo de etiqueta y de la configuración de lectura, además de auditoría de cargas y modificaciones. :contentReference[oaicite:3]{index=3}

## Actores involucrados
- Administrador
- Supervisor :contentReference[oaicite:4]{index=4}

## Disparador
Un usuario autorizado inicia una carga de archivo Excel desde el sistema.

## Precondiciones
- El usuario debe estar autenticado.
- El usuario debe tener permisos para gestionar partes.
- Debe existir un mecanismo de carga de archivos en la aplicación.
- El archivo debe ser de tipo Excel compatible con el formato admitido por el sistema.

## Flujo base esperado
1. El usuario selecciona un archivo Excel.
2. El sistema recibe el archivo.
3. El sistema valida que el archivo tenga el formato esperado.
4. El sistema valida que la estructura del archivo sea correcta.
5. El sistema procesa las filas válidas del archivo.
6. El sistema calcula automáticamente el tipo de etiqueta.
7. El sistema calcula automáticamente la configuración de lectura asociada.
8. El sistema almacena la información procesada en la base de datos.
9. El sistema registra la operación en auditoría o historial de carga.
10. El sistema informa el resultado de la carga al usuario. :contentReference[oaicite:5]{index=5} :contentReference[oaicite:6]{index=6}

## Resultado esperado
- La información de partes queda registrada o actualizada en el sistema.
- La carga queda registrada en historial o auditoría.
- El usuario recibe confirmación o detalle de errores.

## Errores esperables
- Archivo inválido.
- Estructura incorrecta.
- Datos faltantes o incompatibles.
- Filas no procesables.
- Error de almacenamiento.
- Error de cálculo de tipo de etiqueta.
- Error de cálculo de configuración.

## Definición cerrada para v1 (implementada)
- Se procesa una sola hoja y su nombre no importa.
- La fila de encabezados se detecta por mejor coincidencia contra los encabezados obligatorios normalizados.
- Columnas mínimas obligatorias: `Part Number`, `Model`, `Minghua description`, `CADUCIDAD`, `CCO`, `Certification EAC`, `4 FIRST NUMERS`.
- Si falta una columna obligatoria en encabezado, se rechaza toda la carga como archivo inválido.
- La carga es parcial a nivel de filas: las filas válidas se insertan y las inválidas/duplicadas se rechazan con error por fila.
- Regla de duplicado contra sistema en v1: mismo `Part Number`.
- El módulo v1 solo crea nuevas partes; no actualiza existentes.
- El archivo original se conserva.
- Se registra historial básico de cargas desde v1.
- El historial básico puede consultarse por API en `GET /api/excel-uploads` y `GET /api/excel-uploads/{id}`.
- En v1 no se calcula tipo de etiqueta ni configuración de lectura.
- Se persiste resultado por fila en `ExcelUploadRowResult` con estado `Inserted` o `Rejected`.
- Cada parte insertada queda vinculada a la carga origen mediante `CreatedByExcelUploadId`.
- UX frontend v1.3: `MudFileUpload` como dropzone principal de ancho completo (drag & drop + clic), feedback por `Snackbar`, limpieza post-carga, refresco automático de historial y panel lateral de detalle histórico con vista general y vista por fila.
- UX frontend v1.4: en la vista por fila del drawer se agregan filtros (campo + texto y status), acción de limpiar filtros y paginación local (5/10/20), manteniendo la vista general y vista por fila.

## Pendiente de formalización
- reglas de validación avanzadas por columna
- catálogo definitivo de estados de carga
- cálculo de tipo de etiqueta
- cálculo de configuración de lectura

## Reglas de parseo cerradas en la iteración de trazabilidad
- `CADUCIDAD`: `NA`/vacío => `null`; entero válido => `int`; otro valor => rechazo de fila.
- `Certification EAC`: `YES` => `true`; `NO` => `false`; `NA`/vacío => `null`; otro valor => rechazo de fila.
- `4 FIRST NUMERS`: obligatorio y entero; si no parsea => rechazo de fila.

---

# 2. Flujo de Gestión de Partes

## Estado del flujo
**Identificado en propuesta, pendiente de detalle funcional**

## Objetivo
Permitir la administración del catálogo oficial de partes del sistema.

## Actores involucrados
- Administrador
- Supervisor :contentReference[oaicite:7]{index=7}

## Capacidades esperadas
- visualizar catálogo completo
- registrar partes
- editar partes
- eliminar partes
- consultar información de partes
- usar las partes como base oficial para validaciones posteriores :contentReference[oaicite:8]{index=8}

## Flujo base esperado
1. El usuario autorizado accede al módulo de partes.
2. El sistema muestra el catálogo disponible.
3. El usuario puede crear, editar o eliminar una parte.
4. El sistema valida la acción solicitada.
5. El sistema persiste el cambio.
6. El sistema registra auditoría cuando corresponda.

## Resultado esperado
- El catálogo queda actualizado conforme a las acciones permitidas.
- La información oficial de partes queda disponible para otros módulos.

## Pendiente de formalización
- atributos exactos de una parte
- validaciones exactas de alta y edición
- restricciones de unicidad
- política de eliminación
- si existe baja lógica o física
- relación exacta entre parte, configuración y tipo de etiqueta
- eventos auditables exactos

---

# 3. Flujo de Verificación de Etiquetas

## Estado del flujo
**Identificado en propuesta, pendiente de detalle funcional**

## Objetivo
Validar una etiqueta física comparando la información escaneada contra los datos oficiales del sistema mediante un proceso de dos escaneos. La propuesta inicial define que el primer escaneo identifica automáticamente el número de parte y que el segundo escaneo compara la etiqueta completa contra la información oficial. :contentReference[oaicite:9]{index=9}

## Actores involucrados
- Operador :contentReference[oaicite:10]{index=10}

## Disparador
El operador inicia un proceso de verificación.

## Precondiciones
- El operador debe tener acceso al flujo de operación.
- Debe existir una parte oficial registrada que pueda ser localizada por el primer escaneo.
- Debe existir un mecanismo de captura desde lectora o entrada equivalente.
- Deben existir datos oficiales disponibles para comparar la etiqueta.

---

## 3.1 Flujo de Primer Escaneo

### Objetivo
Identificar automáticamente la parte y preparar el sistema para el segundo escaneo.

### Flujo base esperado
1. El operador realiza el primer escaneo.
2. El sistema recibe el valor escaneado.
3. El sistema identifica automáticamente el número de parte.
4. El sistema valida los campos clave requeridos en esta etapa.
5. Si la parte existe:
   - el sistema muestra la configuración requerida
   - el sistema prepara el paso hacia el segundo escaneo
6. Si la parte no existe:
   - el sistema informa error al operador :contentReference[oaicite:11]{index=11}

### Resultado esperado
- Parte identificada y contexto preparado para segundo escaneo
- o error de parte no encontrada

### Pendiente de formalización
- formato exacto del valor escaneado
- campos clave exactos a validar
- comportamiento exacto cuando hay coincidencias ambiguas
- integración exacta con la lectora
- si el cambio de configuración es automático, sugerido o asistido
- tiempos y condiciones de expiración del contexto de primer escaneo

---

## 3.2 Flujo de Segundo Escaneo

### Objetivo
Leer la etiqueta completa y compararla contra la información oficial.

### Flujo base esperado
1. El operador realiza el segundo escaneo.
2. El sistema recibe la lectura completa de la etiqueta.
3. El sistema extrae todos los campos relevantes.
4. El sistema compara esos campos contra la información oficial registrada para la parte identificada.
5. El sistema determina si la etiqueta es correcta o errónea.
6. El sistema informa el resultado.
7. El sistema retorna a la configuración inicial de lectura de códigos de barras. :contentReference[oaicite:12]{index=12}

### Resultado esperado
- Etiqueta correcta
- Etiqueta errónea

### Pendiente de formalización
- campos exactos que se leerán de la etiqueta
- reglas exactas de comparación
- tolerancias permitidas o no permitidas
- política de error por campo
- mensaje de resultado esperado por UI
- detalle de la integración de retorno a configuración inicial

---

## 3.3 Flujo completo de verificación

### Flujo base esperado
1. Escaneo de código de barras.
2. Identificación de parte y configuración.
3. Cambio automático de configuración en lectora.
4. Escaneo de etiqueta completa.
5. Validación exacta.
6. Resultado: correcta o errónea. :contentReference[oaicite:13]{index=13}

### Pendiente de formalización
- qué ocurre si el operador abandona el flujo a mitad del proceso
- qué se audita de cada verificación
- si se almacena historial de intentos fallidos
- cómo se reinicia el proceso manualmente
- tratamiento de reintentos
- tiempos máximos entre primer y segundo escaneo

---

# 4. Flujo de Packing List

## Estado del flujo
**Identificado en propuesta, pendiente de detalle funcional**

## Objetivo
Permitir la creación, uso colaborativo y cierre de packing lists usando el mismo proceso de verificación de dos escaneos.

## Actores involucrados
- Operador
- Supervisor
- Administrador :contentReference[oaicite:14]{index=14}

## Precondiciones
- Debe existir el módulo de packing lists habilitado.
- Debe existir un identificador o número de packing list.
- Debe existir verificación operativa integrada al flujo.

---

## 4.1 Creación o unión a Packing List

### Flujo base esperado
1. El operador ingresa el número de packing list.
2. El sistema verifica si ese packing list existe.
3. Si existe:
   - el operador se une al proceso existente
4. Si no existe:
   - el sistema crea un nuevo packing list con estado Abierto
5. El sistema habilita la operación sobre ese packing list. :contentReference[oaicite:15]{index=15}

### Pendiente de formalización
- reglas exactas de unicidad del número
- quién puede crear
- si hay validaciones de formato del número
- si existe límite de operadores simultáneos
- qué datos se muestran al unirse

---

## 4.2 Escaneo dentro de Packing List

### Flujo base esperado
1. El operador inicia el escaneo dentro del packing list activo.
2. El sistema usa el mismo proceso de verificación de dos escaneos.
3. Si la etiqueta es correcta:
   - se registra una línea en el packing list
4. El sistema muestra en tiempo real las líneas acumuladas. :contentReference[oaicite:16]{index=16}

### Resultado esperado
- La línea queda registrada dentro del packing list si la verificación es correcta.

### Pendiente de formalización
- estructura exacta de la línea registrada
- política ante etiquetas erróneas dentro del packing list
- posibilidad de duplicados
- actualizaciones en tiempo real
- manejo de concurrencia entre operadores

---

## 4.3 Operaciones del Operador en Packing List

### Flujo base esperado
El operador puede:

- agregar líneas mediante escaneo
- eliminar líneas registradas
- cerrar el packing list al finalizar :contentReference[oaicite:17]{index=17}

### Pendiente de formalización
- restricciones para eliminar líneas
- qué validaciones existen antes del cierre
- si el cierre requiere confirmación adicional
- si hay permisos especiales sobre líneas ajenas

---

## 4.4 Operaciones de Supervisor / Administrador en Packing List

### Flujo base esperado
Supervisor y Administrador pueden:

- monitorear el avance en tiempo real
- visualizar operadores activos
- exportar el packing list a Excel
- reabrir el packing list, de forma opcional :contentReference[oaicite:18]{index=18}

### Pendiente de formalización
- criterios exactos para reapertura
- formato de exportación
- datos visibles en monitoreo
- reglas de acceso por rol
- visibilidad histórica de packing lists cerrados

---

## 4.5 Flujo completo de Packing List

### Flujo base esperado
1. Ingreso de número de packing list.
2. Creación o unión al proceso.
3. Escaneo 1 y configuración.
4. Escaneo 2 y validación.
5. Registro de línea correcta.
6. Eliminación de líneas, si aplica.
7. Cierre del packing list.
8. Supervisión y exportación. :contentReference[oaicite:19]{index=19}

---

# 5. Flujo de Administración de Usuarios y Roles

## Estado del flujo
**Identificado en propuesta, pendiente de detalle funcional**

## Objetivo
Permitir la gestión de usuarios del sistema y su clasificación por roles.

## Actores involucrados
- Administrador :contentReference[oaicite:20]{index=20}

## Flujo base esperado
1. El administrador accede al módulo de usuarios.
2. El sistema permite registrar, consultar y administrar usuarios.
3. El sistema asocia un rol al usuario.
4. El sistema aplica permisos según rol.

## Roles base identificados
- Operador
- Supervisor
- Administrador :contentReference[oaicite:21]{index=21}

## Pendiente de formalización
- mecanismo exacto de autenticación
- alta, edición, baja y bloqueo de usuarios
- permisos detallados por rol
- política de contraseñas
- auditoría de accesos
- recuperación de cuenta
- reglas de sesión

---

# 6. Flujo de Auditoría e Historial

## Estado del flujo
**Identificado en propuesta, pendiente de detalle funcional**

## Objetivo
Registrar eventos relevantes de operación, carga y administración para trazabilidad y control.

## Fuentes funcionales identificadas
- auditoría de cargas y modificaciones
- historial de cargas de Excel
- auditoría como responsabilidad del backend :contentReference[oaicite:22]{index=22} :contentReference[oaicite:23]{index=23}

## Flujo base esperado
1. Ocurre una acción relevante en el sistema.
2. El sistema registra el evento correspondiente.
3. Los usuarios autorizados pueden consultar el historial cuando aplique.

## Pendiente de formalización
- catálogo exacto de eventos auditables
- estructura del evento
- nivel de detalle almacenado
- retención de datos
- consulta y filtros
- visibilidad por rol

---

# 7. Flujo de Configuración General

## Estado del flujo
**Identificado en propuesta, pendiente de detalle funcional**

## Objetivo
Permitir la administración de parámetros generales del sistema.

## Actores involucrados
- Administrador :contentReference[oaicite:24]{index=24}

## Flujo base esperado
1. El administrador accede al módulo de configuración general.
2. El sistema presenta los parámetros configurables.
3. El administrador actualiza valores permitidos.
4. El sistema valida y persiste la configuración.

## Pendiente de formalización
- qué parámetros existen
- impacto funcional de cada parámetro
- validaciones por parámetro
- auditoría de cambios
- versión o historial de configuración

---

# 8. Flujo de Sesión y Autenticación por Tokens

## Estado del flujo
**Implementación backend fase 1 + integración frontend de sesión (fase 1) completadas para `/api/auth/login|refresh|logout|me`**

## Objetivo
Garantizar sesión estable en Blazor WebAssembly usando access token corto y refresh token rotativo, con soporte de bypass controlado por configuración.

## Actores involucrados
- Usuario real (Operador/Supervisor/Administrador)
- Sistema en modo bypass (`Authentication:Bypass:Enabled`)

## Precondiciones
- Contratos de `/api/auth/*` vigentes según `docs/api/api-contracts.md`.
- UI de `Pages/Authentication` se mantiene como shell actual para login/reset.
- Access token con TTL de 20 minutos.
- Ventana de refresh proactivo de 3 minutos antes de vencimiento.
- Frontend Blazor WASM con servicio de sesión, restauración en recarga, refresh proactivo y single-flight implementados.
- Integración HTTP vía cliente nombrado `BackendApi` con inyección de bearer y manejo consistente de `401/403`.

## 8.1 Inicio de sesión (modo usuario)
1. Usuario envía credenciales a `POST /api/auth/login`.
2. Backend valida y crea sesión lógica (`sid`).
3. Backend emite access token + refresh token.
4. Frontend hidrata estado autenticado y consume `GET /api/auth/me` como fuente canónica.

## 8.2 Continuidad de sesión (refresh proactivo)
1. Frontend monitorea expiración del access token.
2. Al entrar en ventana `<= 3 minutos`, ejecuta `POST /api/auth/refresh`.
3. Backend valida refresh token, rota token y devuelve nuevo par.
4. Frontend actualiza credenciales sin interrumpir la navegación (objetivo de estabilidad de sesión).

## 8.3 Restauración al recargar aplicación
1. App carga estado local de sesión.
2. Si token está próximo a vencer, intenta refresh antes de bootstrap de UI.
3. Si refresh/validación resulta exitosa, llama `/api/auth/me` y reconstruye contexto.
4. Si falla con `401/409`, limpia sesión y redirige a login (o bypass si está activo).

## 8.4 Detección de replay/reuse
1. Cliente envía refresh token ya usado o revocado.
2. Backend responde `409 Conflict`.
3. Backend revoca cadena de sesión asociada.
4. Frontend termina sesión local por seguridad.

## 8.5 Cierre de sesión
1. Cliente invoca `POST /api/auth/logout`.
2. Backend revoca refresh token/sesión activa de forma idempotente.
3. Cliente elimina credenciales locales y vuelve a estado no autenticado.

## 8.6 Modo bypass configurable
1. Si `Authentication:Bypass:Enabled=true` y el entorno está autorizado, backend permite identidad sintética.
2. `/api/auth/me` debe responder `authenticationMode = "Bypass"`.
3. Operaciones deben auditar `authMode=Bypass`.
4. Implementación fase 1: `/api/auth/login` y `/api/auth/refresh` responden `409 Conflict` cuando bypass está habilitado en entorno permitido.

## 8.7 Control de entrada inicial y guard de rutas (cerrado en esta iteración)
1. Al iniciar la app, frontend ejecuta bootstrap de sesión de forma bloqueante antes de renderizar navegación principal.
2. Si no hay sesión válida o recuperable, redirige a `/signin`.
3. Si hay sesión de usuario válida (o recuperada por refresh), permite entrada normal.
4. Si aplica bypass vigente en backend, permite entrada con identidad sintética sin login manual.
5. En cada navegación, frontend decide acceso usando el estado de sesión en memoria, sin invocar `/api/auth/me` en cada cambio de página.

## 8.8 Clasificación de rutas frontend
- Rutas públicas: `/signin`, `/signin-basic`, `/signup`, `/reset-password`, `/error`, `/error401`.
- Rutas protegidas: cualquier otra ruta del SPA (incluyendo actuales y futuras), protegidas por política por defecto.


## 8.9 Perfil autenticado en UI (frontend fase 1.2)
1. En header profile se muestra identidad real de la sesión (`displayName`/`username` y `email` si existe).
2. `Account Settings` y `View Profile` navegan a `/profile-settings`.
3. La vista `/profile-settings` refleja sólo datos disponibles en `GET /api/auth/me` (sin edición).
4. `Log Out` desde menú de perfil reutiliza `AuthSessionService.LogoutAsync()` y mantiene cierre de sesión controlado.
5. Si algún dato no existe en sesión, UI lo declara explícitamente como no disponible.

## 8.10 Reset de contraseña real (Bloque A en Fase 4 abierta)
1. Usuario abre `/reset-password` y envía `usernameOrEmail` a `POST /api/auth/password/reset-request`.
2. Backend siempre responde `202` con mensaje neutral (sin revelar existencia de cuenta).
3. Si la cuenta existe/activa, backend genera token opaco de un solo uso (hash persistido + expiración) y revoca tokens previos activos del mismo usuario.
4. En esta fase, la entrega del token se hace fuera de banda por operación/logs; no hay proveedor email integrado aún.
5. Usuario envía `resetToken + newPassword + confirmPassword` a `POST /api/auth/password/reset-confirm`.
6. Backend valida token (vigente, no usado, no revocado), valida política de password y persiste credencial efectiva.
7. Al reset exitoso se invalida el token consumido y los demás tokens activos de reset del usuario.
8. Con configuración vigente, backend revoca todas las sesiones activas y refresh tokens del usuario (`reason=password_reset`), forzando re-login en todos los dispositivos.


## 8.11 Administración backend de usuarios (Bloque B en Fase 4 abierta)
1. Administrador autenticado consulta `GET /api/users` con filtros (`query`, `isActive`) y paginación (`page`, `pageSize`).
2. Backend responde colección paginada con metadatos para grid administrativo.
3. Administrador crea cuenta por `POST /api/users` con password inicial, roles/permisos y estado.
4. Backend persiste `SystemUser` + `UserPasswordCredential` y devuelve recurso creado.
5. Administrador consulta detalle por `GET /api/users/{userId}` para cargar edición.
6. Administrador actualiza cuenta por `PUT /api/users/{userId}` (incluyendo cambio opcional de contraseña).
7. Administrador activa/desactiva cuenta por `PATCH /api/users/{userId}/activation` según política operativa vigente.
8. Flujo de autenticación reutiliza estas cuentas persistidas como fuente primaria, con fallback compatible al catálogo estático existente.

## Reglas cerradas en esta iteración
- Access token: 20 minutos.
- Refresh proactivo: 3 minutos antes de vencimiento.
- Rotación de refresh token obligatoria.
- Single-flight de refresh en frontend para evitar concurrencia.
- Retry acotado solo para fallos transitorios de red/5xx.

## Decisiones abiertas explícitas
- Estrategia final de almacenamiento seguro de refresh token en cliente (cookie HttpOnly u otro mecanismo endurecido para producción).
- Política de sesiones simultáneas por usuario.
- Definir si bypass emitirá sesión virtual en una fase futura o se mantiene el comportamiento `409` actual.
- Canal final de entrega de reset token (email/SMTP u otro) para reemplazar estrategia temporal fuera de banda.

---

## Reglas generales aún pendientes

Existen definiciones transversales que todavía deberán cerrarse y que impactarán a varios flujos:

- contratos API
- estructura del modelo de datos
- reglas de errores de negocio
- mensajes operativos al usuario
- validaciones exactas por rol
- estrategia de concurrencia
- estrategia de auditoría
- integración real con lectoras
- criterios exactos de exportación
- políticas de seguridad y autenticación

---

## Regla de evolución

Este documento debe actualizarse cuando ocurra cualquiera de estas situaciones:

- un módulo pase de flujo general a flujo detallado
- se cierre una regla de negocio relevante
- se creen contratos API que fijen comportamiento
- se defina el modelo de datos
- se valide una integración real con hardware
- se incorporen nuevos estados, errores o decisiones operativas

---

## Historial

### Versión inicial
- Se documentan los flujos funcionales base a partir de la propuesta inicial.
- Se identifica Carga de Excel como primer módulo real a construir.
- Se describen los flujos principales sin inventar detalle no formalizado.
- Se deja explícito qué partes del comportamiento siguen pendientes de definición.
