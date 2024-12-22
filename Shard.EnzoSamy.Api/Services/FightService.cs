using Shard.EnzoSamy.Api.Specifications;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api.Services;

public class FightService(List<UserSpecification> users, UnitService unitService, IClock clock)
{
    public void PerformFights()
    {
        List<string> fightType = ["cruiser", "fighter", "bomber"];
        var allUnits = users.SelectMany(user => user.Units).ToList();
        var combatUnits = allUnits.Where(u => fightType.Contains(u.Type)).ToList();

        foreach (var attacker in combatUnits)
        {
            UnitSpecification? target;
            if (attacker.Planet == null)
            {
                target = combatUnits.
                    Where(u => u.Id != attacker.Id && attacker.System == u.System).
                    OrderBy(u => attacker.TypePriority.IndexOf(u.Type))
                    .FirstOrDefault();
            } else {
                target = combatUnits.Where(u => u.Id != attacker.Id && attacker.Planet == u.Planet)
                    .OrderBy(u => attacker.TypePriority.IndexOf(u.Type)).FirstOrDefault();
            }

            if (target is null) continue;
            
            attacker.Attack(target, clock.Now.Second);

            /*var enemy = units.Where(u => u.Id != unit.Id).OrderBy(u => unit.TypePriority.IndexOf(u.Type)).FirstOrDefault();
            if (enemy is null) continue;
            Fight f = new Fight(unit, enemy);
            Fights.Add(f);*/
        }

        foreach (var unit in allUnits)
        {
            var user = users.FirstOrDefault(u => u.Units != null && u.Units.Contains(unit));
            if (user is null) continue;
            if (unit.Health <= 0)
            {
                if (unit.Id != null) unitService.DestroyUnit(user.Id, unit.Id);
            }
        }
            //if unit.health <= 0
            //destroy -> remove from user list
    }

    public async Task StartFights(CancellationToken stoppingToken)
    {
        clock.CreateTimer(_ => PerformFights(), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }
    
    
}