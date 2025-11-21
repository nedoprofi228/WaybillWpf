using Microsoft.EntityFrameworkCore;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Enums;

namespace WaybillWpf.DataBase;

public class ApplicationContext: DbContext
{
    public DbSet<Car> Cars { get; set; }
    public DbSet<DriveLicense> DriveLicenses { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Waybill> Waybills { get; set; }
    public DbSet<WaybillDetails> WaybillDetails { get; set; }

    public ApplicationContext(DbContextOptions<ApplicationContext> options): base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=9643;Database=mydatabase7;");
        
        
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

        modelBuilder.Entity<User>().HasData(new User()
            {
                Id = 1,
                Name = "admin",
                Password = "admin",
                Role = UserRole.Admin
            }
        );
        
        base.OnModelCreating(modelBuilder);
   }
}