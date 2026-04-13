# Mapa de Reutilización y Descarte

## Propósito de este documento

Este documento define cómo debe interpretarse y utilizarse la base actual del proyecto durante la construcción del sistema.

Su objetivo es evitar ambigüedad respecto a:

- qué partes de la solución actual se consideran base activa
- qué partes se usan solo como referencia
- qué partes no deben tomarse como base funcional
- qué puede refactorizarse, reemplazarse o descartarse
- cómo debe actuar cualquier persona o agente al trabajar sobre el repositorio

Este documento no evalúa el valor histórico del código existente.  
Evalúa únicamente su utilidad práctica dentro del enfoque actual del proyecto.

---

## Regla principal

La solución actual **sí es la base activa del proyecto**, pero no todo lo que contiene debe interpretarse como base funcional del sistema final.

La regla de trabajo es la siguiente:

- se conserva lo que aporte valor real como shell, estructura o soporte de implementación
- se reutiliza con criterio lo que sea útil
- se toma solo como referencia lo que no sea directamente aprovechable
- no se arrastra funcionalidad heredada como si ya representara el sistema real

---

## Categorías oficiales

Toda pieza del proyecto actual debe caer en una de estas categorías:

### 1. Se conserva como base activa
Componentes, archivos o estructuras que forman parte de la base real sobre la que se va a construir.

### 2. Se usa como referencia
Elementos que pueden servir para orientación visual, estructural o técnica, pero que no deben asumirse como parte activa del sistema final.

### 3. No se usa como base funcional
Elementos que no deben ser tomados como implementación válida del negocio, aunque sigan existiendo temporalmente en el repositorio.

### 4. Puede refactorizarse, reemplazarse o descartarse
Elementos actualmente útiles pero no protegidos como decisión permanente.  
Si estorban al desarrollo real, pueden modificarse o eliminarse, siempre que se documente el cambio cuando sea relevante.

---

## Se conserva como base activa

A día de hoy, se consideran base activa del proyecto los siguientes elementos:

### Solución y estructura general
- la solución actual como base de trabajo
- la organización general del repositorio
- la estructura arquitectónica del backend
- la separación por capas ya establecida

### Frontend como shell visual
- layout principal
- estructura de navegación
- menú principal
- header
- body/layout como contenedor de nuevos módulos
- estilos globales
- assets visuales reutilizables
- componentes puramente visuales que aporten valor real al nuevo sistema

### Decisiones de dirección
- uso de la propuesta como línea base funcional inicial
- uso del frontend actual como template y shell
- conservación de la arquitectura backend
- dirección tecnológica de frontend en Blazor WebAssembly
- dirección tecnológica de backend en API .NET

---

## Se usa como referencia

Los siguientes elementos pueden utilizarse como referencia, pero no deben asumirse automáticamente como parte activa o definitiva del sistema:

- componentes heredados del template actual
- pantallas de ejemplo
- servicios de apoyo visual heredados
- utilidades de UI no vinculadas todavía al dominio real
- decisiones implícitas presentes en código viejo que no estén documentadas
- estructuras previas de navegación o presentación que sirvan como inspiración
- cualquier implementación heredada que ayude a entender la base, pero no represente funcionalidad confirmada

Usar algo como referencia **no significa** que deba mantenerse, integrarse o extenderse.

---

## No se usa como base funcional

Los siguientes elementos no deben tratarse como base funcional válida del sistema final:

- lógica heredada que no responda al dominio real actual
- implementaciones antiguas que provengan de enfoques tecnológicos descartados
- suposiciones no documentadas embebidas en código viejo
- servicios o flujos que aparenten funcionalidad de negocio pero no estén confirmados como parte del sistema actual
- componentes del template utilizados solo para demostración o decoración
- código heredado que mezcle responsabilidades sin alinearse al enfoque actual
- cualquier lógica que se pretenda “rescatar” sin validar antes su encaje funcional y técnico

Regla explícita:

**Nada debe considerarse funcionalidad oficial del sistema solo porque ya exista en el repositorio.**

La existencia de código no equivale a validación funcional.

---

## Puede refactorizarse, reemplazarse o descartarse

Los siguientes tipos de elementos pueden modificarse libremente si dejan de ser útiles para la implementación real:

- shell visual actual
- layout y composición visual
- componentes de navegación
- componentes puramente visuales
- servicios de frontend que no formen parte de una decisión arquitectónica protegida
- assets o recursos de UI que no aporten valor real
- utilidades heredadas de la base actual

Esto aplica especialmente al frontend.

La regla es:

**La UI actual se conserva mientras aporte valor. Si estorba, se refactoriza, simplifica, reemplaza o descarta parcialmente.**

---

## Elementos protegidos por decisión

Hay elementos que no deben moverse como parte del trabajo normal de implementación, salvo decisión formal posterior:

### Backend
- estructura arquitectónica actual
- enfoque en capas
- base de organización del backend

### Dirección de solución
- frontend como Blazor WebAssembly
- backend como API .NET
- construcción progresiva de contratos, flujos y modelo de datos a partir de la implementación real
- uso de la solución actual como base activa del proyecto

---

## Cómo debe actuar cualquier persona o agente

Antes de reutilizar, modificar o descartar una pieza del proyecto actual, debe responderse internamente lo siguiente:

1. ¿Este elemento aporta valor real al sistema que estamos construyendo?
2. ¿Es un componente de shell/template o pretende resolver negocio real?
3. ¿Está alineado con la arquitectura y enfoque actuales?
4. ¿Su reutilización reduce trabajo real o solo arrastra complejidad heredada?
5. ¿Su permanencia mejora la implementación o la entorpece?

Si la respuesta a una o más de estas preguntas es negativa, el elemento debe tratarse como referencia o descarte, no como base activa.

---

## Regla para nueva funcionalidad

Toda nueva funcionalidad de negocio debe construirse de forma explícita y consciente.

No debe:

- apoyarse en supuestos implícitos del código heredado
- extender lógica antigua no validada
- tratar componentes existentes como dominio confirmado
- asumir contratos o flujos inexistentes

La nueva funcionalidad debe nacer documentada y alineada con la evolución actual del proyecto.

---

## Qué no debe ocurrir

No debe ocurrir ninguno de estos escenarios:

- reutilizar código heredado solo porque “ya está hecho”
- extender servicios viejos sin validar si pertenecen al nuevo enfoque
- asumir que una pieza visual heredada define comportamiento funcional
- usar código previo como verdad de negocio sin respaldo documental
- mover la arquitectura backend por conveniencia local
- mezclar otra vez enfoques tecnológicos descartados

---

## Criterio de actualización

Este documento debe actualizarse cuando ocurra cualquiera de estas situaciones:

- una parte del shell UI pase de “base activa” a “referencia”
- una parte actualmente heredada se adopte como pieza oficial del sistema
- un componente o servicio sea descartado de forma explícita
- un bloque de frontend sea reemplazado por implementación nueva
- una nueva decisión formal cambie la clasificación de alguna parte del proyecto

---

## Estado actual resumido

### Se conserva
- solución actual como base activa
- shell UI
- layout, menú, header y estructura visual útil
- arquitectura backend actual
- organización general del repositorio

### Se usa como referencia
- piezas heredadas del template
- componentes y servicios no confirmados como parte del dominio real
- implementaciones previas útiles para orientación

### No se asume como base funcional
- lógica heredada no validada
- comportamientos implícitos en código viejo
- funcionalidad aparente no formalizada

### Puede cambiar
- shell visual
- componentes de frontend
- composición de pantallas
- servicios auxiliares de frontend
- cualquier pieza no protegida por decisión arquitectónica

---

## Historial

### Versión inicial
- Se establece la clasificación oficial entre conservación, referencia, descarte y refactorización.
- Se declara la solución actual como base activa del proyecto.
- Se define el frontend actual como shell y template útil, no como funcionalidad ya resuelta.
- Se protege la arquitectura backend como decisión estable.
- Se fija el criterio de reutilización consciente para cualquier desarrollo futuro.