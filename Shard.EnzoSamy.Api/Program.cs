using Microsoft.Extensions.Options;
using Shard.EnzoSamy.Api.Services;
using Shard.EnzoSamy.Api.Specifications;
using Shard.Shared.Core;
using Microsoft.AspNetCore.Authentication;
using Shard.EnzoSamy.Api.Security; 
using MapGenerator = Shard.EnzoSamy.Api.MapGenerator;
using SystemClock = Microsoft.Extensions.Internal.SystemClock;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
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
builder.Services.AddSingleton(new List<FightService.Fight>());
//builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<SectorService>();
builder.Services.AddScoped<UnitService>();
builder.Services.AddScoped<ResourceService>();
builder.Services.AddScoped<FightService>();

// Add authentication service with custom handler
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, CustomAuthHandler>("BasicAuthentication", null);

// Add the admin credentials (replace with secure storage or configuration in production)
builder.Services.AddSingleton(new AdminCredentials("admin", "password"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Ajouter l'authentification
app.UseAuthorization();

app.MapControllers();

app.Run();

namespace Shard.EnzoSamy.Api
{
    public partial class Program { }
}