# Crowd Parlay's *users* microservice
 
### Technologies
- Languages `C#` `SQL`
- Frameworks `.NET 7` `ASP.NET Core`
- Persistence `PostgreSQL` `Dapper` `EF Core` `FluentMigrator`
- Testing `xUnit` `Testcontainers` `AutoFixture`
- Other `MassTransit` `RabbitMQ` `OpenIddict` `Mediator` `FluentValidation` `Mapster` `Swashbuckle`
 
### Responsibilities
- User profiles
- Personal preferences management
- OpenID Connect authentication
 
### Running in development environment
1. `docker network create users-network`
1. `docker compose up`
1. The service is now available at `0.0.0.0:8080`

