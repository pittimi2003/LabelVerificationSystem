# Project Guidelines

Este documento concentra la guía principal del proyecto.

Su objetivo es establecer una referencia única para:

- estructura del repositorio
- arquitectura
- límites entre capas
- convenciones de código
- lineamientos para agentes
- reglas de documentación
- criterios de calidad y mantenimiento

Este archivo debe ser considerado la referencia base para cualquier persona o agente que cree, modifique o documente código en la solución.

---

# 1. Objetivo del proyecto

La solución está diseñada para soportar una arquitectura limpia, mantenible y escalable, con separación explícita entre:

- dominio
- aplicación
- infraestructura
- exposición HTTP
- frontend
- orquestación local

La intención es que las decisiones funcionales vivan en el lugar correcto, evitando acoplamiento innecesario y reduciendo deuda técnica desde el inicio.

---

# 2. Estructura del repositorio

## 2.1 Raíz del repositorio

La raíz debe contener únicamente elementos de coordinación general.

Contenido válido:

- archivo de solución (`.sln` o `.slnx`)
- `README.md`
- `Directory.Build.props`
- `.gitignore`
- carpetas raíz (`source`, `test`, `docs`, `scripts`, `build`, `tools`)

No debe contener:

- lógica de negocio
- código fuente de producto
- scripts sueltos sin clasificar
- documentos temporales
- archivos exportados manualmente
- pruebas fuera de `test`

---

## 2.2 `/source`

Contiene el código fuente principal del sistema.

Aquí vive el producto.

---

## 2.3 `/source/AppHost`

Contiene la orquestación del entorno local con Aspire.

Debe contener:

- definición de recursos
- registro de proyectos
- configuración de composición local
- wiring de servicios orquestados

No debe contener:

- reglas de negocio
- acceso a base de datos
- lógica HTTP
- lógica de UI

---

## 2.4 `/source/ServiceDefaults`

Contiene configuración transversal compartida entre servicios.

Debe contener:

- defaults técnicos
- observabilidad base
- configuración compartida entre servicios

No debe contener:

- reglas funcionales
- entidades del dominio
- comportamiento específico de un caso de uso

---

## 2.5 `/source/Backend`

Contiene el backend del sistema.

### `/source/Backend/Api`

Proyecto de entrada HTTP.

Debe contener:

- `Program.cs`
- controllers
- endpoints
- middleware
- configuración HTTP
- composición final de dependencias

No debe contener:

- reglas de dominio complejas
- decisiones funcionales profundas
- persistencia incrustada en controllers

Regla práctica:
los controllers deben coordinar, no decidir el negocio.

---

### `/source/Backend/Application`

Contiene los casos de uso del sistema.

Debe contener:

- commands
- queries
- handlers
- validators
- contratos de aplicación
- DTOs/resultados de aplicación
- orquestación funcional

No debe contener:

- detalles concretos de persistencia
- acceso directo a SQL
- comportamiento de UI
- implementaciones técnicas de infraestructura

Aquí se expresa **qué hace el sistema**.

#### Convención recomendada

Commands
- `CreatePackingListCommand`
- `Scan1Command`

Queries
- `GetPackingListByIdQuery`
- `GetOpenPackingListsQuery`

Handlers
- `CreatePackingListCommandHandler`
- `GetPackingListByIdQueryHandler`

Validators
- `CreatePackingListCommandValidator`

Results / responses
- `Scan1Result`
- `PackingListDetailResponse`

Evitar nombres vagos como:

- `Service`
- `Manager`
- `Processor`

si en realidad representan un caso de uso concreto.

---

### `/source/Backend/Domain`

Contiene el núcleo del negocio.

Debe contener:

- entidades
- value objects
- enums
- reglas de dominio
- invariantes
- contratos del dominio si aplica
- excepciones de dominio si aplica

No debe contener:

- EF Core
- HTTP
- UI
- detalles de infraestructura
- dependencias a frameworks técnicos innecesarios

Aquí se expresa **qué es el negocio**.

---

### `/source/Backend/Infrastructure`

Contiene las implementaciones técnicas.

Debe contener:

- `DbContext`
- repositorios
- persistencia
- integraciones externas
- configuración EF Core
- servicios técnicos
- seguridad técnica
- acceso a archivos, Excel, DB, mensajería, etc.

No debe contener:

- reglas nucleares del negocio
- decisiones funcionales profundas escondidas en código técnico
- lógica de UI

#### Convención recomendada

Implementaciones:
- `PartRepository`
- `PackingListRepository`
- `ExcelImportService`
- `SqlServerUnitOfWork`

Configuraciones EF:
- `PartConfiguration`
- `PackingListConfiguration`

Contexto:
- `AppDbContext`

---

## 2.6 `/source/Frontend`

Contiene el frontend del sistema.

### `/source/Frontend/Web`

Proyecto frontend principal.

Debe contener:

- páginas
- componentes
- layouts
- servicios HTTP de cliente
- configuración del frontend
- estado de UI
- navegación

No debe contener:

- acceso directo a base de datos
- lógica de dominio crítica
- reglas de negocio profundas que deben vivir en backend
- secretos

#### Convención recomendada

Componentes:
- `ScanPanel.razor`
- `PackingListTable.razor`
- `AdminSidebar.razor`

Páginas:
- `Pages/Admin/Parts.razor`
- `Pages/Operator/Scan.razor`

Servicios cliente:
- `PartsApiClient`
- `VerificationApiClient`
- `PackingListApiClient`

Evitar nombres demasiado genéricos como:

- `ApiService`
- `DataService`

---

### `/source/Frontend/Shared`

Contiene modelos y utilidades compartidas del frontend.

Debe contener:

- modelos compartidos del cliente
- constantes de UI
- contratos o DTOs compartidos si aplica
- helpers de cliente

No debe contener:

- lógica de negocio crítica
- acceso a infraestructura
- controllers
- persistencia

---

## 2.7 `/test`

Contiene las pruebas del sistema.

### `/test/UnitTests`

Debe contener:

- pruebas de dominio
- pruebas de aplicación
- pruebas de reglas
- pruebas de validación

No debe contener:

- dependencias externas reales innecesarias
- scripts
- pruebas manuales

---

### `/test/IntegrationTests`

Debe contener:

- pruebas de API
- pruebas de persistencia
- pruebas de wiring entre capas
- pruebas de integración con infraestructura controlada

No debe contener:

- resultados manuales
- artefactos temporales
- scripts ad hoc

---

## 2.8 `/docs`

Contiene documentación técnica y funcional.

Debe contener:

- arquitectura
- decisiones técnicas
- convenciones
- flujos funcionales
- restricciones
- lineamientos para agentes
- deuda técnica documentada

No debe contener:

- basura temporal
- capturas sin contexto
- borradores sin clasificar

---

### `/docs/architecture`

Documentación de arquitectura.

Ejemplos:

- capas
- límites
- decisiones estructurales
- integraciones

---

### `/docs/flows`

Documentación de flujos funcionales.

Ejemplos:

- Scan1
- Scan2
- importación de Excel
- packing list
- auditoría

---

### `/docs/database`

Documentación de base de datos.

Ejemplos:

- modelo lógico
- decisiones de persistencia
- convenciones de migraciones
- estrategia de concurrencia

---

### `/docs/decisions`

Registro de decisiones técnicas.

Ejemplos:

- ADRs
- trade-offs
- restricciones
- decisiones aceptadas o pendientes

---

## 2.9 `/scripts`

Contiene scripts operativos y de automatización.

Debe contener:

- bootstrap
- setup
- scripts de build
- scripts de base de datos
- scripts de deploy
- automatizaciones repetibles

No debe contener:

- lógica productiva del sistema
- scripts experimentales sin contexto
- archivos sueltos sin clasificar

Todo script debe ser, cuando sea posible:

- reproducible
- idempotente
- seguro de reejecutar
- claro en su propósito

---

## 2.10 `/build`

Contiene archivos relacionados con compilación y pipeline.

Debe contener:

- props compartidos
- configuración de CI/CD
- packaging
- helpers de build

No debe contener:

- lógica funcional
- documentación de negocio
- código del producto

---

## 2.11 `/tools`

Contiene utilidades auxiliares de desarrollo.

Debe contener:

- generadores
- herramientas locales
- utilidades de apoyo

No debe contener:

- código principal del sistema
- pruebas
- documentación del producto

---

# 3. Arquitectura

## 3.1 Capas principales

La solución está organizada en estas capas:

- Frontend
- Api
- Application
- Domain
- Infrastructure
- AppHost

---

## 3.2 Intención de cada capa

### Domain
Contiene el núcleo del negocio y sus reglas.

### Application
Coordina casos de uso y flujos funcionales.

### Infrastructure
Implementa persistencia e integraciones técnicas.

### Api
Expone el sistema por HTTP.

### Frontend
Consume la API y presenta la experiencia de usuario.

### AppHost
Orquesta el entorno local y los recursos con Aspire.

---

## 3.3 Dependencias permitidas

Permitido:

- `Api` depende de `Application`
- `Api` depende de `Infrastructure`
- `Application` depende de `Domain`
- `Infrastructure` depende de `Application`
- `Infrastructure` depende de `Domain`
- `Frontend` consume `Api` por HTTP
- `AppHost` referencia proyectos que orquesta

---

## 3.4 Dependencias no permitidas

No permitido:

- `Domain` depende de `Infrastructure`
- `Domain` depende de `Api`
- `Domain` depende de `Frontend`
- `Application` depende de implementaciones concretas de infraestructura
- `Frontend` accede directamente a base de datos
- `Controllers` con lógica de dominio compleja
- `Infrastructure` decidiendo reglas nucleares del negocio

---

# 4. Reglas de diseño por capa

## 4.1 Domain

Un agente o desarrollador puede:

- crear entidades
- crear value objects
- crear enums
- crear reglas
- agregar invariantes
- modelar excepciones de dominio

No debe:

- introducir EF Core
- introducir lógica HTTP
- introducir lógica de UI
- acoplar el dominio a infraestructura

---

## 4.2 Application

Puede:

- crear commands
- crear queries
- crear handlers
- crear validators
- crear contratos de puertos
- mapear resultados
- orquestar casos de uso

No debe:

- hablar con SQL directamente
- depender de clases concretas de infraestructura
- meter lógica UI
- duplicar reglas que pertenecen a Domain

---

## 4.3 Infrastructure

Puede:

- implementar repositorios
- configurar EF Core
- crear `DbContext`
- integrar servicios externos
- implementar puertos definidos por la aplicación

No debe:

- redefinir reglas centrales del dominio
- esconder decisiones funcionales profundas dentro de código técnico

---

## 4.4 Api

Puede:

- crear controllers
- exponer endpoints
- configurar middleware
- registrar dependencias
- traducir entrada/salida HTTP

No debe:

- concentrar lógica compleja de negocio
- duplicar validación de aplicación y dominio sin necesidad
- acceder a DB sin pasar por la capa correcta

### Convención recomendada

Controllers:
- `PartsController`
- `VerificationController`
- `PackingListsController`

Endpoints:
- `/api/parts`
- `/api/packing-lists`
- `/api/verification`

No mezclar estilos de naming en rutas.

---

## 4.5 Frontend

Puede:

- crear páginas
- crear componentes
- crear clientes HTTP
- construir estado de UI
- mejorar navegación y layout
- documentar la integración con API

No debe:

- duplicar reglas críticas del dominio salvo validaciones de UX
- inventar contratos distintos a la API real
- almacenar secretos
- acceder directo a base de datos

---

# 5. Lineamientos para agentes

## 5.1 Qué puede hacer un agente

Un agente puede:

- crear código en la capa correcta
- refactorizar manteniendo arquitectura
- mejorar organización
- agregar documentación
- crear pruebas
- proponer mejoras
- crear scripts repetibles
- documentar riesgos, limitaciones y deuda técnica

---

## 5.2 Qué no puede hacer un agente sin instrucción explícita

Un agente no debe:

- cambiar la arquitectura base sin documentarlo
- introducir una nueva tecnología grande sin justificarlo
- mover masivamente proyectos o carpetas sin razón
- cambiar proveedor de base de datos sin instrucción
- alterar contratos públicos sin indicar impacto
- inventar reglas de negocio
- documentar comportamientos no implementados como si fueran reales
- borrar documentación importante sin reemplazo

---

## 5.3 Qué puede documentar un agente

Puede documentar:

- arquitectura
- estructura del repositorio
- flujos funcionales
- convenciones
- decisiones técnicas
- riesgos
- limitaciones
- deuda técnica
- ADRs
- onboarding
- lineamientos para agentes

---

## 5.4 Qué no debe documentar como hecho

No debe documentar como confirmado:

- comportamiento no implementado
- decisiones no aprobadas
- integraciones no existentes
- seguridad no implementada
- flujos futuros como si ya fueran vigentes

Si algo no está cerrado, debe marcarse como:

- propuesta
- supuesto
- pendiente
- borrador
- no aprobado

---

## 5.5 Regla de honestidad técnica

Si algo no puede verificarse, el agente debe decirlo.

Aceptable:

- no implementado todavía
- pendiente de definición
- propuesta no aprobada
- supuesto de trabajo
- requiere validación

No aceptable:
- disfrazar incertidumbre como certeza

---

## 5.6 Regla operativa para agentes

Antes de crear o modificar algo, el agente debe preguntarse:

1. ¿En qué capa pertenece realmente esto?
2. ¿Estoy moviendo una responsabilidad a una capa incorrecta?
3. ¿Esto describe el estado actual o una propuesta?
4. ¿Estoy agregando una dependencia innecesaria?
5. ¿Lo que escribo puede verificarse en el repo?

---

# 6. Convenciones de código

## 6.1 Principios generales

El código debe priorizar:

- claridad
- intención explícita
- cohesión alta
- bajo acoplamiento
- mantenibilidad
- consistencia

Es preferible código simple y explícito antes que código ingenioso y difícil de leer.

---

## 6.2 Convenciones de nombres

### Clases, interfaces, enums y métodos
Usar `PascalCase`.

Ejemplos:

- `Part`
- `PackingList`
- `VerificationResult`
- `CreatePackingListCommand`
- `ScanLabelAsync`

---

### Campos privados
Usar `_camelCase`.

Ejemplos:

- `_repository`
- `_logger`
- `_httpClient`

---

### Variables locales y parámetros
Usar `camelCase`.

Ejemplos:

- `partNumber`
- `packingListId`
- `currentUser`

---

### Interfaces
Usar prefijo `I`.

Ejemplos:

- `IPartRepository`
- `IVerificationService`
- `IApplicationClock`

---

### Enums
Usar `PascalCase` para tipo y miembros.

Ejemplos:

- `PackingListStatus`
- `Pending`
- `Closed`
- `Cancelled`

---

### Constantes
Usar `PascalCase`.

Ejemplos:

- `MaxLabelLength`
- `DefaultPageSize`

---

## 6.3 Namespaces

El namespace debe reflejar la estructura real del proyecto.

Ejemplos:

- `LabelVerificationSystem.Domain.Entities`
- `LabelVerificationSystem.Application.Features.Verification.Scan1`
- `LabelVerificationSystem.Infrastructure.Persistence`
- `LabelVerificationSystem.Web.Pages.Admin`

Evitar namespaces genéricos como:

- `Common`
- `Utils`
- `Helpers`

salvo que el contenido sea realmente transversal y claro.

---

# 7. Convenciones por capa

## 7.1 Domain

Preferir:

- `Part`
- `PackingList`
- `LabelData`

Evitar sufijos innecesarios como:

- `PartEntity`

salvo que aporten una distinción real.

---

## 7.2 Application

Organizar por feature o caso de uso.

Ejemplo:

```text
Features/
  Verification/
    Scan1/
      Scan1Command.cs
      Scan1CommandHandler.cs
      Scan1CommandValidator.cs
      Scan1Result.cs```
## Convención recomendada

### Commands
- `CreatePackingListCommand`
- `Scan1Command`

### Queries
- `GetPackingListByIdQuery`
- `GetOpenPackingListsQuery`

### Handlers
- `CreatePackingListCommandHandler`
- `GetPackingListByIdQueryHandler`

### Validators
- `CreatePackingListCommandValidator`

### Results / responses
- `Scan1Result`
- `PackingListDetailResponse`

Evitar nombres vagos como:

- `Service`
- `Manager`
- `Processor`

si en realidad representan un caso de uso concreto.

---

## 4.3 Infrastructure

Nombrar implementaciones según el contrato o responsabilidad técnica.

### Ejemplos
- `PartRepository`
- `PackingListRepository`
- `ExcelImportService`
- `SqlServerUnitOfWork`

### Configuraciones EF
- `PartConfiguration`
- `PackingListConfiguration`

### Contexto
- `AppDbContext`

---

## 4.4 Api

### Controllers

Nombrar con sufijo `Controller`.

#### Ejemplos
- `PartsController`
- `VerificationController`
- `PackingListsController`

### Endpoints

Usar rutas consistentes, pluralizadas cuando tenga sentido.

#### Ejemplos
- `/api/parts`
- `/api/packing-lists`
- `/api/verification`

No mezclar estilos de naming en rutas.

---

## 4.5 Frontend

### Componentes

Usar `PascalCase`.

#### Ejemplos
- `ScanPanel.razor`
- `PackingListTable.razor`
- `AdminSidebar.razor`

### Páginas

Ubicar según módulo funcional.

#### Ejemplos
- `Pages/Admin/Parts.razor`
- `Pages/Operator/Scan.razor`

### Servicios cliente

Nombrar según recurso o intención.

#### Ejemplos
- `PartsApiClient`
- `VerificationApiClient`
- `PackingListApiClient`

Evitar nombres demasiado genéricos como:

- `ApiService`
- `DataService`

---

## 5. Convenciones de métodos

### 5.1 Métodos asíncronos

Deben terminar en `Async`.

#### Ejemplos
- `GetByIdAsync`
- `ValidateLabelAsync`
- `CreatePackingListAsync`

No omitir `Async`.

### 5.2 Métodos booleanos

Preferir nombres que expresen condición.

#### Ejemplos
- `IsClosed`
- `HasErrors`
- `CanBePacked`
- `RequiresSecondScan`

### 5.3 Métodos de comandos

Preferir verbos claros.

#### Ejemplos
- `Create`
- `Close`
- `Validate`
- `Assign`
- `Import`

No usar verbos vagos como:

- `HandleData`
- `Manage`
- `DoWork`

---

## 6. DTOs, Requests y Responses

### 6.1 Sufijos

Usar sufijos explícitos cuando aplique:

- `Dto`
- `Request`
- `Response`

#### Ejemplos
- `PartDto`
- `CreatePackingListRequest`
- `PackingListResponse`

### 6.2 Regla práctica

- si cruza fronteras HTTP: `Request` / `Response`
- si es transporte interno o compartido: `Dto`
- si es resultado de caso de uso: nombre específico o `Result`

---

## 7. Validaciones

### 7.1 Validaciones de aplicación

Usar validators dedicados.

#### Ejemplos
- `Scan1CommandValidator`
- `CreateUserCommandValidator`

No meter validación extensa dentro de controllers.

### 7.2 Validaciones de dominio

Las invariantes reales del negocio deben vivir en `Domain`.

#### Ejemplo
Si un `PackingList` cerrado no admite líneas nuevas, esa regla pertenece al dominio.

---

## 8. Excepciones

### 8.1 Nombrado

Usar sufijo `Exception`.

#### Ejemplos
- `BusinessRuleViolationException`
- `PackingListClosedException`
- `EntityNotFoundException`

### 8.2 Uso

No usar excepciones para flujo normal del sistema cuando un resultado explícito sea suficiente.

Preferir resultados claros cuando el escenario sea esperado.

Usar excepción cuando:

- hay una violación real
- hay un estado inválido
- no puede continuarse de forma segura

---

## 9. Logging

### 9.1 Reglas

Los logs deben ser:

- útiles
- concretos
- trazables
- sin información sensible innecesaria

### 9.2 No loguear

No loguear:

- contraseñas
- tokens
- secretos
- cadenas de conexión completas
- datos sensibles innecesarios

### 9.3 Mensajes

Usar mensajes orientados a contexto.

Preferir:

`"Packing list {PackingListId} closed by user {UserId}"`

en vez de:

`"Operation done"`

---

## 10. Comentarios

### 10.1 Regla general

Preferir código claro antes que comentarios explicativos de obviedades.

No comentar lo evidente.

Evitar:

```csharp
// Incrementa en uno
counter++;```

### 10.2 Cuándo sí comentar

Sí comentar cuando haga falta explicar:

- una decisión no obvia
- una restricción de negocio
- un workaround
- una integración delicada
- un motivo arquitectónico

---

## 11. Estructura de archivos

### 11.1 Una responsabilidad clara por archivo

Evitar archivos con múltiples conceptos no relacionados.

### 11.2 Orden sugerido en clases

- campos privados
- constantes
- constructor
- propiedades públicas
- métodos públicos
- métodos privados

---

## 12. Convenciones de pruebas

### 12.1 Nombres de tests

Usar nombres descriptivos.

Formato sugerido:

```text
Metodo_Escenario_ResultadoEsperado

Ejemplos
AddLine_WhenPackingListIsClosed_ShouldThrowException
ValidateLabel_WhenBarcodeIsInvalid_ShouldReturnFailure
```
### 12.2 Estructura

Preferir patrón:

Arrange
Act
Assert

Separado claramente.

### 12.3 Qué probar

Prioridad:

reglas de dominio
casos de uso
validaciones
integración entre capas

## 13. Convenciones de DI

Registrar dependencias en el lugar correcto.

Application

Solo contratos y wiring de aplicación.

Infrastructure

Implementaciones concretas.

Api

Composición final.

Evitar registrar todo en cualquier sitio sin criterio.

## 14. Convenciones de configuración
### 14.1 Archivos de configuración

Usar:

appsettings.json
appsettings.Development.json

No hardcodear:

URLs
cadenas de conexión
secretos
flags de entorno

### 14.2 Nombres de secciones

Usar nombres claros y agrupados.

Ejemplos
ConnectionStrings
Jwt
ExcelImport
Scanner
Features


## 15. Convenciones para agentes

Los agentes deben seguir estas reglas.

Sí hacer
respetar nombres existentes
seguir el estilo del proyecto
crear archivos donde corresponde
usar nombres explícitos
documentar supuestos
distinguir hecho de propuesta
No hacer
inventar abreviaturas
mezclar capas
crear utilidades genéricas sin necesidad
introducir nombres ambiguos
escribir código “mágico” o críptico
documentar como hecho algo no validado

## 16. Reglas de simplicidad

Preferir:

composición sobre complejidad innecesaria
nombres largos pero claros sobre nombres cortos ambiguos
pocos patrones bien usados sobre sobreingeniería
cambios pequeños y revisables

Evitar:

sobreabstracción
patrones innecesarios
acoplamiento transversal sin necesidad
refactors masivos silenciosos

## 17. Convención de cambios seguros

Los cambios preferidos son:

pequeños
reversibles
localizados
documentados

Evitar cambios que sean:

masivos
silenciosos
transversales sin necesidad
difíciles de revisar


## 18. Qué hacer cuando una decisión no está clara

Si una decisión no está cerrada:

Sí hacer
listar alternativas
explicar trade-offs
dejar recomendación
indicar impacto
No hacer
inventar una decisión final
presentar una propuesta como decisión aprobada
introducir complejidad sin justificarla

## 19. Alcance de documentación permitido
Permitido
describir el estado actual
proponer mejoras
documentar limitaciones
documentar deuda técnica
registrar decisiones
No permitido
inventar roadmap oficial
prometer implementaciones futuras
asumir aprobaciones no dadas
documentar integraciones no existentes

## 20. Regla de honestidad técnica

Si algo no puede verificarse, debe indicarse explícitamente.

Aceptable:

no implementado todavía
pendiente de definición
propuesta no aprobada
supuesto de trabajo
requiere validación

No aceptable:

disfrazar incertidumbre como certeza

## 21. Reglas para documentación de agentes

Todo documento generado debe ser:

claro
verificable
honesto
explícito en supuestos
alineado con el repositorio

Debe distinguir entre:

estado actual
propuesta
decisión pendiente

## 22. Criterio de calidad

Antes de agregar o modificar código o documentación:

¿Se entiende qué hace?
¿Se entiende dónde pertenece?
¿Respeta la arquitectura?
¿Introduce complejidad innecesaria?
¿Es consistente con el resto del proyecto?

## 23. Regla final

Si una decisión de naming, estructura o implementación no está clara, elegir la opción que mejor responda:

¿Se entiende qué hace?
¿Se entiende dónde pertenece?
¿Se entiende por qué existe?
¿Reduce ambigüedad?
¿Respeta la arquitectura?
