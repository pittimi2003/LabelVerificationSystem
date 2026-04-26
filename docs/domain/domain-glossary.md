# Glosario de Dominio

## Propósito de este documento

Este documento define el vocabulario base del dominio del proyecto.

Su objetivo es asegurar que cualquier persona o agente que trabaje en el sistema use los mismos términos con el mismo significado, evitando interpretaciones inconsistentes durante el diseño, implementación y documentación.

Este glosario es una referencia viva.  
Debe actualizarse a medida que el dominio se vaya precisando durante la construcción del sistema.

---

## Regla de uso

Cuando un término definido en este glosario aparezca en:

- documentación
- código
- contratos API
- modelo de datos
- historias técnicas
- decisiones arquitectónicas
- pruebas

debe usarse con el significado aquí establecido, salvo que una actualización formal de este documento lo cambie.

---

## Términos del dominio


### Fase 4 cerrada al 100%
Estado formal/técnico del proyecto para Bloque B donde el modelo robusto de autorización quedó operativo en runtime y validado con build, migraciones y E2E.

---

### Fase 5 pendiente
Siguiente etapa del roadmap, explícitamente fuera del alcance del cierre de Fase 4.

---

### Sistema de Verificación de Etiquetas
Sistema web orientado a operación en planta cuyo objetivo es verificar etiquetas de partes, gestionar packing lists, administrar catálogos de partes y centralizar procesos operativos relacionados.

Es el sistema global que se está construyendo.

---

### Parte
Elemento del catálogo oficial del sistema que representa una unidad identificable por su información de negocio y por los datos usados en verificación de etiquetas.

Una parte puede tener información asociada necesaria para:

- identificación
- validación
- configuración de lectura
- uso en packing lists

El detalle exacto de sus atributos será definido en el modelo de datos.

---

### Número de Parte
Identificador principal de una parte dentro del sistema.

Se usa como referencia central para localizar la información oficial de una parte y para relacionar la parte con su configuración de lectura y sus reglas de validación.

En la propuesta también aparece como **Part Number**. :contentReference[oaicite:0]{index=0}

---

### Etiqueta
Representación física o escaneable asociada a una parte.

La etiqueta contiene información que debe compararse contra los datos oficiales del sistema para determinar si es correcta o errónea.

Su estructura exacta todavía debe formalizarse.

---

### Verificación de Etiqueta
Proceso funcional mediante el cual el sistema valida si una etiqueta física coincide con la información oficial registrada para una parte.

En la línea base funcional inicial, esta verificación se realiza mediante un proceso de dos escaneos. :contentReference[oaicite:1]{index=1}

---

### Primer Escaneo
Primer paso del proceso de verificación.

Según la propuesta inicial, corresponde al escaneo del código de barras para identificar automáticamente el número de parte y validar información clave previa al segundo escaneo. :contentReference[oaicite:2]{index=2}

También puede referirse como:

- Scan 1
- Escaneo 1

La definición detallada de qué datos exactos se extraen y validan se documentará en los flujos funcionales y contratos correspondientes.

---

### Segundo Escaneo
Segundo paso del proceso de verificación.

Según la propuesta inicial, corresponde al escaneo de la etiqueta completa para leer todos sus campos y compararlos contra la información oficial del sistema. :contentReference[oaicite:3]{index=3}

También puede referirse como:

- Scan 2
- Escaneo 2

La definición exacta de campos, reglas de comparación y criterios de éxito o error se documentará posteriormente.

---

### Resultado de Verificación
Resultado final del proceso de verificación de una etiqueta.

En la línea base funcional actual, los resultados esperados son, como mínimo:

- etiqueta correcta
- etiqueta errónea :contentReference[oaicite:4]{index=4}

Más adelante podrán definirse resultados intermedios, estados técnicos o errores específicos.

---

### Configuración de Lectura
Conjunto de parámetros o instrucciones requeridas para que una lectora procese correctamente una determinada parte o una determinada etapa del flujo de verificación.

La propuesta inicial contempla cálculo y uso de configuraciones de lectura, así como cambio de configuración asociado al proceso de escaneo. :contentReference[oaicite:5]{index=5} :contentReference[oaicite:6]{index=6}

Su representación exacta todavía no está formalizada.

---

### Lectora
Dispositivo utilizado para capturar códigos o datos de etiquetas dentro del proceso operativo.

También puede referirse como:

- escáner
- scanner
- lectora de código de barras

La forma exacta en que se integra con el sistema todavía debe detallarse técnicamente.

---

### Catálogo de Partes
Conjunto de partes registradas oficialmente en el sistema.

Se usa como base para:

- localizar partes
- validar información
- calcular configuraciones
- alimentar procesos de verificación
- soportar packing lists

La propuesta inicial contempla visualización completa, alta, edición, eliminación y carga mediante Excel. :contentReference[oaicite:7]{index=7}

---

### Carga de Excel
Proceso mediante el cual se importa al sistema un archivo Excel con información de partes.

La línea base funcional inicial indica que esta carga incluye:

- validación de formato y estructura
- cálculo automático del tipo de etiqueta
- cálculo de configuración de lectura
- almacenamiento de la información en base de datos
- auditoría de la carga :contentReference[oaicite:8]{index=8}

Será el primer módulo real a construir.

---

### Archivo Excel
Archivo de entrada utilizado para cargar o actualizar información de partes en el sistema.

Su estructura exacta, columnas esperadas, validaciones y reglas de rechazo aún deben formalizarse.

---

### Tipo de Etiqueta
Clasificación o categoría de etiqueta utilizada por el sistema para determinar cómo debe interpretarse o validarse una parte.

La propuesta inicial menciona cálculo automático del tipo de etiqueta. :contentReference[oaicite:9]{index=9}

Su definición exacta todavía no ha sido formalizada.

---

### Packing List
Entidad operativa que agrupa líneas registradas por operadores durante un proceso de trabajo.

Según la propuesta inicial, un packing list puede:

- crearse si no existe
- permitir que un operador se una si ya existe
- mantenerse abierto durante la operación
- cerrarse al finalizar
- opcionalmente reabrirse
- exportarse a Excel :contentReference[oaicite:10]{index=10}

---

### Número de Packing List
Identificador usado para localizar, crear o unirse a un packing list.

En la propuesta inicial, el operador ingresa este número para determinar si el packing list ya existe o si debe crearse. :contentReference[oaicite:11]{index=11}

---

### Línea de Packing List
Registro individual agregado a un packing list como resultado de una verificación correcta.

Según la propuesta inicial, cada etiqueta correcta se registra como una línea dentro del packing list. :contentReference[oaicite:12]{index=12}

La estructura exacta de la línea todavía debe formalizarse.

---

### Estado de Packing List
Condición operativa actual de un packing list.

En la línea base funcional inicial, al menos existe el estado:

- Abierto :contentReference[oaicite:13]{index=13}

También se contempla cierre y posible reapertura, por lo que existirán estados adicionales a definir posteriormente.

---

### Operador
Usuario del sistema orientado a ejecución operativa en planta.

En la línea base funcional inicial, puede realizar acciones como:

- ingresar o unirse a packing lists
- escanear etiquetas
- agregar líneas
- eliminar líneas
- cerrar packing lists :contentReference[oaicite:14]{index=14}

---

### Supervisor
Usuario del sistema con capacidades de supervisión y administración parcial.

En la línea base funcional inicial, participa en:

- gestión de partes
- monitoreo del avance de packing lists
- visualización de operadores activos
- exportación de packing lists :contentReference[oaicite:15]{index=15} :contentReference[oaicite:16]{index=16}

---

### Administrador
Usuario del sistema con capacidades administrativas ampliadas.

En la línea base funcional inicial, participa en:

- gestión de usuarios
- administración del sistema
- historial de cargas de Excel
- configuración general
- supervisión y acciones avanzadas sobre packing lists :contentReference[oaicite:17]{index=17}

---

### Usuario
Persona autenticada que interactúa con el sistema y opera bajo un rol determinado.

Los detalles de autenticación, sesiones, permisos y reglas de acceso se definirán posteriormente.

---

### Rol
Clasificación funcional asignada a un usuario que determina el tipo de operaciones que puede realizar dentro del sistema.

En la línea base funcional inicial existen al menos los siguientes roles:

- Operador
- Supervisor
- Administrador :contentReference[oaicite:18]{index=18}

---

### Auditoría
Capacidad del sistema para registrar y consultar eventos relevantes de operación, carga o modificación.

La propuesta inicial contempla auditoría de cargas y modificaciones, además de historial de cargas de Excel. :contentReference[oaicite:19]{index=19} :contentReference[oaicite:20]{index=20}

La forma exacta de los eventos auditables todavía debe definirse.

---

### Historial de Cargas
Registro de eventos asociados a la carga de archivos Excel en el sistema.

Forma parte de las capacidades administrativas contempladas en la propuesta inicial. :contentReference[oaicite:21]{index=21}

---

### Configuración General
Conjunto de parámetros administrativos del sistema que no forman parte directa de una operación de escaneo, pero que influyen en su comportamiento general.

Su contenido exacto todavía no está formalizado.

---

### API
Backend del sistema encargado de exponer endpoints HTTP para la operación del frontend y concentrar la lógica de aplicación, validación, acceso a datos y servicios internos.

La propuesta inicial contempla endpoints para:

- carga y procesamiento de Excel
- CRUD de partes
- cálculo de configuraciones
- validación de escaneo 1 y 2
- gestión de packing lists
- auditoría y exportación :contentReference[oaicite:22]{index=22}

---

### Frontend
Aplicación cliente del sistema orientada a operación desde navegador.

La línea base actual establece frontend en Blazor WebAssembly y uso de la UI existente como shell y template para la nueva funcionalidad. :contentReference[oaicite:23]{index=23}

---

### Portal Operador
Vista o conjunto de pantallas del sistema orientadas a la operación rápida de escaneo y trabajo en planta.

La propuesta inicial contempla dos portales diferenciados y menciona explícitamente el portal operador. :contentReference[oaicite:24]{index=24}

---

### Portal Administrativo
Vista o conjunto de pantallas del sistema orientadas a gestión, administración, supervisión y control.

La propuesta inicial contempla explícitamente un portal administrativo. :contentReference[oaicite:25]{index=25}

---

## Términos pendientes de definición detallada

Los siguientes términos ya existen en el lenguaje del proyecto, pero todavía requieren definición más precisa en documentos posteriores:

- estructura exacta de etiqueta
- campos exactos del primer escaneo
- campos exactos del segundo escaneo
- definición formal de configuración de lectura
- definición detallada de tipo de etiqueta
- estructura exacta de una línea de packing list
- eventos exactos de auditoría
- reglas de validación del archivo Excel
- reglas de error y recuperación operativa

---

## Regla de evolución

Este glosario debe actualizarse cuando ocurra cualquiera de estas situaciones:

- se define formalmente un flujo funcional
- se crea un contrato API que fija significado operativo
- se introduce el modelo de datos
- una regla de negocio convierte un término general en uno preciso
- se detecta ambigüedad entre lenguaje funcional y lenguaje técnico
- aparece un nuevo concepto relevante para el sistema

---

## Historial

### Versión inicial
- Se crea el vocabulario base del dominio a partir de la propuesta funcional inicial.
- Se definen los conceptos principales del sistema sin inventar detalles aún no formalizados.
- Se identifican términos que deberán precisarse más adelante mediante flujos, contratos y modelo de datos.
### Administración de Tipos de Etiqueta
Módulo administrativo para definir clasificaciones de etiqueta por reglas `columna + valor esperado` y habilitar el cálculo automático durante carga de Excel. `Columns` puede mantenerse como proyección legible, pero la fuente operativa es `LabelTypeRule`.

### Matching de Tipo de Etiqueta
Regla de comparación entre valores normalizados de una `Part` y las reglas `LabelTypeRule` (`ColumnName + ExpectedValue`). Se normaliza con `trim` y comparación case-insensitive. Si no hay match, el sistema usa fallback `Por asignar`. En múltiples matches: mayor cantidad de reglas y luego `Name` ascendente.
