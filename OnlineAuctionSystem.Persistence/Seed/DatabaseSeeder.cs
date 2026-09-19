using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Domain.Enums;
using OnlineAuctionSystem.Persistence.Context;

namespace OnlineAuctionSystem.Persistence.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AuctionDbContext context, IPasswordHasher passwordHasher, ILogger logger)
        {
            await context.Database.MigrateAsync();

            async Task SeedCoreAsync()
            {
                if (!await context.Categories.AnyAsync())
                {
                    context.Categories.AddRange(
                        new Category { Name = "Electronics" },
                        new Category { Name = "Collectibles" },
                        new Category { Name = "Art" },
                        new Category { Name = "Fashion" },
                        new Category { Name = "Home & Garden" },
                        new Category { Name = "Sports" }
                    );

                    await context.SaveChangesAsync();
                }

                User? seller = null;
                User? buyer = null;
                User? buyer2 = null;
                if (!await context.Users.AnyAsync())
                {
                    var demoSeller = new User
                    {
                        Username = "demo_seller",
                        Email = "seller@demo.com",
                        PasswordHash = passwordHasher.Hash("Password123!"),
                        Role = UserRole.Seller
                    };

                    var demoBuyer = new User
                    {
                        Username = "demo_buyer",
                        Email = "buyer@demo.com",
                        PasswordHash = passwordHasher.Hash("Password123!"),
                        Role = UserRole.Buyer
                    };

                    var demoAdmin = new User
                    {
                        Username = "demo_admin",
                        Email = "admin@demo.com",
                        PasswordHash = passwordHasher.Hash("Password123!"),
                        Role = UserRole.Admin
                    };

                    var demoBuyer2 = new User
                    {
                        Username = "demo_buyer2",
                        Email = "buyer2@demo.com",
                        PasswordHash = passwordHasher.Hash("Password123!"),
                        Role = UserRole.Buyer
                    };

                    context.Users.Add(demoSeller);
                    context.Users.Add(demoBuyer);
                    context.Users.Add(demoAdmin);
                    context.Users.Add(demoBuyer2);
                    await context.SaveChangesAsync();

                    seller = demoSeller;
                    buyer = demoBuyer;
                    buyer2 = demoBuyer2;
                }
                else
                {
                    seller = await context.Users.FirstOrDefaultAsync(u => u.Email == "seller@demo.com");
                    buyer = await context.Users.FirstOrDefaultAsync(u => u.Email == "buyer@demo.com");
                    buyer2 = await context.Users.FirstOrDefaultAsync(u => u.Email == "buyer2@demo.com");
                }

                if (!await context.Auctions.AnyAsync() && seller != null && buyer != null && buyer2 != null)
                {
                    var electronics = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Electronics");
                    var art = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Art");
                    var collectibles = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Collectibles");
                    var fashion = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Fashion");
                    var homeGarden = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Home & Garden");
                    var sports = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Sports");

                    if (electronics != null && art != null && collectibles != null && fashion != null && homeGarden != null && sports != null)
                    {
                        var auctions = new List<Auction>
                    {
                        new Auction
                        {
                            Title = "Sony A7III Mirrorless Camera",
                            Description = "Full-frame mirrorless camera with excellent image quality. Low shutter count (8,500). Includes battery and charger. Perfect for professional photographers.",
                            ImageUrl = null,
                            StartingPrice = 800m,
                            EndTime = DateTime.UtcNow.AddHours(2),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = electronics.Id
                        },
                        new Auction
                        {
                            Title = "Vintage Canon AE-1 35mm Film Camera",
                            Description = "Professional vintage film camera in excellent condition. Fully functional with original Canon FD 50mm f/1.8 lens. Perfect for photography enthusiasts and collectors.",
                            ImageUrl = null,
                            StartingPrice = 150m,
                            EndTime = DateTime.UtcNow.AddDays(3),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = electronics.Id
                        },
                        new Auction
                        {
                            Title = "Apple AirPods Pro (2nd Generation)",
                            Description = "Latest generation Apple AirPods Pro with active noise cancellation. Original box and accessories included. Barely used, excellent condition.",
                            ImageUrl = null,
                            StartingPrice = 200m,
                            EndTime = DateTime.UtcNow.AddDays(1),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = electronics.Id
                        },
                        new Auction
                        {
                            Title = "Retro Nintendo Entertainment System (NES) Console",
                            Description = "Classic NES console with 2 controllers and 10 game cartridges including Super Mario Bros. Fully functional and well-preserved.",
                            ImageUrl = null,
                            StartingPrice = 250m,
                            EndTime = DateTime.UtcNow.AddDays(2),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = electronics.Id,
                            WinnerId = null
                        },
                        new Auction
                        {
                            Title = "Rolex Submariner Vintage (1970s)",
                            Description = "Authentic vintage Rolex Submariner watch from the 1970s in stainless steel. Keeps excellent time and has service history. A true collector's piece.",
                            ImageUrl = null,
                            StartingPrice = 2000m,
                            EndTime = DateTime.UtcNow.AddDays(5),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = collectibles.Id
                        },
                        new Auction
                        {
                            Title = "Rare Ancient Coin Collection - Roman Empire",
                            Description = "Collection of 5 authentic ancient Roman coins from 100-200 AD. Includes certificates of authenticity. Excellent investment piece.",
                            ImageUrl = null,
                            StartingPrice = 800m,
                            EndTime = DateTime.UtcNow.AddDays(4),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = collectibles.Id
                        },
                        new Auction
                        {
                            Title = "Omega Speedmaster Professional Chronograph",
                            Description = "Iconic Omega Speedmaster Professional with moonwatch heritage. Chronograph in perfect working condition. Complete with original box and papers.",
                            ImageUrl = null,
                            StartingPrice = 3500m,
                            EndTime = DateTime.UtcNow.AddDays(3),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = collectibles.Id,
                            WinnerId = null
                        },
                        new Auction
                        {
                            Title = "First Edition Harry Potter Book - Philosopher's Stone",
                            Description = "Rare first edition, first printing of Harry Potter and the Philosopher's Stone (UK). Excellent condition with dustjacket. Very collectible.",
                            ImageUrl = null,
                            StartingPrice = 1500m,
                            EndTime = DateTime.UtcNow.AddDays(6),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = collectibles.Id
                        },
                        new Auction
                        {
                            Title = "Abstract Oil Painting 24x36 inches",
                            Description = "Original abstract oil painting by contemporary artist. Bold colors and dynamic composition. Certificate of authenticity included. Signed and dated.",
                            ImageUrl = null,
                            StartingPrice = 500m,
                            EndTime = DateTime.UtcNow.AddDays(7),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = art.Id
                        },
                        new Auction
                        {
                            Title = "Claude Monet Water Lilies - Museum Quality Print",
                            Description = "High-quality museum reproduction of Monet's Water Lilies series. Professionally framed with UV-protected glass. Perfect for fine art collectors.",
                            ImageUrl = null,
                            StartingPrice = 200m,
                            EndTime = DateTime.UtcNow.AddDays(6),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = art.Id
                        },
                        new Auction
                        {
                            Title = "Van Gogh Starry Night - Museum Print",
                            Description = "Authentic museum-quality reproduction of Van Gogh's Starry Night. Archival quality paper with premium frame. Excellent for art enthusiasts.",
                            ImageUrl = null,
                            StartingPrice = 180m,
                            EndTime = DateTime.UtcNow.AddDays(4),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = art.Id
                        },
                        new Auction
                        {
                            Title = "Hermès Silk Scarf - Vintage Collection",
                            Description = "Authentic Hermès silk scarf in pristine condition. Classic design with vibrant colors. Never worn, still in original packaging. Collector's item.",
                            ImageUrl = null,
                            StartingPrice = 300m,
                            EndTime = DateTime.UtcNow.AddDays(4),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = fashion.Id
                        },
                        new Auction
                        {
                            Title = "Vintage Chanel Quilted Handbag",
                            Description = "Authentic vintage Chanel quilted handbag in black lambskin leather. Serial number verified. Excellent condition with minimal signs of wear.",
                            ImageUrl = null,
                            StartingPrice = 1200m,
                            EndTime = DateTime.UtcNow.AddDays(5),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = fashion.Id
                        },
                        new Auction
                        {
                            Title = "Designer Sunglasses - Ray-Ban Vintage",
                            Description = "Vintage Ray-Ban Wayfarer sunglasses with original brown case. Authentic model from 1980s. Minimal wear, perfect condition.",
                            ImageUrl = null,
                            StartingPrice = 120m,
                            EndTime = DateTime.UtcNow.AddDays(3),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = fashion.Id
                        },
                        new Auction
                        {
                            Title = "Antique Wooden Desk - Victorian Era",
                            Description = "Handcrafted Victorian-era wooden desk made from solid mahogany. Multiple drawers with brass hardware. Perfect home office or study piece.",
                            ImageUrl = null,
                            StartingPrice = 450m,
                            EndTime = DateTime.UtcNow.AddDays(8),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = homeGarden.Id
                        },
                        new Auction
                        {
                            Title = "Vintage Garden Lamp - Wrought Iron",
                            Description = "Vintage wrought iron garden lamp with original frosted glass shade. Fully functional and restored. Perfect for outdoor garden ambiance.",
                            ImageUrl = null,
                            StartingPrice = 150m,
                            EndTime = DateTime.UtcNow.AddDays(5),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = homeGarden.Id
                        },
                        new Auction
                        {
                            Title = "Vintage Baseball Glove - Rawlings Professional",
                            Description = "Authentic Rawlings baseball glove from 1960s. High-quality leather construction. Perfect for collectors or those who appreciate vintage sports equipment.",
                            ImageUrl = null,
                            StartingPrice = 180m,
                            EndTime = DateTime.UtcNow.AddDays(2),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = sports.Id
                        },
                        new Auction
                        {
                            Title = "Nike Running Shoes - Rare Vintage Edition",
                            Description = "Rare vintage Nike running shoes from 1990s in mint condition. Original box included. Collector's item for sneaker enthusiasts.",
                            ImageUrl = null,
                            StartingPrice = 250m,
                            EndTime = DateTime.UtcNow.AddDays(3),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = sports.Id
                        }
                    };

                        context.Auctions.AddRange(auctions);
                        await context.SaveChangesAsync();

                        var closedRetroConsole = await context.Auctions
                            .FirstOrDefaultAsync(a => a.Title == "Retro Nintendo Entertainment System (NES) Console");
                        if (closedRetroConsole != null)
                        {
                            context.Bids.AddRange(
                                new Bid { Amount = 260m, AuctionId = closedRetroConsole.Id, BidderId = buyer.Id },
                                new Bid { Amount = 280m, AuctionId = closedRetroConsole.Id, BidderId = buyer2.Id },
                                new Bid { Amount = 300m, AuctionId = closedRetroConsole.Id, BidderId = buyer.Id }
                            );
                        }

                        var closedOmega = await context.Auctions
                            .FirstOrDefaultAsync(a => a.Title == "Omega Speedmaster Professional Chronograph");
                        if (closedOmega != null)
                        {
                            context.Bids.AddRange(
                                new Bid { Amount = 3600m, AuctionId = closedOmega.Id, BidderId = buyer.Id },
                                new Bid { Amount = 3700m, AuctionId = closedOmega.Id, BidderId = buyer2.Id },
                                new Bid { Amount = 3800m, AuctionId = closedOmega.Id, BidderId = buyer.Id },
                                new Bid { Amount = 3900m, AuctionId = closedOmega.Id, BidderId = buyer2.Id }
                            );
                        }

                        await context.SaveChangesAsync();
                    }
                }
            }

            try
            {
                await SeedCoreAsync();
            }
            catch (InvalidCastException ex)
            {
                logger.LogError(ex,
                    "Database seeding failed due to a type mismatch. This usually means the schema is out of sync " +
                    "with a pending migration. Run 'dotnet ef database update' and investigate — the database was " +
                    "NOT deleted.");
                throw;
            }
        }
    }
}