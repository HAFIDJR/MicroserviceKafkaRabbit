using Confluent.Kafka;
using OrderApi.OrderServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var bootstrapServers = builder.Configuration["Kafka:BootstrapServers"];
var config = new ConsumerConfig
{
    GroupId = "add-product-consumer-group",
    BootstrapServers = bootstrapServers,
    AutoOffsetReset = AutoOffsetReset.Earliest
};
builder.Services.AddSingleton<IConsumer<Null , string>>(x=>new ConsumerBuilder<Null,string>(config).Build());
builder.Services.AddSingleton<IOrderService , OrderServices>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.Run();