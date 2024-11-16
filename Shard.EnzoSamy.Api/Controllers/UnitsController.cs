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
    public async Task<ActionResult<UnitSpecification>> PutUnit(string userId, string unitId,
        [FromBody] UnitSpecification updatedUnit)
    {
        // si vaisseau existe pas + User.IsInRole("shard")
        // -> enregistrer le cargo + ( test 2 ;) l'ajouter au user
        // faut créer le vaisseau dans le system qui est enregistré dans la classe DistantShard
        var isAdmin = User.IsInRole("admin");
        logger.LogInformation(
            $"All informations for updatedUnit {updatedUnit.Id}, DestinationPlanet {updatedUnit.DestinationPlanet}, Destination System {updatedUnit.DestinationSystem}");
        if (unitId != updatedUnit.Id)
        {
            return await Task.FromResult<ActionResult<UnitSpecification>>(
                BadRequest("The unitId in the URL does not match the Id in the body."));
        }

        var user = userService.FindUser(userId);
        if (user == null)
        {
            return await Task.FromResult<ActionResult<UnitSpecification>>(NotFound($"User with ID {userId} not found."));
        }

        var unit = userService.GetUnitsForUser(userId).FirstOrDefault(u => u.Id == unitId);

        if (unit == null)
        {
            if (!isAdmin) return await Task.FromResult<ActionResult<UnitSpecification>>(Unauthorized());
            unit = unitService.CreateUnit(updatedUnit, userId);

            if (unit is null) return await Task.FromResult<ActionResult<UnitSpecification>>(BadRequest("Error"));
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
            var travelTime =  unitService.CalculateTripTimeSpan(unit, clock.Now, isAdmin);
            if ((unit.Planet != null && unit.DestinationPlanet == null) || travelTime.TotalSeconds > 0)
                unit.EstimatedTimeOfArrival = clock.Now + travelTime;
            unit.StartTravel(unit.DestinationSystem, unit.DestinationPlanet, unit.EstimatedTimeOfArrival, clock);
        }
        else
        {
            unit.DestinationSystem = updatedUnit.System;
            unit.DestinationPlanet = updatedUnit.Planet;
        }
        
        if (updatedUnit.ResourcesQuantity is { Count: > 0 } && !unitService.SameResourceQuantity(unit.ResourcesQuantity, updatedUnit.ResourcesQuantity))
        {
            if (updatedUnit.Type != "cargo")
                return await Task.FromResult<ActionResult<UnitSpecification>>(BadRequest("Cannot unload or load a unit if it is not a cargo"));
            
            if (updatedUnit.DestinationPlanet == null)
                return await Task.FromResult<ActionResult<UnitSpecification>>(BadRequest("Cannot unload or load a unit if it is not a cargo 2"));

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
            catch (Exception ex)
            {
                return await Task.FromResult<ActionResult<UnitSpecification>>(BadRequest("Resource not available in the unit's inventory OR Insufficient resource quantity for the operation"));
            }
        }
        
        return await Task.FromResult<ActionResult<UnitSpecification>>(unit);
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