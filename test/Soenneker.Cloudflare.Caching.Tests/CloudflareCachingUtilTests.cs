using Soenneker.Cloudflare.Caching.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Cloudflare.Caching.Tests;

[Collection("Collection")]
public sealed class CloudflareCachingUtilTests : FixturedUnitTest
{
    private readonly ICloudflareCachingUtil _util;

    public CloudflareCachingUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<ICloudflareCachingUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
