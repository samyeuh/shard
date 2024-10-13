using Microsoft.AspNetCore.Mvc;
using Shard.EnzoSamy.Api.Enumerations;
using Shard.EnzoSamy.Api.Services;
using Shard.EnzoSamy.Api.Specifications;
using Shard.EnzoSamy.Api.Utilities;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class BuildingController(UserService userService, SectorService sectorService, IClock clock) : ControllerBase
{
    
    
    [HttpPost]
    [Route("/users/{userId}/buildings")]
    public ActionResult<BuildingSpecification> BuildBuildingOnPlanet(string userId, [FromBody] BuildingSpecification buildingSpecification)
    {
        if (!ValidationUtils.IsValidUserId(userId)) return NotFound("Invalid user Id");

        var user = userService.FindUser(userId);
        if (user is null) return NotFound($"User with ID {userId} not found.");

        var userUnit = userService.GetUnitsForUser(userId);
        if (userUnit == null || !userUnit.Any()) return NotFound("User don't have any units.");

        var userBuilderUnit = userUnit.FirstOrDefault(unit => unit.Type == "builder");

        if (userBuilderUnit is null) return BadRequest("User don't have any builders.");
        if (string.IsNullOrEmpty(userBuilderUnit.Planet)) return BadRequest("User don't have any planet.");

        if (userBuilderUnit.Id != buildingSpecification.BuilderId) return BadRequest("Builder ID doesn't match unit id.");

        // Vérification du type de bâtiment
        if (buildingSpecification.Type == "mine")
        {
            // La catégorie de ressource est nécessaire pour une mine
            if (string.IsNullOrEmpty(buildingSpecification.ResourceCategory))
                return BadRequest("Resource category is required for a mine.");
            if (!Enum.GetNames(typeof(ResourceCategory)).Contains(buildingSpecification.ResourceCategory))
                return BadRequest("Invalid resource category.");
        }
        else if (buildingSpecification.Type == "starport")
        {
            // Le starport n'a pas besoin de catégorie de ressource
            buildingSpecification.ResourceCategory = null;
        }
        else
        {
            return BadRequest("Invalid building type.");
        }

        var building = new BuildingSpecification(
            buildingSpecification.Type,
            userBuilderUnit.Planet,
            userBuilderUnit.System,
            buildingSpecification.BuilderId,
            buildingSpecification.ResourceCategory
        );

        user.Buildings.Add(building);
        building.StartBuild(clock, sectorService, userService, userId);

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
        var building = user.Buildings.FirstOrDefault(building => building.Id == buildingId);
        if(building is null) return NotFound("User dont have the specified building.");

        await building.WaitIfBuild();
        return building;
    }

}