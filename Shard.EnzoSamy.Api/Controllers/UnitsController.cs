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
}