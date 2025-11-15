using Microsoft.EntityFrameworkCore;
using WaybillWpf.Core.Entities;

namespace WaybillWpf.DataBase;

public class ApplicationContext: DbContext
{
    public DbSet<Car> Cars { get; set; }
    public DbSet<DriveLicense> DriveLicenses { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Waybill> Waybills { get; set; }
    public DbSet<WaybillDetails> WaybillDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=9643;Database=mydatabase3;");
        
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.Waybills)
            .WithOne(w => w.User);
        
        modelBuilder.Entity<Waybill>()
            .HasMany(w => w.WaybillDetails)
            .WithOne(d => d.Waybill);
        
        base.OnModelCreating(modelBuilder);
    }
}