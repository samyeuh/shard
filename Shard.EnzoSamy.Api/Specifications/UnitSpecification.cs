﻿using Microsoft.Extensions.Logging.Abstractions;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Specifications;

public class UnitSpecification
{
    public string Id { get; init; }
    public string Type { get; init; }
    public string System { get; set; }
    public string? Planet { get; set; }
    public string? DestinationSystem { get; set; }
    public string? DestinationPlanet { get; set; }
    public DateTime? EstimatedTimeOfArrival { get; set; }
    public int? Health { get; set; }

    public record Weapon(int minuteInterval, int damage);
    
    public List<Weapon>? Weapons { get; set; }
    public List<String>? TypePriority { get; set; }
    public Boolean? Deflector { get; set; }
    private Task? Arrive { get; set; }
    private Task? ArriveMinus2Sec { get; set; }
    private IClock? _clock;

    public UnitSpecification() { }

    public UnitSpecification(SystemSpecification system, string type)
    {
        Id = Guid.NewGuid().ToString();
        Type = type;
        System = system.Name;
        Planet = null;
        SetCombatSpec();
    }
    
    public void SetCombatSpec()
    {
        Weapons = new List<Weapon>();
        TypePriority = new List<string>();
        switch (Type.ToLower())
        {
            case "cruiser": SetCruiserSpec(); break;
            case "bomber": SetBomberSpec(); break;
            case "fighter": SetFighterSpec(); break;
        }
    }

    private void SetCruiserSpec()
    {
        Health = 400;
        Weapons.Add(new Weapon(6, 10));
        Weapons.Add(new Weapon(6, 10));
        Weapons.Add(new Weapon(6, 10));
        Weapons.Add(new Weapon(6, 10));
        TypePriority = ["fighter", "cruiser", "bomber"];
        Deflector = false;
    }

    private void SetBomberSpec()
    {
        Health = 50;
        Weapons.Add(new Weapon(1, 400));
        TypePriority = ["cruiser", "bomber", "fighter"];
        Deflector = true;
    }

    private void SetFighterSpec()
    {
        Health = 80;
        Weapons.Add(new Weapon(6, 10));
        TypePriority = ["bomber", "fighter", "cruiser"];
    }
    
    public bool CanAttack(int currentMinute)
    {
        foreach (var weapon in Weapons)
        {
            int interval = weapon.minuteInterval;
            if (currentMinute % interval == 0)
            {
                return true; 
            }
        }
        return false;
    }

    public void Attack(UnitSpecification enemy, int currentMinute)
    {
        if (CanAttack(currentMinute))
        {
            foreach (var weapon in Weapons)
            {
                int interval = weapon.minuteInterval;
                if (currentMinute % interval == 0)
                {
                    enemy.Health -= weapon.damage;
                }
            }
        }
    }

    public void StartTravel(string destinationSystem, string destinationPlanet, DateTime estimatedArrivalTime, IClock? clock)
    {
        DestinationSystem = destinationSystem;
        DestinationPlanet = destinationPlanet;
        EstimatedTimeOfArrival = estimatedArrivalTime;
        _clock = clock;
        
        var timeUntilArrival = CalculateEstimatedArrivalTime(estimatedArrivalTime);
        Arrive = _clock.Delay(timeUntilArrival).ContinueWith(_ => ArriveAtDestination());
        ArriveMinus2Sec = _clock.Delay(timeUntilArrival-TimeSpan.FromSeconds(2));
    }

    public async Task WaitIfArrived()
    {
        if(ArriveMinus2Sec is { IsCompleted: false }) return;
        if (Arrive != null) await Arrive;
    }

    private void ArriveAtDestination()
    {
        if(DestinationSystem is not null) 
            System = DestinationSystem;
        Planet = DestinationPlanet;
        DestinationSystem = null;
        DestinationPlanet = null;
        EstimatedTimeOfArrival = null;

        if (Arrive is { IsCompleted: false })
        {
            Arrive = Task.CompletedTask;
        }
    }
    
    private TimeSpan CalculateEstimatedArrivalTime(DateTime? estimatedArrivalTime)
    {
        if (_clock == null) return TimeSpan.Zero;
        var now = _clock.Now;
        var timeUntilArrival = estimatedArrivalTime - now;
        return timeUntilArrival ?? TimeSpan.Zero;
    }
}