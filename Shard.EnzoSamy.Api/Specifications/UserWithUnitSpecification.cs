using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api;

public class UserWithUnitSpecification
{
    public string Id { get; set; }
    public string Pseudo { get; set; }
    public List<UnitSpecification> Units { get; set; }
    
    public UserWithUnitSpecification()
    { }
    
    public UserWithUnitSpecification(string id, string pseudo, IReadOnlyList<SystemSpecification> systemList)
    {
        Random random = new Random();
        Id = id;
        Pseudo = pseudo;
        Units = Generate(random, systemList);
    }

    private List<UnitSpecification> Generate(Random random, IReadOnlyList<SystemSpecification> systemList)
    {
        return new List<UnitSpecification> { new UnitSpecification(random, systemList) };
    }
}