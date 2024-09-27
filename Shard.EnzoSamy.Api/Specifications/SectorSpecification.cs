using Shard.EnzoSamy.Api.Specifications;

namespace Shard.EnzoSamy.Api;

public class SectorSpecification
{
    public IReadOnlyList<SystemSpecification> Systems { get; }

    internal SectorSpecification(Random random)
    {
        Systems = Generate(10, random);
    }

    private static List<SystemSpecification> Generate(int count, Random random)
    {
        return Enumerable.Range(1, count)
            .Select(_ => new SystemSpecification(random))
            .ToList();
    }
}