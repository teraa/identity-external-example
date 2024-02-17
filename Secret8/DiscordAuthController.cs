using System.Diagnostics;
using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Secret8;

[ApiController]
[Route("auth")]
public sealed class DiscordAuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    
    public DiscordAuthController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("[action]")]
    public async Task Login()
    {
        var authenticationProperties = new AuthenticationProperties
        {
            RedirectUri = "/",
        };

        await HttpContext.ChallengeAsync(DiscordAuthenticationDefaults.AuthenticationScheme, authenticationProperties);
    }

    [HttpPost("[action]")]
    public async Task Logout()
    {
        // TODO: This just deletes the cookie client side, it's still valid
        await HttpContext.SignOutAsync(); // Cookie scheme
    }

    [Authorize]
    [HttpGet("info")]
    public IActionResult Info()
    {
        return Ok(User.Identities.Select(i => new
        {
            i.AuthenticationType,
            Claims = i.Claims.Select(c => new
            {
                c.Type,
                c.Value
            })
        }));
    }

    [Authorize]
    [HttpGet("add")]
    public async Task<IActionResult> AddClaim(string type, string value)
    {
        var claim = new Claim(type, value);
        var user = await _userManager.GetUserAsync(User);
        Debug.Assert(user is not null);
        await _userManager.AddClaimAsync(user, claim);
        return Ok($"Added: {type} = {value}");
    }
}
