using Microsoft.AspNetCore.Mvc;
using Shard.EnzoSamy.Api.Services;
using Shard.EnzoSamy.Api.Specifications;
using Shard.EnzoSamy.Api.Utilities;

namespace Shard.EnzoSamy.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class BuildingController(UserService userService, List<BuildingSpecification> listBuildingSpecification) : ControllerBase
{
    
    public record BuildingWithoutBuilderId(string Planet, string System, string Type, string resourceCategory);
    
    [HttpPost]
    [Route("/users/{userId}/buildings")]
    public ActionResult<BuildingWithoutBuilderId> BuildMineOnPlanet(string userId, [FromBody] BuildingSpecification buildingSpecification)
    {
        if (!ValidationUtils.IsValidUserId(userId)) return NotFound("Invalid user Id");

        var user = userService.FindUser(userId);
        if (user is null) return NotFound($"User with ID {userId} not found.");

        var userUnit = userService.GetUnitsForUser(userId);
        if (userUnit == null || !userUnit.Any()) return NotFound("User dont have any units.");
        
        var userBuilderUnit = userUnit.FirstOrDefault(unit => unit.Type == "builder");
        
        

        if (userBuilderUnit is null) return BadRequest("User dont have any builders.");
        if (string.IsNullOrEmpty(userBuilderUnit.Planet)) return BadRequest("User dont have any planet.");
        
        if (userBuilderUnit.Id != buildingSpecification.BuilderId) return BadRequest("Builder ID doesn't match unit id.");

        if (userBuilderUnit.Planet is null) return BadRequest("An error occured.");
        
        if (buildingSpecification.Type != "mine") return BadRequest("Invalid type.");
        
        listBuildingSpecification.Add(new BuildingSpecification(buildingSpecification.Type, userBuilderUnit.Planet, userBuilderUnit.System, buildingSpecification.BuilderId, buildingSpecification.ResourceCategory));
        return new BuildingWithoutBuilderId(userBuilderUnit.Planet, userBuilderUnit.System, buildingSpecification.Type, buildingSpecification.ResourceCategory);
    }

}