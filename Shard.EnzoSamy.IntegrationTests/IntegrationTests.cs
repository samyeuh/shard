using Microsoft.AspNetCore.Mvc.Testing;
using Shard.Shared.Web.IntegrationTests;
using Shard.EnzoSamy.Api;
using Xunit.Abstractions;

namespace Shard.EnzoSamy.IntegrationTests;

public class IntegrationTests: BaseIntegrationTests<Program>
{
    public IntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public void Test1()
    {
    }
}