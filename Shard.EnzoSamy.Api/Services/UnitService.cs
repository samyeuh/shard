using Microsoft.AspNetCore.Mvc;
using Shard.EnzoSamy.Api.Specifications;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Services;

public class UnitService(UserService userService, SectorService sectorService, FightService fightService, List<UserSpecification> userSpecifications)
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
    
    public DateTime CalculateTripTimeSpan(UnitSpecification unit, DateTime currentTime)
    {
        var travelTime = TimeSpan.Zero;
        if (unit.System != unit.DestinationSystem && unit.DestinationSystem != null)
        {
            travelTime += TimeSpan.FromMinutes(1);
        }
        
        travelTime += TimeSpan.FromSeconds(15);
        unit.Planet = null;
        return currentTime + travelTime;
    }

    public async void FightUnits(string userId, string unitId)
    {
        var user = userService.FindUser(userId);
        if (user is null) return;
        var unit = user.Units.FirstOrDefault(u => u.Id == unitId);
        if (unit is null) return;
        var enemyUnits = GetUnitInSystem(unit.System).Where(u => u.Id != unitId);

        if (enemyUnits.Any())
        {
            var enemyPriority = enemyUnits.Where(e => unit.TypePriority.Contains(e.Type)).OrderBy(e => unit.TypePriority.IndexOf(e.Type)).ToList();
            foreach (var enemy in enemyPriority)
            {
                await fightService.Fight(unit, enemy);
            }
        }
    }


    public List<UnitSpecification> GetUnitInSystem(String system)
    {
        List<UnitSpecification> units = new List<UnitSpecification>();
        foreach (var user in userSpecifications)
        {
            var unitsOfUser = userService.GetUnitsForUser(user.Id);
            if (unitsOfUser.Count == 0 || unitsOfUser is null) continue;
            
            foreach (var unit in unitsOfUser)
            {
                if (unit.System == system) units.Add(unit);
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
    
}
