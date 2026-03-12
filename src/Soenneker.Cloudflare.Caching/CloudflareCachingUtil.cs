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

///<inheritdoc cref="ICloudflareCachingUtil"/>
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
                Zone_settings_get_single_setting_200? browserCacheResponse = await client.Zones[zoneId].Settings["browser_cache_ttl"].GetAsync(cancellationToken: cancellationToken).NoSync();
                if (browserCacheResponse?.Result?.ZonesSchemasBrowserCacheTtl != null)
                {
                    settings.BrowserCacheTtl = browserCacheResponse.Result.ZonesSchemasBrowserCacheTtl.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not get browser cache TTL setting for zone {ZoneId}", zoneId);
            }
            
            try
            {
                Zone_settings_get_single_setting_200? queryStringResponse = await client.Zones[zoneId].Settings["sort_query_string_for_cache"].GetAsync(cancellationToken: cancellationToken).NoSync();
                if (queryStringResponse?.Result?.ZonesSchemasSortQueryStringForCache?.Value != null)
                {
                    settings.QueryStringSort = queryStringResponse.Result.ZonesSchemasSortQueryStringForCache.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not get query string sort setting for zone {ZoneId}", zoneId);
            }
            
            try
            {
                Zone_settings_get_single_setting_200? cacheLevelResponse = await client.Zones[zoneId].Settings["cache_level"].GetAsync(cancellationToken: cancellationToken).NoSync();
                if (cacheLevelResponse?.Result?.ZonesSchemasCacheLevel != null)
                {
                    settings.CacheLevel = cacheLevelResponse.Result.ZonesSchemasCacheLevel.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not get cache level setting for zone {ZoneId}", zoneId);
            }
            
            try
            {
                Zone_settings_get_single_setting_200? alwaysOnlineResponse = await client.Zones[zoneId].Settings["always_online"].GetAsync(cancellationToken: cancellationToken).NoSync();
                if (alwaysOnlineResponse?.Result?.ZonesAlwaysOnline?.Value != null)
                {
                    settings.AlwaysOnline = alwaysOnlineResponse.Result.ZonesAlwaysOnline.Value.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not get always online setting for zone {ZoneId}", zoneId);
            }
            
            try
            {
                Zone_settings_get_single_setting_200? developmentModeResponse = await client.Zones[zoneId].Settings["development_mode"].GetAsync(cancellationToken: cancellationToken).NoSync();
                if (developmentModeResponse?.Result?.ZonesDevelopmentMode?.Value != null)
                {
                    settings.DevelopmentMode = developmentModeResponse.Result.ZonesDevelopmentMode.Value.Value;
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
            
            // Update browser cache TTL (uses Branch8 for integer values)
            if (settings.BrowserCacheTtl.HasValue)
            {
                var browserCacheRequest = new Zones_zone_settings_single_request
                {
                    ZonesZoneSettingsSingleRequestMember2 = new Zones_zone_settings_single_requestMember2
                    {
                        Value = new Zones_setting_value
                        {
                            ZonesSettingValueBranch8 = new Zones_setting_value_Branch8
                            {
                                Value = settings.BrowserCacheTtl.Value
                            }
                        }
                    }
                };
                updateTasks.Add(client.Zones[zoneId].Settings["browser_cache_ttl"].PatchAsync(browserCacheRequest, cancellationToken: cancellationToken));
            }
            
            // Update query string sort (uses Branch35 for double values)
            if (settings.QueryStringSort.HasValue)
            {
                var queryStringRequest = new Zones_zone_settings_single_request
                {
                    ZonesZoneSettingsSingleRequestMember2 = new Zones_zone_settings_single_requestMember2
                    {
                        Value = new Zones_setting_value
                        {
                            ZonesSettingValueBranch35 = new Zones_setting_value_Branch35
                            {
                                Value = (int)settings.QueryStringSort.Value
                            }
                        }
                    }
                };
                updateTasks.Add(client.Zones[zoneId].Settings["sort_query_string_for_cache"].PatchAsync(queryStringRequest, cancellationToken: cancellationToken));
            }
            
            // Update cache level (uses wrapper for enum values)
            if (settings.CacheLevel.HasValue)
            {
                var cacheLevelRequest = new Zones_zone_settings_single_request
                {
                    ZonesZoneSettingsSingleRequestMember2 = new Zones_zone_settings_single_requestMember2
                    {
                        Value = new Zones_setting_value
                        {
                            ZonesCacheLevelValueWrapper = new Zones_cache_level_value_Wrapper
                            {
                                Value = (Zones_cache_level_value)settings.CacheLevel.Value
                            }
                        }
                    }
                };
                updateTasks.Add(client.Zones[zoneId].Settings["cache_level"].PatchAsync(cacheLevelRequest, cancellationToken: cancellationToken));
            }
            
            // Update always online (uses wrapper for enum values)
            var alwaysOnlineRequest = new Zones_zone_settings_single_request
            {
                ZonesZoneSettingsSingleRequestMember2 = new Zones_zone_settings_single_requestMember2
                {
                    Value = new Zones_setting_value
                    {
                        ZonesAlwaysOnlineValueWrapper = new Zones_always_online_value_Wrapper
                        {
                            Value = settings.AlwaysOnline
                        }
                    }
                }
            };
            updateTasks.Add(client.Zones[zoneId].Settings["always_online"].PatchAsync(alwaysOnlineRequest, cancellationToken: cancellationToken));
            
            // Update development mode (uses wrapper for enum values)
            var developmentModeRequest = new Zones_zone_settings_single_request
            {
                ZonesZoneSettingsSingleRequestMember2 = new Zones_zone_settings_single_requestMember2
                {
                    Value = new Zones_setting_value
                    {
                        ZonesDevelopmentModeValueWrapper = new Zones_development_mode_value_Wrapper
                        {
                            Value = settings.DevelopmentMode
                        }
                    }
                }
            };
            updateTasks.Add(client.Zones[zoneId].Settings["development_mode"].PatchAsync(developmentModeRequest, cancellationToken: cancellationToken));
            
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
            var request = new Zone_purge
            {
                CachePurgeSingleFile = new Cache_purge_SingleFile
                {
                    Files = urls
                }
            };

            Cache_purge_api_response_single_id? response = await client.Zones[zoneId].Purge_cache.PostAsync(request, cancellationToken: cancellationToken).NoSync();
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

            var request = new Zone_purge
            {
                CachePurgeFlexPurgeByHostnames = new Cache_purge_FlexPurgeByHostnames
                {
                    Hosts = hostnames
                }
            };

            Cache_purge_api_response_single_id? response = await client.Zones[zoneId].Purge_cache.PostAsync(request, cancellationToken: cancellationToken).NoSync();
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

            var request = new Zone_purge
            {
                CachePurgeFlexPurgeByTags = new Cache_purge_FlexPurgeByTags
                {
                    Tags = tags
                }
            };

            Cache_purge_api_response_single_id? response = await client.Zones[zoneId].Purge_cache.PostAsync(request, cancellationToken: cancellationToken).NoSync();
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

            var request = new Zone_purge
            {
                CachePurgeFlexPurgeByPrefixes = new Cache_purge_FlexPurgeByPrefixes
                {
                    Prefixes = prefixes
                }
            };

            Cache_purge_api_response_single_id? response = await client.Zones[zoneId].Purge_cache.PostAsync(request, cancellationToken: cancellationToken).NoSync();
            return response?.Success == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging cache by prefixes for zone {ZoneId}", zoneId);
            return false;
        }
    }

    public async ValueTask<bool> PurgeEverything(string zoneId, CancellationToken cancellationToken = default)
    {
        try
        {
            CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
            var request = new Zone_purge
            {
                CachePurgeEverything = new Cache_purge_Everything
                {
                    PurgeEverything = true
                }
            };

            Cache_purge_api_response_single_id? response = await client.Zones[zoneId].Purge_cache.PostAsync(request, cancellationToken: cancellationToken).NoSync();
            return response?.Success == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging everything for zone {ZoneId}", zoneId);
            return false;
        }
    }

    public async ValueTask<Smart_tiered_cache_get_smart_tiered_cache_setting_200?> GetSmartTieredCache(string zoneId,
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

    public async ValueTask<Smart_tiered_cache_patch_smart_tiered_cache_setting_200?> UpdateSmartTieredCache(string zoneId, bool enabled,
        CancellationToken cancellationToken = default)
    {
        try
        {
            CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
            var request = new Cache_rules_smart_tiered_cache_patch
            {
                Value = enabled ? Cache_rules_smart_tiered_cache_patch_value.On : Cache_rules_smart_tiered_cache_patch_value.Off
            };

            return await client.Zones[zoneId].Cache.Tiered_cache_smart_topology_enable.PatchAsync(request, cancellationToken: cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating smart tiered cache for zone {ZoneId}", zoneId);
            return null;
        }
    }

    public ValueTask<Smart_tiered_cache_patch_smart_tiered_cache_setting_200?> EnableSmartTieredCache(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return UpdateSmartTieredCache(zoneId, true, cancellationToken);
    }

    public ValueTask<Smart_tiered_cache_patch_smart_tiered_cache_setting_200?> DisableSmartTieredCache(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return UpdateSmartTieredCache(zoneId, false, cancellationToken);
    }

    public async ValueTask<Zone_settings_get_single_setting_200?> GetCrawlerHints(string zoneId, CancellationToken cancellationToken = default)
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

    public async ValueTask<Zone_settings_edit_single_setting_200?> UpdateCrawlerHints(string zoneId, bool enabled, CancellationToken cancellationToken = default)
    {
        try
        {
            CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();

            var request = new Zones_zone_settings_single_request
            {
                ZonesZoneSettingsSingleRequestMember1 = new Zones_zone_settings_single_requestMember1
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

    public ValueTask<Zone_settings_edit_single_setting_200?> EnableCrawlerHints(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return UpdateCrawlerHints(zoneId, true, cancellationToken);
    }

    public ValueTask<Zone_settings_edit_single_setting_200?> DisableCrawlerHints(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return UpdateCrawlerHints(zoneId, false, cancellationToken);
    }

    public async ValueTask<Zone_settings_get_single_setting_200?> GetAlwaysOnline(string zoneId, CancellationToken cancellationToken = default)
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

    public async ValueTask<Zone_settings_edit_single_setting_200?> UpdateAlwaysOnline(string zoneId, bool enabled,
        CancellationToken cancellationToken = default)
    {
        try
        {
            CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();

            var request = new Zones_zone_settings_single_request
            {
                ZonesZoneSettingsSingleRequestMember1 = new Zones_zone_settings_single_requestMember1
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

    public ValueTask<Zone_settings_edit_single_setting_200?> EnableAlwaysOnline(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return UpdateAlwaysOnline(zoneId, true, cancellationToken);
    }

    public ValueTask<Zone_settings_edit_single_setting_200?> DisableAlwaysOnline(string zoneId,
        CancellationToken cancellationToken = default)
    {
        return UpdateAlwaysOnline(zoneId, false, cancellationToken);
    }
}