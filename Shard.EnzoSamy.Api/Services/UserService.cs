﻿using Shard.EnzoSamy.Api.Specifications;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Services;

public class UserService
{
    private readonly List<UserSpecification> _users;
    private readonly SectorSpecification _sector;
    private readonly Random _random = new Random();

    public UserService(List<UserSpecification> users, SectorSpecification sector)
    {
        _users = users;
        _sector = sector;
    }

    public UserSpecification? FindUser(string userId)
    {
        return _users.FirstOrDefault(user => user.Id == userId);
    }

    public int FindUserIndex(string userId)
    {
        return _users.FindIndex(user => user.Id == userId);
    }

    public List<UnitSpecification>? GetUnitsForUser(string userId)
    {
        var user = FindUser(userId);
        return user?.Units;
    }
}