﻿using Shard.EnzoSamy.Api.Enumerations;

namespace Shard.EnzoSamy.Api.Services;

public class SectorService(SectorSpecification sectorSpecification, ResourceService resourceService)
{
    public IReadOnlyList<SystemSpecification> GetSystemSpecifications()
    {
        return sectorSpecification.Systems;
    }

    public SystemSpecification GetOneSystem(string systemId)
    {
        var system = sectorSpecification.Systems.FirstOrDefault(s => s.Name == systemId);
        if (system == null)
        {
            throw new InvalidOperationException($"System with ID {systemId} not found.");
        }
        return system;
    }

    public PlanetSpecification GetOnePlanet(string planetId, string systemId)
    {
        var system = GetOneSystem(systemId);
        var planet = system.Planets.FirstOrDefault(p => p.Name == planetId);
        if (planet is null) throw new NotSupportedException();
        return planet;
    }
    
    public ResourceKind? ExtractResource(string resourceCategoryStr, string planetId, string systemId)
    {
        if (!Enum.TryParse<ResourceCategory>(resourceCategoryStr, true, out var resourceCategory))
        {
            throw new ArgumentException($"Invalid resource category: {resourceCategory}");
        }
        
        var planet = GetOnePlanet(planetId, systemId);
        ResourceKind[] resourceKindList = resourceService.getResourceKindOfCategory(resourceCategory);
        ResourceKind resourceKind = planet.ResourceQuantity.Where(r => r.Value > 0)
            .OrderByDescending(r => r.Value)
            .ThenByDescending(r => (int)r.Key)
            .First(r => resourceKindList.Contains(r.Key))
            .Key;
        if (planet.ResourceQuantity.ContainsKey(resourceKind))
        {
            if (planet.ResourceQuantity[resourceKind] == 0) return null;
            planet.ResourceQuantity[resourceKind] -= 1;
            return resourceKind;
        }

        return null;
    }

}