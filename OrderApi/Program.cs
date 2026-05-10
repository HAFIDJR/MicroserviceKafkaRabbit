using Confluent.Kafka;
using InventoryApi.BackgroundServices;
using OrderApi.Kafka.Consumers;
using OrderApi.Kafka.Producers;
using OrderApi.Repositories;
using OrderApi.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
// =============================
// Kafka Config
// =============================

var bootstrapServers = builder.Configuration["Kafka:BootstrapServers"];

// =============================
// Kafka Producer
// =============================

builder.Services.AddSingleton<IProducer<string,string>>(_=>
{
    var config = new ProducerConfig
    {
        BootstrapServers = bootstrapServers
    };

    return new ProducerBuilder<string,string>(config).Build();
});


// =============================
// Kafka Consumer
// =============================

// builder.Services.AddSingleton<IConsumer<string, string>>(_ =>
// {
//     var config = new ConsumerConfig
//     {
//         BootstrapServers = bootstrapServers,

//         GroupId = "order-group",

//         AutoOffsetReset =
//             AutoOffsetReset.Earliest
//     };

//     return new ConsumerBuilder<string,string>(
//         config).Build();
// });

// =============================
// Repositories
// =============================

builder.Services.AddSingleton<
    ProductCacheRepository>();

builder.Services.AddSingleton<
    OrderRepository>();

// =============================
// Services
// =============================

builder.Services.AddSingleton<
    OrderProducer>();

builder.Services.AddSingleton<
    OrderService>();

// =============================
// Kafka Consumers
// =============================

builder.Services.AddHostedService<
    ProductConsumer>();

builder.Services.AddHostedService<
    InventoryConsumer>();

    


var app = builder.Build();

// =============================
// Middleware
// =============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.Run();