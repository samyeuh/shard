namespace Shard.EnzoSamy.Api.Specifications;

public class BuildingSpecification(string type, string planet, string system)
{
    
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = type;
    public string? Planet { get; set; } = planet;
    public string? System { get; set; } = system;
}