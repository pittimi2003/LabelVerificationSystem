# Fuente de Verdad del Proyecto

## Propósito de este documento

Este documento define cuál es la fuente de verdad del proyecto y cómo debe resolverse cualquier duda, conflicto o discrepancia entre:

- código existente
- documentación
- propuesta inicial
- decisiones arquitectónicas
- implementación en curso
- supuestos de trabajo

Su objetivo es evitar que personas o agentes:

- inventen reglas no definidas
- tomen código heredado como verdad funcional
- contradigan decisiones ya aceptadas
- documenten como definitivo algo que todavía está en construcción
- mezclen contexto viejo con contexto vigente

---

## Principio general

En este proyecto, la verdad no se asume por intuición, por conveniencia ni por antigüedad del código.

La verdad del proyecto debe surgir de lo que esté:

- explícitamente decidido
- documentado
- vigente
- alineado con el estado actual real de implementación

---

## Jerarquía oficial de fuente de verdad

Cuando exista conflicto o duda, debe aplicarse esta jerarquía en orden descendente.

### 1. Documentos de decisión aceptados
Tienen máxima prioridad las decisiones formales aceptadas y vigentes.

Ejemplos:
- ADRs aceptadas
- decisiones fundacionales de arquitectura
- decisiones explícitas sobre enfoque de construcción

Si una ADR vigente contradice una interpretación de código o una suposición, **manda la ADR**.

---

### 2. Estado actual documentado del proyecto
Después de las decisiones formales, manda el estado actual documentado.

Documento principal:
- `docs/project-current-state.md`

Si el proyecto está en construcción inicial y un artefacto todavía no existe formalmente, no debe asumirse como ya definido solo porque aparezca insinuado en código o conversación previa.

---

### 3. Documentación funcional y estructural vigente
Después del estado actual, mandan los documentos funcionales y estructurales vigentes del proyecto.

Incluye, entre otros:

- `docs/rebuild/reuse-and-discard-map.md`
- `docs/domain/domain-glossary.md`
- `docs/flows/functional-flows.md`
- `docs/database/data-model.md`
- `docs/api/api-contracts.md`

Si uno de estos documentos define una intención o estructura actual, esa definición tiene prioridad sobre interpretaciones informales.

---

### 4. Propuesta funcional inicial
La propuesta del proyecto es la línea base funcional inicial del sistema.

Sirve para definir:

- objetivo general
- alcance base
- módulos esperados
- arquitectura objetivo inicial
- contexto operativo inicial

Pero la propuesta **no tiene prioridad absoluta sobre decisiones posteriores documentadas**.

Si la implementación y la documentación vigente refinan algo de la propuesta, manda lo más reciente y formalmente documentado.

La propuesta es referencia inicial, no autoridad perpetua. :contentReference[oaicite:0]{index=0} :contentReference[oaicite:1]{index=1} :contentReference[oaicite:2]{index=2}

---

### 5. Código implementado y validado
El código existente tiene valor como fuente de verdad **solo cuando**:

- está alineado con la documentación vigente
- representa implementación real actual
- no contradice decisiones formales
- no pertenece a lógica heredada descartada o no validada

El código por sí solo **no manda** sobre una decisión documentada.

La mera existencia de código no convierte ese comportamiento en verdad oficial del proyecto.

---

### 6. Supuestos de trabajo
Los supuestos temporales tienen la prioridad más baja.

Pueden servir para avanzar, pero deben tratarse explícitamente como provisionales hasta quedar documentados o implementados de forma confirmada.

Ningún supuesto debe presentarse como hecho.

---

## Reglas prácticas de interpretación

### Regla 1
Si algo no está documentado ni implementado claramente, **no existe todavía como verdad oficial**.

### Regla 2
Si algo existe en código heredado pero no está validado por la documentación vigente, **no debe asumirse como comportamiento oficial**.

### Regla 3
Si una conversación introduce una idea nueva, esa idea no pasa a ser verdad estable hasta quedar reflejada en la documentación correspondiente.

### Regla 4
Si un módulo está en construcción inicial, sus contratos, reglas y modelos deben tratarse como evolutivos, no como definitivos.

### Regla 5
Si hay conflicto entre propuesta inicial y decisión posterior documentada, **manda la decisión posterior documentada**.

### Regla 6
Si hay conflicto entre código heredado y documentación vigente, **manda la documentación vigente**, salvo que se documente explícitamente una corrección de la documentación.

---

## Cómo resolver conflictos

Ante cualquier duda o contradicción, se debe seguir este orden:

1. Revisar si existe una ADR o decisión formal vigente.
2. Revisar el estado actual documentado del proyecto.
3. Revisar la documentación funcional o estructural relacionada.
4. Revisar la propuesta inicial como línea base.
5. Revisar el código actual solo después de lo anterior.
6. Si sigue habiendo ambigüedad, la duda debe resolverse documentando la decisión en el archivo que corresponda.

No debe resolverse una ambigüedad simplemente “eligiendo lo que parece lógico” sin dejar rastro documental.

---

## Qué no debe tomarse como verdad automática

No debe asumirse automáticamente como verdad oficial ninguno de estos elementos:

- código heredado del template
- nombres de clases heredadas
- servicios existentes no documentados
- comportamiento inferido por costumbre
- comentarios viejos no alineados con documentos vigentes
- conversaciones pasadas no reflejadas en documentación
- ideas provisionales expresadas durante exploración

---

## Verdad actual del proyecto en este momento

A la fecha actual, las siguientes afirmaciones sí forman parte de la verdad vigente del proyecto:

- **Fase 4 (Bloque B) está cerrada al 100%** (cierre formal 2026-04-23, revalidado en runtime el 2026-04-26).
- **Fase 5 permanece pendiente** y fuera de alcance de este cierre.
- el proyecto está en **Construcción inicial**
- la solución actual es la base activa de trabajo
- la UI actual se utiliza como template y shell visual
- la estructura arquitectónica del backend se conserva
- el sistema se construirá funcionalmente a partir de ahora
- la propuesta es la línea base funcional inicial
- todavía no existen contratos, flujos detallados ni modelo de datos definitivos
- esos artefactos se están creando y deberán mantenerse actualizados
- el primer módulo real a construir es **Carga de Excel**

Estas afirmaciones derivan del estado actual documentado y de las decisiones ya aceptadas.

---

## Relación entre documentación e implementación

La implementación debe seguir la documentación vigente.

Pero la documentación también debe evolucionar cuando la implementación cierre decisiones nuevas.

Por tanto:

- la documentación no puede quedarse atrás respecto al estado real
- la implementación no puede adelantarse inventando reglas que no queden registradas
- cuando se materialice una decisión relevante, debe actualizarse el documento correspondiente

---

## Regla para humanos y agentes

Cualquier persona o agente que trabaje en el proyecto debe operar bajo este criterio:

- no asumir
- no inventar
- no extender código heredado sin validación
- no tratar una intuición como una regla cerrada
- no tratar como definitivo lo que todavía está en construcción
- documentar toda decisión relevante que pase a formar parte de la verdad del proyecto

---

## Cuándo actualizar este documento

Este documento debe actualizarse cuando ocurra alguna de estas situaciones:

- se agregue una nueva categoría documental con prioridad relevante
- cambie la jerarquía de fuente de verdad
- se adopte una nueva forma oficial de decisión
- se detecte que el equipo está usando una fuente incorrecta como verdad principal
- una fase más madura del proyecto requiera ajustar estas reglas

---

## Historial

### Versión inicial
- Se define la jerarquía oficial de fuente de verdad del proyecto.
- Se establece que las decisiones documentadas tienen prioridad sobre el código heredado.
- Se fija la propuesta como línea base funcional inicial, pero no como autoridad absoluta.
- Se documenta la regla de no asumir como verdad nada no decidido ni vigente.
- Desde el 2026-04-26, el módulo `Tipos de Etiqueta` forma parte del Core funcional activo y su fallback `Por asignar` es obligatorio para partes sin coincidencia.
