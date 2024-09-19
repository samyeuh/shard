using Microsoft.AspNetCore.Mvc;
using Shard.EnzoSamy.Api.Services;
using Shard.EnzoSamy.Api.Specifications;
using Shard.EnzoSamy.Api.Utilities;

namespace Shard.EnzoSamy.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class BuildingController(UserService userService) : ControllerBase
{
    
    public record BuildingWithoutBuilderId(string planet, string system, string type);
    
    [HttpPost]
    [Route("/users/{userId}/buildings")]
    public ActionResult<BuildingWithoutBuilderId> BuildMineOnPlanet(string userId, [FromBody] BuildingSpecification buildingSpecification)
    {
        if (!ValidationUtils.IsValidUserId(userId)) return NotFound("Invalid user Id");

        var user = userService.FindUser(userId);
        if (user is null) return NotFound($"User with ID {userId} not found.");

        var userUnit = userService.GetUnitsForUser(userId);
        if (userUnit is null) return NotFound("User dont have any units.");
        
        var userBuilderUnit = userUnit.FirstOrDefault(unit => unit.Type == "builder");
        
        

        if (userBuilderUnit is null) return BadRequest("User dont have any builders.");
        if (userBuilderUnit.Planet is null) return BadRequest("User dont have any planet.");
        
        if (userBuilderUnit.Id != buildingSpecification.BuilderId) return BadRequest("BuilderID dont match unit id.");

        if (userBuilderUnit.Planet is null) return BadRequest("An error occured.");
        
        if (buildingSpecification.Type != "mine") return BadRequest("Invalid type.");
        
        var specification = new BuildingSpecification(buildingSpecification.Type, userBuilderUnit.Planet, userBuilderUnit.System, buildingSpecification.BuilderId);
        return new BuildingWithoutBuilderId(userBuilderUnit.Planet, userBuilderUnit.System, buildingSpecification.Type);
    }

}