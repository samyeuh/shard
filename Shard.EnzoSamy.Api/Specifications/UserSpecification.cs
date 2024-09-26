using System.Globalization;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Specifications;

public class UserSpecification
{
    public string Id { get; set; }
    public string? Pseudo { get; set; }
    
    public String DateOfCreation { get; }
    
    public Dictionary<String, int?> ResourcesQuantity { get; set; }

    public UserSpecification()
    {
        Id = Guid.NewGuid().ToString();
        Pseudo = Guid.NewGuid().ToString();
        Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
        DateOfCreation = DateTime.Now.ToShortDateString();
        ResourcesQuantity = initializeResources();
    }

    private Dictionary<String, int?> initializeResources()
    {
        var resourceQuantities = new Dictionary<String, int?>();
        
        foreach (ResourceKind resourceKind in Enum.GetValues(typeof(ResourceKind)))
        {
            switch (resourceKind)
            {  
               case ResourceKind.Carbon: resourceQuantities[resourceKind.ToString().ToLower()] = 20; break;
               case ResourceKind.Iron: resourceQuantities[resourceKind.ToString().ToLower()] = 10; break;
               case ResourceKind.Oxygen: 
               case ResourceKind.Water: 
                   resourceQuantities[resourceKind.ToString().ToLower()] = 50; break;
               default: resourceQuantities[resourceKind.ToString().ToLower()] = 0; break;
            }
        }

        return resourceQuantities;

    }
}
