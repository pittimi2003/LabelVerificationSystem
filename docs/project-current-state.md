# Estado Actual del Proyecto

## Propósito de este documento

Este documento es la referencia del estado actual del proyecto.

Su propósito es mantener un registro explícito y continuamente actualizado de:

- en qué punto se encuentra el proyecto
- qué decisiones ya están fijadas
- qué se está usando como base activa
- qué todavía no existe
- qué se ha añadido o cambiado con el tiempo

Este documento debe actualizarse cada vez que se introduzca un cambio relevante a nivel arquitectónico, funcional, estructural o de implementación.

El objetivo es conservar el contexto del proyecto en todo momento y evitar suposiciones, desviaciones no documentadas o interpretaciones inventadas por cualquier persona o agente que trabaje sobre la solución.

---

## Estado actual

**Estado del proyecto:** Construcción inicial

El proyecto se considera oficialmente iniciado.

La solución actual es la base activa de trabajo.  
La UI existente se utilizará como template y shell visual.  
La estructura arquitectónica del backend se mantiene y no será modificada como decisión fundacional.

La funcionalidad real de negocio comienza a construirse a partir de esta base.

---

## Línea base funcional de referencia

La referencia funcional inicial del sistema es el documento de propuesta del proyecto.

Dicha propuesta define el alcance inicial esperado de la solución, incluyendo:

- verificación de etiquetas mediante un proceso de dos escaneos
- gestión del catálogo de partes
- carga y procesamiento de Excel
- gestión colaborativa de packing lists
- administración de usuarios y roles
- capacidades de auditoría
- operación en planta mediante un sistema web
- frontend basado en Blazor WebAssembly
- backend basado en API .NET
- uso inicial de SQLite
- despliegue sobre infraestructura IIS local :contentReference[oaicite:0]{index=0} :contentReference[oaicite:1]{index=1} :contentReference[oaicite:2]{index=2}

La propuesta es la **línea base funcional inicial**, pero no es inmutable.  
Podrá refinarse, aclararse, ampliarse o ajustarse durante la implementación, siempre que dichos cambios queden documentados de forma explícita.

---

## Enfoque activo de construcción

El proyecto avanzará con el siguiente enfoque:

### Frontend
La UI actual se utiliza como:

- template visual
- shell de navegación
- base de layout
- contenedor de renderizado para la nueva funcionalidad

Los nuevos módulos de negocio se renderizarán dentro de la estructura existente de layout y cuerpo de aplicación.

La base actual de frontend podrá ser **refactorizada o reemplazada parcialmente** si alguna de sus partes se convierte en un obstáculo para la implementación real.

### Backend
La estructura arquitectónica del backend ya presente en la solución se conserva como base de trabajo.

La arquitectura en capas no debe reestructurarse como parte del trabajo normal de implementación.

La funcionalidad de negocio se añadirá progresivamente dentro de esa base arquitectónica existente.

---

## Qué existe hoy

En el momento de redactar este documento, el proyecto cuenta con:

- una estructura de solución existente que servirá como base activa
- un shell UI frontend que se utilizará como template
- una dirección establecida hacia Blazor WebAssembly en frontend
- una estructura arquitectónica backend preservada
- una propuesta funcional inicial que define el problema a resolver y el alcance esperado de la solución :contentReference[oaicite:3]{index=3} :contentReference[oaicite:4]{index=4}

Lo que existe actualmente debe entenderse principalmente como **base, estructura y punto de partida**, no como una implementación de negocio ya completada.

---

## Qué no existe todavía

Los siguientes artefactos **todavía no están formalmente definidos** en esta etapa:

- contratos API
- flujos funcionales detallados
- modelo de datos formal
- reglas de negocio finalizadas a nivel de detalle de implementación
- contratos definitivos de integración entre UI y backend
- glosario de dominio finalizado

Estos artefactos se crearán como parte del proceso real de construcción del proyecto.

No faltan por descuido; forman parte del trabajo que comienza ahora.

---

## Módulos iniciales oficiales

Con base en la propuesta, el proyecto parte con los siguientes módulos funcionales esperados:

- carga y procesamiento de Excel
- gestión de partes
- verificación de etiquetas
- packing lists
- administración de usuarios y roles
- auditoría / historial
- configuración general del sistema :contentReference[oaicite:5]{index=5} :contentReference[oaicite:6]{index=6}

Estos módulos podrán refinarse posteriormente en submódulos o fases de implementación.

---

## Primer objetivo de implementación

El primer módulo real que se construirá es:

**Carga de Excel**

Este es el primer objetivo funcional de construcción del proyecto.

La decisión es intencional: los contratos, flujos y modelo de datos empezarán a formalizarse durante la implementación real, comenzando por la capacidad de carga de Excel.

---

## Línea base de infraestructura

La línea base de infraestructura actualmente prevista es:

- Frontend: Blazor WebAssembly
- Backend: API .NET
- Base de datos: inicialmente SQLite
- Hosting: infraestructura local sobre IIS :contentReference[oaicite:7]{index=7}

Esta línea base se mantiene activa salvo que sea modificada y documentada explícitamente.

---

## Riesgos y áreas abiertas en esta etapa

En esta etapa, las siguientes áreas permanecen abiertas y deberán aclararse progresivamente durante la implementación:

- definición exacta de contratos API
- definición formal del modelo de datos
- definición detallada de flujos funcionales
- reglas exactas de negocio por módulo
- detalles de integración con lectoras / escáneres
- manejo de concurrencia para trabajo colaborativo en packing lists
- reglas de validación de estructura y contenido para la carga de Excel :contentReference[oaicite:8]{index=8}

Estas áreas abiertas son esperables para la fase actual del proyecto.

---

## Regla de documentación

Este documento no es estático.

Debe actualizarse siempre que ocurra una o más de las siguientes situaciones:

- se inicia la implementación de un módulo
- se toma una decisión arquitectónica relevante
- un flujo antes indefinido pasa a estar definido
- se crea o modifica un contrato
- se introduce o modifica un modelo de datos
- una suposición funcional pasa a ser una regla confirmada
- parte del shell UI heredado es reemplazado, refactorizado o descartado
- un hito cambia el estado práctico del proyecto

Este documento funciona como un **registro del estado actual con conciencia de línea temporal**, para que el equipo pueda entender siempre:

- qué tenía el proyecto en un momento dado
- qué cambió
- cómo evolucionó
- cuál es la verdad vigente

---


## Avance implementado: Carga de Excel v1 (backend mínimo)

Se implementó el primer corte real del backend del módulo **Carga de Excel** con alcance mínimo funcional:

- endpoint `POST /api/excel-uploads`
- endpoint `GET /api/excel-uploads`
- endpoint `GET /api/excel-uploads/{id}`
- lectura de una sola hoja de Excel
- validación de encabezados mínimos obligatorios
- procesamiento fila por fila con carga parcial
- inserción de nuevas partes únicamente
- rechazo de filas inválidas o duplicadas por `Part Number`
- respuesta con resumen y errores por fila
- almacenamiento del archivo original
- registro de historial básico de carga (`ExcelUpload`)
- estrategia técnica provisional de inicialización de base de datos con `EnsureCreated()` (pendiente migrar a esquema formal de migraciones)

No se implementó en esta versión:

- actualización de partes existentes
- cálculo de tipo de etiqueta
- cálculo de configuración de lectura
- procesamiento en background
- persistencia detallada de errores por fila en base de datos


## Avance implementado: Carga de Excel v1 (frontend mínimo)

Se implementó una pantalla frontend mínima para operar el módulo **Carga de Excel v1** sobre la API ya existente, manteniendo el shell actual de la aplicación:

- página de carga en `/excel-uploads`
- selección de archivo y envío por `POST /api/excel-uploads`
- indicador de envío en progreso y mensajes de error/estado
- visualización de resumen de resultado de carga
- visualización de errores por fila (`rowNumber`, `partNumber`, `error`)
- historial básico consumiendo `GET /api/excel-uploads`
- acceso del módulo desde el menú actual


## Historial de cambios

### Versión inicial
- Se establece el estado del proyecto como **Construcción inicial**
- Se define la propuesta como línea base funcional inicial
- Se establece la solución actual como base activa de trabajo
- Se define la UI existente como template / shell para la nueva funcionalidad
- Se declara estable la estructura arquitectónica del backend como base de implementación
- Se confirma que todavía no existen formalmente contratos, flujos ni modelo de datos
- Se confirma que dichos artefactos se crearán durante la implementación
- Se fija como primer objetivo de implementación el módulo de **Carga de Excel**
