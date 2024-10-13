using Shard.EnzoSamy.Api.Specifications;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Services;

public class FightService(IClock clock)
{
    private readonly IClock _clock = clock;

    public async Task Fight(UnitSpecification unit1, UnitSpecification unit2)
    {
        int currentMinute = 0;

        while (unit1.Health > 0 && unit2.Health > 0)
        {
            currentMinute++;
            await _clock.Delay(TimeSpan.FromMinutes(1));
            
            unit1.Attack(unit2, currentMinute);
            unit2.Attack(unit1, currentMinute);
            
            if (unit1.Health <= 0 || unit2.Health <= 0)
            {
                break; 
            }
            
        }
    }

}