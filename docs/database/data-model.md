# Modelo de Datos

## Propósito de este documento

Este documento define el modelo de datos lógico inicial del proyecto.

Su objetivo es:

- identificar las entidades principales del sistema
- establecer sus relaciones conceptuales
- servir como base para la construcción del backend
- alinear dominio, flujos funcionales, persistencia y contratos API
- evitar decisiones improvisadas o estructuras inconsistentes

Este documento no representa todavía el diseño físico final de base de datos.

Mientras no existan reglas cerradas de implementación, este documento debe leerse como **modelo lógico inicial**.

---

## Estado actual del modelo

**Estado:** Inicial

El modelo de datos aún no está formalizado a nivel físico ni definitivo.

En esta etapa se documentan:

- entidades principales
- propósito de cada entidad
- relaciones esperadas
- decisiones todavía pendientes

La propuesta funcional inicial ya establece una base de tablas esperadas para el sistema:

- Parts
- Configurations
- Users
- ExcelUploads
- PackingLists
- PackingListLines
- Verifications :contentReference[oaicite:1]{index=1}

Ese conjunto se toma como punto de partida del modelo lógico actual.

---

## Principios del modelo

### 1. El modelo nace desde la funcionalidad real
Las entidades deben responder a necesidades reales del sistema y no a estructuras heredadas del template o del shell UI.

### 2. La persistencia debe soportar trazabilidad
El sistema requiere trazabilidad para cargas, verificaciones, packing lists y acciones administrativas.

### 3. El modelo debe poder evolucionar
Este modelo inicial podrá refinarse, separarse o ampliarse a medida que se definan contratos, reglas y decisiones más precisas.

### 4. No se asumen columnas definitivas todavía
Mientras no exista definición cerrada por módulo, este documento no fija aún el detalle final de todos los campos físicos.

---

## Entidades principales

# 1. Part

## Propósito
Representa una parte oficial registrada en el sistema.

Es la entidad base para:

- identificación por número de parte
- validación de etiquetas
- cálculo de configuraciones
- uso dentro del catálogo de partes
- soporte a operaciones de packing list

## Origen funcional
La propuesta inicial define gestión de partes, visualización del catálogo, carga por Excel y uso de partes como base para validaciones. :contentReference[oaicite:2]{index=2}

## Atributos mínimos implementados en v1.1 de Carga de Excel
- `Id`
- `PartNumber`
- `Model`
- `MinghuaDescription`
- `Caducidad` (`int?`)
- `Cco`
- `CertificationEac` (`bool?`)
- `FirstFourNumbers` (`int`)
- `CreatedByExcelUploadId` (`Guid?`)
- `CreatedAtUtc`

## Regla de unicidad implementada en v1
- `PartNumber` es único (índice único).

## Relaciones esperadas
- una parte puede estar asociada a una o varias configuraciones
- una parte puede participar en múltiples verificaciones
- una parte puede originarse a partir de una carga de Excel
- una parte puede vincularse explícitamente a la carga que la creó mediante `CreatedByExcelUploadId`
- una parte puede aparecer en múltiples líneas de packing list

---

# 2. Configuration

## Propósito
Representa la configuración de lectura asociada a una parte o a una necesidad operativa del proceso de verificación.

## Origen funcional
La propuesta inicial contempla cálculo automático de configuración de lectura y uso de dicha configuración durante el proceso de escaneo. :contentReference[oaicite:3]{index=3} :contentReference[oaicite:4]{index=4}

## Atributos lógicos esperados
Todavía pendientes de formalización, pero se espera que una configuración represente al menos:

- identificador interno
- tipo o nombre de configuración
- parámetros de lectura
- relación con parte o tipo de etiqueta
- estado o vigencia

## Relaciones esperadas
- una configuración puede pertenecer a una parte
- una configuración puede estar asociada a un tipo de etiqueta
- una configuración puede ser usada en verificaciones

## Decisión pendiente
Debe definirse si una configuración es:
- propia de cada parte
- reutilizable entre múltiples partes
- dependiente del tipo de etiqueta
- combinación de varios factores

---

# 3. User

## Propósito
Representa una cuenta de usuario del sistema.

## Origen funcional
La propuesta inicial contempla gestión de usuarios, roles y administración del sistema. :contentReference[oaicite:5]{index=5}

## Atributos lógicos esperados
- identificador interno
- identidad del usuario
- datos de acceso
- rol
- estado del usuario
- información de auditoría básica

## Relaciones esperadas
- un usuario puede ejecutar múltiples cargas de Excel
- un usuario puede realizar múltiples verificaciones
- un usuario puede operar sobre múltiples packing lists
- un usuario puede generar eventos auditables

## Decisión pendiente
Debe definirse si el modelo de roles será:
- simple por campo directo
- basado en tabla de roles separada
- basado en claims/permisos más detallados

---

# 4. ExcelUpload

## Propósito
Representa una operación de carga de archivo Excel.

## Origen funcional
La propuesta inicial incluye carga de Excel, validación del archivo, procesamiento, almacenamiento y auditoría de cargas. :contentReference[oaicite:6]{index=6} :contentReference[oaicite:7]{index=7}

## Atributos mínimos implementados en v1 de Carga de Excel
- `Id`
- `OriginalFileName`
- `StoredFilePath`
- `UploadedAtUtc`
- `Status`
- `TotalRows`
- `InsertedRows`
- `RejectedRows`

## Relaciones esperadas
- una carga pertenece a un usuario
- una carga puede afectar múltiples partes
- una carga puede generar eventos auditables

## Estado v1.1
- El archivo físico original sí se almacena.
- Se almacena resumen de la carga en `ExcelUpload`.
- Se persiste detalle por fila en `ExcelUploadRowResult` con estado y error cuando aplica.
- El estado se maneja de forma básica (`Processed` / `ProcessedWithErrors`).

---

# 5. Verification

## Propósito
Representa una ejecución del proceso de verificación de etiqueta.

## Origen funcional
La propuesta inicial define un proceso de dos escaneos con resultado correcto o erróneo. :contentReference[oaicite:8]{index=8} :contentReference[oaicite:9]{index=9}

## Atributos lógicos esperados
- identificador interno
- usuario operador
- parte identificada
- datos del primer escaneo
- datos del segundo escaneo
- resultado de verificación
- fecha y hora
- contexto operativo
- detalle de errores o discrepancias

## Relaciones esperadas
- una verificación pertenece a una parte
- una verificación pertenece a un usuario
- una verificación puede asociarse a una configuración usada
- una verificación puede asociarse a una línea de packing list

## Decisiones pendientes
Debe definirse:
- si una verificación guarda ambos escaneos de forma separada
- si se registran intentos fallidos parciales
- si se guardan diferencias campo a campo
- si una verificación existe aunque no termine correctamente

---

# 6. PackingList

## Propósito
Representa una agrupación operativa de líneas registradas durante un proceso de trabajo.

## Origen funcional
La propuesta inicial contempla creación o unión a un packing list, estado abierto, cierre, posible reapertura y exportación. :contentReference[oaicite:10]{index=10}

## Atributos lógicos esperados
- identificador interno
- número de packing list
- estado
- fecha de creación
- fecha de cierre
- usuarios involucrados o contexto operativo
- información de auditoría básica

## Relaciones esperadas
- un packing list tiene muchas líneas
- un packing list puede ser operado por múltiples usuarios
- un packing list puede generar eventos auditables

## Decisiones pendientes
Debe definirse:
- estados exactos permitidos
- reglas de reapertura
- si el número de packing list es único globalmente
- si existe ownership principal o solo colaboración

---

# 7. PackingListLine

## Propósito
Representa un registro individual agregado a un packing list como resultado de una verificación correcta.

## Origen funcional
La propuesta inicial indica que cada etiqueta correcta se registra como línea del packing list. :contentReference[oaicite:11]{index=11}

## Atributos lógicos esperados
- identificador interno
- packing list al que pertenece
- parte asociada
- verificación origen
- usuario que la registró
- fecha y hora de registro
- datos operativos relevantes de la línea

## Relaciones esperadas
- una línea pertenece a un packing list
- una línea puede vincularse a una verificación
- una línea puede vincularse a una parte
- una línea puede vincularse al usuario que la registró

## Decisiones pendientes
Debe definirse:
- si una línea exige verificación correcta obligatoria
- si se permiten duplicados de parte
- si una línea guarda snapshot de datos o solo referencias
- qué información exacta se exportará

---

# 8. AuditLog

## Propósito
Representa un evento auditable del sistema.

## Origen funcional
La propuesta inicial menciona auditoría de cargas y modificaciones, además de historial y capacidades administrativas. :contentReference[oaicite:12]{index=12} :contentReference[oaicite:13]{index=13}

## Observación importante
La propuesta no lista explícitamente una tabla `AuditLogs`, pero funcionalmente el sistema necesita una entidad o mecanismo equivalente para trazabilidad.

Por ello, esta entidad se incorpora al modelo lógico inicial como necesidad funcional prevista.

## Atributos lógicos esperados
- identificador interno
- fecha y hora del evento
- usuario origen
- tipo de evento
- entidad afectada
- identificador de la entidad afectada
- detalle resumido del cambio o acción
- contexto técnico mínimo

## Relaciones esperadas
- un evento puede relacionarse con usuario
- un evento puede relacionarse con carga de Excel
- un evento puede relacionarse con parte
- un evento puede relacionarse con packing list
- un evento puede relacionarse con configuración

## Decisiones pendientes
Debe definirse:
- nivel de granularidad del log
- catálogo exacto de eventos
- política de retención
- formato de almacenamiento del detalle

---

## Entidades auxiliares probables

Las siguientes entidades todavía no están confirmadas como tablas separadas, pero son altamente probables según cómo evolucione el sistema.

### Role
Podría separarse si se desea gestión formal de roles y permisos.

### VerificationErrorDetail
Podría existir si se necesita persistir discrepancias específicas por campo en una verificación.

### ExcelUploadRowResult
Entidad implementada para trazabilidad detallada por fila procesada en la carga de Excel.

Atributos implementados:
- `Id`
- `ExcelUploadId`
- `RowNumber`
- `PartNumber`
- `Model`
- `Status` (`Inserted` / `Rejected`)
- `ErrorCode`
- `ErrorMessage`
- `CreatedAtUtc`

### SystemConfiguration
Podría existir para representar configuración general persistente del sistema.

Estas entidades no forman parte todavía del núcleo obligatorio del modelo actual, pero deben mantenerse en observación.

---

## Relaciones principales del modelo

## Relación: Part -> Configuration
- una parte puede tener una o varias configuraciones asociadas
- una configuración puede pertenecer a una sola parte o reutilizarse, decisión aún pendiente

## Relación: User -> ExcelUpload
- un usuario puede ejecutar muchas cargas
- una carga pertenece a un usuario

## Relación: User -> Verification
- un usuario puede ejecutar muchas verificaciones
- una verificación pertenece a un usuario

## Relación: ExcelUpload -> ExcelUploadRowResult
- una carga tiene muchos resultados por fila
- un resultado por fila pertenece a una sola carga

## Relación: ExcelUpload -> Part
- una carga puede crear múltiples partes
- una parte puede registrar la carga origen en `CreatedByExcelUploadId`

## Relación: Part -> Verification
- una parte puede aparecer en muchas verificaciones
- una verificación se asocia a una parte identificada

## Relación: PackingList -> PackingListLine
- un packing list tiene muchas líneas
- una línea pertenece a un packing list

## Relación: Verification -> PackingListLine
- una línea puede originarse a partir de una verificación correcta
- debe definirse si la relación es obligatoria o solo opcional

## Relación: User -> PackingListLine
- una línea puede registrar al usuario que la generó

## Relación: AuditLog -> entidades funcionales
- los eventos auditables podrán referirse a varias entidades del sistema

---

## Vista conceptual resumida

A nivel lógico, el sistema puede leerse así:

- `Part` es la base oficial del catálogo.
- `Configuration` soporta la lógica de lectura/validación.
- `ExcelUpload` incorpora o actualiza información del catálogo.
- `Verification` registra el proceso de verificación de etiquetas.
- `PackingList` agrupa trabajo operativo.
- `PackingListLine` materializa líneas correctas dentro de un packing list.
- `User` representa operación y administración.
- `AuditLog` asegura trazabilidad.

---

## Reglas de diseño aún pendientes

Las siguientes decisiones todavía no están cerradas y afectarán al modelo físico definitivo:

- unicidad exacta del número de parte
- versión o vigencia de configuraciones
- borrado lógico vs borrado físico
- estrategia de auditoría
- modelo de roles
- granularidad de los resultados de carga de Excel
- granularidad de errores de verificación
- concurrencia en packing lists
- estados exactos de cada entidad operativa
- campos exactos del snapshot exportable

---

## Posibles estados por entidad

Los estados siguientes son tentativos y se documentan solo como orientación funcional inicial.

### ExcelUpload
Posibles estados:
- recibido
- validando
- procesado
- procesado con errores
- rechazado

### Verification
Posibles estados o resultados:
- en proceso
- correcta
- errónea
- cancelada

### PackingList
Posibles estados:
- abierto
- cerrado
- reabierto

Estos estados no deben tratarse aún como definitivos.

---

## Regla de evolución del modelo

Este documento debe actualizarse cuando ocurra cualquiera de estas situaciones:

- se definen contratos API que fijan estructura persistente
- se implementa el primer módulo real del backend
- se cierra una regla funcional que obligue a crear o dividir entidades
- se define una estrategia real de auditoría
- se define concurrencia para packing lists
- se valida persistencia detallada de errores o resultados
- se toma una decisión sobre roles, estados o configuración general

---

## Historial

### Versión inicial
- Se establece el modelo lógico inicial del proyecto.
- Se toman como base las entidades listadas en la propuesta funcional.
- Se añade AuditLog como necesidad lógica de trazabilidad.
- Se documentan relaciones principales sin fijar aún el diseño físico final.
- Se identifican decisiones pendientes que deberán cerrarse durante la implementación.
