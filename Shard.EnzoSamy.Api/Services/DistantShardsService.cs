namespace Shard.EnzoSamy.Api.Services;

public class DistantShardsService
{
    public Dictionary<string, DistantShard> Shards { get; set; } = [];

    public DistantShardsService(IConfiguration configuration)
    {
        var section = configuration.GetSection("Wormholes");
        foreach (var shard in section.GetChildren())
        {
            var shardConfiguration = shard.Get<DistantShard>() ?? throw new Exception();
            Shards.Add($"shard-{shard.Key}", shardConfiguration);
        }
    }
    
    public DistantShard? this [string username]
        => Shards.GetValueOrDefault(username);
}

public class DistantShard
{
    public required Uri BaseUri { get; set; }
    public required string System { get; set; }
    public required string User { get; set; }
    public required string SharedPassword { get; set; }
}