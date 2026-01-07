var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.TradingEventsStream>("tradingeventsstream");

builder.Build().Run();
