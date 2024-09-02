using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Controllers;


[Route("[controller]")]
[ApiController]

public class SystemsController : ControllerBase
{
    
    private SectorSpecification _sectorSpecification = new MapGenerator(new MapGeneratorOptions()
    {
        Seed = "EnzoSamy"
    }).Generate();
    
    [HttpGet]
    public string GetSystems()
    {
        return "Systems";
    }
}