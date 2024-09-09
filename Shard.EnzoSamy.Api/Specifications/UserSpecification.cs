using System.CodeDom.Compiler;
using System.Text.Json.Serialization;
using Shard.Shared.Core;

namespace Shard.EnzoSamy.Api;

public class UserSpecification
{
    public string Id { get; set; }
    public string Pseudo { get; set; }

    public UserSpecification()
    {
        Id = Guid.NewGuid().ToString();
    }

    public UserSpecification(Random random) : this()
    {
        Pseudo = random.NextGuid().ToString();
    }
}
