# Crowd Parlay's *users* microservice [![Test](https://github.com/crowdparlay/users/actions/workflows/test.yml/badge.svg)](https://github.com/crowdparlay/users/actions/workflows/test.yml)
- **languages:** <kbd>C#</kbd> <kbd>SQL</kbd>
- **frameworks:** <kbd>.NET 7</kbd> <kbd>ASP.NET Core</kbd>
- **persistence:** <kbd>PostgreSQL</kbd> <kbd>Dapper</kbd> <kbd>EF Core</kbd> <kbd>FluentMigrator</kbd>
- **testing:** <kbd>xUnit</kbd> <kbd>Testcontainers</kbd> <kbd>AutoFixture</kbd>
- **other:** <kbd>MassTransit</kbd> <kbd>RabbitMQ</kbd> <kbd>OpenIddict</kbd> <kbd>Mediator</kbd> <kbd>FluentValidation</kbd> <kbd>Mapster</kbd> <kbd>Swashbuckle</kbd>

### Running in development environment
1. `docker network create users-network`
1. `docker compose up`
1. The service is now available at `0.0.0.0:8080`
