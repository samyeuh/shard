using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Shard.EnzoSamy.Api.Utilities;
using Shard.Shared.Core;


namespace Shard.EnzoSamy.Api.Controllers;

[Route("[controller]")]
[ApiController]

public class UnitsController : ControllerBase
{
    private readonly UserService _userService;
    private readonly UnitService _unitService;

    public record UnitLocation(string system, string? planet, IReadOnlyDictionary<string, int>? resourcesQuantity);

    public UnitsController(UserService userService, UnitService unitService)
    {
        _userService = userService;
        _unitService = unitService;
    }
    
    
    [HttpGet]
    [Route("/users/{userId}/units")]
    public ActionResult<IReadOnlyList<UnitSpecification>> GetUnits(string userId)
    {
        var units = _userService.GetUnitsForUser(userId);

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
    public ActionResult<UnitSpecification> GetOneUnit(string userId, string unitId)
    {
        var userWithUnits = _userService.GetUsersWithUnit().FirstOrDefault(u => u.Id == userId);
        if (userWithUnits == null)
        {
            return NotFound($"User with ID {userId} not found.");
        }

        var unit = userWithUnits.Units.FirstOrDefault(u => u.Id == unitId);
        if (unit != null)
        {
            return unit;
        }
        else
        {
            return NotFound($"Unit with ID {unitId} not found.");
        }
    }

    [HttpPut]
    [Route("/users/{userId}/units/{unitId}")]
    public ActionResult<UnitSpecification> MoveSystemUnit(string userId, string unitId, [FromBody] UnitSpecification updatedUnit)
    {
        if (unitId != updatedUnit.Id)
        {
            return BadRequest("The unitId in the URL does not match the Id in the body.");
        }

        var userWithUnits = _userService.GetUsersWithUnit().FirstOrDefault(u => u.Id == userId);
        if (userWithUnits == null)
        {
            return NotFound($"User with ID {userId} not found.");
        }

        var unit = userWithUnits.Units.FirstOrDefault(u => u.Id == unitId);
        if (unit == null)
        {
            return NotFound($"Unit with ID {unitId} not found.");
        }

        unit.System = updatedUnit.System;
        unit.Planet = updatedUnit.Planet;
        unit.DestinationSystem = updatedUnit.DestinationSystem;
        unit.DestinationPlanet = updatedUnit.DestinationPlanet;

        return unit;
    }
    
    [HttpGet]
    [Route("/users/{userId}/units/{unitId}/location")]
    public ActionResult<UnitLocation> GetUnitLocation(string userId, string unitId)
    {
        var unit = _unitService.GetUnitForUser(userId, unitId);
        if (unit == null)
        {
            return NotFound($"User or Unit not found: User ID {userId}, Unit ID {unitId}");
        }
        
        var planet = _unitService.GetPlanetForUnit(unit);
        if (planet == null)
        {
            return new UnitLocation(unit.System, null, null);
        }
        
        var resources = _unitService.MapPlanetResources(planet);

        return new UnitLocation(unit.System, planet.Name, resources);
    }
}