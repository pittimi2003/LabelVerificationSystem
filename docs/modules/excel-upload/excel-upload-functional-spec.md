# Especificación Funcional del Módulo de Carga de Excel

## Propósito de este documento

Este documento define la especificación funcional inicial del módulo de Carga de Excel.

Su objetivo es dejar explícito:

- qué problema resuelve el módulo
- quién puede usarlo
- cómo debe comportarse
- qué valida
- qué guarda
- qué muestra al usuario
- qué queda pendiente de definición

Este documento describe la primera versión funcional del módulo y debe evolucionar conforme se cierre más detalle de negocio y de implementación.

---

## Estado del documento

**Estado:** Inicial  
**Módulo:** Carga de Excel  
**Fase del proyecto:** Construcción inicial

Este es el primer módulo funcional real a construir dentro del sistema.

---

## Objetivo del módulo

Permitir que usuarios autorizados carguen un archivo Excel con información de partes para registrar nuevas partes en el sistema.

En esta primera versión, la carga de Excel está orientada a:

- **crear registros nuevos**
- **descartar duplicados**
- **continuar procesando aunque existan errores por fila**
- **mostrar al usuario el resultado de la carga y el detalle de errores**

La actualización de registros ya existentes **no forma parte de este módulo**.  
La actualización se resolverá posteriormente mediante el CRUD del catálogo de partes.

---

## Alcance funcional de esta versión

Esta versión del módulo incluye:

- selección de archivo Excel
- validación de archivo recibido
- lectura de una sola hoja
- validación de columnas requeridas
- procesamiento fila por fila
- registro parcial de filas válidas
- descarte de filas inválidas o duplicadas
- generación de detalle de errores por fila
- almacenamiento del archivo original
- visualización de historial de cargas desde la primera versión

Esta versión no incluye todavía:

- actualización masiva de registros existentes
- reglas definitivas de cálculo de tipo de etiqueta
- reglas definitivas de cálculo de configuración de lectura
- definición cerrada de todos los campos del modelo de parte

---

## Actores autorizados

Pueden ejecutar la carga de Excel los siguientes roles:

- Administrador
- Supervisor

---

## Archivo de entrada

## Tipo de archivo esperado
Archivo Excel compatible con el formato admitido por el sistema.

## Hoja esperada
- Por el momento, el sistema trabajará con **una sola hoja obligatoria**.
- El nombre de la hoja **no importa**.
- Se procesará la hoja visible esperada del archivo recibido.

## Referencia actual
El archivo de referencia actual utilizado para esta primera definición funcional es `matrix_service_parts.xlsx`.

A nivel visible, el archivo actual contiene una hoja con estructura tabular y encabezados como:

- `Part Number`
- `Minghua description`
- `CADUCIDAD`
- `CCO`
- `Certification EAC`
- `4 FIRST NUMERS`

Además, en la referencia actual también aparecen otras columnas visibles como:

- `AI Level`
- `Model`
- `STD PACK`
- `Minghua Process`
- `Internal PN`
- `Codigo de Empaque`
- `SEPARADOR O CAJA INDIVIDUAL`
- `BOLSA PLASTICA`
- `FOAM SHEET`
- `STD x PALLET`
- `TARIMA`
- `Medida`
- `PESO (KG)`
- `WI PISO`

Estas columnas visibles sirven como referencia de entrada actual, pero la definición funcional mínima de esta versión se centra primero en las columnas confirmadas por negocio.

---

## Columnas mínimas requeridas

Para esta primera versión, las columnas mínimas confirmadas son:

- `Part Number`
- `Model`
- `Minghua description`
- `CADUCIDAD`
- `CCO`
- `Certification EAC`
- `4 FIRST NUMERS`

## Identificador principal de la parte
La columna que identifica funcionalmente a una parte es:

- `Part Number`

## Columnas opcionales
Sí, existen columnas opcionales además de las obligatorias.

En esta etapa todavía no queda cerrada la lista completa de columnas opcionales ni su impacto funcional definitivo.

---

## Reglas funcionales principales

### Regla 1. La carga es para creación de registros nuevos
El módulo de carga de Excel solo crea nuevas partes.

No debe actualizar registros existentes.

### Regla 2. La carga es parcial
Si algunas filas son válidas y otras no, el sistema debe:

- registrar las filas válidas
- descartar las inválidas
- continuar con el procesamiento
- mostrar al usuario qué se registró y qué no

La carga **no es todo o nada**.

### Regla 3. Los duplicados no bloquean toda la carga
Si se detectan duplicados, el sistema no debe rechazar el archivo completo.

Debe:

- descartar la fila duplicada
- registrar el motivo en el log de resultado
- continuar con el resto del archivo

### Regla 4. Los errores se manejan por fila
Errores de estructura de fila, datos incorrectos, duplicados o problemas de validación deben tratarse por fila.

### Regla 5. El resultado debe ser visible para el usuario
Al finalizar, el usuario debe ver:

- total de filas registradas
- detalle de errores por fila

### Regla 6. El archivo original debe conservarse
El archivo Excel original cargado debe guardarse.

---

## Flujo principal

### Flujo principal de carga

1. El usuario autorizado accede al módulo de Carga de Excel.
2. El usuario selecciona un archivo Excel.
3. El sistema recibe el archivo.
4. El sistema valida que el archivo sea procesable como Excel.
5. El sistema localiza la hoja de trabajo que debe procesarse.
6. El sistema detecta la fila de encabezados reales buscando la mejor coincidencia con los encabezados obligatorios.
7. El sistema valida la existencia de las columnas requeridas sobre la fila detectada.
8. El sistema comienza a procesar las filas de datos desde la fila siguiente al encabezado detectado.
9. Para cada fila:
   - valida la estructura mínima requerida
   - valida tipos y contenido
   - detecta si la fila es duplicada
   - si la fila es válida, registra la parte
   - si la fila es inválida, genera error por fila y continúa
10. El sistema guarda el archivo original cargado.
11. El sistema genera el resultado final de la carga.
12. El sistema muestra al usuario:
   - cuántas filas fueron registradas
   - el detalle de errores por fila
13. El sistema registra la auditoría de la carga.
14. La carga queda visible en el historial de cargas.

### Comportamiento de experiencia de usuario (frontend v1.3)

Para la pantalla `/excel-uploads`, la experiencia de carga debe cumplir:

- la selección de archivo se presenta como dropzone principal de ancho completo
- la dropzone permite arrastrar/soltar y también clic para selección manual
- no se expone un input nativo de archivo visible en la interfaz
- al seleccionar archivo se muestra estado "listo para cargar", nombre y tamaño
- el usuario puede quitar el archivo para dejar el formulario limpio antes de cargar
- el botón `Cargar` se mantiene como acción final del bloque
- al terminar una carga se limpia el estado de selección y se refresca historial automáticamente

### Comportamiento de experiencia de usuario (frontend v1.4)

En el drawer de detalle de carga (`/excel-uploads`) se mantiene la estructura funcional existente (vista general + vista por fila), y se refuerza usabilidad para cargas grandes:

- en **vista por fila** se habilitan filtros combinables por:
  - `PartNumber`
  - `Model`
  - `Status`
- se agrega acción explícita **Limpiar filtros** para volver al estado inicial sin cerrar el drawer
- se agrega paginación local en frontend con tamaños de página:
  - 10
  - 20
  - 50
- el `Status` por fila se presenta con chips consistentes por color/estilo para escaneo rápido
- en **vista general** se mejora la agrupación visual de métricas
- la agrupación de errores por `ErrorCode` se presenta destacada
- cuando no hay errores por fila se muestra un estado vacío limpio (mensaje positivo, no tabla vacía)

---

## Validaciones del archivo

## Validación de estructura general
El sistema debe validar que el archivo recibido:

- sea un archivo Excel aceptado por el sistema
- contenga una hoja procesable
- contenga las columnas requeridas

## Validación de columnas obligatorias
Si falta una columna obligatoria en el encabezado:

- se rechaza toda la carga como archivo inválido
- no se procesa ninguna fila
- el usuario recibe el listado exacto de columnas faltantes, la fila tomada como encabezado y los encabezados realmente detectados en esa fila

La validación de encabezados se realiza sobre una forma canónica normalizada para reducir falsos negativos por variaciones de formato:

- trim en extremos
- reemplazo de espacios no separables por espacios comunes
- colapso de espacios internos múltiples en un solo espacio
- comparación case-insensitive

## Validación por fila
Si una fila viene vacía, corrupta o con tipos de dato incorrectos:

- se marca error por fila
- se continúa con el resto del archivo

### Reglas de parseo de tipos cerradas
- `CADUCIDAD`:
  - `NA` o vacío => `null`
  - entero válido => `int`
  - otro valor => fila rechazada
- `Certification EAC`:
  - `YES` => `true`
  - `NO` => `false`
  - `NA` o vacío => `null`
  - otro valor => fila rechazada
- `4 FIRST NUMERS`:
  - obligatorio
  - debe parsear a entero
  - si no parsea => fila rechazada

## Duplicados dentro del Excel
Si una fila se considera duplicada dentro del mismo archivo:

- la fila se descarta
- se registra error
- se agrega al log de resultado
- se muestra al usuario el motivo

---

## Reglas de duplicidad

## Duplicidad confirmada
A nivel funcional, la duplicidad debe reportarse al usuario y la fila afectada no debe registrarse.

## Criterio implementado para v1
- Duplicado contra sistema: mismo `Part Number`.
- Duplicado dentro del archivo: mismo `Part Number` repetido en la carga actual.
- En ambos casos, la fila se rechaza y se reporta error por fila.

## Alcance v1 cerrado
- el módulo es solo para creación
- `Part Number` es el identificador principal de la parte

## Criterio de duplicidad vigente para v1
- El criterio operativo vigente es `Part Number`.
- Esta regla se considera cerrada para la versión v1 del módulo.
- Cualquier cambio futuro deberá documentarse explícitamente antes de implementarse.

---

## Reglas de persistencia

## Registros válidos
Toda fila válida debe registrarse como nueva parte en base de datos y como resultado `Inserted` en `ExcelUploadRowResult`.

## Registros inválidos
Toda fila inválida debe rechazarse sin interrumpir el resto del procesamiento y registrarse como `Rejected` en `ExcelUploadRowResult` con `ErrorCode` y `ErrorMessage`.

## Actualización
La actualización de registros existentes no se hará en este módulo.

## Archivo original
El archivo original cargado debe almacenarse.

## Detalle de errores por fila
El detalle de errores por fila:

- debe mostrarse al usuario en el momento de la carga
- debe persistirse por fila en `ExcelUploadRowResult` para trazabilidad histórica

---

## Cálculo de tipo de etiqueta y configuración

La propuesta funcional del proyecto establece que la carga de Excel incluirá:

- cálculo automático del tipo de etiqueta
- cálculo automático de la configuración de lectura

Sin embargo, en este momento:

- la regla exacta no está definida
- se sabe que saldrá de una concatenación de columnas
- luego se asignará una cadena resultante

## Estado actual
**Pendiente de definición**

En esta primera especificación no debe cerrarse ninguna regla inventada para ese cálculo.

Cuando la regla exista, este documento deberá actualizarse.

---

## Auditoría

Cada carga de Excel debe auditar como mínimo:

- usuario que ejecutó la carga
- fecha y hora
- nombre del archivo
- total de filas procesadas
- cantidad de filas registradas
- cantidad de filas rechazadas
- resultado final de la carga

---

## Resultado visible para el usuario

Al finalizar la carga, el sistema debe mostrar como mínimo:

- total de filas registradas
- detalle del listado de errores por fila

## Resultado deseable ampliado
Aunque hoy no quedó exigido como obligatorio mínimo, es recomendable que la respuesta funcional también contemple:

- total de filas leídas
- total de filas válidas
- total de filas inválidas
- total de filas duplicadas
- total de filas rechazadas

Esto mejora trazabilidad y comprensión del resultado.

---

## Historial de cargas

El historial de cargas debe existir desde la primera versión del módulo.

Como mínimo debería permitir ver:

- fecha y hora
- usuario
- nombre del archivo
- resultado general
- total de filas registradas
- total de filas rechazadas

El detalle exacto del historial podrá refinarse más adelante.

---

## Pantalla inicial del módulo

La primera versión de UI del módulo puede ser simple.

## Ubicación inicial
Se indicó como punto de entrada inicial: **Home**

## Contenido mínimo de la pantalla
- selector de archivo
- acción de carga
- resumen del resultado
- detalle de errores por fila
- acceso a historial de cargas

---

## Errores funcionales esperables

El módulo debe ser capaz de reportar, como mínimo, estos tipos de error:

- archivo no válido
- hoja no procesable
- columnas requeridas faltantes
- fila vacía
- fila corrupta
- tipo de dato incorrecto
- fila duplicada
- error de registro en base de datos
- error de procesamiento interno

Cada error debe informar motivo suficiente para que el usuario entienda por qué una fila no fue registrada.

---

## Reglas pendientes de definición

Los siguientes puntos quedan expresamente pendientes y no deben asumirse como ya cerrados:

- criterio definitivo de duplicidad
- lista exacta de columnas opcionales
- comportamiento exacto cuando falta una columna obligatoria a nivel global
- definición completa de todos los campos de una parte
- reglas exactas de cálculo de tipo de etiqueta
- reglas exactas de cálculo de configuración de lectura
- formato exacto del historial de cargas
- ubicación definitiva del archivo original almacenado
- política exacta de almacenamiento físico del archivo
- qué campos del Excel se persistirán en la primera versión del modelo

---

## Decisiones confirmadas en esta versión

- La carga de Excel es para **crear partes nuevas**.
- La actualización se resolverá por CRUD posterior.
- Se procesará **una sola hoja obligatoria**.
- El nombre de la hoja **no importa**.
- `Part Number` es el identificador principal de la parte.
- Los errores se manejan **por fila**.
- Los duplicados se descartan y se reportan al usuario.
- La carga es **parcial**, no transaccional total.
- El detalle de errores por fila se muestra **al momento**.
- El archivo original **sí se guarda**.
- El historial de cargas existe **desde la primera versión**.
- El cálculo de tipo de etiqueta y configuración queda **pendiente de definición**.

---

## Historial

### Versión inicial
- Se define la primera especificación funcional del módulo de Carga de Excel.
- Se establece que el módulo crea nuevas partes y no actualiza existentes.
- Se fija la carga parcial con tratamiento de errores por fila.
- Se fija la gestión de duplicados con descarte y log visible para usuario.
- Se establece el guardado del archivo original.
- Se establece historial de cargas desde la primera versión.
- Se dejan expresamente marcados los puntos todavía pendientes de definición.

## Avance implementado: UX/UI de Carga de Excel v1.2 (frontend)

Se implementó una iteración de mejora de experiencia de uso del módulo en `/excel-uploads` sin cambiar reglas de negocio del procesamiento:

- selección de archivo migrada a `MudFileUpload` para consistencia visual con el stack MudBlazor
- mensajería de estado migrada de alertas embebidas a `Snackbar` (éxito, error, warning, info)
- limpieza post-carga del control de archivo para permitir cargas consecutivas inmediatas
- refresco automático del historial al terminar cada carga (se mantiene refresco manual)
- columna de acción al inicio del grid de historial para abrir inspección de detalle
- panel lateral derecho (drawer) para detalle histórico de una carga
- selector de vista dentro del panel:
  - **Vista General**: resumen + métricas + agrupación de errores
  - **Vista por fila**: tabla con `rowNumber`, `partNumber`, `model`, `status`, `errorCode`, `errorMessage`

Esta iteración mantiene las reglas funcionales cerradas de v1/v1.1 (solo inserción de nuevas partes, carga parcial, errores por fila, una sola hoja, duplicado por `Part Number`, `Model` obligatorio y validación robusta de encabezados).
