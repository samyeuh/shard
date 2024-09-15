using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Controllers;


[Route("[controller]")]
[ApiController]

public class SystemsController : ControllerBase
{
    
    private readonly SectorSpecification _sectorSpecification;

    public SystemsController(SectorSpecification sectorSpecification)
    {
        _sectorSpecification = sectorSpecification;
    }
    
    
    [HttpGet]
    [Route("/systems")]
    public ActionResult<IReadOnlyList<SystemSpecification>> GetSystems()
    {
        var systems = _sectorSpecification.Systems;
        return new (systems);
    }
    
    [HttpGet]
    [Route("/systems/{systemId}")]
    public ActionResult<SystemSpecification> GetOneSystem(string systemId)
    {
        var system = _sectorSpecification.Systems.First(system => system.Name == systemId);
        
        return system;
    }
    
    [HttpGet]
    [Route("/systems/{systemId}/planets/{planetId}")]
    public ActionResult<PlanetSpecification> GetOnePlanet(string systemId, string planetId)
    {
        var system = _sectorSpecification.Systems.First(system => system.Name == systemId);
        var planet = system.Planets.First(planet => planet.Name == planetId);
        return planet;
    }
    
    [HttpGet]
    [Route("/systems/{systemId}/planets")]
    public ActionResult<IReadOnlyList<PlanetSpecification>> GetPlanets(string systemId)
    {
        var system = _sectorSpecification.Systems.First(system => system.Name == systemId);
        var planets = system.Planets;
        return new (planets);
    }
}