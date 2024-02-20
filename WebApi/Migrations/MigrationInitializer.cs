using Extensions.Hosting.AsyncInitialization;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Migrations;

public sealed class MigrationInitializer : IAsyncInitializer
{
    private readonly AppDbContext _ctx;
    
    public MigrationInitializer(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _ctx.Database.MigrateAsync(cancellationToken);
    }
}