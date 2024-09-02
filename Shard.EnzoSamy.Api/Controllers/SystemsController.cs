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
    [Route("/systems")]
    public string GetSystems()
    {
        return "[\n  {\n    \"name\": \"alpha-centauri\",\n    \"planets\": [\n      {\n        \"name\": \"mars\",\n        \"size\": 42\n      }\n    ]\n  }\n]";
    }
}