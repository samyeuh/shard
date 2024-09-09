using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api;
public class UserService
{
    private readonly List<UserSpecification> _users;
    private readonly SectorSpecification _sector;

    public UserService(List<UserSpecification> users, SectorSpecification sector)
    {
        _users = users;
        _sector = sector;
    }

    public UserSpecification FindUser(string userId)
    {
        return _users.FirstOrDefault(user => user.Id == userId);
    }

    public List<UnitSpecification> GetUnitsForUser(string userId)
    {
        var user = FindUser(userId);
        if (user == null) return null;
        
        UserWithUnitSpecification userWithUnit = new UserWithUnitSpecification(user.Id, user.Pseudo, _sector.Systems);
        return userWithUnit.Units;
    }
}