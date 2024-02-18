using Microsoft.AspNetCore.Authentication;

namespace Secret8;

public sealed class DefaultAuthenticationHandler : IAuthenticationHandler
{
    public const string SchemeName = nameof(DefaultAuthenticationHandler);
    
    private AuthenticationScheme Scheme { get; set; } = default!;
    private HttpContext Context { get; set; } = default!;
    private HttpRequest Request => Context.Request;
    private HttpResponse Response => Context.Response;

    public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
    {
        Scheme = scheme;
        Context = context;
        return Task.CompletedTask;
    }

    public Task<AuthenticateResult> AuthenticateAsync()
    {
        throw new NotImplementedException();
    }

    public Task ChallengeAsync(AuthenticationProperties? properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    public Task ForbidAsync(AuthenticationProperties? properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }
}
