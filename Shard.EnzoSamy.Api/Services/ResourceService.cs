using Shard.EnzoSamy.Api;
using Shard.EnzoSamy.Api.Enumerations;

namespace Shard.EnzoSamy.Api.Services;

public class ResourceService
{
    public ResourceKind[] getResourceKindOfCategory(ResourceCategory category)
    {
        switch (category)
        {
            case ResourceCategory.gaseous: return [ResourceKind.Oxygen];
            case ResourceCategory.liquid: return [ResourceKind.Water];
            case ResourceCategory.solid: return [ResourceKind.Carbon, ResourceKind.Iron, ResourceKind.Aluminium, ResourceKind.Gold, ResourceKind.Titanium];
            default: return null;
        }
    }
}