using System.Globalization;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Specifications;

public class UserSpecification
{
    public string Id { get; set; }
    public string Pseudo { get; set; }
    public DateTime DateOfCreation { get; set; }
    public Dictionary<string, int?>? ResourcesQuantity { get; set; }
    public List<UnitSpecification>? Units { get; set; }
    
    public List<BuildingSpecification>? Buildings { get; set; }

    public UserSpecification(){}
    

    public UserSpecification(string id, string pseudo, List<UnitSpecification> units)
    {
        Id = id;
        Pseudo = pseudo;
        Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
        DateOfCreation = DateTime.Now;
        ResourcesQuantity = _initializeResources();
        Units = units;
        Buildings = [];
    }

    public UserSpecification(string id, string pseudo, DateTime dateOfCreation, List<UnitSpecification> units)
    {
        Id = id;
        Pseudo = pseudo;
        Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
        DateOfCreation = dateOfCreation;
        ResourcesQuantity = _initializeResources();
        Units = units;
        Buildings = []; 
    }

    private Dictionary<string, int?> _initializeResources()
    {
        var resourceQuantities = new Dictionary<string, int?>();

        foreach (ResourceKind resourceKind in Enum.GetValues(typeof(ResourceKind)))
        {
            resourceQuantities[resourceKind.ToString().ToLower()] = resourceKind switch
            {
                ResourceKind.Carbon => 20,
                ResourceKind.Iron => 10,
                ResourceKind.Oxygen or ResourceKind.Water => 50,
                _ => 0
            };
        }

        return resourceQuantities;
    }
}
