using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;
namespace Shard.EnzoSamy.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class SystemsController(SectorSpecification sectorSpecification) : ControllerBase
{
    public record SystemWithoutPlanetsResources(string Name, List<PlanetsController.PlanetWithoutResource> Planets);

    [HttpGet]
    [Route("/systems")]
    public ActionResult<IReadOnlyList<SystemWithoutPlanetsResources>> GetSystems()
    {
        var systems = sectorSpecification.Systems
            .Select(system => new SystemWithoutPlanetsResources(system.Name, system.Planets
                .Select(planet => new PlanetsController.PlanetWithoutResource(planet.Name, planet.Size)).ToList())).ToList();
        return new ActionResult<IReadOnlyList<SystemWithoutPlanetsResources>>(systems);
    }
    
    [HttpGet]
    [Route("/systems/{systemId}")]
    public ActionResult<SystemWithoutPlanetsResources> GetOneSystem(string systemId)
    {
        var systems = sectorSpecification.Systems
            .Select(system => new SystemWithoutPlanetsResources(system.Name, system.Planets
                .Select(planet => new PlanetsController.PlanetWithoutResource(planet.Name, planet.Size)).ToList())).ToList();
        var system = systems.First(system => system.Name == systemId);
        
        return system;
    }
}