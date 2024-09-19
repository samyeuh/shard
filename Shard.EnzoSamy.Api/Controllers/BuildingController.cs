using Microsoft.AspNetCore.Mvc;

namespace Shard.EnzoSamy.Api.Controllers;

public class BuildingController(UserService userService) : ControllerBase
{
    [HttpPost]
    [Route("/{userId}/buildings")]
    public ActionResult<BuildingSpecification> BuildMineOnPlanet(string userId, [FromBody] BuildingSpecification buildingSpecification)
    {
        var userBuilderUnit = userService.GetUnitsForUser(userId).FirstOrDefault(unit => unit.Type == "builder");

        BuildingSpecification specification = new BuildingSpecification(buildingSpecification.Type, userBuilderUnit.Planet, userBuilderUnit.System );
        
        
        
        return specification;
    }
}