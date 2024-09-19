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
        var system = _sectorSpecification.Systems.FirstOrDefault(s => s.Name == systemId);
        if (system == null)
        {
            throw new InvalidOperationException($"System with ID {systemId} not found.");
        }
        return system;
    }

}