using Shard.EnzoSamy.Api.Specifications;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Services;

public class FightService(IClock clock)
{
    private readonly IClock _clock = clock;

    public async Task Fight(UnitSpecification unit1, UnitSpecification unit2)
    {

        while (unit1.Health > 0 && unit2.Health > 0)
        {
            await _clock.Delay(TimeSpan.FromSeconds(1));
            
            int currentSecond = _clock.Now.Second;
            
            unit1.Attack(unit2, currentSecond);
            unit2.Attack(unit1, currentSecond);
            
            if (unit1.Health <= 0 || unit2.Health <= 0)
            {
                break; 
            }
            
        }
    }

}