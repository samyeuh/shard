using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api;

public class UnitService
{
    private readonly UserService _userService;
    private readonly SectorService _sectorService;

    public UnitService(UserService userService, SectorService sectorService)
    {
        _userService = userService;
        _sectorService = sectorService;
    }
    
    public UnitSpecification? GetUnitForUser(string userId, string unitId)
    {
        var userWithUnit = _userService.GetUsersWithUnit().Find(u => u.Id == userId);
        if (userWithUnit == null) return null;

        return userWithUnit.Units.Find(u => u.Id == unitId);
    }
    
    public PlanetSpecification? GetPlanetForUnit(UnitSpecification unit)
    {
        var system = _sectorService.GetOneSystem(unit.System);
        if (system == null) return null;

        return system.Planets.FirstOrDefault(p => p.Name == unit.Planet);
    }
    
    public Dictionary<string, int>? MapPlanetResources(PlanetSpecification planet)
    {
        return planet.ResourceQuantity.ToDictionary(
            resource => resource.Key.ToString().ToLower(),
            resource => (int)resource.Value
        );
    }
}
