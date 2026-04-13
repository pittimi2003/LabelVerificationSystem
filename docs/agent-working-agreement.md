# Acuerdo de Trabajo para Agentes

## Propósito de este documento

Este documento define cómo debe trabajar cualquier agente dentro de este proyecto.

Su objetivo es asegurar que toda intervención sobre el repositorio:

- respete el estado real del proyecto
- siga las decisiones ya documentadas
- no invente reglas de negocio ni arquitectura
- no trate código heredado como verdad funcional automática
- mantenga el contexto actualizado en todo momento

Este acuerdo aplica a cualquier agente que lea, analice, modifique, proponga o genere contenido para el proyecto.

---

## Principios obligatorios de trabajo

### 1. No inventar
Ningún agente debe inventar:

- reglas de negocio
- contratos API
- estructuras de datos
- flujos funcionales
- validaciones
- decisiones de arquitectura
- comportamiento de UI
- integraciones con hardware

si esos elementos no están documentados o explícitamente definidos durante el avance del proyecto.

Cuando algo no exista todavía, debe tratarse como pendiente, no como hecho.

---

### 2. No asumir que el código heredado es la verdad funcional
La solución actual es la base activa del proyecto, pero no todo lo que contiene debe interpretarse como implementación oficial del sistema final.

El agente no debe:

- extender lógica heredada sin validación
- tratar servicios existentes como parte del dominio real solo porque ya están en el repositorio
- usar comportamiento antiguo como verdad funcional automática
- inferir reglas de negocio desde código de template o shell

El código heredado puede servir como:

- base estructural
- base visual
- shell
- referencia técnica

pero no como verdad funcional por defecto.

---

### 3. Respetar la arquitectura backend actual
La estructura arquitectónica del backend es una decisión protegida del proyecto.

El agente no debe:

- mover capas sin justificación formal
- mezclar responsabilidades entre capas
- introducir accesos indebidos entre frontend y backend
- romper la separación arquitectónica actual

Si una necesidad real parece exigir un cambio de arquitectura, eso debe elevarse como decisión explícita y documentarse antes de ejecutarse.

---

### 4. Respetar el frontend como shell y template activo
La UI actual se usa como:

- template visual
- shell de navegación
- base de layout
- contenedor para renderizar nueva funcionalidad

El agente puede:

- refactorizar partes del shell
- simplificarlo
- reemplazar piezas parciales
- descartar elementos que estorben

pero no debe confundir shell visual con funcionalidad ya resuelta.

---

### 5. Tratar el proyecto como construcción progresiva
El proyecto está en construcción inicial.

Eso significa que muchos elementos todavía no existen formalmente, por ejemplo:

- contratos API definitivos
- modelo de datos final
- flujos funcionales detallados definitivos
- validaciones completas
- reglas cerradas por módulo

El agente debe trabajar entendiendo que el proyecto gana definición mientras se construye.

---

## Orden obligatorio de lectura antes de trabajar

Antes de analizar, proponer o modificar algo, el agente debe leer como mínimo estos documentos:

1. `docs/source-of-truth.md`
2. `docs/project-current-state.md`
3. `docs/decisions/adr-001-frontend-and-solution-approach.md`
4. `docs/rebuild/reuse-and-discard-map.md`
5. `docs/domain/domain-glossary.md`
6. `docs/flows/functional-flows.md`
7. `docs/database/data-model.md`
8. `docs/api/api-contracts.md`

Si el trabajo afecta un área concreta, el agente debe revisar además cualquier documento específico relacionado con esa área.

---

## Regla de prioridad documental

Si existe conflicto entre distintas fuentes, el agente debe seguir la jerarquía definida en:

`docs/source-of-truth.md`

En términos prácticos:

- primero mandan las decisiones documentadas
- luego el estado actual documentado
- luego la documentación funcional y estructural vigente
- luego la propuesta inicial
- después el código validado
- al final, cualquier supuesto provisional

El agente no debe resolver conflictos “a criterio” sin reflejarlos en documentación si son relevantes.

---

## Qué puede hacer un agente

Un agente sí puede:

- proponer estructura técnica alineada al proyecto
- redactar documentación faltante
- construir módulos nuevos
- refactorizar partes del frontend shell si estorban
- implementar contratos, modelos y flujos a medida que se formalicen
- señalar inconsistencias entre documentos y código
- sugerir decisiones cuando haya vacíos todavía no cerrados

---

## Qué no puede hacer un agente

Un agente no puede:

- inventar hechos no confirmados
- cerrar reglas de negocio sin respaldo
- reinterpretar unilateralmente la propuesta como si todo detalle ya estuviera decidido
- mover la arquitectura backend porque le parezca más cómodo
- convertir código heredado en base funcional sin validación
- ocultar incertidumbre
- simular que algo está terminado si aún está en definición
- presentar una suposición como decisión oficial

---

## Cómo debe manejar la incertidumbre

Cuando el agente no tenga definición suficiente, debe actuar así:

### Correcto
- declarar que algo está pendiente
- proponer opciones
- diferenciar claramente entre hecho, decisión y supuesto
- dejar constancia documental si una definición nueva queda adoptada

### Incorrecto
- completar huecos inventando
- asumir por similitud con otros proyectos
- elegir una regla arbitraria y presentarla como verdad
- construir sobre una interpretación dudosa sin señalarla

---

## Cómo debe proponer cambios

Cuando el agente proponga una modificación relevante, debe dejar claro:

- qué cambia
- por qué cambia
- qué documento respalda el cambio
- si el cambio introduce una nueva decisión
- qué impacto tiene en frontend, backend, dominio o documentación

Si el cambio es estructural o fundacional, debe proponerse como decisión formal y no como ajuste silencioso.

---

## Cómo debe trabajar sobre módulos nuevos

Cuando un agente intervenga en un módulo nuevo, debe seguir este orden:

1. revisar el estado actual del proyecto
2. revisar el flujo funcional relacionado
3. revisar el glosario de dominio
4. revisar el modelo lógico de datos
5. revisar los contratos API relacionados
6. identificar vacíos reales de definición
7. construir solo lo que ya está suficientemente respaldado
8. documentar cualquier nueva definición que pase a formar parte de la verdad del proyecto

---

## Regla para contratos, modelo y flujos

Si durante la implementación un agente necesita fijar algo que todavía no existe formalmente, debe hacerlo de forma controlada.

Ejemplos:

- si define un contrato nuevo, debe actualizar `docs/api/api-contracts.md`
- si define una entidad o relación nueva, debe actualizar `docs/database/data-model.md`
- si cierra un comportamiento funcional, debe actualizar `docs/flows/functional-flows.md`
- si fija vocabulario nuevo, debe actualizar `docs/domain/domain-glossary.md`
- si cambia una decisión de enfoque, debe crear o actualizar una ADR

La implementación y la documentación deben evolucionar juntas.

---

## Regla para código nuevo

Todo código nuevo debe cumplir estas condiciones:

- responder a una necesidad real del sistema
- alinearse con la arquitectura vigente
- no introducir acoplamientos innecesarios
- no depender de supuestos no documentados
- no mezclar responsabilidades
- poder explicarse en términos del dominio actual del proyecto

---

## Regla para refactorización

Una refactorización es válida si:

- mejora claridad
- reduce complejidad heredada
- elimina ruido del template
- facilita la implementación real
- no contradice decisiones vigentes

Una refactorización no es válida si:

- cambia estructura fundacional sin decisión formal
- oculta un cambio funcional no documentado
- convierte preferencia técnica en desviación arquitectónica

---

## Regla para documentación

El agente debe asumir que la documentación es parte del producto de trabajo.

Por tanto, no debe:

- dejar documentos obsoletos cuando cierre una definición nueva
- seguir avanzando sobre una decisión relevante sin actualizar la documentación correspondiente
- crear contradicción entre implementación y documentos vigentes

Cuando una definición nueva pase a ser verdad del proyecto, debe reflejarse en el documento correcto.

---

## Señales que obligan a detenerse y documentar

El agente debe detener el avance implícito y documentar cuando ocurra alguno de estos casos:

- aparece una decisión arquitectónica nueva
- aparece una relación de datos no prevista
- un flujo funcional gana una regla concreta importante
- se detecta contradicción entre documentos
- una pieza heredada deja de ser shell y pasa a ser funcional
- un supuesto deja de ser provisional y se vuelve regla real

---

## Regla de trazabilidad mínima

Toda contribución relevante del agente debe poder responder:

- de qué documento partió
- qué definió
- qué cambió
- qué sigue pendiente
- qué impacto tiene sobre el contexto del proyecto

Si una intervención no puede responder eso, está mal trazada.

---

## Comportamiento esperado ante vacíos

Cuando falte definición, el agente debe preferir este orden:

1. usar lo ya documentado
2. usar la propuesta como línea base inicial
3. detectar la parte pendiente
4. proponer una definición explícita
5. documentarla si se adopta

Nunca debe saltar directamente del vacío a la implementación “porque parece lógico”.

---

## Regla final

El objetivo del agente no es solo producir código.

El objetivo es ayudar a construir un sistema consistente, documentado, entendible y gobernable en cualquier momento del proyecto.

Por tanto, todo agente debe trabajar para que:

- el contexto se conserve
- la verdad vigente sea visible
- el repositorio no derive hacia interpretaciones implícitas
- cada avance reduzca incertidumbre en vez de aumentarla

---

## Historial

### Versión inicial
- Se define el comportamiento esperado de cualquier agente dentro del proyecto.
- Se fija el orden obligatorio de lectura documental.
- Se prohíbe inventar reglas, contratos o flujos no documentados.
- Se protege la arquitectura backend y el uso del frontend como shell activo.
- Se establece la obligación de mantener documentación e implementación alineadas.