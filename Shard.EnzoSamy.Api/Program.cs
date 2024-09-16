using Shard.EnzoSamy.Api;
using Shard.Shared.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enregistrer SectorSpecification comme singleton
builder.Services.AddSingleton(provider =>
{
    return new MapGenerator(new MapGeneratorOptions()
    {
        Seed = "EnzoSamy"
    }).Generate();
});

builder.Services.AddSingleton(new List<UserSpecification>());
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton(new List<UserWithUnitSpecification>());
builder.Services.AddScoped<UserService>();
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