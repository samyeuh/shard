﻿using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api;
public class UserService
{
    private readonly List<UserSpecification> _users;
    private readonly SectorSpecification _sector;
    private List<UserWithUnitSpecification> _usersWithUnit;
    private Random _random = new Random();
    private int indexSystem;

    public UserService(List<UserSpecification> users, SectorSpecification sector, List<UserWithUnitSpecification> usersWithUnits)
    {
        _users = users;
        _sector = sector;
        _usersWithUnit = usersWithUnits;
        indexSystem = _random.Next(_sector.Systems.Count);
        
        
    }

    public UserSpecification FindUser(string userId)
    {
        return _users.FirstOrDefault(user => user.Id == userId);
    }

    public List<UnitSpecification> GetUnitsForUser(string userId)
    {
        UserSpecification? user = FindUser(userId);
        if (user == null)return null;

        UserWithUnitSpecification? userWithUnits = GetUserWithUnits(userId);
        return userWithUnits == null ? new UserWithUnitSpecification(user.Id, user.Pseudo, _sector.Systems[indexSystem], _usersWithUnit).Units : userWithUnits.Units;
    }
    
    public int FindUserIndex(string userId)
    {
        return _users.FindIndex(user => user.Id == userId);
    }

    public List<UserWithUnitSpecification> GetUsersWithUnit()
    {
        return _usersWithUnit;
    }
    
    public UserWithUnitSpecification? GetUserWithUnits(string userId)
    {
        UserWithUnitSpecification? userWithUnit = _usersWithUnit.FirstOrDefault(u => u.Id == userId);
        return userWithUnit == null ? null : userWithUnit;
    }
    
    
}