using System.Numerics;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api;

public class UnitSpecification
{
    public string Id { get; init; }
    public string Type { get; init; }
    public string System { get; set; }
    public string? Planet { get; set; }

    public UnitSpecification() { }

    public UnitSpecification(Random random, SystemSpecification system, String type)
    {
        Id = random.Next(1000).ToString();
        Type = type;
        SystemSpecification systemSpecification = system;
        System = systemSpecification.Name;
        IReadOnlyList<PlanetSpecification> planetList = systemSpecification.Planets.ToList();
        Planet = planetList[random.Next(planetList.Count)].Name;
    }
}