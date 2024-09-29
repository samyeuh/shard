using Microsoft.AspNetCore.Mvc;
using Shard.EnzoSamy.Api.Services;
using Shard.EnzoSamy.Api.Specifications;
using Shard.Shared.Core;


namespace Shard.EnzoSamy.Api.Controllers;

[Route("[controller]")]
[ApiController]

public class UnitsController(
    UserService userService,
    UnitService unitService,
    IClock? clock,
    ILogger<UnitsController> logger)
    : ControllerBase
{
    public record UnitLocation(string System, string? Planet, IReadOnlyDictionary<string, int>? ResourcesQuantity);


    [HttpGet]
    [Route("/users/{userId}/units")]
    public ActionResult<IReadOnlyList<UnitSpecification>> GetUnits(string userId)
    {
        var units = userService.GetUnitsForUser(userId);

        if (units != null)
        {
            return units;
        }
        else
        {
            return NotFound($"User with ID {userId} not found.");
        }
    }

    [HttpGet]
    [Route("/users/{userId}/units/{unitId}")]
    public async Task<ActionResult<UnitSpecification>> GetOneUnit(string userId, string unitId)
    {
        var userWithUnits = userService.GetUsersWithUnit().FirstOrDefault(u => u.Id == userId);
        if (userWithUnits == null)
        {
            return NotFound($"User with ID {userId} not found.");
        }

        var unit = userWithUnits.Units.FirstOrDefault(u => u.Id == unitId);
        if (unit == null) return NotFound($"Unit with ID {unitId} not found.");
        try
        {
            await unit.WaitIfArrived();
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, "Request canceled");
        }
        await unit.WaitIfArrived();
        return unit;

    }

    [HttpPut]
    [Route("/users/{userId}/units/{unitId}")]
    public ActionResult<UnitSpecification> MoveSystemUnit(string userId, string unitId, [FromBody] UnitSpecification updatedUnit)
    {
        logger.LogInformation($"All informations for updatedUnit {updatedUnit.Id}, DestinationPlanet {updatedUnit.DestinationPlanet}, Destination System {updatedUnit.DestinationSystem}");
        if (unitId != updatedUnit.Id)
        {
            return BadRequest("The unitId in the URL does not match the Id in the body.");
        }

        var userWithUnits = userService.GetUsersWithUnit().FirstOrDefault(u => u.Id == userId);
        if (userWithUnits == null)
        {
            return NotFound($"User with ID {userId} not found.");
        }

        var unit = userWithUnits.Units.FirstOrDefault(u => u.Id == unitId);
        if (unit == null)
        {
            return NotFound($"Unit with ID {unitId} not found.");
        }
        
        unit.DestinationSystem = updatedUnit.DestinationSystem;
        unit.DestinationPlanet = updatedUnit.DestinationPlanet;
        unit.EstimatedTimeOfArrival = unitService.CalculateTripTimeSpan(unit, clock.Now);

        unit.StartTravel(unit.DestinationSystem, unit.DestinationPlanet, unit.EstimatedTimeOfArrival.Value, clock);
    
        return unit;
    }

    [HttpGet]
    [Route("/users/{userId}/units/{unitId}/location")]
    public ActionResult<UnitLocation> GetUnitLocation(string userId, string unitId)
    {
        var unit = unitService.GetUnitForUser(userId, unitId);
        if (unit == null)
        {
            return NotFound($"User or Unit not found: User ID {userId}, Unit ID {unitId}");
        }
        
        var planet = unitService.GetPlanetForUnit(unit);
        if (unit.EstimatedTimeOfArrival != null)
        {
            logger.LogInformation($"Unit System : {unit.System}, Planet : {planet?.Name}, Estimated Time : {unit.EstimatedTimeOfArrival}");
            return new UnitLocation(unit.System, planet?.Name, null);
        }

        if (planet == null) return new UnitLocation(unit.System, null, null);
        
        var resources = unitService.MapPlanetResources(planet);

        logger.LogInformation($"Unit System : {unit.System}, Planet : {planet.Name}, Resources : {resources}");
        
        return unit.Type == "scout" ? new UnitLocation(unit.System, planet.Name, resources) : new UnitLocation(unit.System, planet.Name, null);
    }
}