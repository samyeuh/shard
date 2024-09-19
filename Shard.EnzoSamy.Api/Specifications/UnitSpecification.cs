using System.Numerics;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api;

public class UnitSpecification
{
    public string Id { get; init; }
    public string Type { get; init; }
    public string System { get; set; }
    public string? Planet { get; set; }
    public string? DestinationSystem { get; set; }
    public string? DestinationPlanet { get; set; }
    public DateTime? EstimatedTimeOfArrival { get; set; }
    public Task? Arrive { get; private set; }
    public Task? ArriveMinus2Sec { get; private set; }
    private IClock _clock;

    public UnitSpecification() { }

    public UnitSpecification(Random random, SystemSpecification system, string type)
    {
        Id = random.Next(1000).ToString();
        Type = type;
        SystemSpecification systemSpecification = system;
        System = systemSpecification.Name;
        IReadOnlyList<PlanetSpecification> planetList = systemSpecification.Planets.ToList();
        Planet = planetList[random.Next(planetList.Count)].Name;
    }

    public void StartTravel(string destinationSystem, string destinationPlanet, DateTime estimatedArrivalTime, IClock clock)
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

        if (Arrive != null && !Arrive.IsCompleted)
        {
            Arrive = Task.CompletedTask;
        }
    }
    
    private TimeSpan CalculateEstimatedArrivalTime(DateTime? estimatedArrivalTime)
    {
        var now = _clock.Now;
        var timeUntilArrival = estimatedArrivalTime - now;
        return timeUntilArrival ?? TimeSpan.Zero;
    }
}