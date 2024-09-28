using Shard.EnzoSamy.Api.Services;
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
    private SectorService _sectorService;
    private UserService _userService;
    private string _userId;
    
    public async void StartBuild(IClock clock, SectorService sectorService, UserService userService, string userId)
    {
        _clock = clock;
        _sectorService = sectorService;
        _userService = userService;
        _userId = userId;
        EstimatedBuildTime = _clock.Now + TimeSpan.FromMinutes(5);
        _startBuildTask = _clock.Delay(TimeSpan.FromMinutes(5));
        FinishBuild();
        await ExtractContinuously();
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
    
    private async Task ExtractContinuously()
    {
            try
            {
                await _clock.Delay(TimeSpan.FromMinutes(1));
                FinishExtractContinuously();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during extraction: {ex.Message}");
            }
    }



    private async Task WaitIfExtract1Minutes()
    {
        if (_startExtract1Minutes is { IsCompleted: false }) return;
        if (_startExtract1Minutes != null) await _startExtract1Minutes;
    }

    private void FinishExtractContinuously()
    {
        var resourceKind = ExtractResourceFromPlanet();
        if (resourceKind != null)
        {
            AddResourceToUser(resourceKind);
            ExtractContinuously();
        }
        else
        {
            _startExtract1Minutes = Task.CompletedTask;
        }
    }

    private ResourceKind? ExtractResourceFromPlanet()
    {
          ResourceKind? resourceKind = _sectorService.ExtractResource(ResourceCategory, Planet, System);
          if (resourceKind is null) return null;
          return resourceKind;
    }

    private void AddResourceToUser(ResourceKind? resourceKind)
    {
        var user = _userService.FindUser(_userId);
        _userService.AddResourceToUser(user.Id, resourceKind.ToString());
    }
    

}