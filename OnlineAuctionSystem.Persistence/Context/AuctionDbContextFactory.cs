using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OnlineAuctionSystem.Persistence.Context
{
    public class AuctionDbContextFactory : IDesignTimeDbContextFactory<AuctionDbContext>
    {
        public AuctionDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AuctionDbContext>();
            // Use LocalDB by default for design-time tools so migrations and tooling work on developer machines.
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=OnlineAuctionSystemDB;Trusted_Connection=True;MultipleActiveResultSets=true");

            return new AuctionDbContext(optionsBuilder.Options);
        }
    }
}
