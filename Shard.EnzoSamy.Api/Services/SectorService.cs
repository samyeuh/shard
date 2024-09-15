using Shard.Shared.Core;
namespace Shard.EnzoSamy.Api;

public class SectorService
{
    public readonly SectorSpecification _sectorSpecification;
    
    public SectorService(SectorSpecification sectorSpecification)
    {
        _sectorSpecification = sectorSpecification;
    }

    public IReadOnlyList<SystemSpecification> GetSystemSpecifications()
    {
        return _sectorSpecification.Systems;
    }

    public SystemSpecification GetOneSystem(string systemId)
    {
        return _sectorSpecification.Systems.First(system => system.Name == systemId);
    }
}