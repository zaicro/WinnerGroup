# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/2.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [1.1.0] - 2026-08-31

### Added

* Added version 2 of the Event API controller without using MediatR.
* Added direct integration between the Event API controller and Application services through dependency injection.
* Added unit tests for the version 2 Event controller methods.
* Added available capacity tracking for events to support concurrent ticket reservations.
* Added database-level protection against overselling tickets through atomic conditional capacity updates.
* Added transaction handling to ensure capacity updates and reservation creation are committed or rolled back together.
* Added idempotency support for reservation creation using the `Idempotency-Key` request header.
* Added centralized persistence of idempotency keys to prevent duplicate reservation operations.
* Added a unique database constraint for idempotency keys to protect against simultaneous duplicate requests.

### Changed

* Simplified the Event request flow in API version 2 by removing MediatR and command/query handlers from the controller execution path.
* Event API version 2 now invokes Application services directly while preserving the existing Application, Domain, and Infrastructure layer separation.
* Reservation creation now acquires event capacity through an atomic database update before creating the reservation.
* Reservation creation now executes capacity and reservation changes within the same database transaction.

### Deprecated

* N/A.

### Removed

* N/A. MediatR remains available and unchanged in the original API version 1 implementation.

### Fixed

* Prevented concurrent reservation requests from exceeding the available event capacity.
* Prevented repeated reservation requests with the same idempotency key from creating duplicate operations.
* Insufficient event capacity is now handled as a conflict instead of an unexpected server error.

### Security

* API Key authentication remains enabled for protected API endpoints.

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
