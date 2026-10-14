# TMApi

The project is based on a microservice architecture and implements a task management system with support for user roles, assignments and notifications.
The API covers the following domain areas:

## Domains
- **UserService** – user and role management.
- **AuthenticationService** – authentication and authorization with JWT.
- **TaskService** – creation and management of tasks, assignment of performers.
- **NotificationService** – consuming RabbitMQ events and storing notifications in Redis.
- **ChatService** – message exchange between the task creator and the assigned.

## Architecture
- **ASP.NET Core 8.0** is the basis for microservices.
- **gRPC** – synchronous user lookups, authentication and sessions.
- **RabbitMQ** – task assignment and private chat events, with a PostgreSQL outbox, retries and deduplication.
- **EF Core** – access to the database.
- **PostgreSQL** is the main database.
- **Redis** – cache and temporary storage of notifications.
- **Docker + Docker Compose** – containerization and launch.
- **Swagger** – REST API documentation.

## Launch:
Before starting this branch, apply the additive [outbox SQL migration](database/migrations/20260907_notification_outbox.sql).оь

```sh
docker compose up --build
```