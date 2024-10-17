using Shard.EnzoSamy.Api.Contracts;
using Shard.EnzoSamy.Api.Specifications;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Services;

public class UserService
{
    private readonly List<UserSpecification> _users;
    private readonly SectorSpecification _sector;

    public UserService(List<UserSpecification> users, SectorSpecification sector)
    {
        _users = users;
        _sector = sector;
    }

    public UserSpecification CreateUser(UserSpecification newUser, bool isAdmin=false)
    {
        var generatedUnits = _generateUnits();
        if (!isAdmin && newUser.ResourcesQuantity != null) newUser.ResourcesQuantity = null;
        var user = new UserSpecification(newUser.Id, newUser.Pseudo, newUser.DateOfCreation, newUser.ResourcesQuantity, generatedUnits);
        _users.Add(user);
        return user;
    }
    
    private List<UnitSpecification> _generateUnits()
    {
        var system = _sector.Systems.FirstOrDefault();
        var unitScout = new UnitSpecification(system, "scout");
        var unitBuilder = new UnitSpecification(system, "builder");
        return [unitScout, unitBuilder];
    }

    public UserSpecification? FindUser(string userId)
    {
        return _users.FirstOrDefault(user => user.Id == userId);
    }

    public List<UnitSpecification>? GetUnitsForUser(string userId)
    {
        var user = FindUser(userId);
        return user?.Units;
    }

    public void AddResourceToUser(string userId, string resourceKind)
    {
        var user = FindUser(userId);
        user.ResourcesQuantity[resourceKind] += 1;
    }

    public UserSpecification? getUserWithUnit(string unitId)
    {
        foreach (UserSpecification user in _users)
        {
            var goodUser = user.Units.FirstOrDefault(u => u.Id == unitId);
            if (goodUser != null) return user;
        }

        return null;
    }
    public void DeductResources(UserSpecification user, Dictionary<string, int> resources)
    {
        foreach (var resource in resources)
        {
            if (user.ResourcesQuantity.ContainsKey(resource.Key))
            {
                user.ResourcesQuantity[resource.Key] -= resource.Value;
            }
        }
    }

    public bool HasSufficientResources(UserSpecification user, Dictionary<string, int> requiredResources)
    {
        return requiredResources.All(resource =>
            user.ResourcesQuantity.TryGetValue(resource.Key, out var quantity) && quantity >= resource.Value);
    }

}