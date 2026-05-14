using System.Text.Json.Serialization;
using Soenneker.Cloudflare.OpenApiClient.Models;

namespace Soenneker.Cloudflare.Caching.Models;

/// <summary>
/// Represents the cache settings for a Cloudflare zone
/// </summary>
public class CloudflareCacheSettings
{
    /// <summary>
    /// The browser cache TTL in seconds
    /// </summary>
    [JsonPropertyName("browser_cache_ttl")]
    public int? BrowserCacheTtl { get; set; }

    /// <summary>
    /// Whether to respect the origin's cache control headers
    /// </summary>
    [JsonPropertyName("respect_strong_etags")]
    public bool RespectStrongEtags { get; set; }

    /// <summary>
    /// The cache level setting
    /// </summary>
    [JsonPropertyName("cache_level")]
    public ZonesCacheLevelValue? CacheLevel { get; set; }

    /// <summary>
    /// Whether to enable always online
    /// </summary>
    [JsonPropertyName("always_online")]
    public ZonesAlwaysOnlineValue AlwaysOnline { get; set; }

    /// <summary>
    /// The development mode status
    /// </summary>
    [JsonPropertyName("development_mode")]
    public ZonesDevelopmentModeValue DevelopmentMode { get; set; }

    /// <summary>
    /// Whether to enable query string sort
    /// </summary>
    [JsonPropertyName("query_string_sort")]
    public ZonesSortQueryStringForCacheValue? QueryStringSort { get; set; }

    /// <summary>
    /// Whether to purge cache on change
    /// </summary>
    [JsonPropertyName("purge_cache_on_change")]
    public bool PurgeCacheOnChange { get; set; }
} 