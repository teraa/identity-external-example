using System.Diagnostics;
using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Secret8;

[ApiController]
[Route("[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet("[action]")]
    public async Task Login()
    {
        // Clear the existing external cookie to ensure a clean login process
        // await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(
            "Discord",
            "/auth/Continue");
        
        await HttpContext.ChallengeAsync(DiscordAuthenticationDefaults.AuthenticationScheme, properties);
    }

    // Both Identity.External and Discord schemes will work here,
    // but Discord will redirect to discord.com and External will redirect to /Account/Login...
    // when request is unauthenticated
    [Authorize(AuthenticationSchemes = DiscordAuthenticationDefaults.AuthenticationScheme)]
    [HttpGet("[action]")]
    public async Task<IActionResult> Continue()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        Debug.Assert(info is not null);
        
        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: true,
            bypassTwoFactor: true);

        if (result.Succeeded)
        {
            return Ok(new {Message = "Existing account"});
        }

        if (result.IsLockedOut)
        {
            return Forbid();
        }

        var user = new AppUser();

        var discordUserName = User.Identity!.Name!;
        var discordId = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier);
        await _userManager.SetUserNameAsync(user, $"{discordUserName}:{discordId.Value}");
        
        var identityResult = await _userManager.CreateAsync(user);
        if (!identityResult.Succeeded)
        {
            _logger.LogWarning("Failed to create user: {Errors}", identityResult.Errors.Select(x => x.Description));
            return BadRequest();
        }

        identityResult = await _userManager.AddClaimsAsync(user,
        [
            new Claim("discord.id", discordId.Value),
            new Claim("discord.name", discordUserName)
        ]);

        if (!identityResult.Succeeded)
        {
            _logger.LogWarning("Failed to add user claims: {Errors}", identityResult.Errors.Select(x => x.Description));
            return BadRequest();
        }
        
        identityResult = await _userManager.AddLoginAsync(user, info);
        if (!identityResult.Succeeded)
        {
            _logger.LogWarning("Failed to add login for user: {Errors}", identityResult.Errors.Select(x => x.Description));
            return BadRequest();
        }

        result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: true,
            bypassTwoFactor: true);

        if (result.Succeeded)
        {
            return Ok(new {Message = "New account"});
        }

        return BadRequest(result);
    }

    [HttpPost("[action]")]
    public async Task Logout()
    {
        // This just deletes the cookie client side, but it's still valid
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
}
