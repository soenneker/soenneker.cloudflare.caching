using Soenneker.Cloudflare.Caching.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Cloudflare.Caching.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class CloudflareCachingUtilTests : HostedUnitTest
{
    private readonly ICloudflareCachingUtil _util;

    public CloudflareCachingUtilTests(Host host) : base(host)
    {
        _util = Resolve<ICloudflareCachingUtil>(true);
    }

    [Test]
    public void Default()
    {

    }
}
