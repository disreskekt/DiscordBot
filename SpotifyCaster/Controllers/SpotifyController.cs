using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;
using SpotifyCaster.Configs;
using SpotifyCaster.Services;

namespace SpotifyCaster.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SpotifyController : ControllerBase
{
    private readonly SpotifyService _spotifyService;
    private readonly SpotifyConfig _config;

    public SpotifyController(SpotifyService spotifyService, IOptions<SpotifyConfig> config)
    {
        _spotifyService = spotifyService;
        _config = config.Value;
    }
    
    [HttpGet]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        OAuthClient authClient = new();
        AuthorizationCodeTokenResponse response = await authClient.RequestToken(
            new AuthorizationCodeTokenRequest(
                _config.ClientId,
                _config.ClientSecret,
                code,
                new Uri("https://localhost:7104/Spotify/Callback")));

        // Also important for later: response.RefreshToken
        SpotifyClient spotify = new(response.AccessToken);

        _spotifyService.SpotifyClient = spotify;

        return Ok();
    }
}