using Confluent.Kafka;
using System;

Console.WriteLine("Testing Kafka Connection...");
Console.WriteLine($"Attempting to connect to: localhost:9092");
Console.WriteLine($"Time: {DateTime.Now}");
Console.WriteLine();

var config = new AdminClientConfig
{
    BootstrapServers = "localhost:9092",
    SocketTimeoutMs = 10000,
    Debug = "broker,topic,msg"
};

try
{
    Console.WriteLine("Creating admin client...");
    using var adminClient = new AdminClientBuilder(config).Build();

    Console.WriteLine("Fetching metadata (10 second timeout)...");
    var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));

    Console.WriteLine($"✅ SUCCESS! Connected to {metadata.Brokers.Count} broker(s)");
    Console.WriteLine();

    foreach (var broker in metadata.Brokers)
    {
        Console.WriteLine($"Broker {broker.BrokerId}:");
        Console.WriteLine($"  Host: {broker.Host}");
        Console.WriteLine($"  Port: {broker.Port}");
    }

    Console.WriteLine($"\nTopics found: {metadata.Topics.Count}");
    foreach (var topic in metadata.Topics)
    {
        Console.WriteLine($"  - {topic.Topic}");
    }
}
catch (KafkaException ex)
{
    Console.WriteLine($"❌ KAFKA ERROR: {ex.Message}");
    Console.WriteLine($"Error Code: {ex.Error.Code}");
    Console.WriteLine($"Error Reason: {ex.Error.Reason}");
    Console.WriteLine($"Is Broker Error: {ex.Error.IsBrokerError}");
    Console.WriteLine($"Is Local Error: {ex.Error.IsLocalError}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ GENERAL ERROR: {ex.Message}");
    Console.WriteLine($"Type: {ex.GetType().Name}");
    Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();