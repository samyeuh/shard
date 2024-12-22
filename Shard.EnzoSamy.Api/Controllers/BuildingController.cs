using System.Runtime.InteropServices;
using System.Security.AccessControl;
using Microsoft.AspNetCore.Mvc;
using Shard.EnzoSamy.Api.Contracts;
using Shard.EnzoSamy.Api.Enumerations;
using Shard.EnzoSamy.Api.Services;
using Shard.EnzoSamy.Api.Specifications;
using Shard.EnzoSamy.Api.Utilities;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BuildingController(UserService userService, UnitService unitService, SectorService sectorService, IClock clock) : ControllerBase
    {
        // Méthode existante : BuildBuildingOnPlanet
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
                buildingSpecification.BuilderId = buildingSpecification.BuilderId;
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

        [HttpPost]
        [Route("/users/{userId}/buildings/{buildingId}/queue")]
        public ActionResult<UnitSpecification> QueueUnit(string userId, string buildingId, [FromBody] UnitRequest unitRequest)
        {

            var user = userService.FindUser(userId);
            if (user == null) return NotFound("User not found.");

            var building = user.Buildings.FirstOrDefault(b => b.Id == buildingId);
            if (building == null) return NotFound("Building not found.");

            
            if (building.Type != "starport" || !building.IsBuilt)
            {
                return BadRequest("Building must be a built starport.");
            }

           
            var requiredResources = unitService.GetRequiredResources(unitRequest.Type.ToLower());
            if (!requiredResources.Any()) return BadRequest("Invalid unit type");

            
            if (!userService.HasSufficientResources(user, requiredResources))
                return BadRequest("Insufficient resources.");

           
            var systemSpecification = sectorService.GetOneSystem(building.System);
            if (systemSpecification == null) return NotFound($"System with ID {building.System} not found.");

            var planetSpecification = sectorService.GetOnePlanet(building.Planet, building.System);
            if (planetSpecification == null) return NotFound($"System with ID {building.Planet} not found.");
            
            
            userService.DeductResources(user, requiredResources);
            var newUnit = new UnitSpecification(systemSpecification, planetSpecification, unitRequest.Type, userId);
            user.Units.Add(newUnit);

            return newUnit;
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
}
