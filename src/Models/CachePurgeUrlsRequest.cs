using System.Text.Json.Serialization;

namespace Soenneker.Cloudflare.Caching.Models;

/// <summary>
/// Represents a request to purge specific URLs from the cache
/// </summary>
public class CachePurgeUrlsRequest
{
    /// <summary>
    /// The URLs to purge from the cache
    /// </summary>
    [JsonPropertyName("urls")]
    public string[] Urls { get; set; }
} 