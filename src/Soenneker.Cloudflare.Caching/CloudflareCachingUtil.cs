using Microsoft.Extensions.Logging;
using Soenneker.Cloudflare.Caching.Abstract;
using Soenneker.Cloudflare.Caching.Models;
using Soenneker.Cloudflare.OpenApiClient;
using Soenneker.Cloudflare.OpenApiClient.Models;
using Soenneker.Cloudflare.Utils.Client.Abstract;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Cloudflare.Caching;

/// <inheritdoc cref="ICloudflareCachingUtil" />
public sealed class CloudflareCachingUtil : ICloudflareCachingUtil
{
    private readonly ILogger<CloudflareCachingUtil> _logger;
    private readonly ICloudflareClientUtil _client;

    public CloudflareCachingUtil(ILogger<CloudflareCachingUtil> logger, ICloudflareClientUtil client)
    {
        _logger = logger;
        _client = client;
    }

    public async ValueTask<CloudflareCacheSettings?> GetCacheSettings(string zoneId, CancellationToken cancellationToken = default)
    {
        try
        {
            CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
            var settings = new CloudflareCacheSettings();
            
            // Get each setting individually instead of using the obsolete bulk GetAsync
            try
            {
                ZoneSettingsGetSingleSetting200Response? browserCacheResponse = await client.Zones[zoneId].Settings["browser_cache_ttl"].GetAsync(cancellationToken: cancellationToken).NoSync();
                if (browserCacheResponse?.Result?.ZonesBrowserCacheTtl2 != null)
                {
                    settings.BrowserCacheTtl = browserCacheResponse.Result.ZonesBrowserCacheTtl2.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not get browser cache TTL setting for zone {ZoneId}", zoneId);
            }
            
            try
            {
                ZoneSettingsGetSingleSetting200Response? queryStringResponse = await client.Zones[zoneId].Settings["sort_query_string_for_cache"].GetAsync(cancellationToken: cancellationToken).NoSync();
                if (queryStringResponse?.Result?.ZonesSortQueryStringForCache2?.Value != null)
                {
                    settings.QueryStringSort = Enum.Parse<ZonesSortQueryStringForCacheValue>(queryStringResponse.Result.ZonesSortQueryStringForCache2.Value.Value.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not get query string sort setting for zone {ZoneId}", zoneId);
            }
            
            try
            {
                ZoneSettingsGetSingleSetting200Response? cacheLevelResponse = await client.Zones[zoneId].Settings["cache_level"].GetAsync(cancellationToken: cancellationToken).NoSync();
                if (cacheLevelResponse?.Result?.ZonesCacheLevel2 != null)
                {
                    settings.CacheLevel = cacheLevelResponse.Result.ZonesCacheLevel2.Value is { } value ? Enum.Parse<ZonesCacheLevelValue>(value.ToString()) : null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not get cache level setting for zone {ZoneId}", zoneId);
            }
            
            try
            {
                ZoneSettingsGetSingleSetting200Response? alwaysOnlineResponse = await client.Zones[zoneId].Settings["always_online"].GetAsync(cancellationToken: cancellationToken).NoSync();
                if (alwaysOnlineResponse?.Result?.ZonesAlwaysOnline?.Value != null)
                {
                    settings.AlwaysOnline = Enum.Parse<ZonesAlwaysOnlineValue>(alwaysOnlineResponse.Result.ZonesAlwaysOnline.Value.Value.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not get always online setting for zone {ZoneId}", zoneId);
            }
            
            try
            {
                ZoneSettingsGetSingleSetting200Response? developmentModeResponse = await client.Zones[zoneId].Settings["development_mode"].GetAsync(cancellationToken: cancellationToken).NoSync();
                if (developmentModeResponse?.Result?.ZonesDevelopmentMode?.Value != null)
                {
                    settings.DevelopmentMode = Enum.Parse<ZonesDevelopmentModeValue>(developmentModeResponse.Result.ZonesDevelopmentMode.Value.Value.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not get development mode setting for zone {ZoneId}", zoneId);
            }

            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache settings for zone {ZoneId}", zoneId);
            return null;
        }
    }

    public async ValueTask<CloudflareCacheSettings?> UpdateCacheSettings(string zoneId, CloudflareCacheSettings settings,
        CancellationToken cancellationToken = default)
    {
        try
        {
            CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
            // Update each setting individually instead of using the obsolete bulk PatchAsync
            var updateTasks = new List<Task>();
            
            // Update browser cache TTL.
            if (settings.BrowserCacheTtl.HasValue)
            {
                var browserCacheRequest = new ZonesZoneSettingsSingleRequest
                {
                    ZonesZoneSettingsSingleRequestOneOf2 = new ZonesZoneSettingsSingleRequestOneOf2
                    {
                        Value = new ZonesSettingValue
                        {
                            Integer = (int)settings.BrowserCacheTtl.Value
                        }
                    }
                };
                updateTasks.Add(client.Zones[zoneId].Settings["browser_cache_ttl"].PatchAsync(browserCacheRequest, cancellationToken: cancellationToken));
            }
            
            // Query string sorting uses the on/off wire enum.
            if (settings.QueryStringSort.HasValue)
            {
                var queryStringRequest = new ZonesZoneSettingsSingleRequest
                {
                    ZonesZoneSettingsSingleRequestOneOf2 = new ZonesZoneSettingsSingleRequestOneOf2
                    {
                        Value = new ZonesSettingValue
                        {
                            ZonesSortQueryStringForCacheValue = settings.QueryStringSort.Value
                        }
                    }
                };
                updateTasks.Add(client.Zones[zoneId].Settings["sort_query_string_for_cache"].PatchAsync(queryStringRequest, cancellationToken: cancellationToken));
            }
            
            // Update cache level.
            if (settings.CacheLevel.HasValue)
            {
                var cacheLevelRequest = new ZonesZoneSettingsSingleRequest
                {
                    ZonesZoneSettingsSingleRequestOneOf2 = new ZonesZoneSettingsSingleRequestOneOf2
                    {
                        Value = new ZonesSettingValue
                        {
                            ZonesCacheLevelValue = (ZonesCacheLevelValue)settings.CacheLevel.Value
                        }
                    }
                };
                updateTasks.Add(client.Zones[zoneId].Settings["cache_level"].PatchAsync(cacheLevelRequest, cancellationToken: cancellationToken));
            }
            
            // Update always online only when the caller supplied it.
            if (settings.AlwaysOnline.HasValue)
            {
                var alwaysOnlineRequest = new ZonesZoneSettingsSingleRequest
                {
                    ZonesZoneSettingsSingleRequestOneOf2 = new ZonesZoneSettingsSingleRequestOneOf2
                    {
                        Value = new ZonesSettingValue
                        {
                            ZonesAlwaysOnlineValue = settings.AlwaysOnline.Value
                        }
                    }
                };
                updateTasks.Add(client.Zones[zoneId].Settings["always_online"].PatchAsync(alwaysOnlineRequest, cancellationToken: cancellationToken));
            }
            
            // Update development mode only when the caller supplied it.
            if (settings.DevelopmentMode.HasValue)
            {
                var developmentModeRequest = new ZonesZoneSettingsSingleRequest
                {
                    ZonesZoneSettingsSingleRequestOneOf2 = new ZonesZoneSettingsSingleRequestOneOf2
                    {
                        Value = new ZonesSettingValue
                        {
                            ZonesDevelopmentModeValue = settings.DevelopmentMode.Value
                        }
                    }
                };
                updateTasks.Add(client.Zones[zoneId].Settings["development_mode"].PatchAsync(developmentModeRequest, cancellationToken: cancellationToken));
            }
            
            // Wait for all updates to complete
            if (updateTasks.Count > 0)
            {
                await Task.WhenAll(updateTasks).NoSync();
            }

            return await GetCacheSettings(zoneId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cache settings for zone {ZoneId}", zoneId);
            return null;
        }
    }

    public async ValueTask<bool> PurgeCache(string zoneId, List<string> urls, CancellationToken cancellationToken = default)
    {
        try
        {
            CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
            var request = new ZonePurgeRequest
            {
                CachePurgeSingleFile = new CachePurgeSingleFile
                {
                    Files = urls
                }
            };

            CachePurgeApiResponseSingleId? response = await client.Zones[zoneId].Purge_cache.PostAsync(request, cancellationToken: cancellationToken).NoSync();
            return response?.Result?.Id != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging cache for zone {ZoneId}", zoneId);
            return false;
        }
    }

    public async ValueTask<bool> PurgeCacheByHostname(string zoneId, List<string> hostnames, CancellationToken cancellationToken = default)
    {
        try
        {
            CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();

            var request = new ZonePurgeRequest
            {
                CachePurgeFlexPurgeByHostnames = new CachePurgeFlexPurgeByHostnames
                {
                    Hosts = hostnames
                }
            };

            CachePurgeApiResponseSingleId? response = await client.Zones[zoneId].Purge_cache.PostAsync(request, cancellationToken: cancellationToken).NoSync();
            return response?.Success == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging cache by hostname for zone {ZoneId}", zoneId);
            return false;
        }
    }

    public async ValueTask<bool> PurgeCacheByTags(string zoneId, List<string> tags, CancellationToken cancellationToken = default)
    {
        try
        {
            CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();

            var request = new ZonePurgeRequest
            {
                CachePurgeFlexPurgeByTags = new CachePurgeFlexPurgeByTags
                {
                    Tags = tags
                }
            };

            CachePurgeApiResponseSingleId? response = await client.Zones[zoneId].Purge_cache.PostAsync(request, cancellationToken: cancellationToken).NoSync();
            return response?.Success == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging cache by tags for zone {ZoneId}", zoneId);
            return false;
        }
    }

    public async ValueTask<bool> PurgeCacheByPrefixes(string zoneId, List<string> prefixes, CancellationToken cancellationToken = default)
    {
        try
        {
            CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();

            var request = new ZonePurgeRequest
            {
                CachePurgeFlexPurgeByPrefixes = new CachePurgeFlexPurgeByPrefixes
                {
                    Prefixes = prefixes
                }
            };

            CachePurgeApiResponseSingleId? response = await client.Zones[zoneId].Purge_cache.PostAsync(request, cancellationToken: cancellationToken).NoSync();
            return response?.Success == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging cache by prefixes for zone {ZoneId}", zoneId);
            return false;
        }
    }

    public ValueTask<bool> PurgeEverything(string zoneId, CancellationToken cancellationToken = default) =>
        PurgeEverythingCore(zoneId, null, cancellationToken);

    public ValueTask<bool> PurgeEverything(string zoneId, string apiKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        return PurgeEverythingCore(zoneId, apiKey, cancellationToken);
    }

    private async ValueTask<bool> PurgeEverythingCore(string zoneId, string? apiKey, CancellationToken cancellationToken)
    {
        try
        {
            CloudflareOpenApiClient client = apiKey == null
                ? await _client.Get(cancellationToken).NoSync()
                : await _client.Get(apiKey, cancellationToken).NoSync();
            var request = new ZonePurgeRequest
            {
                CachePurgeEverything = new CachePurgeEverything
                {
                    PurgeEverything = true
                }
            };

            CachePurgeApiResponseSingleId? response = await client.Zones[zoneId].Purge_cache.PostAsync(request, cancellationToken: cancellationToken).NoSync();
            return response?.Success == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging everything for zone {ZoneId}", zoneId);
            return false;
        }
    }

    public async ValueTask<SmartTieredCacheGetSmartTieredCacheSetting200Response?> GetSmartTieredCache(string zoneId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
            return await client.Zones[zoneId].Cache.Tiered_cache_smart_topology_enable.GetAsync(cancellationToken: cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting smart tiered cache for zone {ZoneId}", zoneId);
            return null;
        }
    }

    public async ValueTask<SmartTieredCachePatchSmartTieredCacheSetting200Response?> UpdateSmartTieredCache(string zoneId, bool enabled,
        CancellationToken cancellationToken = default)
    {
        try
        {
            CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
            var request = new CacheSettingsSmartTieredCachePatch
            {
                Value = enabled ? CacheSettingsSmartTieredCachePatchValue.On : CacheSettingsSmartTieredCachePatchValue.Off
            };

            return await client.Zones[zoneId].Cache.Tiered_cache_smart_topology_enable.PatchAsync(request, cancellationToken: cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating smart tiered cache for zone {ZoneId}", zoneId);
            return null;
        }
    }

    public ValueTask<SmartTieredCachePatchSmartTieredCacheSetting200Response?> EnableSmartTieredCache(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return UpdateSmartTieredCache(zoneId, true, cancellationToken);
    }

    public ValueTask<SmartTieredCachePatchSmartTieredCacheSetting200Response?> DisableSmartTieredCache(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return UpdateSmartTieredCache(zoneId, false, cancellationToken);
    }

    public async ValueTask<ZoneSettingsGetSingleSetting200Response?> GetCrawlerHints(string zoneId, CancellationToken cancellationToken = default)
    {
        try
        {
            CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
            return await client.Zones[zoneId].Settings["crawler_hints"].GetAsync(cancellationToken: cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting crawler hints for zone {ZoneId}", zoneId);
            return null;
        }
    }

    public async ValueTask<ZoneSettingsEditSingleSetting200Response?> UpdateCrawlerHints(string zoneId, bool enabled, CancellationToken cancellationToken = default)
    {
        try
        {
            CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();

            var request = new ZonesZoneSettingsSingleRequest
            {
                ZonesZoneSettingsSingleRequestOneOf1 = new ZonesZoneSettingsSingleRequestOneOf1
                {
                    Enabled = enabled
                }
            };

            return await client.Zones[zoneId].Settings["crawler_hints"].PatchAsync(request, cancellationToken: cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating crawler hints for zone {ZoneId}", zoneId);
            return null;
        }
    }

    public ValueTask<ZoneSettingsEditSingleSetting200Response?> EnableCrawlerHints(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return UpdateCrawlerHints(zoneId, true, cancellationToken);
    }

    public ValueTask<ZoneSettingsEditSingleSetting200Response?> DisableCrawlerHints(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return UpdateCrawlerHints(zoneId, false, cancellationToken);
    }

    public async ValueTask<ZoneSettingsGetSingleSetting200Response?> GetAlwaysOnline(string zoneId, CancellationToken cancellationToken = default)
    {
        try
        {
            CloudflareOpenApiClient client = await _client.Get(cancellationToken);
            return await client.Zones[zoneId].Settings["always_online"].GetAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting always online for zone {ZoneId}", zoneId);
            return null;
        }
    }

    public async ValueTask<ZoneSettingsEditSingleSetting200Response?> UpdateAlwaysOnline(string zoneId, bool enabled,
        CancellationToken cancellationToken = default)
    {
        try
        {
            CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();

            var request = new ZonesZoneSettingsSingleRequest
            {
                ZonesZoneSettingsSingleRequestOneOf1 = new ZonesZoneSettingsSingleRequestOneOf1
                {
                    Enabled = enabled
                }
            };

            return await client.Zones[zoneId].Settings["always_online"].PatchAsync(request, cancellationToken: cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating always online for zone {ZoneId}", zoneId);
            return null;
        }
    }

    public ValueTask<ZoneSettingsEditSingleSetting200Response?> EnableAlwaysOnline(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return UpdateAlwaysOnline(zoneId, true, cancellationToken);
    }

    public ValueTask<ZoneSettingsEditSingleSetting200Response?> DisableAlwaysOnline(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return UpdateAlwaysOnline(zoneId, false, cancellationToken);
    }
}
