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
    ValueTask<Zone_settings_get_single_setting_Response_200_application_json> GetSmartTieredCache(string zoneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the Smart Tiered Cache settings for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="enabled">Whether to enable Smart Tiered Cache</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated Smart Tiered Cache settings</returns>
    ValueTask<Zone_settings_edit_single_setting_Response_200_application_json> UpdateSmartTieredCache(string zoneId, bool enabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables Smart Tiered Cache for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated Smart Tiered Cache settings</returns>
    ValueTask<Zone_settings_edit_single_setting_Response_200_application_json> EnableSmartTieredCache(string zoneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables Smart Tiered Cache for a zone
    /// </summary>
    /// <param name="zoneId">The zone ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated Smart Tiered Cache settings</returns>
    ValueTask<Zone_settings_edit_single_setting_Response_200_application_json> DisableSmartTieredCache(string zoneId, CancellationToken cancellationToken = default);
}
