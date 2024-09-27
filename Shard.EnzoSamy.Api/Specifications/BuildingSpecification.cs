using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Specifications;

public class BuildingSpecification(string type, string planet, string system, string builderId, string resourceCategory)
{
    
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = type;
    public string BuilderId { get; set; } = builderId;
    public string? Planet { get; set; } = planet;
    public string? System { get; set; } = system;
    public DateTime? EstimatedBuildTime { get; set; }
    public bool IsBuilt { get; set; }
    public string? ResourceCategory { get; set; } = resourceCategory;
    private Task? _startBuildTask;
    private Task? _startBuildTaskMinus2Seconds;
    private Task? _startExtract1Minutes;
    private IClock _clock;
    
    public void StartBuild(IClock clock)
    {
        _clock = clock;
        EstimatedBuildTime = _clock.Now + TimeSpan.FromMinutes(5);
        _startBuildTask = _clock.Delay(TimeSpan.FromMinutes(5)).ContinueWith(_ => FinishBuild());
        _startBuildTaskMinus2Seconds = _clock.Delay(TimeSpan.FromMinutes(5) - TimeSpan.FromSeconds(2));
    }

    public async Task WaitIfBuild()
    {
        if(_startBuildTaskMinus2Seconds is { IsCompleted: false }) return;
        if (_startBuildTask != null) await _startBuildTask;
    }

    private void FinishBuild()
    {
        IsBuilt = true;
        EstimatedBuildTime = null;
        if (_startBuildTask is { IsCompleted: false })
        {
            _startBuildTask = Task.CompletedTask;
        }
    }
    
    private void ExtractContinuously(IClock clock)
    {
        _clock = clock;
        _startExtract1Minutes = _clock.Delay(TimeSpan.FromMinutes(1)).ContinueWith(_ => FinishExtractContinuously(clock));
    }

    private async Task WaitIfExtract1Minutes()
    {
        if (_startExtract1Minutes is { IsCompleted: false }) return;
        if (_startExtract1Minutes != null) await _startExtract1Minutes;
    }

    private void FinishExtractContinuously(IClock clock)
    {
        if (_startExtract1Minutes is { IsCompleted: false })
        {
            _startExtract1Minutes = Task.CompletedTask;
        }
        ExtractContinuously(clock);
    }
    

}