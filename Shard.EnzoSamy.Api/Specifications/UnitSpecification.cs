using System.Numerics;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api;

public class UnitSpecification
{
    public string Id { get; init; }
    public string Type { get; init; }
    public SystemSpecification system { get; init; }
    public PlanetSpecification planet { get; init; }

    public UnitSpecification() { }

    public UnitSpecification(Random random, IReadOnlyList<SystemSpecification> systemList)
    {
        Id = random.Next(1000).ToString();
        Type = "scout";
        system = systemList[random.Next(systemList.Count)];
        List<PlanetSpecification> planetList = system.Planets.ToList();
        planet = planetList[random.Next(planetList.Count)];
    }
}