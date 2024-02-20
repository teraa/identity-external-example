using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

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
        modelBuilder.UseSnakeCaseIdentityNames<AppUser>();
    }
}

public static class ModelBuilderExtensions
{
    public static ModelBuilder UseSnakeCaseIdentityNames<TUser>(this ModelBuilder modelBuilder)
        where TUser : IdentityUser<string>
        => UseSnakeCaseIdentityNames
        <
            string,
            TUser,
            IdentityUserClaim<string>,
            IdentityUserLogin<string>,
            IdentityUserRole<string>,
            IdentityUserToken<string>,
            IdentityRole,
            IdentityRoleClaim<string>
        >(modelBuilder);

    public static ModelBuilder UseSnakeCaseIdentityNames<
        TKey,
        TUser,
        TUserClaim,
        TUserLogin,
        TUserRole,
        TUserToken,
        TRole,
        TRoleClaim>(this ModelBuilder modelBuilder)
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
        where TUserLogin : IdentityUserLogin<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TUserToken : IdentityUserToken<TKey>
        where TRole : IdentityRole<TKey>
        where TRoleClaim : IdentityRoleClaim<TKey>
    {
        // https://github.com/efcore/EFCore.NamingConventions/issues/2#issuecomment-612651161
        
        modelBuilder.Entity<TRoleClaim>().ToTable("asp_net_role_claims");

        modelBuilder.Entity<TRole>(b =>
        {
            b.ToTable("asp_net_roles");
            b.HasIndex(e => e.NormalizedName).HasDatabaseName("ix_asp_net_roles_normalized_name");
        });

        modelBuilder.Entity<TUserClaim>().ToTable("asp_net_user_claims");
        modelBuilder.Entity<TUserLogin>().ToTable("asp_net_user_logins");
        modelBuilder.Entity<TUserRole>().ToTable("asp_net_user_roles");

        modelBuilder.Entity<TUser>(b =>
        {
            b.ToTable("asp_net_users");
            b.HasIndex(e => e.NormalizedUserName).HasDatabaseName("ix_asp_net_users_normalized_user_name");
            b.HasIndex(e => e.NormalizedEmail).HasDatabaseName("ix_asp_net_users_normalized_email");
        });

        modelBuilder.Entity<TUserToken>().ToTable("asp_net_user_tokens");

        return modelBuilder;
    }
}

public sealed class AppUser : IdentityUser { }
