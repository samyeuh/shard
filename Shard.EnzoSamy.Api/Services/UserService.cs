using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
        var user = new UserSpecification(newUser.Id, newUser.Pseudo, newUser.DateOfCreation, newUser.ResourcesQuantity, newUser.Buildings,generatedUnits);
        var usersId = _users.Select(user => user.Id).ToList();
        if (usersId.Contains(newUser.Id)) _users.Remove(_users.Where(user => user.Id == newUser.Id).First());
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

    public void removeResourceToUser(UserSpecification user, Dictionary<string, int?> resources)
    {
        foreach (var resource in resources)
        {
            // Vérifie d'abord si l'unité a bien cette ressource dans son inventaire
            if (!user.ResourcesQuantity.ContainsKey(resource.Key))
            {
                throw new KeyNotFoundException($"Resource '{resource.Key}' not found in the unit's inventory.");
            }
        
            // Ensuite, vérifie que la quantité n'est pas négative après soustraction
            if (user.ResourcesQuantity[resource.Key] - resource.Value < 0)
            {
                throw new InvalidOperationException($"Insufficient quantity of '{resource.Key}' in the unit to remove {resource.Value}.");
            }
        
            // Effectue la soustraction si tout est vérifié
            user.ResourcesQuantity[resource.Key] -= resource.Value;
        }
    }

    public void AddResourceToUser(UserSpecification user, Dictionary<string, int?> resources)
    {
        foreach (var ressource in resources)
        {
            if (user.ResourcesQuantity.ContainsKey(ressource.Key))
            {
                user.ResourcesQuantity[ressource.Key] += ressource.Value;
            }
            else
            {
                user.ResourcesQuantity.Add(ressource.Key, ressource.Value);
            }
        }
    }

}