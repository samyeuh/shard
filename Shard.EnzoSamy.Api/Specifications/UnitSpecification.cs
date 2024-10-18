using Microsoft.Extensions.Logging.Abstractions;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Specifications;

public class UnitSpecification
{
    public string? Id { get; init; }
    public string Type { get; init; }
    public string System { get; set; }
    public string? Planet { get; set; }
    public string? DestinationSystem { get; set; }
    public string? DestinationPlanet { get; set; }
    public DateTime? EstimatedTimeOfArrival { get; set; }
    public int? Health { get; set; }

    public record Weapon(int secondInterval, int damage);
    
    public List<Weapon>? Weapons { get; set; }
    public List<String>? TypePriority { get; set; }
    private Task? Arrive { get; set; }
    private Task? ArriveMinus2Sec { get; set; }
    private IClock? _clock;
    public string? UserId { get; set; }
    public Dictionary<string, int?>? ResourceQuantity { get; set; } = new Dictionary<string, int?>();

    public UnitSpecification() { }
    
    public UnitSpecification(SystemSpecification system, string type)
    {
        Id = Guid.NewGuid().ToString();
        Type = type;
        System = system.Name;
        Planet = null;
        UserId = string.Empty;
        SetCombatSpec();
    }

    public UnitSpecification(SystemSpecification system, string type, string userId)
    {
        Id = Guid.NewGuid().ToString();
        Type = type;
        System = system.Name;
        Planet = null;
        UserId = userId;
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
    }

    private void SetBomberSpec()
    {
        Health = 50;
        Weapons.Add(new Weapon(60, 400));
        TypePriority = ["cruiser", "bomber", "fighter"];
    }

    private void SetFighterSpec()
    {
        Health = 80;
        Weapons.Add(new Weapon(6, 10));
        TypePriority = ["bomber", "fighter", "cruiser"];
    }
    
    public bool CanAttack(int currentSecond)
    {
        foreach (var weapon in Weapons)
        {
            int interval = weapon.secondInterval;
            if (currentSecond % interval == 0)
            {
                return true; 
            }
        }
        return false;
    }

    public void Attack(UnitSpecification enemy, int currentSecond)
    {
        if (CanAttack(currentSecond))
        {
            foreach (var weapon in Weapons)
            {
                int interval = weapon.secondInterval;
                if (currentSecond % interval == 0)
                {
                    int damage = weapon.damage;
                    if (Type == "cruiser" && enemy.Type == "bomber") damage /= 10;
                    enemy.Health -= enemy.Health < damage ? enemy.Health : damage;
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

    public void ArriveAtDestination()
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
    
    public string Url => $"/users/{UserId}/units/{Id}";
}