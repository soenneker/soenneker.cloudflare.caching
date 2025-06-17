using Microsoft.Extensions.Logging;
using Soenneker.Cloudflare.Caching.Abstract;
using Soenneker.Cloudflare.Caching.Models;
using Soenneker.Cloudflare.OpenApiClient;
using Soenneker.Cloudflare.OpenApiClient.Models;
using Soenneker.Cloudflare.OpenApiClient.Zones;
using Soenneker.Cloudflare.OpenApiClient.Zones.Item;
using Soenneker.Cloudflare.OpenApiClient.Zones.Item.Cache;
using Soenneker.Cloudflare.OpenApiClient.Zones.Item.Cache.Tiered_cache_smart_topology_enable;
using Soenneker.Cloudflare.OpenApiClient.Zones.Item.Purge_cache;
using Soenneker.Cloudflare.OpenApiClient.Zones.Item.Settings;
using Soenneker.Cloudflare.Utils.Client.Abstract;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Utils.AsyncSingleton;

namespace Soenneker.Cloudflare.Caching;

/// <inheritdoc cref="ICloudflareCachingUtil"/>
public sealed class CloudflareCachingUtil : ICloudflareCachingUtil
{
    private readonly ILogger<CloudflareCachingUtil> _logger;
    private readonly ICloudflareClientUtil _client;

    public CloudflareCachingUtil(ILogger<CloudflareCachingUtil> logger, ICloudflareClientUtil client)
    {
        _logger = logger;
        _client = client;
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.GetCacheSettings"/>
    public async ValueTask<CloudflareCacheSettings?> GetCacheSettings(string zoneId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await _client.Get(cancellationToken);
            var response = await client.Zones[zoneId].Settings.GetAsync();

            if (response?.Result == null)
                return null;

            var settings = new CloudflareCacheSettings();

            foreach (var setting in response.Result)
            {
                if (setting.ZonesSchemasBrowserCacheTtl != null)
                {
                    settings.BrowserCacheTtl = setting.ZonesSchemasBrowserCacheTtl.Value;
                }
                else if (setting.ZonesSchemasSortQueryStringForCache?.Value != null)
                {
                    settings.QueryStringSort = setting.ZonesSchemasSortQueryStringForCache.Value.Value;
                }
                else if (setting.ZonesSchemasCacheLevel != null)
                {
                    settings.CacheLevel = setting.ZonesSchemasCacheLevel.Value;
                }
                else if (setting.ZonesAlwaysOnline?.Value != null)
                {
                    settings.AlwaysOnline = setting.ZonesAlwaysOnline.Value.Value;
                }
                else if (setting.ZonesDevelopmentMode?.Value != null)
                {
                    settings.DevelopmentMode = setting.ZonesDevelopmentMode.Value.Value;
                }
            }

            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache settings for zone {ZoneId}", zoneId);
            return null;
        }
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.UpdateCacheSettings"/>
    public async ValueTask<CloudflareCacheSettings?> UpdateCacheSettings(string zoneId, CloudflareCacheSettings settings,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await _client.Get(cancellationToken);
            var request = new List<SettingsRequestBuilder.Zones_multiple_settings>
            {
                new()
                {
                    ZonesSchemasBrowserCacheTtl = new Zones_schemasBrowser_cache_ttl
                    {
                        Id = "browser_cache_ttl",
                        Value = settings.BrowserCacheTtl
                    }
                },
                new()
                {
                    ZonesSchemasSortQueryStringForCache = new Zones_schemasSort_query_string_for_cache
                    {
                        Id = "sort_query_string_for_cache",
                        Value = settings.QueryStringSort
                    }
                },
                new()
                {
                    ZonesSchemasCacheLevel = new Zones_schemasCache_level
                    {
                        Id = "cache_level",
                        Value = settings.CacheLevel
                    }
                },
                new()
                {
                    ZonesAlwaysOnline = new Zones_always_online
                    {
                        Id = "always_online",
                        Value = settings.AlwaysOnline
                    }
                },
                new()
                {
                    ZonesDevelopmentMode = new Zones_development_mode
                    {
                        Id = "development_mode",
                        Value = settings.DevelopmentMode
                    }
                }
            };

            var response = await client.Zones[zoneId].Settings.PatchAsync(request);

            if (response?.Result == null)
                return null;

            return await GetCacheSettings(zoneId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cache settings for zone {ZoneId}", zoneId);
            return null;
        }
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.PurgeCache"/>
    public async ValueTask<bool> PurgeCache(string zoneId, List<string> urls, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await _client.Get(cancellationToken);
            var request = new Zone_purge
            {
                CachePurgeSingleFile = new CachePurge_SingleFile
                {
                    Files = urls
                }
            };

            var response = await client.Zones[zoneId].Purge_cache.PostAsync(request, cancellationToken: cancellationToken);
            return response?.Result?.Id != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging cache for zone {ZoneId}", zoneId);
            return false;
        }
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.PurgeCacheByHostname"/>
    public async ValueTask<bool> PurgeCacheByHostname(string zoneId, List<string> hostnames, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await _client.Get(cancellationToken);
            var request = new Zone_purge
            {
                CachePurgeFlexPurgeByHostnames = new CachePurge_FlexPurgeByHostnames
                {
                    Hosts = hostnames
                }
            };

            var response = await client.Zones[zoneId].Purge_cache.PostAsync(request, cancellationToken: cancellationToken);
            return response?.Success == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging cache by hostname for zone {ZoneId}", zoneId);
            return false;
        }
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.PurgeCacheByTags"/>
    public async ValueTask<bool> PurgeCacheByTags(string zoneId, List<string> tags, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await _client.Get(cancellationToken);
            var request = new Zone_purge
            {
                CachePurgeFlexPurgeByTags = new CachePurge_FlexPurgeByTags
                {
                    Tags = tags
                }
            };

            var response = await client.Zones[zoneId].Purge_cache.PostAsync(request, cancellationToken: cancellationToken);
            return response?.Success == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging cache by tags for zone {ZoneId}", zoneId);
            return false;
        }
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.PurgeCacheByPrefixes"/>
    public async ValueTask<bool> PurgeCacheByPrefixes(string zoneId, List<string> prefixes, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await _client.Get(cancellationToken);
            var request = new Zone_purge
            {
                CachePurgeFlexPurgeByPrefixes = new CachePurge_FlexPurgeByPrefixes
                {
                    Prefixes = prefixes
                }
            };

            var response = await client.Zones[zoneId].Purge_cache.PostAsync(request, cancellationToken: cancellationToken);
            return response?.Success == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging cache by prefixes for zone {ZoneId}", zoneId);
            return false;
        }
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.PurgeEverything"/>
    public async ValueTask<bool> PurgeEverything(string zoneId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await _client.Get(cancellationToken);
            var request = new Zone_purge
            {
                CachePurgeEverything = new CachePurge_Everything
                {
                    PurgeEverything = true
                }
            };

            var response = await client.Zones[zoneId].Purge_cache.PostAsync(request, cancellationToken: cancellationToken);
            return response?.Success == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging everything for zone {ZoneId}", zoneId);
            return false;
        }
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.GetSmartTieredCache"/>
    public async ValueTask<Smart_tiered_cache_get_smart_tiered_cache_setting_200?> GetSmartTieredCache(string zoneId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await _client.Get(cancellationToken);
            return await client.Zones[zoneId].Cache.Tiered_cache_smart_topology_enable.GetAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting smart tiered cache for zone {ZoneId}", zoneId);
            return null;
        }
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.UpdateSmartTieredCache"/>
    public async ValueTask<Smart_tiered_cache_patch_smart_tiered_cache_setting_200?> UpdateSmartTieredCache(string zoneId, bool enabled,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await _client.Get(cancellationToken);
            var request = new CacheRules_smart_tiered_cache_patch
            {
                Value = enabled ? CacheRules_smart_tiered_cache_patch_value.On : CacheRules_smart_tiered_cache_patch_value.Off
            };

            return await client.Zones[zoneId].Cache.Tiered_cache_smart_topology_enable.PatchAsync(request, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating smart tiered cache for zone {ZoneId}", zoneId);
            return null;
        }
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.EnableSmartTieredCache"/>
    public async ValueTask<Smart_tiered_cache_patch_smart_tiered_cache_setting_200?> EnableSmartTieredCache(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return await UpdateSmartTieredCache(zoneId, true, cancellationToken);
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.DisableSmartTieredCache"/>
    public async ValueTask<Smart_tiered_cache_patch_smart_tiered_cache_setting_200?> DisableSmartTieredCache(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return await UpdateSmartTieredCache(zoneId, false, cancellationToken);
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.GetCrawlerHints"/>
    public async ValueTask<Zone_settings_get_single_setting_200?> GetCrawlerHints(string zoneId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await _client.Get(cancellationToken);
            return await client.Zones[zoneId].Settings["crawler_hints"].GetAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting crawler hints for zone {ZoneId}", zoneId);
            return null;
        }
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.UpdateCrawlerHints"/>
    public async ValueTask<Zone_settings_edit_single_setting_200?> UpdateCrawlerHints(string zoneId, bool enabled, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await _client.Get(cancellationToken);

            var request = new Zones_zone_settings_single_request()
            {
                ZonesZoneSettingsSingleRequestMember1 = new Zones_zone_settings_single_requestMember1()
                {
                    Enabled = enabled
                }
            };

            return await client.Zones[zoneId].Settings["crawler_hints"].PatchAsync(request, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating crawler hints for zone {ZoneId}", zoneId);
            return null;
        }
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.EnableCrawlerHints"/>
    public async ValueTask<Zone_settings_edit_single_setting_200?> EnableCrawlerHints(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return await UpdateCrawlerHints(zoneId, true, cancellationToken);
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.DisableCrawlerHints"/>
    public async ValueTask<Zone_settings_edit_single_setting_200?> DisableCrawlerHints(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return await UpdateCrawlerHints(zoneId, false, cancellationToken);
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.GetAlwaysOnline"/>
    public async ValueTask<Zone_settings_get_single_setting_200?> GetAlwaysOnline(string zoneId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await _client.Get(cancellationToken);
            return await client.Zones[zoneId].Settings["always_online"].GetAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting always online for zone {ZoneId}", zoneId);
            return null;
        }
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.UpdateAlwaysOnline"/>
    public async ValueTask<Zone_settings_edit_single_setting_200?> UpdateAlwaysOnline(string zoneId, bool enabled,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await _client.Get(cancellationToken);

            var request = new Zones_zone_settings_single_request()
            {
                ZonesZoneSettingsSingleRequestMember1 = new Zones_zone_settings_single_requestMember1()
                {
                    Enabled = enabled
                }
            };

            return await client.Zones[zoneId].Settings["always_online"].PatchAsync(request, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating always online for zone {ZoneId}", zoneId);
            return null;
        }
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.EnableAlwaysOnline"/>
    public async ValueTask<Zone_settings_edit_single_setting_200?> EnableAlwaysOnline(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return await UpdateAlwaysOnline(zoneId, true, cancellationToken);
    }

    /// <inheritdoc cref="ICloudflareCachingUtil.DisableAlwaysOnline"/>
    public async ValueTask<Zone_settings_edit_single_setting_200?> DisableAlwaysOnline(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return await UpdateAlwaysOnline(zoneId, false, cancellationToken);
    }
}