using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Reshaping.ConsoleApp.Tables;

// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident

namespace Reshaping.ConsoleApp;

internal class ApplicationDbContext : DbContext
{
    public DbSet<Users> Users { get; set; } = null!;
    public DbSet<Addresses> Addresses { get; set; } = null!;
    public DbSet<Cars> Cars { get; set; } = null!;
    public DbSet<Roles> Roles { get; set; } = null!;
    public DbSet<UserRoles> UserRoles { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(new SqlConnectionStringBuilder { DataSource = @"(localdb)\MSSQLLocalDB", InitialCatalog = "Reshaping.ConsoleApp" }.ConnectionString);
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Users>(builder =>
        {
            builder.HasKey(user => user.Id);
            builder.HasMany<Cars>().WithOne().HasForeignKey(car => car.UserId);
            builder.HasData(
                new() { Id = 1, Name = "David", AddressId = 1 },
                new() { Id = 2, Name = "Zira", AddressId = 1 }
            );
        });

        modelBuilder.Entity<Addresses>(builder =>
        {
            builder.HasKey(address => address.Id);
            builder.HasData(new Addresses { Id = 1, Province = "湖北省", City = "武汉市" });
        });
        modelBuilder.Entity<Cars>(builder =>
        {
            builder.HasKey(car => car.Id);
            builder.HasData(
                new() { Id = 1, Name = "特斯拉", UserId = 1 },
                new() { Id = 2, Name = "大众", UserId = 2 }
            );
        });

        modelBuilder.Entity<Roles>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.HasData(
                new() { Id = 1, Name = "Manager" },
                new() { Id = 2, Name = "Worker" }
            );
        });

        modelBuilder.Entity<UserRoles>(builder =>
        {
            builder.HasKey(x => new { x.UserId, x.RoleId });
            builder.HasOne<Users>().WithMany().HasForeignKey(userRole => userRole.UserId);
            builder.HasOne<Roles>().WithMany().HasForeignKey(userRole => userRole.RoleId);
            builder.HasData(
                new() { UserId = 1, RoleId = 1 },
                new() { UserId = 1, RoleId = 2 },
                new() { UserId = 2, RoleId = 2 }
            );
        });
    }
}