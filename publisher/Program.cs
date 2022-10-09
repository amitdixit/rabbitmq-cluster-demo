using baselibrary;
using System.Text.Json;


while (true)
{
    Console.WriteLine("enter message");
    var input = Console.ReadLine();
    if (input.ToLower() != "q")
    {
        RabbitMqManager.Send("demo-queue", JsonSerializer.Serialize(new { Input = input, Name = "Amit", Age = 40 }));
    }
    else
    {
        break;
    }
}

