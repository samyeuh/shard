using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Shard.EnzoSamy.Api.Services;

namespace Shard.EnzoSamy.Api.Background;

public class FightBackground : BackgroundService
{
    private readonly FightService _fightService;

    public FightBackground(FightService fightService)
    {
        _fightService = fightService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _fightService.StartFights(stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
