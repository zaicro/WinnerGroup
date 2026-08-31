# FunEvents

Prototipo de gestión de eventos y reservas de entradas desarrollado como solución para el ejercicio técnico.

El proyecto implementa una **Web API** para la gestión de usuarios, eventos y reservas, junto con un **cliente de consola** que permite realizar una reserva utilizando un código de evento y un usuario previamente registrados.

## 1. Objetivo

El objetivo principal del ejercicio es implementar un prototipo mínimo de un cliente de consola capaz de realizar la reserva de entradas para un evento a partir de códigos conocidos, utilizando una API Web desarrollada por el candidato.

La solución está organizada en diferentes capas para separar responsabilidades y facilitar su mantenimiento y evolución.

El flujo principal es:

```text
Cliente de Consola
       │
       │ HTTP
       ▼
   FunEvents.Api
       │
       ▼
 FunEvents.Application
       │
       ▼
    Domain
       │
       ▼
Infrastructure.Sql
       │
       ▼
   SQL Server
```

---

## 2. Tecnologías

* **.NET 8**
* **C#**
* **ASP.NET Core Web API**
* **Entity Framework Core**
* **SQL Server**
* **MediatR**
* **Swagger / OpenAPI**
* **API Key Authentication**
* **log4net**
* **NUnit** para pruebas
* **PowerShell** para ejecución y administración durante el desarrollo

La solución utiliza .NET 8 como framework principal y mantiene una separación entre dominio, aplicación, infraestructura y API.

---

## 3. Estructura de la solución

La solución se encuentra organizada de la siguiente manera:

```text
FunEvents.Api.sln
│
├── src
│   │
│   ├── FunEvents.Api
│   │   ├── Authentication
│   │   ├── Configuration
│   │   ├── Controllers
│   │   │   └── v1
│   │   ├── Middleware
│   │   └── Program.cs
│   │
│   ├── FunEvents.Application
│   │   ├── Features
│   │   │   ├── Event
│   │   │   ├── Reservation
│   │   │   └── User
│   │   └── Wrappers
│   │
│   ├── FunEvents.Domain
│   │   ├── DTOs
│   │   ├── Entities
│   │   ├── Enums
│   │   ├── Interfaces
│   │   └── Repositories
│   │
│   ├── FunEvents.Infrastructure.Sql
│   │   ├── Configurations
│   │   ├── Contexts
│   │   ├── Migrations
│   │   ├── Repositories
│   │   └── Tables
│   │
│   ├── FunEvents.Console
│   │
│   ├── FunEvents.Application.Tests
│   ├── FunEvents.Infrastructure.Sql.Tests
│   └── FunEvents.Logging.Tests
│
└── docs
    ├── API contracts
    └── Console execution images
```

La capa de dominio contiene las entidades principales `Event`, `Reservation` y `User`, además de los enums asociados a estados y canales de reserva.

## La infraestructura SQL implementa el acceso a datos mediante `FunEventsDbContext`, repositorios específicos y Unit of Work. También contiene la migración inicial de la base de datos.

## 4. Capas

### Domain

Contiene las reglas y elementos centrales del dominio:

* Entidades.
* Enumeraciones.
* Interfaces.
* Contratos de repositorio.
* Abstracciones necesarias para desacoplar el dominio de la infraestructura.

Las entidades principales son:

* `Event`
* `Reservation`
* `User`

También se definen los estados de eventos y reservas y el canal utilizado para realizar una reserva.

### Application

Contiene los casos de uso de la aplicación.

La implementación está organizada por funcionalidad:

```text
Features
├── Event
├── Reservation
└── User
```

Cada funcionalidad utiliza comandos, consultas, handlers, DTOs y servicios.

Para reservas se implementan operaciones relacionadas con:

* Creación.
* Actualización.
* Consulta.
* Obtención de reservas.
* Validaciones propias del proceso de reserva.

La separación mediante handlers y servicios permite mantener la lógica de negocio fuera de los controladores.

### Infrastructure.Sql

Implementa la persistencia utilizando Entity Framework Core y SQL Server.

Incluye:

* `FunEventsDbContext`
* Configuraciones de entidades.
* Tablas de persistencia.
* Repositorios.
* Unit of Work.
* Migraciones.

La infraestructura contiene configuraciones específicas para eventos, reservas y usuarios.

### API

Expone los casos de uso mediante endpoints HTTP.

Los controladores están organizados por versión:

```text
Controllers
└── v1
    ├── EventController
    ├── ReservationController
    └── UserController
```

También se implementan:

* Versionamiento de API.
* Swagger/OpenAPI.
* Autenticación mediante API Key.
* Middleware centralizado para manejo de excepciones.
* Configuración centralizada de dependencias.

Los componentes de autenticación se encuentran separados dentro de la API.

### Console

El proyecto `FunEvents.Console` contiene el cliente mínimo de consola solicitado en el ejercicio.

El cliente realiza llamadas HTTP contra la API para ejecutar el flujo de reserva.

El proyecto contiene un `Program.cs` y genera el ejecutable correspondiente para .NET 8.

---

## 5. Flujo principal de reserva

El flujo implementado para cumplir el objetivo del ejercicio es:

```text
1. Ejecutar la API
       │
       ▼
2. Ejecutar el cliente de consola
       │
       ▼
3. Ingresar/usar los datos conocidos
       │
       ├── Usuario
       └── Evento
       │
       ▼
4. El cliente construye la solicitud HTTP
       │
       ▼
5. La API recibe la solicitud
       │
       ▼
6. Application procesa el caso de uso
       │
       ▼
7. Se validan usuario y evento
       │
       ▼
8. Se crea la reserva
       │
       ▼
9. Se persiste en SQL Server
       │
       ▼
10. La API devuelve el resultado
       │
       ▼
11. El cliente muestra la respuesta
```

Este flujo permite demostrar la integración completa entre el cliente de consola, la API, la capa de aplicación y la persistencia.

---

## 6. API

La API se encuentra versionada bajo `/api/v1`.

Para la funcionalidad de reservas, el endpoint utilizado por el cliente para consultar las reservas es:

```http
GET /api/v1/Reservation/getAll
```

Ejemplo:

```bash
curl --location 'https://localhost:7023/api/v1/Reservation/getAll' \
--header 'accept: */*'
```

La creación y actualización de reservas forman parte de la API y se encuentran implementadas dentro de `ReservationController`.

Los contratos completos de las operaciones disponibles se encuentran en:

```text
docs/
```

La documentación de contratos debe ser considerada la referencia para consultar los request y response definitivos de la API.

---

## 7. Swagger / OpenAPI

La API incluye Swagger para facilitar la exploración y prueba de los endpoints.

Al ejecutar la aplicación en ambiente de desarrollo, Swagger permite:

* Consultar los endpoints.
* Revisar los modelos.
* Ejecutar solicitudes.
* Validar las respuestas.
* Revisar los contratos expuestos por la API.

Los contratos también fueron incluidos dentro de la carpeta `docs`.

---

## 8. Base de datos

La persistencia se implementó utilizando **SQL Server** y **Entity Framework Core**.

Las principales entidades persistidas son:

```text
TbEvents
TbReservations
TbUsers
```

La infraestructura contiene las configuraciones correspondientes a estas tablas y una migración inicial:

```text
FunEvents.Infrastructure.Sql
└── Migrations
    ├── 20260816024634_Init.cs
    ├── 20260816024634_Init.Designer.cs
    └── FunEventsDbContextModelSnapshot.cs
```

La migración inicial se encuentra incluida en el proyecto.

---

## 9. Configuración

La configuración de la API se encuentra en:

```text
FunEvents.Api
├── appsettings.json
└── appsettings.Development.json
```

La configuración contiene los parámetros necesarios para ejecutar la aplicación, incluyendo la conexión a la base de datos y la configuración correspondiente a la API.

> Antes de ejecutar el proyecto, verificar que la cadena de conexión apunte a una instancia de SQL Server disponible en el entorno local.

---

## 10. Ejecución

### Requisitos

Para ejecutar el proyecto se requiere:

* .NET 8 SDK.
* SQL Server.
* Visual Studio 2022, Visual Studio Code u otro IDE compatible.
* Una instancia de SQL Server disponible.

### Restaurar dependencias

Desde la carpeta que contiene la solución:

```powershell
dotnet restore
```

### Compilar

```powershell
dotnet build
```

### Ejecutar la API

```powershell
dotnet run --project .\src\FunEvents.Api
```

La URL exacta dependerá de la configuración de lanzamiento del proyecto.

Una vez iniciada la API, validar que Swagger esté disponible.

---

## 11. Base de datos y migraciones

Si la base de datos aún no ha sido creada, aplicar la migración incluida en el proyecto de infraestructura.

Desde Visual Studio Package Manager Console:

```powershell
Update-Database `
    -Project FunEvents.Infrastructure.Sql `
    -StartupProject FunEvents.Infrastructure.Sql
```

También es posible utilizar las herramientas de Entity Framework Core mediante CLI según la configuración local del entorno.

La solución incluye una `FunEventsDbContextFactory`, lo que permite trabajar con Entity Framework Core en tiempo de diseño.

---

## 12. Ejecutar el cliente de consola

Con la API ejecutándose, abrir una segunda terminal y ejecutar:

```powershell
dotnet run --project .\src\FunEvents.Console
```

El cliente realizará las solicitudes contra la API configurada y permitirá ejecutar el flujo de reserva.

El proyecto genera el ejecutable de consola para .NET 8.

### Evidencia de ejecución

El paso a paso de la ejecución del cliente de consola se encuentra documentado mediante imágenes dentro de:

```text
docs/
```

Estas imágenes muestran el flujo de interacción del cliente y la respuesta obtenida durante la ejecución.

---

## 13. Documentación

La carpeta `docs` contiene material complementario para revisar la solución:

```text
docs/
├── Contratos del API
└── Imágenes de ejecución del cliente de consola
```

### Contratos del API

Contiene los contratos utilizados para documentar las operaciones expuestas por la API.

### Evidencia del cliente de consola

Contiene las imágenes correspondientes al paso a paso de la ejecución del cliente de consola y permite verificar visualmente el flujo de reserva.

---

## 14. Manejo de errores

La API utiliza un middleware centralizado para el manejo de excepciones.

Esto permite evitar que cada controlador tenga que implementar individualmente la gestión de errores y mantener un formato consistente de respuesta.

La aplicación utiliza además un wrapper de respuesta para mantener una estructura uniforme en las respuestas de los casos de uso.

---

## 15. Logging

El proyecto incorpora un componente independiente de logging basado en **log4net**.

La configuración de logging se mantiene separada de la lógica de negocio y de la API.

Esto permite registrar información relevante durante la ejecución y facilita el diagnóstico de errores.

---

## 16. Pruebas

La solución incluye proyectos de pruebas para diferentes componentes:

```text
FunEvents.Application.Tests
FunEvents.Infrastructure.Sql.Tests
FunEvents.Logging.Tests
```

Las pruebas utilizan NUnit.

Para ejecutar las pruebas:

```powershell
dotnet test
```

Las pruebas se mantienen separadas por responsabilidad, de acuerdo con las capas de la solución.

---

## 17. Decisiones técnicas

### .NET 8

Se utilizó .NET 8, cumpliendo con la recomendación del ejercicio de utilizar una versión moderna de .NET.

### SQL Server

Se utilizó SQL Server como motor de persistencia. El enunciado permitía utilizar PostgreSQL si se consideraba necesario, pero no lo establecía como requisito.

### Arquitectura por capas

Se optó por separar:

```text
API
Application
Domain
Infrastructure
```

Esto permite mantener separadas las responsabilidades y facilita la evolución del prototipo.

### CQRS / MediatR

La capa de aplicación organiza los casos de uso mediante Commands, Queries y Handlers, utilizando MediatR como mecanismo de desacoplamiento.

### Repository + Unit of Work

El acceso a datos se abstrae mediante interfaces de repositorio definidas en el dominio e implementadas en infraestructura.

El dominio contiene las abstracciones `IEventRepository`, `IReservationRepository`, `IUserRepository` e `IUnitOfWork`.

---

## 18. Alcance

La solución se mantiene intencionalmente como un **prototipo**, de acuerdo con el objetivo del ejercicio.

El foco principal es demostrar:

* Desarrollo de una API Web.
* Persistencia de información.
* Separación de responsabilidades.
* Implementación de operaciones de eventos, usuarios y reservas.
* Validaciones de negocio.
* Manejo centralizado de errores.
* Autenticación.
* Documentación mediante OpenAPI.
* Integración entre API y cliente de consola.
* Ejecución completa del flujo de reserva.

No se implementaron componentes adicionales de infraestructura como .NET Aspire o Kubernetes, ya que no eran necesarios para cumplir el objetivo del prototipo.

---

## 19. Resultado

El resultado es una solución .NET 8 en la que el cliente de consola se comunica con la API Web para realizar el proceso de reserva, pasando por las capas de aplicación, dominio e infraestructura hasta persistir la información en SQL Server.

La solución queda acompañada por:

* Código fuente.
* Migración de base de datos.
* Contratos del API.
* Documentación Swagger/OpenAPI.
* Proyectos de pruebas.
* Evidencia visual de la ejecución del cliente de consola.

---

## 20. Referencia al ejercicio

La implementación responde al requerimiento de construir un prototipo mínimo de un cliente de consola que realice una reserva de entradas para un evento a partir de un código de evento y un usuario conocidos, utilizando una API Web implementada como parte de la solución.
