using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Domain.Enums;
using OnlineAuctionSystem.Persistence.Context;

namespace OnlineAuctionSystem.Persistence.Seed
{
    // Called once at startup (Program.cs) to guarantee categories and demo accounts exist
    // along with a comprehensive set of demo auctions (active, closing soon, and closed)
    // so the app is immediately usable and fully demonstrates all features after first run.
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AuctionDbContext context, IPasswordHasher passwordHasher, ILogger logger)
        {
            await context.Database.MigrateAsync();

            async Task SeedCoreAsync()
            {
                // Seed Categories
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

                // Seed Users
                User? seller = null;
                User? buyer = null;
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

                    context.Users.Add(demoSeller);
                    context.Users.Add(demoBuyer);
                    await context.SaveChangesAsync();

                    seller = demoSeller;
                    buyer = demoBuyer;
                }
                else
                {
                    seller = await context.Users.FirstOrDefaultAsync(u => u.Email == "seller@demo.com");
                    buyer = await context.Users.FirstOrDefaultAsync(u => u.Email == "buyer@demo.com");
                }

                // Seed Auctions (18 comprehensive demo auctions across all categories)
                if (!await context.Auctions.AnyAsync() && seller != null && buyer != null)
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
                        // ELECTRONICS (4 items)
                        new Auction
                        {
                            Title = "Sony A7III Mirrorless Camera",
                            Description = "Full-frame mirrorless camera with excellent image quality. Low shutter count (8,500). Includes battery and charger. Perfect for professional photographers.",
                            ImageUrl = "https://loremflickr.com/1600/900/electronics?lock=1",
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
                            ImageUrl = "/images/lots/canon-camera/1.jpg",
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
                            ImageUrl = "https://loremflickr.com/1600/900/electronics?lock=2",
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
                            ImageUrl = "/images/lots/nes-console/1.jpg",
                            StartingPrice = 250m,
                            EndTime = DateTime.UtcNow.AddDays(2),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = electronics.Id,
                            WinnerId = null
                        },

                        // COLLECTIBLES (4 items)
                        new Auction
                        {
                            Title = "Rolex Submariner Vintage (1970s)",
                            Description = "Authentic vintage Rolex Submariner watch from the 1970s in stainless steel. Keeps excellent time and has service history. A true collector's piece.",
                            ImageUrl = "https://loremflickr.com/1600/900/watch?lock=1",
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
                            ImageUrl = "/images/lots/ancient-coins/1.jpg",
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
                            ImageUrl = "/images/lots/omega-watch/1.jpg",
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
                            ImageUrl = "https://loremflickr.com/1600/900/book?lock=1",
                            StartingPrice = 1500m,
                            EndTime = DateTime.UtcNow.AddDays(6),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = collectibles.Id
                        },

                        // ART (3 items)
                        new Auction
                        {
                            Title = "Abstract Oil Painting 24x36 inches",
                            Description = "Original abstract oil painting by contemporary artist. Bold colors and dynamic composition. Certificate of authenticity included. Signed and dated.",
                            ImageUrl = "https://loremflickr.com/1600/900/painting?lock=1",
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
                            ImageUrl = "https://loremflickr.com/1600/900/painting?lock=2",
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
                            ImageUrl = "https://loremflickr.com/1600/900/painting?lock=3",
                            StartingPrice = 180m,
                            EndTime = DateTime.UtcNow.AddDays(4),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = art.Id
                        },

                        // FASHION (3 items)
                        new Auction
                        {
                            Title = "Hermès Silk Scarf - Vintage Collection",
                            Description = "Authentic Hermès silk scarf in pristine condition. Classic design with vibrant colors. Never worn, still in original packaging. Collector's item.",
                            ImageUrl = "https://loremflickr.com/1600/900/fashion?lock=1",
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
                            ImageUrl = "https://loremflickr.com/1600/900/fashion?lock=2",
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
                            ImageUrl = "https://loremflickr.com/1600/900/fashion?lock=3",
                            StartingPrice = 120m,
                            EndTime = DateTime.UtcNow.AddDays(3),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = fashion.Id
                        },

                        // HOME & GARDEN (2 items)
                        new Auction
                        {
                            Title = "Antique Wooden Desk - Victorian Era",
                            Description = "Handcrafted Victorian-era wooden desk made from solid mahogany. Multiple drawers with brass hardware. Perfect home office or study piece.",
                            ImageUrl = "https://loremflickr.com/1600/900/furniture?lock=2",
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
                            ImageUrl = "https://loremflickr.com/1600/900/furniture?lock=1",
                            StartingPrice = 150m,
                            EndTime = DateTime.UtcNow.AddDays(5),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = homeGarden.Id
                        },

                        // SPORTS (2 items)
                        new Auction
                        {
                            Title = "Vintage Baseball Glove - Rawlings Professional",
                            Description = "Authentic Rawlings baseball glove from 1960s. High-quality leather construction. Perfect for collectors or those who appreciate vintage sports equipment.",
                            ImageUrl = "https://loremflickr.com/1600/900/sports?lock=1",
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
                            ImageUrl = "/images/lots/nike-shoes/1.jpg",
                            StartingPrice = 250m,
                            EndTime = DateTime.UtcNow.AddDays(3),
                            Status = AuctionStatus.Active,
                            SellerId = seller.Id,
                            CategoryId = sports.Id
                        }
                    };

                        context.Auctions.AddRange(auctions);
                        await context.SaveChangesAsync();

                        // Add some bids to closed auctions for demonstration
                        var closedRetroConsole = await context.Auctions
                            .FirstOrDefaultAsync(a => a.Title == "Retro Nintendo Entertainment System (NES) Console");
                        if (closedRetroConsole != null)
                        {
                            context.Bids.AddRange(
                                new Bid { Amount = 260m, AuctionId = closedRetroConsole.Id, BidderId = buyer.Id },
                                new Bid { Amount = 280m, AuctionId = closedRetroConsole.Id, BidderId = seller.Id },
                                new Bid { Amount = 300m, AuctionId = closedRetroConsole.Id, BidderId = buyer.Id }
                            );
                        }

                        var closedOmega = await context.Auctions
                            .FirstOrDefaultAsync(a => a.Title == "Omega Speedmaster Professional Chronograph");
                        if (closedOmega != null)
                        {
                            context.Bids.AddRange(
                                new Bid { Amount = 3600m, AuctionId = closedOmega.Id, BidderId = buyer.Id },
                                new Bid { Amount = 3700m, AuctionId = closedOmega.Id, BidderId = seller.Id },
                                new Bid { Amount = 3800m, AuctionId = closedOmega.Id, BidderId = buyer.Id },
                                new Bid { Amount = 3900m, AuctionId = closedOmega.Id, BidderId = seller.Id }
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
                // NEVER call EnsureDeletedAsync() here. A schema/type mismatch during
                // seeding is a bug to fix (usually a stale migration or a manually
                // edited DB), not a reason to silently wipe every user's data —
                // that would destroy real production data on any transient cast
                // error. Log it loudly and let the exception propagate so startup
                // fails visibly instead of the app "recovering" by deleting the DB.
                logger.LogError(ex,
                    "Database seeding failed due to a type mismatch. This usually means the schema is out of sync " +
                    "with a pending migration. Run 'dotnet ef database update' and investigate — the database was " +
                    "NOT deleted.");
                throw;
            }
        }
    }
}
