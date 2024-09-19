using Microsoft.AspNetCore.Mvc;
using Shard.EnzoSamy.Api.Services;
using Shard.EnzoSamy.Api.Specifications;
using Shard.EnzoSamy.Api.Utilities;

namespace Shard.EnzoSamy.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class BuildingController(UserService userService) : ControllerBase
{
    [HttpPost]
    [Route("/users/{userId}/buildings")]
    public ActionResult<BuildingSpecification> BuildMineOnPlanet(string userId, [FromBody] BuildingSpecification buildingSpecification)
    {
        if (!ValidationUtils.IsValidUserId(userId)) return NotFound("Invalid user Id");

        var user = userService.FindUser(userId);
        if (user is null) return NotFound($"User with ID {userId} not found.");

        var userUnit = userService.GetUnitsForUser(userId);
        if (userUnit is null) return NotFound("User dont have any units.");
        
        var userBuilderUnit = userUnit.FirstOrDefault(unit => unit.Id == buildingSpecification.Id);

        if (userBuilderUnit is null) return BadRequest("Invalid builder ID.");

        if (userBuilderUnit.Planet is null) return BadRequest("An error occured.");
        
        var specification = new BuildingSpecification(buildingSpecification.Type, userBuilderUnit.Planet, userBuilderUnit.System);
        return specification;
    }

}