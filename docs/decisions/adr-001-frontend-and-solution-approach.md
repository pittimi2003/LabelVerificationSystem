# ADR-001: Enfoque de solución y uso de la base actual del proyecto

## Estado

Aceptado

---

## Contexto

El proyecto se encuentra en fase de construcción inicial.

Existe una solución base ya creada y esa solución será la base activa de trabajo para la implementación del sistema.

La propuesta funcional del proyecto se toma como línea base inicial del alcance esperado, incluyendo:

- carga y procesamiento de Excel
- gestión de partes
- verificación de etiquetas mediante dos escaneos
- packing lists
- administración de usuarios y roles
- auditoría
- configuración general del sistema
- frontend en Blazor WebAssembly
- backend en API .NET
- base de datos inicialmente SQLite
- despliegue sobre IIS local

Sin embargo, al momento de adoptar esta decisión, todavía no existen formalmente definidos:

- contratos API
- flujos funcionales detallados
- modelo de datos formal
- reglas de negocio cerradas a nivel de implementación

Dichos artefactos se crearán progresivamente durante la construcción real del sistema.

---

## Problema

Era necesario fijar una decisión explícita sobre cómo debe utilizarse la solución actual para evitar ambigüedad en el desarrollo.

Sin esta decisión, existe riesgo de:

- intentar reutilizar como base funcional código heredado que no representa el sistema final
- mezclar enfoques tecnológicos incompatibles
- modificar la arquitectura backend sin necesidad
- confundir template visual con implementación de negocio
- inventar contratos, modelos o flujos no definidos todavía

---

## Decisión

Se adopta el siguiente enfoque oficial de trabajo:

### 1. La solución actual es la base activa del proyecto
La solución existente no se considera desechable ni temporal.  
Es la base real sobre la que se construirá el sistema.

### 2. El frontend actual se utilizará como template y shell visual
La UI existente se toma como:

- base visual
- template
- shell de navegación
- estructura de layout
- contenedor donde se renderizará la nueva funcionalidad

La nueva funcionalidad se construirá dentro del `@Body` y de la estructura visual existente, siempre que esta no se convierta en un obstáculo técnico o funcional.

### 3. El frontend podrá refactorizarse o reemplazarse parcialmente
Aunque la UI actual se utilice como base activa, cualquier parte de esa base podrá:

- refactorizarse
- simplificarse
- reemplazarse
- descartarse parcialmente

si deja de ser útil para la implementación real del sistema.

### 4. La arquitectura backend actual se mantiene
La estructura arquitectónica del backend ya existente en la solución se conserva como decisión fundacional.

No se debe reestructurar la arquitectura base del backend como parte del trabajo normal de implementación.

La evolución del backend debe ocurrir dentro de esa estructura.

### 5. El sistema se construirá funcionalmente a partir de ahora
La funcionalidad real del sistema no se considera ya resuelta por el código heredado existente.

El proyecto entra en una etapa de construcción efectiva en la que se irán definiendo e implementando:

- contratos API
- flujos funcionales
- modelo de datos
- reglas de negocio
- integración entre frontend y backend

### 6. La arquitectura objetivo queda fijada
La arquitectura objetivo del sistema queda establecida como:

- frontend: Blazor WebAssembly
- backend: API .NET
- persistencia inicial: SQLite
- despliegue previsto: IIS local

### 7. El primer módulo real a construir será Carga de Excel
El primer frente funcional de implementación será el módulo de carga y procesamiento de Excel.

A partir de este módulo comenzarán a materializarse los primeros contratos, decisiones de modelo de datos y reglas de negocio reales del sistema.

---

## Consecuencias

### Consecuencias positivas

- Se elimina la ambigüedad sobre el uso de la solución actual.
- Se evita tratar el código heredado como si ya fuera la base funcional definitiva.
- Se preserva el valor de la UI existente como acelerador visual y estructural.
- Se protege la arquitectura backend de cambios arbitrarios.
- Se crea una base clara para que humanos y agentes trabajen con el mismo criterio.
- Se reconoce explícitamente que contratos, flujos y modelo de datos todavía deben construirse.
- Se habilita una evolución controlada y documentada del proyecto.

### Consecuencias operativas

- Toda nueva funcionalidad debe construirse con base en esta decisión.
- Cualquier refactor relevante del shell UI debe documentarse.
- Cualquier cambio a la arquitectura backend requerirá una decisión formal posterior.
- La propuesta funcional inicial podrá ajustarse durante la implementación, pero esos cambios deberán documentarse.
- La documentación del proyecto deberá evolucionar junto con la implementación real.

### Restricciones derivadas

- No se debe inventar comportamiento funcional no documentado como si ya estuviera decidido.
- No se debe asumir que existen contratos o modelos definitivos si todavía no han sido formalizados.
- No se debe mezclar de nuevo un enfoque híbrido de frontend que contradiga la decisión de Blazor WebAssembly.
- No se debe mover la estructura arquitectónica backend sin una ADR posterior que lo justifique.

---

## Alternativas consideradas

### 1. Rehacer toda la solución desde cero
Se descartó como enfoque principal.

Motivo:
La solución actual sí aporta una base válida de estructura, layout y shell UI que puede acelerar el arranque del proyecto.

### 2. Reutilizar el proyecto existente como base funcional completa
Se descartó.

Motivo:
La base existente no representa todavía una implementación funcional madura del sistema final y reutilizarla sin criterio aumentaría el riesgo de arrastrar errores, mezcla de enfoques o supuestos no validados.

### 3. Mantener el frontend en un enfoque híbrido anterior
Se descartó.

Motivo:
La dirección oficial del proyecto queda fijada en Blazor WebAssembly + API, y mantener un enfoque híbrido volvería a introducir ambigüedad técnica y de responsabilidades.

---

## Criterios de aplicación

Esta decisión debe considerarse activa y obligatoria mientras no exista otra ADR que la modifique.

Todo trabajo nuevo debe evaluarse contra estas preguntas:

1. ¿Aprovecha la base actual sin quedar rehén de ella?
2. ¿Construye funcionalidad nueva en lugar de asumir funcionalidad previa no confirmada?
3. ¿Respeta el frontend como shell/template y el backend como arquitectura estable?
4. ¿Evita introducir supuestos no documentados?
5. ¿Ayuda a que el proyecto gane definición real en contratos, flujos, modelo y reglas?

Si una implementación contradice una o más de estas preguntas, debe revisarse antes de continuar.

---

## Relación con otros documentos

Este ADR debe leerse junto con:

- `docs/project-current-state.md`
- futuros documentos de flujos funcionales
- futuros contratos API
- futuro modelo de datos
- futuras ADRs que amplíen o ajusten esta decisión

---

## Historial

### Versión inicial
- Se establece que la solución actual es la base activa del proyecto.
- Se define la UI existente como template y shell visual.
- Se permite refactor o reemplazo parcial del shell UI cuando sea necesario.
- Se fija la arquitectura backend actual como base estable.
- Se reconoce que contratos, flujos y modelo de datos todavía no existen formalmente.
- Se fija la arquitectura objetivo en Blazor WebAssembly + API .NET.
- Se establece Carga de Excel como primer módulo real a construir.