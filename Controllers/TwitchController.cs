using api.peter.biz.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.peter.biz.Controllers;

[ApiController]
public class TwitchController : ControllerBase
{
  private readonly ILogger<TwitchController> _logger;
  private readonly TwitchService _twitchService;

  public TwitchController(ILogger<TwitchController> logger, TwitchService twitchService)
  {
    _logger = logger;
    _twitchService = twitchService;
  }

  [HttpGet("twitch/token")]
  public async Task<string> Get()
  {
    return await _twitchService.GetToken();
  }
}
