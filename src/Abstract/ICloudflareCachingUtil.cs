using System.Threading;
using System.Threading.Tasks;
using Soenneker.Cloudflare.Caching.Models;
using Soenneker.Cloudflare.OpenApiClient.Models;

namespace Soenneker.Cloudflare.Caching.Abstract;

/// <summary>
/// Utility for managing Cloudflare cache settings
/// </summary>
public interface ICloudflareCachingUtil
{
    /// <summary>
    /// Gets the cache settings for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The cache settings</returns>
    ValueTask<CloudflareCacheSettings> GetCacheSettings(string zoneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the cache settings for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="settings">The cache settings to update</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated cache settings</returns>
    ValueTask<CloudflareCacheSettings> UpdateCacheSettings(string zoneId, CloudflareCacheSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Purges the cache for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    ValueTask PurgeCache(string zoneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Purges specific URLs from the cache
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="urls">The URLs to purge</param>
    /// <param name="cancellationToken">The cancellation token</param>
    ValueTask PurgeUrls(string zoneId, string[] urls, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Smart Tiered Cache settings for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The Smart Tiered Cache settings</returns>
    ValueTask<Smart_tiered_cache_get_smart_tiered_cache_setting_Response_200_application_json?> GetSmartTieredCache(string zoneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the Smart Tiered Cache settings for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="enabled">Whether to enable Smart Tiered Cache</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated Smart Tiered Cache settings</returns>
    ValueTask<Smart_tiered_cache_patch_smart_tiered_cache_setting_Response_200_application_json?> UpdateSmartTieredCache(string zoneId, bool enabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables Smart Tiered Cache for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated Smart Tiered Cache settings</returns>
    ValueTask<Smart_tiered_cache_patch_smart_tiered_cache_setting_Response_200_application_json?> EnableSmartTieredCache(string zoneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables Smart Tiered Cache for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated Smart Tiered Cache settings</returns>
    ValueTask<Smart_tiered_cache_patch_smart_tiered_cache_setting_Response_200_application_json?> DisableSmartTieredCache(string zoneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Crawler Hints settings for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The Crawler Hints settings</returns>
    ValueTask<Zone_settings_get_single_setting_Response_200_application_json?> GetCrawlerHints(string zoneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the Crawler Hints settings for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="enabled">Whether to enable Crawler Hints</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated Crawler Hints settings</returns>
    ValueTask<Zone_settings_edit_single_setting_Response_200_application_json?> UpdateCrawlerHints(string zoneId, bool enabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables Crawler Hints for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated Crawler Hints settings</returns>
    ValueTask<Zone_settings_edit_single_setting_Response_200_application_json?> EnableCrawlerHints(string zoneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables Crawler Hints for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated Crawler Hints settings</returns>
    ValueTask<Zone_settings_edit_single_setting_Response_200_application_json?> DisableCrawlerHints(string zoneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Always Online settings for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The Always Online settings</returns>
    ValueTask<Zone_settings_get_single_setting_Response_200_application_json?> GetAlwaysOnline(string zoneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the Always Online settings for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="enabled">Whether to enable Always Online</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated Always Online settings</returns>
    ValueTask<Zone_settings_edit_single_setting_Response_200_application_json?> UpdateAlwaysOnline(string zoneId, bool enabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables Always Online for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated Always Online settings</returns>
    ValueTask<Zone_settings_edit_single_setting_Response_200_application_json?> EnableAlwaysOnline(string zoneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables Always Online for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated Always Online settings</returns>
    ValueTask<Zone_settings_edit_single_setting_Response_200_application_json?> DisableAlwaysOnline(string zoneId, CancellationToken cancellationToken = default);
}
