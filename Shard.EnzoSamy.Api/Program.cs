using Microsoft.Extensions.Options;
using Shard.EnzoSamy.Api;
using Shard.EnzoSamy.Api.Services;
using Shard.EnzoSamy.Api.Specifications;
using Shard.Shared.Core;
using MapGenerator = Shard.EnzoSamy.Api.MapGenerator;
using MapGeneratorOptions = Shard.EnzoSamy.Api.MapGeneratorOptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<MapGeneratorOptions>(
    builder.Configuration.GetSection("shard:MapGenerator"));

builder.Services.AddSingleton(provider =>
{
    var options = provider.GetRequiredService<IOptions<MapGeneratorOptions>>().Value;
    return new MapGenerator(options).Generate();
});

builder.Services.AddSingleton(new List<UserSpecification>());
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<SectorService>();
builder.Services.AddScoped<UnitService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

namespace Shard.EnzoSamy.Api
{
    public partial class Program { }
}