using Microsoft.EntityFrameworkCore;

namespace BaseApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Add your DbSet properties here, for example:
    // public DbSet<YourModel> YourModels { get; set; }
}
