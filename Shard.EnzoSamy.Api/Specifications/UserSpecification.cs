namespace Shard.EnzoSamy.Api.Specifications;

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
