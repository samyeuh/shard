using Microsoft.AspNetCore.Mvc;
using Shard.EnzoSamy.Api.Enumerations;
using Shard.EnzoSamy.Api.Services;
using Shard.EnzoSamy.Api.Specifications;
using Shard.EnzoSamy.Api.Utilities;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class BuildingController(UserService userService, IClock clock) : ControllerBase
{
    
    
    [HttpPost]
    [Route("/users/{userId}/buildings")]
    public ActionResult<BuildingSpecification> BuildMineOnPlanet(string userId, [FromBody] BuildingSpecification buildingSpecification)
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
        
        if (!Enum.GetNames(typeof(ResourceCategory)).Contains(buildingSpecification.ResourceCategory)) return BadRequest("Invalid resource category.");
        
        var building = new BuildingSpecification(buildingSpecification.Type, userBuilderUnit.Planet, userBuilderUnit.System, buildingSpecification.BuilderId, buildingSpecification.ResourceCategory);
        user.Buildings.Add(building);
        building.StartBuild(clock);
        
        return building;
    }
    
    [HttpGet]
    [Route("/users/{userId}/buildings")]
    public ActionResult<List<BuildingSpecification>> GetBuildings(string userId)
    {
        if (!ValidationUtils.IsValidUserId(userId)) return NotFound("Invalid user Id");

        var user = userService.FindUser(userId);
        if (user is null) return NotFound($"User with ID {userId} not found.");

        return user.Buildings;
    }
    
    [HttpGet]
    [Route("/users/{userId}/buildings/{buildingId}")]
    public async Task<ActionResult<BuildingSpecification>> GetBuilding(string userId, string buildingId)
    {
        if (!ValidationUtils.IsValidUserId(userId)) return NotFound("Invalid user Id");

        var user = userService.FindUser(userId);
        if (user is null) return NotFound($"User with ID {userId} not found.");
        //Check si l'unit est encore sur la planet sinon interrompre la construction.
        var building = user.Buildings.FirstOrDefault(building => building.Id == buildingId);
        if(building is null) return BadRequest("User dont have any buildings.");

        await building.WaitIfBuild();
        return building;
    }

}