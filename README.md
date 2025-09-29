# TMApi

The project is based on a microservice architecture and implements a task management system with support for user roles, assignments and notifications.
The API covers the following domain areas:

## Domains
- **UserService** – user and role management.
- **AuthenticationService** – authentication and authorization with JWT.
- **TaskService** – creation and management of tasks, assignment of performers.
- **NotificationService** – sending notifications with gRPC and Redis.
- **ChatService** – message exchange between the task creator and the assigned.

## Architecture
- **ASP.NET Core 8.0** is the basis for microservices.
- **gRPC** – interaction between services.
- **EF Core** – access to the database.
- **PostgreSQL** is the main database.
- **Redis** – cache and temporary storage of notifications.
- **Docker + Docker Compose** – containerization and launch.
- **Swagger** – REST API documentation.

## Launch:
docker-compose up --build
