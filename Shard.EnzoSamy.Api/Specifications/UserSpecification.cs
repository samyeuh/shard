using System.Globalization;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Specifications;

public class UserSpecification
{
    public string Id { get; set; }
    public string? Pseudo { get; set; }
    public string DateOfCreation { get; set; } // Convertir en set pour permettre la désérialisation
    public Dictionary<string, int?> ResourcesQuantity { get; set; }
    public List<UnitSpecification> Units { get; set; }
    
    private readonly string[] _typeList = new[] { "scout", "builder" };
    
    public UserSpecification()
    {
        Id = Guid.NewGuid().ToString();
        Pseudo = Guid.NewGuid().ToString();
        Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
        DateOfCreation = DateTime.Now.ToShortDateString();
        ResourcesQuantity = _initializeResources();
        Units = [];
    }

    private Dictionary<string, int?> _initializeResources()
    {
        var resourceQuantities = new Dictionary<string, int?>();

        foreach (ResourceKind resourceKind in Enum.GetValues(typeof(ResourceKind)))
        {
            switch (resourceKind)
            {
                case ResourceKind.Carbon:
                    resourceQuantities[resourceKind.ToString().ToLower()] = 20;
                    break;
                case ResourceKind.Iron:
                    resourceQuantities[resourceKind.ToString().ToLower()] = 10;
                    break;
                case ResourceKind.Oxygen:
                case ResourceKind.Water:
                    resourceQuantities[resourceKind.ToString().ToLower()] = 50;
                    break;
                default:
                    resourceQuantities[resourceKind.ToString().ToLower()] = 0;
                    break;
            }
        }

        return resourceQuantities;
    }
}
