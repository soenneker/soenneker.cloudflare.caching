[![](https://img.shields.io/nuget/v/soenneker.cloudflare.caching.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.cloudflare.caching/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.cloudflare.caching/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.cloudflare.caching/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.cloudflare.caching.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.cloudflare.caching/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.cloudflare.caching/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.cloudflare.caching/actions/workflows/codeql.yml)

# Soenneker.Cloudflare.Caching

Manages selected Cloudflare zone cache settings and cache-purge operations through the generated Cloudflare client.

## Installation

```bash
dotnet add package Soenneker.Cloudflare.Caching
```

## Configuration

```json
{
  "Cloudflare": {
    "ApiKey": "your-api-token"
  }
}
```

Use a scoped API token with permission to read zone settings and edit the specific settings or cache entries your application changes.

## Registration

```csharp
using Soenneker.Cloudflare.Caching.Registrars;

services.AddCloudflareCachingUtilAsScoped();
```

Singleton registration is also available with `AddCloudflareCachingUtilAsSingleton()`.

## Purging cache entries

```csharp
using Soenneker.Cloudflare.Caching.Abstract;

public sealed class CacheInvalidator(ICloudflareCachingUtil caching)
{
    public ValueTask<bool> PurgeProduct(
        string zoneId,
        string productUrl,
        CancellationToken cancellationToken)
    {
        return caching.PurgeCache(zoneId, [productUrl], cancellationToken);
    }
}
```

The utility can purge URLs, hostnames, cache tags, prefixes, or the entire zone cache. Purging everything is destructive and should not be used as a routine substitute for targeted invalidation.

## Updating selected settings

```csharp
using Soenneker.Cloudflare.Caching.Models;

CloudflareCacheSettings? updated = await caching.UpdateCacheSettings(
    zoneId,
    new CloudflareCacheSettings
    {
        BrowserCacheTtl = 3600
    },
    cancellationToken);
```

Only non-null values for browser-cache TTL, query-string sorting, cache level, Always Online, and Development Mode are patched. Omitted nullable values are left unchanged. `RespectStrongEtags` and `PurgeCacheOnChange` are currently data-model fields only and are not read or written by `GetCacheSettings` or `UpdateCacheSettings`.

`GetCacheSettings` reads several settings independently and may return a partially populated object when Cloudflare rejects an individual setting. Mutating methods log failures and return `null` or `false`; a successful boolean indicates Cloudflare accepted the purge request.

Smart Tiered Cache, Crawler Hints, and Always Online also have dedicated get, enable, and disable methods when only one feature should change.
