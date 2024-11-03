using Microsoft.AspNetCore.Mvc;
using Shard.EnzoSamy.Api.Specifications;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Services;

public class UnitService(UserService userService, SectorService sectorService, List<UserSpecification> userSpecifications)
{
    /*public UnitSpecification? GetUnitForUser(string userId, string unitId)
    {
        var userWithUnit = userService.GetUsersWithUnit().Find(u => u.Id == userId);
        return userWithUnit?.Units.Find(u => u.Id == unitId);
    }*/
    
    public PlanetSpecification? GetPlanetForUnit(UnitSpecification unit)
    {
        var system = sectorService.GetOneSystem(unit.System);
        return system.Planets.FirstOrDefault(p => p.Name == unit.Planet);
    }
    
    public Dictionary<string, int> MapPlanetResources(PlanetSpecification planet)
    {
        return planet.ResourceQuantity.ToDictionary(
            resource => resource.Key.ToString().ToLower(),
            resource => resource.Value
        );
    }
    
    public DateTime CalculateTripTimeSpan(UnitSpecification unit, DateTime currentTime, bool isAdmin)
    {
        var travelTime = TimeSpan.Zero;
        if (unit.System != unit.DestinationSystem && unit.DestinationSystem != null)
        {
            travelTime += TimeSpan.FromMinutes(1);
        }
        
        travelTime += TimeSpan.FromSeconds(15);
        if (!isAdmin) unit.Planet = null;
        if (isAdmin) unit.DestinationPlanet = unit.Planet;
        return currentTime + travelTime;
    }


    public Dictionary<UnitSpecification, string> GetUnitInSystem(String system)
    {
        Dictionary<UnitSpecification, string> units = new Dictionary<UnitSpecification, string>();
        foreach (var user in userSpecifications)
        {
            var unitsOfUser = userService.GetUnitsForUser(user.Id);
            if (unitsOfUser.Count == 0 || unitsOfUser is null) continue;
            
            foreach (var unit in unitsOfUser)
            {
                if (unit.System == system) units.Add(unit, user.Id);
            }
        }
        return units;
    }

    public UnitSpecification? CreateUnit(UnitSpecification unit, string userId)
    {
        var user = userService.FindUser(userId);
        if (user is null) return null;
        unit.SetCombatSpec();
        user.Units.Add(unit);
        return unit;
    }

    public bool DestroyUnit(string userId, string unitId)
    {
        var user = userService.FindUser(userId);
        if (user == null) return false;
        
        var unit = user.Units.FirstOrDefault(u => u.Id == unitId);
        if (unit == null) return false;
        
        user.Units.Remove(unit);
        return true;
    }

    public Dictionary<string, int>? GetRequiredResources(string unitType)
    {
        var requiredResources = new Dictionary<string, int>();
        switch (unitType)
        {
            case "scout":
                requiredResources["carbon"] = 5;
                requiredResources["iron"] = 5;
                break;
            case "builder":
                requiredResources["carbon"] = 5;
                requiredResources["iron"] = 10;
                break;
            case "fighter":
                requiredResources["aluminium"] = 10;
                requiredResources["iron"] = 20;
                break;
            case "bomber":
                requiredResources["titanium"] = 10;
                requiredResources["iron"] = 30;
                break;
            case "cruiser":
                requiredResources["gold"] = 20;
                requiredResources["iron"] = 60;
                break;
            case "cargo":
                requiredResources["carbon"] = 10;
                requiredResources["iron"] = 10;
                requiredResources["gold"] = 5;
                break;
        }
        return requiredResources;
    }

    public bool checkIfUnitHasMoreRessourceThanUser(UnitSpecification unit, UserSpecification user)
    {
        foreach (var ressource in unit.ResourcesQuantity)
        {
            if (user.ResourcesQuantity.TryGetValue(ressource.Key, out int? userQuantity) &&
                ressource.Value > userQuantity)
            {
                return true;
            }
        }

        return false;
    }

    public void addResourceToUnit(UnitSpecification unitSpecification, Dictionary<string, int?> ressources)
    {
        foreach (var ressource in ressources)
        {
            if (unitSpecification.ResourcesQuantity.ContainsKey(ressource.Key))
            {
                unitSpecification.ResourcesQuantity[ressource.Key] += ressource.Value;
            }
            else
            {
                unitSpecification.ResourcesQuantity.Add(ressource.Key, ressource.Value);
            }
        }
    }

    public void removeResourceToUnit(UnitSpecification unit, Dictionary<string, int?> resources)
    {
        foreach (var resource in resources)
        {
            // Vérifie d'abord si l'unité a bien cette ressource dans son inventaire
            if (!unit.ResourcesQuantity.ContainsKey(resource.Key))
            {
                throw new KeyNotFoundException($"Resource '{resource.Key}' not found in the unit's inventory.");
            }
        
            // Ensuite, vérifie que la quantité n'est pas négative après soustraction
            if (unit.ResourcesQuantity[resource.Key] - resource.Value < 0)
            {
                throw new InvalidOperationException($"Insufficient quantity of '{resource.Key}' in the unit to remove {resource.Value}.");
            }
        
            // Effectue la soustraction si tout est vérifié
            unit.ResourcesQuantity[resource.Key] -= resource.Value;
        }
    }


    public Dictionary<string, int?> calculateUnload(UnitSpecification unit, Dictionary<string, int?> resources)
    {
        Dictionary<string, int?> newResource = new Dictionary<string, int?>();
        foreach (var resource in resources)
        {
            if (unit.ResourcesQuantity.ContainsKey(resource.Key) &&
                unit.ResourcesQuantity[resource.Key].Value != resource.Value)
            {
                int? diff = unit.ResourcesQuantity[resource.Key].Value - resource.Value;
                newResource.Add(resource.Key, diff);
            }
        }

        return newResource;
    }

}
