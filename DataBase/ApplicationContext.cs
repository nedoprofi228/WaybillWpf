using Microsoft.EntityFrameworkCore;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Enums;

namespace WaybillWpf.DataBase;

public class ApplicationContext : DbContext
{
    public DbSet<Car> Cars { get; set; }
    public DbSet<DriveLicense> DriveLicenses { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Waybill> Waybills { get; set; }
    public DbSet<WaybillDetails> WaybillDetails { get; set; }
    public DbSet<FuelType> FuelTypes { get; set; }

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=9643;Database=db2.db;");


        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Driver>()
            .HasMany(d => d.Waybills)
            .WithOne(w => w.Driver);

        modelBuilder.Entity<Logist>()
            .HasMany(d => d.Waybills)
            .WithOne(w => w.Logist);

        modelBuilder.Entity<Waybill>()
            .HasMany(w => w.WaybillDetails)
            .WithOne(d => d.Waybill);

        modelBuilder.Entity<Waybill>()
            .HasMany(w => w.WaybillTasks)
            .WithOne(d => d.Waybill);

        modelBuilder.Entity<Car>()
            .HasOne(c => c.FuelType)
            .WithMany()
            .HasForeignKey(c => c.FuelTypeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasDiscriminator<string>("UserType")
            .HasValue<Admin>("Admin")
            .HasValue<Driver>("Driver")
            .HasValue<Logist>("Logist");

        modelBuilder.Entity<Driver>()
            .HasOne(d => d.DriveLicense)
            .WithOne()
            .HasForeignKey<Driver>(d => d.DriverLicenseId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<Admin>().HasData(new Admin("Admin", "admin", "admin")
        {
            Id = 1,
        }
        );

        modelBuilder.Entity<FuelType>().HasData(
            new FuelType { Id = 1, Name = "АИ-92", Price = 50.50m },
            new FuelType { Id = 2, Name = "АИ-95", Price = 55.70m },
            new FuelType { Id = 3, Name = "ДТ", Price = 60.10m }
        );

        base.OnModelCreating(modelBuilder);
    }
}