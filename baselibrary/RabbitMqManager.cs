using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace baselibrary;
public class RabbitMqManager
{
    static string globalQueue = "";
    public static IConnection GetConnection()
    {
        //Random rnd = new Random();
        //var selectedPort = rnd.Next(5672, 5674);
        var ports = new int[] { 5672, 5673, 5674 };
        var selectedPort = ports[Random.Shared.Next(ports.Length)];
        globalQueue = selectedPort.ToString();
        Console.WriteLine(selectedPort);
        var connectionFactory = new ConnectionFactory()
        {
            HostName = "localhost",//rabbitmq-0.host.docker.internal
            UserName = "guest",
            Password = "guest",
            Port = 5672
        };
        //   
        return connectionFactory.CreateConnection();
    }

    public static void Send(string queue, string data)
    {
        using (IConnection connection = GetConnection())
        {
            using (IModel channel = connection.CreateModel())
            {
                channel.QueueDeclare($"{globalQueue}-{queue}", false, false, false, null);
                channel.BasicPublish(string.Empty, $"{globalQueue}-{queue}", null, Encoding.UTF8.GetBytes(data));
            }
        }
    }

    public static void Receive(string queue)
    {
        var rabbitMqConnection = GetConnection();

        var rabbitMqChannel = rabbitMqConnection.CreateModel();

        rabbitMqChannel.QueueDeclare(queue: $"{globalQueue}-{queue}", durable: false, exclusive: false, autoDelete: false, arguments: null);

        //rabbitMqChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);


        var consumer = new EventingBasicConsumer(rabbitMqChannel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(" [x] Received {0}", message);
            rabbitMqChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        };
        rabbitMqChannel.BasicConsume(queue: $"{globalQueue}-{queue}", autoAck: false, consumer: consumer);

    }
}
