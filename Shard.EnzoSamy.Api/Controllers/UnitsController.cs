using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;


namespace Shard.EnzoSamy.Api.Controllers;

[Route("[controller]")]
[ApiController]

public class UnitsController : ControllerBase
{
    private readonly UserService _userService;

    public UnitsController(UserService userService)
    {
        _userService = userService;
    }
    
    
    [HttpGet]
    [Route("/users/{userId}/units")]
    public ActionResult<IReadOnlyList<UnitSpecification>> GetUnits(string userId)
    {
        var units = _userService.GetUnitsForUser(userId);

        if (units != null)
        {
            return Ok(units);
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
        var unit = _userService.GetUsersWithUnit().Find(u => u.Id == userId).Units.Find(u => u.Id == unitId);

        if (unit != null)
        {
            return Ok(unit);
        }
        else
        {
            return NotFound($"User with ID {userId} not found. {unitId} not found.");
        }
    }

    [HttpPut]
    [Route("/users/{userId}/units/{unitId}")]
    public ActionResult<UnitSpecification> MoveSystemUnit(string userId, string unitId, [FromBody] UnitSpecification updatedUnit)
    {
        string pattern = "^[a-zA-Z0-9_-]+$";
        
        if (unitId != updatedUnit.Id)
        {
            return BadRequest("The unitId in the URL does not match the Id in the body.");
        }
        
        if (!Regex.IsMatch(userId, pattern))
        {
            return BadRequest("The body does not contain a valid identifier.");
        }

        try
        {
            var user = _userService.GetUsersWithUnit().Find(u => u.Id == userId);
            var unit = user.Units.Find(u => u.Id == unitId);
            
            if (updatedUnit.Planet != null)
            {
                unit.Planet = updatedUnit.Planet;
                unit.System = updatedUnit.System;
            }
            else
            {
                unit.System = updatedUnit.System;
                unit.Planet = null;

            }
            return unit;
        }
        catch
        {
            return NotFound($"Unit with ID {unitId} or User with ID {userId} not found.");
        }
    }
}