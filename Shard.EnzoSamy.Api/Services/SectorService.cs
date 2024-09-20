using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Services;

public class SectorService(SectorSpecification sectorSpecification)
{
    public IReadOnlyList<SystemSpecification> GetSystemSpecifications()
    {
        return sectorSpecification.Systems;
    }

    public SystemSpecification GetOneSystem(string systemId)
    {
        var system = sectorSpecification.Systems.FirstOrDefault(s => s.Name == systemId);
        if (system == null)
        {
            throw new InvalidOperationException($"System with ID {systemId} not found.");
        }
        return system;
    }

}