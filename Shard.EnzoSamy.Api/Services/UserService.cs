using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api;
public class UserService
{
    private readonly List<UserSpecification> _users;
    private readonly SectorSpecification _sector;
    private List<UserWithUnitSpecification> _usersWithUnit;

    public UserService(List<UserSpecification> users, SectorSpecification sector, List<UserWithUnitSpecification> usersWithUnits)
    {
        _users = users;
        _sector = sector;
        _usersWithUnit = usersWithUnits;
    }

    public UserSpecification FindUser(string userId)
    {
        return _users.FirstOrDefault(user => user.Id == userId);
    }

    public List<UnitSpecification> GetUnitsForUser(string userId)
    {
        var user = FindUser(userId);
        if (user == null) return null;
        
        UserWithUnitSpecification userWithUnit = new UserWithUnitSpecification(user.Id, user.Pseudo, _sector.Systems, _usersWithUnit);
        return userWithUnit.Units;
    }
    
    public int FindUserIndex(string userId)
    {
        return _users.FindIndex(user => user.Id == userId);
    }

    public List<UserWithUnitSpecification> GetUsersWithUnit()
    {
        return _usersWithUnit;
    }
    
    public UserWithUnitSpecification GetUserWithUnits(string userId)
    {
        return _usersWithUnit.FirstOrDefault(u => u.Id == userId);
    }
}