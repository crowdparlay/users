using System.Text;
using CrowdParlay.Users.Application.Abstractions.Communication;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace CrowdParlay.Users.Infrastructure.Communication.Services;

public class RabbitMqExchange : IMessageDestination
{
    private readonly string _exchange;
    private readonly IConnectionFactory _connectionFactory;

    public RabbitMqExchange(string exchange, IConnectionFactory connectionFactory)
    {
        _exchange = exchange;
        _connectionFactory = connectionFactory;
    }

    public void Publish(object message)
    {
        var connection = _connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();
        
        var json = JsonConvert.SerializeObject(message);
        var body = Encoding.UTF8.GetBytes(json);
        
        channel.BasicPublish(_exchange, "users.created", body: body);
    }
}