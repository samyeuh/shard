using Shard.EnzoSamy.Api.Specifications;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Services;

public class FightService(List<FightService.Fight> fights)
{
    public record Fight(UnitSpecification unit1, UnitSpecification unit2);

    public List<Fight> Fights = fights;
    private IClock _clock;


    public void checkFightPriority(UnitSpecification attacker, UnitSpecification target)
    {
        var fighter = IsInFight(target);
        if (fighter != null && fighter != attacker)
        {
            if (target.TypePriority.IndexOf(fighter.Type) > target.TypePriority.IndexOf(attacker.Type))
            {
                StopFight(fighter, target);
            }
        }
        
        
    }

    public async Task StartFight(UnitSpecification unit1, UnitSpecification unit2, IClock clock)
    {
        checkFightPriority(unit1, unit2);
        if (IsInFight(unit2) != null) return;
        Fight fight = new Fight(unit1, unit2);
        Fights.Add(fight);
        _clock = clock;
        
        while (unit1.Health > 0 && unit2.Health > 0 && Fights.Contains(fight))
        {
            await _clock.Delay(TimeSpan.FromSeconds(1));
            var currentSecond = _clock.Now.Second;
            
            unit1.Attack(unit2, currentSecond);
            unit2.Attack(unit1, currentSecond);
            
            if (unit1.Health <= 0 || unit2.Health <= 0)
            {
                break; 
            }
        }
    }

    public void StopFight(UnitSpecification unit1, UnitSpecification unit2)
    {
        var fightsToRemove = Fights
            .Where(f => (f.unit1 == unit1 && f.unit2 == unit2) || (f.unit1 == unit2 && f.unit2 == unit1))
            .ToList();
        
        foreach (var fight in fightsToRemove)
        {
            Fights.Remove(fight);
        }
    }

    public void StopFightOfAUnit(UnitSpecification unit)
    {
        var fightsToRemove = Fights
            .Where(f => (f.unit1 == unit || f.unit2 == unit))
            .ToList();
        
        foreach (var fight in fightsToRemove)
        {
            Fights.Remove(fight);
        }
    }

    public UnitSpecification IsInFight(UnitSpecification unit)
    {
        foreach (Fight fight in Fights)
        {
            if (fight.unit1 == unit) return fight.unit2;
            if (fight.unit2 == unit) return fight.unit1;
        }

        return null;
    }
}