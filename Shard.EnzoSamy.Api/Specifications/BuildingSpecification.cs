namespace Shard.EnzoSamy.Api;

public class BuildingSpecification
{
    
    public string Id { get; set; }
    public string Type { get; set; }
    public string Planet { get; set; }
    public string System { get; set; }

    public Random random;
    public BuildingSpecification(string type, string planet, string system)
    {
        random = new Random();
        Id = Guid.NewGuid().ToString();
        Type = type;
        Planet = planet;
        System = system;
    }
    
}