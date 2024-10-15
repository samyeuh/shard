﻿using Microsoft.AspNetCore.Mvc;
using Shard.EnzoSamy.Api.Services;
using Shard.EnzoSamy.Api.Specifications;
using Shard.Shared.Core;


namespace Shard.EnzoSamy.Api.Controllers;

[Route("[controller]")]
[ApiController]

public class UnitsController(
    UserService userService,
    UnitService unitService,
    FightService fightService,
    IClock? clock,
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
    public Task<ActionResult<UnitSpecification>> PutUnit(string userId, string unitId, [FromBody] UnitSpecification updatedUnit)
    {
        logger.LogInformation($"All informations for updatedUnit {updatedUnit.Id}, DestinationPlanet {updatedUnit.DestinationPlanet}, Destination System {updatedUnit.DestinationSystem}");
        if (unitId != updatedUnit.Id)
        {
            return Task.FromResult<ActionResult<UnitSpecification>>(BadRequest("The unitId in the URL does not match the Id in the body."));
        }
        var user = userService.FindUser(userId);
        if (user == null)
        {
            return Task.FromResult<ActionResult<UnitSpecification>>(NotFound($"User with ID {userId} not found."));
        }

        var unit = userService.GetUnitsForUser(userId).FirstOrDefault(u => u.Id == unitId);
        if (unit == null)
        {
            unit = unitService.CreateUnit(updatedUnit, userId);
            if (unit is null) return Task.FromResult<ActionResult<UnitSpecification>>(BadRequest("Error"));
        }
            
        
        var buildingNotConstruct = user.Buildings.FirstOrDefault(b => b.BuilderId == unitId && !b.IsBuilt);
        if (buildingNotConstruct != null)
        {
            if (unit.Planet != updatedUnit.DestinationPlanet)
            {
                logger.LogInformation($"Cancelling building construction for building {buildingNotConstruct.Id} as the builder is moving away.");
                user.Buildings.Remove(buildingNotConstruct);
            }
        }
        
        unit.DestinationSystem = updatedUnit.System;
        unit.DestinationPlanet = updatedUnit.DestinationPlanet;
        unit.EstimatedTimeOfArrival = unitService.CalculateTripTimeSpan(unit, clock.Now);
        unit.StartTravel(unit.DestinationSystem, unit.DestinationPlanet, unit.EstimatedTimeOfArrival.Value, clock);  
        unitService.FightUnits(userId, unitId, clock);
    
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