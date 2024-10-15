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

    public async void FightUnits(string userId, string unitId, IClock clock)
    {
        var user = userService.FindUser(userId);
        if (user is null) return;
        
        var unit = user.Units.FirstOrDefault(u => u.Id == unitId);
        if (unit is null) return;
        
        var enemy = GetUnitInSystem(unit.System)
            .Where(u => u.Key.Id != unitId && unit.TypePriority.Contains(u.Key.Type) && u.Value != userId)
            .OrderBy(u => unit.TypePriority.IndexOf(u.Key.Type)).FirstOrDefault();
        if (enemy.Key is null) return;

        await fightService.StartFight(unit, enemy.Key, clock);
        
        if (unit.Health <= 0) DestroyUnit(userId, unit.Id);
        if (enemy.Key.Health <= 0) DestroyUnit(enemy.Value, enemy.Key.Id);
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
    
}
