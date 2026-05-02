using Confluent.Kafka;
using ProductApi.ProductServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var bootstrapServers = builder.Configuration["Kafka:BootstrapServers"];
var config = new ProducerConfig
{
    BootstrapServers = bootstrapServers
};

builder.Services.AddSingleton<IProducer<Null, string>>(sp =>
    new ProducerBuilder<Null, string>(config).Build());

builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();