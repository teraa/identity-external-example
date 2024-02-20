using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApi;

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
            "Auth/Continue");
        
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
        if (info is null)
        {
            _logger.LogWarning("Couldn't load external login information");
            return BadRequest();
        }
        
        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: true,
            bypassTwoFactor: true);
        
        // Existing user
        if (signInResult.Succeeded)
        {
            _logger.LogInformation("User {ProviderKey} logged in with {LoginProvider}", info.ProviderKey, info.LoginProvider);
            return Redirect("/");
        }

        if (signInResult.IsLockedOut)
        {
            return Forbid();
        }

        // New user
        var user = new AppUser();
        var discordUserName = User.Identity!.Name!;
        var discordId = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier);
        await _userManager.SetUserNameAsync(user, $"{discordUserName}:{discordId.Value}"); // Whatever, not important
        
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

        signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: true,
            bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            _logger.LogInformation("User {ProviderKey} created an account with {LoginProvider}", info.ProviderKey, info.LoginProvider);
            return Redirect("/");
        }

        return BadRequest(signInResult);
    }

    [HttpPost("[action]")]
    public async Task Logout()
    {
        // This just deletes the cookie client side, but it's still valid
        await HttpContext.SignOutAsync(); // Cookie scheme
    }

    [Authorize]
    [HttpGet("[action]")]
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
