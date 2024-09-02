using Xunit;
using Shard.Shared.Web.IntegrationTests;
using Microsoft.AspNetCore.Mvc.Testing; // Pour WebApplicationFactory
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit.Abstractions;

namespace ShardTestProject.Tests
{
    public class IntegrationTests : BaseIntegrationTests<Program>
    {
        public IntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper outputHelper)
            : base(factory, outputHelper)
        {
        }
       
        [Fact]
        public void TestExemple()
        {
            Console.WriteLine("Hey TU 1!");
        }
    }
}