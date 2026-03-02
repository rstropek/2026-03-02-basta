using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChatBotDb;

public partial class ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : DbContext(options)
{
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<BouquetPrice> BouquetPrices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .Property(o => o.Price)
            .HasColumnType("REAL");

        modelBuilder.Entity<BouquetPrice>()
            .Property(b => b.Price)
            .HasColumnType("REAL");

        modelBuilder.Entity<BouquetPrice>().HasData(
            new BouquetPrice { Id = 1, Size = "Small", NumberOfFlowers = 3, Description = "3 flowers arranged with a little bit of green grass", Price = 15m },
            new BouquetPrice { Id = 2, Size = "Medium", NumberOfFlowers = 5, Description = "5 flowers nicely arranged, including some larger green leaves as decoration", Price = 25m },
            new BouquetPrice { Id = 3, Size = "Large", NumberOfFlowers = 10, Description = "10 flowers, beautifully arranged with greenery and smaller filler flowers", Price = 35m }
        );
    }
}

public class ApplicationDataContextFactory : IDesignTimeDbContextFactory<ApplicationDataContext>
{
    public ApplicationDataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDataContext>();
        optionsBuilder.UseSqlite(string.Empty);
        return new ApplicationDataContext(optionsBuilder.Options);
    }
}