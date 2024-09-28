namespace Shard.EnzoSamy.Api;

[Flags]
public enum ResourceKind
{
    Carbon = 1,
    Iron = 2,
    Gold = 4,
    Aluminium = 3,
    Titanium = 5,
    Water = 0,
    Oxygen = -1,
}