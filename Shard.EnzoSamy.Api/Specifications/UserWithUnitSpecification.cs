using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Specifications;

public class UserWithUnitSpecification
{
    public string Id { get; set; }
    private string Pseudo { get; set; }
    public List<UnitSpecification> Units { get; set; }

    private readonly string[] _typeList = new[] { "scout", "builder" };

    private UserWithUnitSpecification(string id, string pseudo, List<UnitSpecification> units)
    {
        Id = id;
        Pseudo = pseudo;
        Units = units;
    }
    
    public UserWithUnitSpecification(string id, string pseudo, SystemSpecification system, List<UserWithUnitSpecification> userWithUnit)
    {
        var random = new Random();
        Id = id;
        Pseudo = pseudo;
        Units = Generate(random, system);
        userWithUnit.Add(new UserWithUnitSpecification(Id, Pseudo, Units));
    }

    private List<UnitSpecification> Generate(Random random, SystemSpecification system)
    {
        return Enumerable.Range(1, _typeList.Count())
            .Select(i => new UnitSpecification(random, system, _typeList[i%2]))
            .ToList();
    }
}