# Arquitectura y lineamientos para agentes

Este documento define la arquitectura base del sistema y las reglas de trabajo para agentes automáticos y asistentes que interactúan con el repositorio.

Su objetivo es reducir ambigüedad, evitar cambios incorrectos y conservar consistencia técnica.

---

# 1. Visión general de la arquitectura

La solución está organizada con separación clara de responsabilidades.

## Capas principales

- `Frontend`
- `Api`
- `Application`
- `Domain`
- `Infrastructure`
- `AppHost`

## Intención arquitectónica

### Domain
Contiene el núcleo del negocio y sus reglas.

### Application
Orquesta casos de uso y coordina operaciones funcionales.

### Api
Expone el backend vía HTTP.

### Infrastructure
Implementa persistencia e integraciones técnicas.

### Frontend
Consume la API y presenta la experiencia de usuario.

### AppHost
Orquesta el entorno local y los recursos con Aspire.

---

# 2. Reglas de dependencia

## Permitido

- `Api` depende de `Application`
- `Api` depende de `Infrastructure`
- `Application` depende de `Domain`
- `Infrastructure` depende de `Application`
- `Infrastructure` depende de `Domain`
- `Frontend` consume `Api` por HTTP
- `AppHost` referencia proyectos ejecutables o recursos que orquesta

## No permitido

- `Domain` depende de `Infrastructure`
- `Domain` depende de `Api`
- `Application` depende de implementaciones concretas de persistencia
- `Frontend` accede directo a base de datos
- `Controllers` con lógica de dominio compleja
- `Infrastructure` decidiendo reglas funcionales nucleares

---

# 3. Qué puede hacer un agente

Un agente puede:

- crear documentación técnica
- mejorar documentación existente
- crear clases, interfaces y archivos dentro de la capa correcta
- refactorizar nombres y organización manteniendo arquitectura
- agregar casos de uso en `Application`
- agregar entidades y reglas en `Domain`
- agregar implementaciones técnicas en `Infrastructure`
- agregar endpoints en `Api`
- agregar componentes y páginas en `Frontend`
- proponer ADRs y convenciones
- crear pruebas unitarias e integración
- mejorar scripts repetibles y de bootstrap
- documentar decisiones y flujos

Un agente puede proponer:

- mejoras de estructura
- endurecimiento de capas
- eliminación de acoplamientos
- mejoras de consistencia de nombres
- mejoras de observabilidad y mantenibilidad

---

# 4. Qué no puede hacer un agente sin instrucción explícita

Un agente no debe:

- cambiar la arquitectura base sin documentarlo
- mover masivamente carpetas o proyectos sin justificarlo
- cambiar proveedor de base de datos sin instrucción explícita
- introducir nuevas librerías grandes sin justificación
- modificar cadenas de conexión productivas
- inventar reglas de negocio no definidas
- asumir comportamiento funcional no especificado
- eliminar documentación relevante
- convertir decisiones temporales en estándares permanentes sin dejar constancia
- alterar contratos públicos sin indicar impacto

---

# 5. Qué puede documentar un agente

Un agente sí puede documentar:

- estructura del repositorio
- arquitectura
- flujos funcionales
- convenciones
- decisiones técnicas
- dependencias entre capas
- limitaciones conocidas
- riesgos técnicos
- supuestos explícitos
- áreas pendientes

También puede crear:

- ADRs
- guías para onboarding
- guías para agentes
- listas de verificación de calidad
- documentación de endpoints
- documentación de módulos

---

# 6. Qué no debe documentar un agente como hecho

Un agente no debe presentar como confirmado algo que no esté validado.

No debe documentar como hecho:

- comportamiento no implementado
- reglas funcionales inferidas sin confirmación
- integraciones no existentes
- seguridad no implementada
- decisiones no aprobadas
- flujos futuros como si ya fueran vigentes

Cuando algo sea tentativo, debe marcarse como:

- propuesta
- supuesto
- pendiente
- borrador
- decisión no aprobada

---

# 7. Reglas específicas para código

## Domain

Un agente puede:

- crear entidades
- crear value objects
- crear enums
- agregar invariantes
- crear reglas de dominio

Un agente no debe:

- meter EF Core
- meter atributos de persistencia si contaminan el dominio
- acoplar el dominio a HTTP o UI

---

## Application

Un agente puede:

- crear commands
- crear queries
- crear handlers
- crear validators
- definir contratos de puertos
- mapear DTOs y flujos de aplicación

Un agente no debe:

- hablar con SQL directamente
- depender de clases concretas de infraestructura
- meter detalles de UI

---

## Infrastructure

Un agente puede:

- implementar repositorios
- configurar EF Core
- crear `DbContext`
- integrar servicios externos
- implementar puertos de aplicación

Un agente no debe:

- redefinir negocio nuclear
- ocultar decisiones funcionales dentro de código técnico

---

## Api

Un agente puede:

- crear endpoints
- crear controllers
- configurar middlewares
- registrar dependencias
- documentar contratos HTTP

Un agente no debe:

- mover el negocio a controllers
- duplicar validaciones que ya pertenezcan a `Application` o `Domain`

---

## Frontend

Un agente puede:

- crear páginas
- crear componentes
- crear clientes HTTP
- crear estado de UI
- mejorar navegación y layouts
- documentar integración con API

Un agente no debe:

- duplicar reglas críticas del dominio salvo validaciones de UX
- inventar contratos distintos a la API real
- meter secretos o configuración sensible

---

# 8. Reglas para documentación de agentes

Todo documento generado por un agente debe intentar ser:

- claro
- verificable
- honesto
- explícito en supuestos
- alineado con la estructura real del repo

Un agente debe preferir:

- documentos cortos y útiles
- encabezados claros
- lenguaje técnico preciso
- distinguir hecho de propuesta

---

# 9. Qué hacer cuando una decisión no está clara

Si una decisión no está cerrada, el agente debe:

## Sí hacer
- dejar alternativas
- explicar trade-offs
- marcar una recomendación
- indicar impacto

## No hacer
- inventar una decisión final
- imponer una librería o patrón sin justificación
- documentar la alternativa elegida como definitiva si no lo es

---

# 10. Convención de cambios seguros

Los cambios preferidos son:

- pequeños
- reversibles
- localizados
- documentados

Evitar cambios que sean:

- masivos
- silenciosos
- transversales sin necesidad
- difíciles de revisar

---

# 11. Alcance de documentación permitido

## Permitido
- describir el estado actual
- proponer mejoras
- documentar limitaciones
- documentar deuda técnica
- documentar decisiones aceptadas

## No permitido
- inventar roadmap oficial
- prometer implementaciones futuras
- asumir aprobación de arquitectura
- documentar integraciones no existentes

---

# 12. Regla de honestidad técnica

Si un agente no puede verificar algo, debe decirlo.

Frases aceptables conceptualmente:

- no implementado todavía
- pendiente de definición
- propuesta no aprobada
- supuesto de trabajo
- requiere validación

Lo que no debe hacer:
- disfrazar incertidumbre como certeza

---

# 13. Recomendación operativa para agentes

Antes de crear o modificar algo, el agente debe preguntarse:

1. ¿En qué capa pertenece realmente esto?
2. ¿Estoy moviendo una responsabilidad a una capa incorrecta?
3. ¿Esto describe el estado actual o una propuesta?
4. ¿Estoy agregando una dependencia innecesaria?
5. ¿Lo que escribo puede verificarse en el repo?

---

# 14. Resumen ejecutivo para agentes

## Puede
- crear
- documentar
- refactorizar
- proponer
- probar
- ordenar

## No puede
- inventar negocio
- romper capas
- cambiar arquitectura base sin explicarlo
- documentar supuestos como hechos
- introducir complejidad sin justificación