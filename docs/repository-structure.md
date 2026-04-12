# Estructura del repositorio

Este documento describe la finalidad de cada carpeta del repositorio y qué tipo de contenido debe vivir en cada una.

---

## Raíz del repositorio

En la raíz solo deben existir elementos de coordinación general del proyecto.

Ejemplos válidos:

- archivo de solución (`.sln` o `.slnx`)
- `README.md`
- `Directory.Build.props`
- `.gitignore`
- carpetas de primer nivel (`source`, `test`, `docs`, `build`, `scripts`, `tools`)

No debe colocarse en raíz:

- código fuente de negocio
- archivos temporales
- exportaciones manuales
- documentos operativos sueltos
- scripts ad hoc sin clasificar

---

## `/source`

Contiene el código fuente principal del sistema.

Aquí vive el producto.

### `/source/AppHost`

Contiene el proyecto de orquestación de Aspire.

Debe incluir:

- definición de recursos
- registro de proyectos
- configuración de composición local de la solución

No debe incluir:

- lógica de negocio
- reglas de dominio
- controladores HTTP
- acceso a base de datos

---

### `/source/ServiceDefaults`

Contiene configuración transversal reutilizable para servicios orquestados por Aspire.

Debe incluir:

- configuración compartida entre servicios
- observabilidad base
- defaults técnicos

No debe incluir:

- lógica funcional
- entidades de dominio
- lógica específica de un módulo

---

### `/source/Backend`

Contiene el backend del sistema.

---

### `/source/Backend/Api`

Proyecto de entrada HTTP del backend.

Debe incluir:

- `Program.cs`
- controllers
- configuración de middleware
- configuración HTTP
- wiring de dependencias

No debe incluir:

- reglas complejas de negocio
- acceso directo desordenado a infraestructura
- lógica de dominio embebida en controllers

Regla práctica:
los controllers deben orquestar, no decidir negocio.

---

### `/source/Backend/Application`

Capa de aplicación.

Debe incluir:

- casos de uso
- comandos
- queries
- handlers
- validaciones
- contratos de servicios de aplicación
- orquestación funcional

No debe incluir:

- detalles de base de datos
- dependencias concretas de infraestructura
- UI
- lógica HTTP

Aquí se expresa **qué hace el sistema** desde el punto de vista funcional.

---

### `/source/Backend/Domain`

Núcleo del negocio.

Debe incluir:

- entidades
- value objects
- enums de negocio
- reglas de dominio
- invariantes
- contratos del dominio si aplican

No debe incluir:

- EF Core
- HTTP
- UI
- dependencias a frameworks de infraestructura

Aquí se expresa **qué es el negocio** y sus reglas.

---

### `/source/Backend/Infrastructure`

Implementaciones técnicas.

Debe incluir:

- `DbContext`
- persistencia
- repositorios
- implementaciones de servicios
- integración con sistemas externos
- seguridad técnica
- acceso a archivos, Excel, DB, mensajería, etc.

No debe incluir:

- reglas nucleares de dominio
- decisiones funcionales de alto nivel incrustadas
- lógica UI

Aquí se expresa **cómo se implementa técnicamente** lo que la aplicación necesita.

---

### `/source/Frontend`

Contiene el frontend del sistema.

---

### `/source/Frontend/Web`

Proyecto frontend principal.

En el modelo actual puede ser:

- Blazor WebAssembly standalone
- o una variante hosteada según la configuración del proyecto

Debe incluir:

- páginas
- componentes
- layouts
- servicios de cliente HTTP
- estado de UI
- configuración del frontend

No debe incluir:

- lógica de persistencia
- lógica de dominio de negocio profunda
- acceso directo a base de datos
- decisiones funcionales críticas que deban vivir en backend

---

### `/source/Frontend/Shared`

Contenido compartido del frontend.

Debe incluir:

- modelos compartidos de cliente
- constantes de UI
- helpers de cliente
- contratos o DTOs compartidos si aplica

No debe incluir:

- implementación de reglas críticas de negocio
- acceso a infraestructura
- controllers
- persistencia

---

## `/test`

Contiene las pruebas del sistema.

---

### `/test/UnitTests`

Pruebas unitarias.

Debe incluir:

- pruebas de dominio
- pruebas de aplicación
- pruebas de reglas
- pruebas de validación

No debe incluir:

- acceso real a infraestructura
- dependencias externas reales
- configuración de entorno compleja

---

### `/test/IntegrationTests`

Pruebas de integración.

Debe incluir:

- pruebas de API
- pruebas de persistencia
- pruebas de wiring entre capas
- pruebas con infraestructura controlada

No debe incluir:

- pruebas manuales
- scripts ad hoc
- resultados exportados

---

## `/docs`

Documentación técnica y funcional del proyecto.

Debe incluir:

- arquitectura
- decisiones técnicas
- convenciones
- flujos funcionales
- documentación para agentes y mantenedores

No debe incluir:

- archivos binarios innecesarios
- capturas temporales sin contexto
- borradores sin clasificar

---

### `/docs/architecture`

Arquitectura del sistema.

Ejemplos:

- capas
- límites
- decisiones estructurales
- integraciones

---

### `/docs/flows`

Flujos funcionales.

Ejemplos:

- Scan1 / Scan2
- packing list
- carga de Excel
- auditoría

---

### `/docs/database`

Documentación de base de datos.

Ejemplos:

- modelo lógico
- decisiones de persistencia
- convenciones de migraciones
- concurrencia

---

### `/docs/decisions`

Registro de decisiones técnicas.

Ejemplos:

- ADRs
- motivos de elección de framework
- trade-offs
- restricciones

---

## `/build`

Archivos relacionados con compilación y pipeline.

Debe incluir:

- configuración de build
- props compartidos
- helpers de CI/CD
- packaging

No debe incluir:

- lógica de negocio
- documentación funcional
- scripts operativos generales no relacionados con build

---

## `/scripts`

Scripts operativos y de automatización del repositorio.

Debe incluir:

- bootstrap del repo
- scripts de setup
- scripts de base de datos
- scripts de build y deploy
- utilidades repetibles

No debe incluir:

- experimentos temporales
- scripts manuales sin nombre claro
- lógica productiva del sistema

Todo script debe ser:

- idempotente cuando sea posible
- claro en su propósito
- seguro de reejecutar o explícito si no lo es

---

## `/tools`

Utilidades auxiliares de desarrollo.

Debe incluir:

- generadores
- utilidades locales
- herramientas de soporte al desarrollo

No debe incluir:

- código principal del producto
- pruebas
- documentación

---

# Reglas generales de organización

## 1. Separar propósito antes que tecnología

La ubicación de un archivo debe responder primero a su propósito dentro del sistema.

## 2. No duplicar responsabilidades

Si una carpeta ya tiene un propósito claro, no crear otra con el mismo objetivo.

## 3. Mantener las capas limpias

- `Domain` no depende de `Infrastructure`
- `Application` no conoce implementaciones concretas
- `Api` no debe contener negocio complejo
- `Frontend` no debe decidir reglas nucleares

## 4. La documentación vive en `docs`

No repartir documentación importante en carpetas aleatorias del repo.

## 5. Los scripts viven en `scripts`

No dejar archivos `.ps1`, `.cmd` o similares esparcidos sin clasificación.

---

# Criterio rápido para ubicar contenido

- Si define el negocio: `Domain`
- Si ejecuta un caso de uso: `Application`
- Si expone HTTP: `Api`
- Si implementa acceso técnico: `Infrastructure`
- Si pinta UI: `Frontend`
- Si prueba: `test`
- Si documenta: `docs`
- Si automatiza: `scripts`
- Si ayuda al desarrollo: `tools`