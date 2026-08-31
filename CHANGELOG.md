# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/2.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [1.1.0] - 2026-08-25

### Added

* Added version 2 of the Event API controller without using MediatR.
* Added direct integration between the Event API controller and Application services through dependency injection.
* Added unit tests for the version 2 Event controller methods.

### Changed

* Simplified the Event request flow in API version 2 by removing MediatR and command/query handlers from the controller execution path.
* Event API version 2 now invokes Application services directly while preserving the existing Application, Domain, and Infrastructure layer separation.

### Deprecated

* N/A.

### Removed

* N/A. MediatR remains available and unchanged in the original API version 1 implementation.

### Fixed

* N/A.

### Security

* API Key authentication remains enabled for Event API endpoints.

## [1.0.0] - 2026-08-16

### Added

* Initial implementation of the FunEvents solution.
* .NET 8 Web API for event, user, and reservation management.
* Versioned API endpoints under `/api/v1`.
* Event management functionality.
* User management functionality.
* Reservation creation and update functionality.
* Reservation query functionality.
* Console client for testing the reservation flow.
* Integration between the console client and the Web API through HTTP.
* SQL Server persistence using Entity Framework Core.
* Entity configurations and database migrations.
* Repository pattern and Unit of Work for data access.
* Application layer organized using commands, queries, and handlers.
* Centralized exception handling middleware.
* Standardized API response structure.
* API Key authentication.
* Swagger/OpenAPI documentation.
* Automated tests for application, infrastructure, and logging components.
* Logging implementation using log4net.
* API contracts documentation under `docs`.
* Console client execution evidence and screenshots under `docs`.

### Changed

* N/A - Initial release.

### Deprecated

* N/A - Initial release.

### Removed

* N/A - Initial release.

### Fixed

* N/A - Initial release.

### Security

* Added API Key authentication to protect API endpoints.
