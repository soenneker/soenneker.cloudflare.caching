using Microsoft.Extensions.Logging;
using Soenneker.Cloudflare.Caching.Abstract;
using Soenneker.Cloudflare.Caching.Models;
using Soenneker.Cloudflare.OpenApiClient;
using Soenneker.Cloudflare.OpenApiClient.Models;
using Soenneker.Cloudflare.Utils.Client.Abstract;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Cloudflare.Caching;

/// <inheritdoc cref="ICloudflareCachingUtil"/>
public sealed class CloudflareCachingUtil : ICloudflareCachingUtil
{
    private readonly ICloudflareClientUtil _client;
    private readonly ILogger<CloudflareCachingUtil> _logger;

    public CloudflareCachingUtil(ICloudflareClientUtil client, ILogger<CloudflareCachingUtil> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async ValueTask<CloudflareCacheSettings> GetCacheSettings(string zoneId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting cache settings for zone {ZoneId}", zoneId);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var response = await client.Zones[zoneId].Settings["cache_level"].GetAsync(cancellationToken: cancellationToken).NoSync();
            if (response?.Result == null)
            {
                _logger.LogWarning("Failed to retrieve cache settings for zone {ZoneId}: response or result was null.", zoneId);
                return new CloudflareCacheSettings();
            }

            var result = response.Result;
            return new CloudflareCacheSettings
            {
                CacheLevel = result.AdditionalData?.TryGetValue("value", out var cacheLevel) == true ? cacheLevel?.ToString() : null,
                BrowserCacheTtl = result.AdditionalData?.TryGetValue("browser_cache_ttl", out var ttl) == true && ttl != null ? Convert.ToInt32(ttl) : 0,
                RespectStrongEtags = result.AdditionalData?.TryGetValue("respect_strong_etags", out var etags) == true && etags != null && Convert.ToBoolean(etags),
                AlwaysOnline = result.AdditionalData?.TryGetValue("always_online", out var alwaysOnline) == true ? alwaysOnline?.ToString() : null,
                DevelopmentMode = result.AdditionalData?.TryGetValue("development_mode", out var devMode) == true && devMode != null ? Convert.ToInt32(devMode) : 0,
                QueryStringSort = result.AdditionalData?.TryGetValue("query_string_sort", out var sort) == true && sort != null && Convert.ToBoolean(sort),
                PurgeCacheOnChange = result.AdditionalData?.TryGetValue("purge_cache_on_change", out var purge) == true && purge != null && Convert.ToBoolean(purge)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache settings for zone {ZoneId}", zoneId);
            throw;
        }
    }

    public async ValueTask<CloudflareCacheSettings> UpdateCacheSettings(string zoneId, CloudflareCacheSettings settings, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating cache settings for zone {ZoneId}", zoneId);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var requestBody = new Zones_zone_settings_single_request
            {
                AdditionalData =
                {
                    ["value"] = settings.CacheLevel,
                    ["browser_cache_ttl"] = settings.BrowserCacheTtl,
                    ["respect_strong_etags"] = settings.RespectStrongEtags,
                    ["always_online"] = settings.AlwaysOnline,
                    ["development_mode"] = settings.DevelopmentMode,
                    ["query_string_sort"] = settings.QueryStringSort,
                    ["purge_cache_on_change"] = settings.PurgeCacheOnChange
                }
            };

            var response = await client.Zones[zoneId].Settings["cache_level"].PatchAsync(requestBody, cancellationToken: cancellationToken).NoSync();
            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cache settings for zone {ZoneId}", zoneId);
            throw;
        }
    }

    public async ValueTask PurgeCache(string zoneId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Purging cache for zone {ZoneId}", zoneId);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var requestBody = new Zone_purge_RequestBody_application_json
            {
                AdditionalData =
                {
                    ["purge_everything"] = true
                }
            };
            await client.Zones[zoneId].Purge_cache.PostAsync(requestBody, cancellationToken: cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging cache for zone {ZoneId}", zoneId);
            throw;
        }
    }

    public async ValueTask PurgeUrls(string zoneId, string[] urls, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Purging URLs for zone {ZoneId}: {Urls}", zoneId, string.Join(", ", urls));
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var requestBody = new Zone_purge_RequestBody_application_json
            {
                AdditionalData =
                {
                    ["files"] = urls
                }
            };
            await client.Zones[zoneId].Purge_cache.PostAsync(requestBody, cancellationToken: cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging URLs for zone {ZoneId}", zoneId);
            throw;
        }
    }

    public async ValueTask<Smart_tiered_cache_get_smart_tiered_cache_setting_Response_200_application_json?> GetSmartTieredCache(string zoneId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting Smart Tiered Cache settings for zone {ZoneId}", zoneId);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            return await client.Zones[zoneId].Cache.Tiered_cache_smart_topology_enable.GetAsync(cancellationToken: cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Smart Tiered Cache settings for zone {ZoneId}", zoneId);
            throw;
        }
    }

    public async ValueTask<Smart_tiered_cache_patch_smart_tiered_cache_setting_Response_200_application_json?> UpdateSmartTieredCache(string zoneId, bool enabled, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating Smart Tiered Cache settings for zone {ZoneId} to {Enabled}", zoneId, enabled);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var requestBody = new CacheRules_smart_tiered_cache_patch
            {
                Value = enabled ? CacheRules_smart_tiered_cache_patch_value.On : CacheRules_smart_tiered_cache_patch_value.Off
            };
            return await client.Zones[zoneId].Cache.Tiered_cache_smart_topology_enable.PatchAsync(requestBody, cancellationToken: cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Smart Tiered Cache settings for zone {ZoneId}", zoneId);
            throw;
        }
    }

    public async ValueTask<Smart_tiered_cache_patch_smart_tiered_cache_setting_Response_200_application_json?> EnableSmartTieredCache(string zoneId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Enabling Smart Tiered Cache for zone {ZoneId}", zoneId);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var requestBody = new CacheRules_smart_tiered_cache_patch
            {
                Value = CacheRules_smart_tiered_cache_patch_value.On
            };
            return await client.Zones[zoneId].Cache.Tiered_cache_smart_topology_enable.PatchAsync(requestBody, cancellationToken: cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling Smart Tiered Cache for zone {ZoneId}", zoneId);
            throw;
        }
    }

    public async ValueTask<Smart_tiered_cache_patch_smart_tiered_cache_setting_Response_200_application_json?> DisableSmartTieredCache(string zoneId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Disabling Smart Tiered Cache for zone {ZoneId}", zoneId);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var requestBody = new CacheRules_smart_tiered_cache_patch
            {
                Value = CacheRules_smart_tiered_cache_patch_value.Off
            };
            return await client.Zones[zoneId].Cache.Tiered_cache_smart_topology_enable.PatchAsync(requestBody, cancellationToken: cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling Smart Tiered Cache for zone {ZoneId}", zoneId);
            throw;
        }
    }
}
