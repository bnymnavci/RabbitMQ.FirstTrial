using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

namespace RabbitMQPublisher;

public static class RabbitMqPublisher
{
    public static async Task PublishAsync()
    {
        var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "host.docker.internal";
        Console.WriteLine(rabbitMqHost);
        //Bağlantı oluşturma adımı
        var factory = new ConnectionFactory() {
            HostName = rabbitMqHost,
            UserName = "guest",
            Password = "guest",
        };

        //Bağlantı aktifleştirme ve kanal ekleme
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        //Queue oluşturma
        channel.QueueDeclare(
            queue: "task_queue",
            exclusive: false,
            autoDelete: false
            );

        var random = new Random();

        while (true)
        {
            var message = new
            {
                Text = "Random message",
                Number = random.Next(1000)
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            
            //Queue'a mesaj gönderme
            channel.BasicPublish(exchange: "",
                                 routingKey: "task_queue",
                                 body: body);

            Console.WriteLine(" [x] Sent {0}", JsonSerializer.Serialize(message));

            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}
