namespace CrowdParlay.Users.Infrastructure.Communication;

public static class RabbitMqConstants
{
    public static class Exchanges
    {
        public const string Users = "users";
    }

    public static class RoutingKeys
    {
        public const string UserCreatedEvent = "users.created";
        public const string UserUpdatedEvent = "users.updated";
        public const string UserDeletedEvent = "users.deleted";
    }
}