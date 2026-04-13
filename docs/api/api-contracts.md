# Contratos API

## Propósito de este documento

Este documento define la estructura inicial de contratos API del proyecto.

Su objetivo es:

- establecer la forma en que el frontend consumirá el backend
- alinear módulos funcionales, endpoints y responsabilidades
- evitar que se inventen contratos durante la implementación
- servir como base para refinamiento progresivo a medida que los módulos se construyan

Este documento no representa todavía el contrato HTTP definitivo de cada endpoint.

En esta etapa debe leerse como una **definición inicial de contratos API por módulo**.

---

## Estado actual de los contratos

**Estado:** Inicial

Todavía no existen contratos cerrados de implementación.

En esta etapa se documentan:

- módulos API esperados
- responsabilidades por módulo
- endpoints probables iniciales
- estructuras de request/response a nivel conceptual
- decisiones pendientes

La propuesta funcional inicial ya establece que la API debe cubrir al menos:

- carga y procesamiento de Excel
- CRUD de partes
- cálculo de configuraciones
- validación de escaneo 1 y 2
- gestión de packing lists
- auditoría
- exportación :contentReference[oaicite:1]{index=1}

---

## Principios de diseño de contratos

### 1. El frontend consume solo API
El frontend en Blazor WebAssembly no debe acceder directamente a dominio, persistencia ni infraestructura.  
Toda interacción funcional debe ocurrir a través de contratos HTTP.

### 2. Los contratos deben nacer desde el comportamiento real
No deben diseñarse endpoints decorativos o genéricos sin relación clara con flujos funcionales reales.

### 3. Los contratos deben crecer por módulo
Se documentarán y cerrarán progresivamente conforme avance la implementación real del proyecto.

### 4. La respuesta API debe ser consistente
Aunque los payloads finales aún no están cerrados, todos los módulos deben converger hacia una convención uniforme de respuestas, errores y validaciones.

### 5. Los contratos deben poder evolucionar
Mientras el proyecto esté en construcción inicial, los contratos podrán ajustarse, siempre que el cambio quede documentado.

---

## Convenciones iniciales

## Base path sugerido
Todos los endpoints del sistema deben exponerse bajo un prefijo común:

`/api`

## Formato de intercambio
- JSON para operaciones estándar
- `multipart/form-data` para carga de archivos cuando aplique

## Convención de nombres
- recursos en plural cuando representen colecciones
- rutas orientadas a intención funcional cuando el caso de uso lo requiera
- evitar nombres ambiguos o genéricos sin contexto de negocio

## Versionado
Pendiente de definición.

Mientras no se defina otra estrategia, se asume una única versión activa no versionada explícitamente.

## Autenticación
Pendiente de definición detallada.

La propuesta contempla gestión de usuarios y roles, pero aún no existe contrato formal de autenticación. :contentReference[oaicite:2]{index=2} :contentReference[oaicite:3]{index=3}

---

## Convención inicial de respuestas

## Respuesta exitosa conceptual
Las respuestas exitosas deberían contener, según el caso:

- resultado principal
- mensajes relevantes para UI
- datos derivados necesarios para el flujo
- metadatos mínimos si son necesarios

## Respuesta de error conceptual
Las respuestas de error deberían poder comunicar, según el caso:

- tipo de error
- mensaje principal
- detalle validable por UI
- errores por campo o por fila, si aplica
- código funcional o técnico, si se define

## Decisión pendiente
Debe definirse si existirá:

- una envoltura uniforme tipo `ApiResponse<T>`
- respuestas directas por endpoint
- una combinación según el caso de uso

---

# 1. Módulo API de Carga de Excel

## Estado
**Primer módulo real a construir**

## Responsabilidad
Recibir un archivo Excel, validar su formato y estructura, procesar su contenido, calcular información derivada y registrar el resultado de la carga. La propuesta funcional inicial incluye validación de formato y estructura, cálculo automático del tipo de etiqueta y cálculo automático de configuración de lectura. :contentReference[oaicite:4]{index=4} :contentReference[oaicite:5]{index=5}

## Endpoints iniciales propuestos

### `POST /api/excel-uploads`
Endpoint principal para registrar una nueva carga de archivo Excel.

#### Request conceptual
- archivo Excel
- datos contextuales mínimos si se requieren
- usuario autenticado implícito por contexto de seguridad

#### Response conceptual
- identificador de carga
- estado de procesamiento
- resumen del resultado
- métricas generales
- lista o referencia de errores si aplica

### `GET /api/excel-uploads`
Consulta de historial o listado de cargas.

#### Response conceptual
- colección de cargas
- estado
- fecha
- usuario
- resumen de resultado

### `GET /api/excel-uploads/{uploadId}`
Consulta de una carga específica.

#### Response conceptual
- detalle de la carga
- métricas
- errores o advertencias
- información de auditoría asociada si aplica

## Decisiones pendientes
- si la carga se procesa en línea o de forma diferida
- si habrá detalle persistente por fila
- si se almacenará el archivo físico
- si la respuesta del `POST` devuelve resultado final o solo estado inicial
- estructura exacta de errores de validación por fila

---

# 2. Módulo API de Partes

## Responsabilidad
Administrar el catálogo oficial de partes del sistema.

## Endpoints iniciales propuestos

### `GET /api/parts`
Obtiene el catálogo de partes.

### `GET /api/parts/{partId}`
Obtiene el detalle de una parte específica.

### `POST /api/parts`
Registra una nueva parte.

### `PUT /api/parts/{partId}`
Actualiza una parte existente.

### `DELETE /api/parts/{partId}`
Elimina o desactiva una parte, según la estrategia que se defina.

## Request conceptual
Todavía pendiente de definición exacta, pero deberá incluir los datos oficiales necesarios para representar una parte y sus atributos de validación.

## Response conceptual
- identificador
- número de parte
- datos oficiales
- tipo de etiqueta
- información de configuración asociada si aplica

## Decisiones pendientes
- atributos exactos de la parte
- política de borrado
- criterios de unicidad
- si las configuraciones viajan embebidas o relacionadas
- si existe búsqueda paginada o filtrada desde la primera versión

---

# 3. Módulo API de Configuraciones

## Responsabilidad
Exponer o resolver configuraciones de lectura asociadas a partes o procesos de validación.

## Justificación
La propuesta funcional contempla cálculo automático de configuraciones y uso de configuraciones durante el flujo de verificación. :contentReference[oaicite:6]{index=6} :contentReference[oaicite:7]{index=7}

## Endpoints iniciales propuestos

### `GET /api/configurations/{configurationId}`
Obtiene una configuración específica.

### `GET /api/parts/{partId}/configuration`
Obtiene la configuración asociada a una parte.

## Alternativa posible
Si la configuración no se administra como recurso independiente, parte de esta información podrá venir incluida en respuestas del módulo de partes o del módulo de verificación.

## Decisiones pendientes
- si Configuration será un recurso API de primer nivel
- si habrá CRUD de configuraciones
- si la configuración se calcula siempre en backend
- si la configuración se expone completa o resumida hacia el frontend

---

# 4. Módulo API de Verificación

## Responsabilidad
Soportar el flujo de verificación de etiquetas mediante dos escaneos.

## Justificación
La propuesta define validación de escaneo 1 y 2, así como servicios internos asociados a verificación. :contentReference[oaicite:8]{index=8} :contentReference[oaicite:9]{index=9}

## Endpoints iniciales propuestos

### `POST /api/verifications/scan-1`
Procesa el primer escaneo.

#### Request conceptual
- valor escaneado
- contexto del operador
- contexto operativo si aplica

#### Response conceptual
- parte identificada
- configuración requerida
- estado del flujo
- mensaje al operador
- error si no se encontró la parte

### `POST /api/verifications/scan-2`
Procesa el segundo escaneo.

#### Request conceptual
- referencia al contexto del primer escaneo o datos equivalentes
- valor de etiqueta completa escaneada
- contexto del operador

#### Response conceptual
- resultado de verificación
- detalle de coincidencia o discrepancia
- estado final del flujo
- instrucciones operativas si aplica

### `GET /api/verifications/{verificationId}`
Consulta una verificación específica, si se decide persistirla como recurso consultable.

## Decisiones pendientes
- cómo se vinculan scan 1 y scan 2
- si el contexto del primer escaneo se guarda en backend, frontend o ambos
- si el backend devuelve discrepancias por campo
- si existe endpoint de reinicio o cancelación
- si se exponen verificaciones históricas desde primera versión

---

# 5. Módulo API de Packing Lists

## Responsabilidad
Permitir creación, consulta, operación colaborativa, cierre, reapertura y exportación de packing lists.

## Justificación
La propuesta funcional inicial define creación o unión por número, registro de líneas a partir de verificaciones correctas, cierre, monitoreo, exportación y posible reapertura. :contentReference[oaicite:10]{index=10} :contentReference[oaicite:11]{index=11}

## Endpoints iniciales propuestos

### `POST /api/packing-lists/open-or-join`
Crea un packing list o une al operador a uno existente a partir del número ingresado.

#### Request conceptual
- número de packing list

#### Response conceptual
- packing list resultante
- estado actual
- información operativa mínima
- indicador de si fue creado o reutilizado

### `GET /api/packing-lists/{packingListId}`
Obtiene detalle de un packing list.

### `GET /api/packing-lists/{packingListId}/lines`
Obtiene líneas asociadas al packing list.

### `POST /api/packing-lists/{packingListId}/lines`
Agrega una línea al packing list.

#### Observación
La línea probablemente se origine a partir de una verificación correcta, pero esa relación exacta aún debe definirse.

### `DELETE /api/packing-lists/{packingListId}/lines/{lineId}`
Elimina una línea registrada.

### `POST /api/packing-lists/{packingListId}/close`
Cierra un packing list.

### `POST /api/packing-lists/{packingListId}/reopen`
Reabre un packing list, si la funcionalidad se implementa.

### `GET /api/packing-lists/{packingListId}/operators`
Obtiene operadores activos, si se materializa como endpoint separado.

### `GET /api/packing-lists/{packingListId}/export`
Exporta el packing list.

## Decisiones pendientes
- si la apertura/unión debe resolverse por número o por id interno
- si las líneas se agregan por verificación o por payload directo
- cómo manejar concurrencia
- si el monitoreo en tiempo real se resuelve por polling o tiempo real real
- formato exacto de exportación

---

# 6. Módulo API de Usuarios y Roles

## Responsabilidad
Gestionar usuarios del sistema y la asignación de roles.

## Justificación
La propuesta funcional inicial contempla gestión de usuarios y roles Operador, Supervisor y Administrador. :contentReference[oaicite:12]{index=12}

## Endpoints iniciales propuestos

### `GET /api/users`
Lista usuarios.

### `GET /api/users/{userId}`
Obtiene detalle de usuario.

### `POST /api/users`
Crea usuario.

### `PUT /api/users/{userId}`
Actualiza usuario.

### `DELETE /api/users/{userId}`
Desactiva o elimina usuario según la estrategia que se defina.

### `GET /api/roles`
Obtiene catálogo de roles disponibles.

## Decisiones pendientes
- si el alta de usuario incluye credenciales
- estrategia de autenticación
- si los roles son catálogo fijo o administrable
- si existirá endpoint de login separado
- política de bloqueo o desactivación

---

# 7. Módulo API de Auditoría

## Responsabilidad
Exponer información auditable para consulta administrativa o trazabilidad operativa.

## Justificación
La propuesta inicial menciona auditoría e historial como capacidades del sistema. :contentReference[oaicite:13]{index=13} :contentReference[oaicite:14]{index=14} :contentReference[oaicite:15]{index=15}

## Endpoints iniciales propuestos

### `GET /api/audit-logs`
Lista eventos auditables.

### `GET /api/audit-logs/{auditLogId}`
Obtiene detalle de un evento auditable.

### `GET /api/excel-uploads/{uploadId}/audit`
Consulta auditoría relacionada con una carga, si se decide exponer por subrecurso.

## Decisiones pendientes
- eventos exactos auditables
- filtros por fecha, usuario, entidad y tipo de evento
- nivel de detalle de cada evento
- retención de información

---

# 8. Módulo API de Configuración General

## Responsabilidad
Permitir consulta y actualización de parámetros generales del sistema.

## Justificación
La propuesta inicial contempla configuración general del sistema. :contentReference[oaicite:16]{index=16}

## Endpoints iniciales propuestos

### `GET /api/system-configuration`
Obtiene configuración general.

### `PUT /api/system-configuration`
Actualiza configuración general.

## Decisiones pendientes
- qué parámetros existirán
- si la configuración será única o versionada
- si se expondrá completa o por secciones
- qué roles pueden modificarla

---

## Contratos transversales probables

Estos contratos todavía no están formalmente definidos, pero son altamente probables:

### Contrato de error de validación
Especialmente necesario para:
- carga de Excel
- alta y edición de partes
- procesos de verificación

### Contrato de error funcional
Necesario para expresar errores de negocio controlados:
- parte no encontrada
- packing list cerrado
- usuario sin permisos
- línea no eliminable
- estructura de archivo inválida

### Contrato de paginación o consulta
Probable para:
- catálogo de partes
- usuarios
- historial de cargas
- auditoría

### Contrato de exportación
Necesario si el sistema expone exportación de packing lists o reportes.

---

## Prioridad de formalización de contratos

La prioridad actual recomendada es:

### Prioridad 1
- `POST /api/excel-uploads`
- `GET /api/excel-uploads`
- `GET /api/excel-uploads/{uploadId}`

### Prioridad 2
- `GET /api/parts`
- `POST /api/parts`
- `PUT /api/parts/{partId}`

### Prioridad 3
- `POST /api/verifications/scan-1`
- `POST /api/verifications/scan-2`

### Prioridad 4
- endpoints de packing lists

### Prioridad 5
- usuarios, auditoría y configuración general

---

## Reglas aún pendientes

Las siguientes decisiones siguen abiertas y afectarán la forma final de los contratos:

- autenticación y autorización
- convención final de respuestas
- convención final de errores
- versionado de API
- estrategia de ids públicos vs internos
- paginación y filtros
- granularidad de resultados de carga
- estrategia de contexto entre escaneo 1 y escaneo 2
- estrategia de concurrencia para packing lists
- estrategia de exportación

---

## Regla de evolución

Este documento debe actualizarse cuando ocurra cualquiera de estas situaciones:

- se cierre el contrato real de un endpoint
- se implemente un módulo y se necesiten payloads definitivos
- se defina la convención de errores
- se adopte autenticación formal
- se introduzca versionado
- se elimine, cambie o consolide un endpoint inicialmente propuesto

---

## Historial

### Versión inicial
- Se documenta la estructura inicial de contratos API por módulo.
- Se fija Carga de Excel como primer módulo con prioridad de formalización.
- Se definen endpoints iniciales propuestos sin fijar aún payloads definitivos.
- Se deja explícito qué aspectos del contrato siguen pendientes de decisión.