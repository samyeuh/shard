using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Controllers;


[Route("[controller]")]
[ApiController]

public class SystemsController : ControllerBase
{
    
    private readonly SectorSpecification _sectorSpecification;
    
    public record SystemWithoutPlanetsResources(string name, List<PlanetsController.PlanetWithoutResource> planets);

    public SystemsController(SectorSpecification sectorSpecification)
    {
        _sectorSpecification = sectorSpecification;
    }
    
    [HttpGet]
    [Route("/systems")]
    public ActionResult<IReadOnlyList<SystemWithoutPlanetsResources>> GetSystems()
    {
        var systems = _sectorSpecification.Systems
            .Select(system => new SystemWithoutPlanetsResources(system.Name, system.Planets
                .Select(planet => new PlanetsController.PlanetWithoutResource(planet.Name, planet.Size)).ToList())).ToList();
        return new (systems);
    }
    
    [HttpGet]
    [Route("/systems/{systemId}")]
    public ActionResult<SystemWithoutPlanetsResources> GetOneSystem(string systemId)
    {
        var systems = _sectorSpecification.Systems
            .Select(system => new SystemWithoutPlanetsResources(system.Name, system.Planets
                .Select(planet => new PlanetsController.PlanetWithoutResource(planet.Name, planet.Size)).ToList())).ToList();
        var system = systems.First(system => system.name == systemId);
        
        return system;
    }
}