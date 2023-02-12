using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using api.peter.biz.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace api.peter.biz.Services;

public class TwitchService
{
  private const string CacheTokenKey = "twitch_oauth_token";
  private readonly ILogger<TwitchService> _logger;
  private readonly IOptionsMonitor<OAuthCredentials> _options;
  private readonly IMemoryCache _cache;
  private readonly HttpClient _client;

  public TwitchService(ILogger<TwitchService> logger, IOptionsMonitor<OAuthCredentials> options, IMemoryCache cache, IHttpClientFactory httpClientFactory)
  {
    _logger = logger;
    _options = options;
    _cache = cache;
    _client = httpClientFactory.CreateClient("TwitchOAuth");
  }

  public async Task<string> GetToken()
  {
    if (await IsTokenValid()) return _cache.Get<string>(CacheTokenKey)!;
    return await RefreshToken();
  }

  private async Task<string> RefreshToken()
  {
    OAuthCredentials creds = _options.CurrentValue;
    string TwitchOAuthClientId = creds.ClientID;
    string TwitchOAuthClientSecret = creds.ClientSecret;
    using HttpResponseMessage response = await _client.PostAsync($"/oauth2/token?client_id={TwitchOAuthClientId}&client_secret={TwitchOAuthClientSecret}&grant_type=client_credentials", null);
    response.EnsureSuccessStatusCode();
    string token = (await response.Content.ReadFromJsonAsync<JsonObject>())!["access_token"]!.GetValue<string>();

    return _cache.Set(CacheTokenKey, token);
  }

  private async Task<bool> IsTokenValid()
  {
    var request = new HttpRequestMessage(HttpMethod.Get, "https://id.twitch.tv/oauth2/validate");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cache.Get<string>(CacheTokenKey));
    using HttpResponseMessage response = await _client.SendAsync(request);

    return response.StatusCode switch
    {
      HttpStatusCode.OK => true,
      HttpStatusCode.Unauthorized => false,
      _ => throw new Exception($"Unexpected status code {response.StatusCode} when validating twitch oauth token"),
    };
  }
}
