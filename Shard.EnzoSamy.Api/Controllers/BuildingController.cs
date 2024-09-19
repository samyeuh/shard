using Microsoft.AspNetCore.Mvc;
using Shard.EnzoSamy.Api.Services;
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
        if (user == null) return NotFound($"User with ID {userId} not found.");

        var userBuilderUnit = userService.GetUnitsForUser(userId)
            .FirstOrDefault(unit => unit.Type == "builder");

        if (userBuilderUnit == null) return BadRequest("Invalid builder ID.");

        if (userBuilderUnit.Planet != null)
        {
            var specification = new BuildingSpecification(buildingSpecification.Type, userBuilderUnit.Planet, userBuilderUnit.System);
            return specification;
        }

        return BadRequest("An error occured.");
    }

}