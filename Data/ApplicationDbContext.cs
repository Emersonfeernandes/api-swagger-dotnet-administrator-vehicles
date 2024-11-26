using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    private readonly IConfiguration? _configAppSettings;
    public ApplicationDbContext(IConfiguration configAppSettings)
    {
        _configAppSettings = configAppSettings;
    }
    //public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Administrator> Administrators { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder){
        modelBuilder.Entity<Administrator>().HasData(
            new Administrator{
                Id = 1,
                Email = "admin@admin",
                Password = "123456",
                Name = "Admin",
            }
        );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
        {
            var stringConexao = _configAppSettings!.GetConnectionString("MySql")?.ToString();
            if(!string.IsNullOrEmpty(stringConexao))
            {
                optionsBuilder.UseMySql(
                    stringConexao,
                    ServerVersion.AutoDetect(stringConexao)
                );
            }
        }
    }
}