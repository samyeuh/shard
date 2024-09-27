using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api;

public class PlanetSpecification
{
    public string Name { get; }
    public int Size { get; }
    
    public Dictionary<ResourceKind, int> ResourceQuantity { get; }

    internal PlanetSpecification(Random random)
    {
        Name = random.NextGuid().ToString();
                   
        Size = 1 + random.Next(999);
        ResourceQuantity = new RandomShareComputer(random).GenerateResources(Size);
    }
}