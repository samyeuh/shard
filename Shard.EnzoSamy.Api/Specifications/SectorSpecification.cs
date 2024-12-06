using System.Text.Json;
using Shard.EnzoSamy.Api.Specifications;

namespace Shard.EnzoSamy.Api;

public class SectorSpecification
{
    public IReadOnlyList<SystemSpecification> Systems { get; }

    internal SectorSpecification(Random random)
    {
        Systems = Generate(10, random);
    }

    public SectorSpecification(string jsonString)
    {
        Systems = JsonSerializer.Deserialize<List<SystemSpecification>>(jsonString);
    }
    
    private static List<SystemSpecification> Generate(int count, Random random)
    {
        return Enumerable.Range(1, count)
            .Select(_ => new SystemSpecification(random))
            .ToList();
    }
}