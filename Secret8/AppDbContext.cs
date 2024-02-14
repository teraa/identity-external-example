using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Secret8;

public sealed class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("asp_net_role_claims");
        
        modelBuilder.Entity<IdentityRole>(b =>
        {
            b.ToTable("asp_net_roles");
            b.HasIndex(e => e.NormalizedName).HasDatabaseName("ix_asp_net_roles_normalized_name");
        });
        
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("asp_net_user_claims");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("asp_net_user_logins");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("asp_net_user_roles");
        
        modelBuilder.Entity<AppUser>(b =>
        {
            b.ToTable("asp_net_users");
            b.HasIndex(e => e.NormalizedUserName).HasDatabaseName("ix_asp_net_users_normalized_user_name");
            b.HasIndex(e => e.NormalizedEmail).HasDatabaseName("ix_asp_net_users_normalized_email");
        });
        
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("asp_net_user_tokens");
    }
}

public sealed class AppUser : IdentityUser { }
