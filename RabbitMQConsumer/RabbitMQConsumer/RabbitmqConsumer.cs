using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace RabbitMQConsumer;

public static class RabbitmqConsumer
{
    public static async Task ConsumeAsync()
    {
        var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "host.docker.internal";
        
        Console.WriteLine(rabbitMqHost);

        //Bağlantı oluşturma adımı
        var factory = new ConnectionFactory() {
            HostName = rabbitMqHost,
            UserName = "guest",
            Password = "guest" ,           
        };

        //Bağlantı aktifleştirme ve kanal ekleme
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        //Consume edilecek queue'u tanımlıyoruz.
        channel.QueueDeclare(queue: "task_queue",
                             exclusive: false,
                             autoDelete: false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var jsonMessage = JsonSerializer.Deserialize<dynamic>(message);
            Console.WriteLine(" [x] Received {0}", jsonMessage);

            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        };

        //Queue'dan mesaj okuma
        channel.BasicConsume(queue: "task_queue",
                             autoAck: false,
                             consumer: consumer);

        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
