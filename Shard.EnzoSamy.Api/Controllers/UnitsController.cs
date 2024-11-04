using Microsoft.AspNetCore.Mvc;
using Shard.EnzoSamy.Api.Services;
using Shard.EnzoSamy.Api.Specifications;
using Shard.Shared.Core;


namespace Shard.EnzoSamy.Api.Controllers;

[Route("[controller]")]
[ApiController]

public class UnitsController(
    UserService userService,
    UnitService unitService,
    IClock clock,
    ILogger<UnitsController> logger)
    : ControllerBase
{
    public record UnitLocation(string System, string? Planet, IReadOnlyDictionary<string, int>? ResourcesQuantity);

    [HttpGet]
    [Route("/users/{userId}/units")]
    public ActionResult<IReadOnlyList<UnitSpecification>> GetUnits(string userId)
    {
        var units = userService.GetUnitsForUser(userId);

        if (units != null)
        {
            return units;
        }
        else
        {
            return NotFound($"User with ID {userId} not found.");
        }
    }

    [HttpGet]
    [Route("/users/{userId}/units/{unitId}")]
    public async Task<ActionResult<UnitSpecification>> GetOneUnit(string userId, string unitId)
    {
        var user = userService.FindUser(userId);
        logger.LogInformation($"user {user} with ID {unitId} found.");
        if (user == null)
        {
            return NotFound($"User with ID {userId} not found.");
        }
        var userUnits = userService.GetUnitsForUser(userId);
        if(userUnits == null) return NotFound($"User with ID {userId} dont have any units.");
        
        var unit = userUnits.FirstOrDefault(u => u.Id == unitId);
        if (unit == null) return NotFound($"Unit with ID {unitId} not found.");
        
        await unit.WaitIfArrived();
        return unit;

    }

    [HttpPut]
    [Route("/users/{userId}/units/{unitId}")]
    public Task<ActionResult<UnitSpecification>> PutUnit(string userId, string unitId,
        [FromBody] UnitSpecification updatedUnit)
    {
        var isAdmin = User.IsInRole("admin");
        logger.LogInformation(
            $"All informations for updatedUnit {updatedUnit.Id}, DestinationPlanet {updatedUnit.DestinationPlanet}, Destination System {updatedUnit.DestinationSystem}");
        if (unitId != updatedUnit.Id)
        {
            return Task.FromResult<ActionResult<UnitSpecification>>(
                BadRequest("The unitId in the URL does not match the Id in the body."));
        }

        var user = userService.FindUser(userId);
        if (user == null)
        {
            return Task.FromResult<ActionResult<UnitSpecification>>(NotFound($"User with ID {userId} not found."));
        }

        var unit = userService.GetUnitsForUser(userId).FirstOrDefault(u => u.Id == unitId);

        if (unit == null)
        {
            if (!isAdmin) return Task.FromResult<ActionResult<UnitSpecification>>(Unauthorized());
            unit = unitService.CreateUnit(updatedUnit, userId);

            if (unit is null) return Task.FromResult<ActionResult<UnitSpecification>>(BadRequest("Error"));
        }

        var buildingNotConstruct = user.Buildings.FirstOrDefault(b => b.BuilderId == unitId && !b.IsBuilt);
        if (buildingNotConstruct != null)
        {
            if (unit.Planet != updatedUnit.DestinationPlanet)
            {
                logger.LogInformation(
                    $"Cancelling building construction for building {buildingNotConstruct.Id} as the builder is moving away.");
                user.Buildings.Remove(buildingNotConstruct);
            }
        }



        if (updatedUnit.DestinationSystem != null || updatedUnit.DestinationPlanet != null)
        {
            unit!.DestinationSystem = updatedUnit.DestinationSystem;
            unit.DestinationPlanet = updatedUnit.DestinationPlanet;
            unit.EstimatedTimeOfArrival = unitService.CalculateTripTimeSpan(unit, clock.Now, isAdmin);
            unit.StartTravel(unit.DestinationSystem, unit.DestinationPlanet, unit.EstimatedTimeOfArrival.Value, clock);
        }
        else
        {
            unit.System = updatedUnit.System;
            unit.Planet = updatedUnit.Planet;
            unit.DestinationSystem = updatedUnit.System;
            unit.DestinationPlanet = updatedUnit.Planet;
        }
        
        if (updatedUnit.ResourcesQuantity is { Count: > 0 })
            {
                if (unit.Type != "cargo")
                    return Task.FromResult<ActionResult<UnitSpecification>>(BadRequest("Cannot unload or load a unit if it is not a cargo"));
                
                if (!user.Buildings.Any(b => b.Type == "starport" ))
                    return Task.FromResult<ActionResult<UnitSpecification>>(BadRequest("Cannot load if no starport"));
                
                /* if (updatedUnit.DestinationPlanet == null)
                    return Task.FromResult<ActionResult<UnitSpecification>>(BadRequest("Cannot load if no starport 2")); */
                // calculer ce qu'il faut enlever et rajouter
                // l'enlever et le rajouter

                try
                {
                    var resourceQuantity = unitService.calculateLoadUnload(unit, updatedUnit.ResourcesQuantity);
                    foreach (var resource in resourceQuantity)
                    {
                        if (resource.Value < 0)
                        {
                            int? valAbsolue = resource.Value.HasValue ? Math.Abs(resource.Value.Value) : null;
                            KeyValuePair<string, int?> resourceKVP = new KeyValuePair<string, int?>(resource.Key, valAbsolue);
                            unitService.removeResourceToUnit(unit, resourceKVP);
                            userService.AddResourceToUser(user, resourceKVP);
                        }
                        else
                        {
                            KeyValuePair<string, int?> resourceKVP = new KeyValuePair<string, int?>(resource.Key, resource.Value);
                            userService.removeResourceToUser(user, resourceKVP);
                            unitService.addResourceToUnit(unit, resourceKVP);
                        }
                    }
                }
                catch (KeyNotFoundException ex)
                {
                    // Log et retour d'une réponse explicite pour ressource manquante
                    logger.LogWarning($"Resource missing for unit {unitId}: {ex.Message}");
                    return Task.FromResult<ActionResult<UnitSpecification>>(
                        BadRequest("Resource not available in the unit's inventory"));
                }
                catch (InvalidOperationException ex)
                {
                    // Log et retour d'une réponse explicite pour quantité insuffisante
                    logger.LogWarning($"Insufficient resource quantity for unit {unitId}: {ex.Message}");
                    return Task.FromResult<ActionResult<UnitSpecification>>(
                        BadRequest("Insufficient resource quantity for the operation"));
                }
            }

        return Task.FromResult<ActionResult<UnitSpecification>>(unit);
    }

    [HttpGet]
    [Route("/users/{userId}/units/{unitId}/location")]
    public ActionResult<UnitLocation> GetUnitLocation(string userId, string unitId)
    {
        var unit = userService.GetUnitsForUser(userId).FirstOrDefault(u => u.Id == unitId);
        if (unit == null)
        {
            return NotFound($"User or Unit not found: User ID {userId}, Unit ID {unitId}");
        }
        
        var planet = unitService.GetPlanetForUnit(unit);
        if (unit.EstimatedTimeOfArrival != null)
        {
            logger.LogInformation($"Unit System : {unit.System}, Planet : {planet?.Name}, Estimated Time : {unit.EstimatedTimeOfArrival}");
            return new UnitLocation(unit.System, planet?.Name, null);
        }

        if (planet == null) return new UnitLocation(unit.System, null, null);
        
        var resources = unitService.MapPlanetResources(planet);

        logger.LogInformation($"Unit System : {unit.System}, Planet : {planet.Name}, Resources : {resources}");
        
        return unit.Type == "scout" ? new UnitLocation(unit.System, planet.Name, resources) : new UnitLocation(unit.System, planet.Name, null);
    }
}