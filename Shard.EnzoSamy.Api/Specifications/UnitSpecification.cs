using Microsoft.Extensions.Logging.Abstractions;
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
    private Task? Arrive { get; set; }
    private Task? ArriveMinus2Sec { get; set; }
    private IClock? _clock;

    public UnitSpecification() { }

    public UnitSpecification(Random random, SystemSpecification system, string type)
    {
        Id = random.Next(1000).ToString();
        Type = type;
        SystemSpecification systemSpecification = system;
        System = systemSpecification.Name;
        Planet = null;
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