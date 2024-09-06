using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Controllers;

[Route("[controller]")]
[ApiController]

public class PlanetsController : ControllerBase
{
    
    private readonly SectorSpecification _sectorSpecification;

    public record PlanetWithoutResource(string Name, int Size);

    public PlanetsController(SectorSpecification sectorSpecification)
    {
        _sectorSpecification = sectorSpecification;
    }
    [HttpGet]
    [Route("/systems/{systemId}/planets/{planetId}")]
    public ActionResult<PlanetWithoutResource> GetOnePlanet(string systemId, string planetId)
    {
        var system = _sectorSpecification.Systems.First(system => system.Name == systemId);
        var planets = system.Planets.Select((planet) => new PlanetWithoutResource(Name: planet.Name, Size: planet.Size)).ToList();
        var planet = planets.First(planet => planet.Name == planetId);
        return planet;
    }
    
    [HttpGet]
    [Route("/systems/{systemId}/planets")]
    public ActionResult<IReadOnlyList<PlanetWithoutResource>> GetPlanets(string systemId)
    {
        var system = _sectorSpecification.Systems.First(system => system.Name == systemId);
        var planets = system.Planets.Select((planet) => new PlanetWithoutResource(Name: planet.Name, Size: planet.Size)).ToList();
        return new (planets);
    }
}