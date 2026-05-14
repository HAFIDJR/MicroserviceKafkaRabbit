using Confluent.Kafka;

using InventoryApi.BackgroundServices;

using OrderApi.Kafka.Consumers;
using OrderApi.Kafka.Producers;

using OrderApi.Messaging.RabbitMQ.Consumers;
using OrderApi.Messaging.RabbitMQ.Producers;

using OrderApi.Repositories;
using OrderApi.Services;

var builder =
    WebApplication.CreateBuilder(args);

// =============================
// Basic Services
// =============================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

// =============================
// Kafka Config
// =============================

var bootstrapServers =
    builder.Configuration[
        "Kafka:BootstrapServers"];

// =============================
// Kafka Producer
// =============================

builder.Services.AddSingleton<
    IProducer<string,string>>(_ =>
{
    var config =
        new ProducerConfig
        {
            BootstrapServers =
                bootstrapServers
        };

    return new ProducerBuilder
        <string,string>(config)
        .Build();
});

// =============================
// Repositories
// =============================

builder.Services.AddSingleton<
    ProductCacheRepository>();

builder.Services.AddSingleton<
    OrderRepository>();

// =============================
// Kafka Producers
// =============================

builder.Services.AddSingleton<
    OrderProducer>();

// =============================
// RabbitMQ Producers
// =============================

builder.Services.AddSingleton<
    NotificationProducer>();

// =============================
// Services
// =============================

builder.Services.AddSingleton<
    OrderService>();

// =============================
// Kafka Consumers
// =============================

builder.Services.AddHostedService<
    ProductConsumer>();

builder.Services.AddHostedService<
    InventoryConsumer>();

// =============================
// RabbitMQ Consumers
// =============================

builder.Services.AddHostedService<
    NotificationConsumer>();

// =============================
// App
// =============================

var app = builder.Build();

// =============================
// Middleware
// =============================

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();