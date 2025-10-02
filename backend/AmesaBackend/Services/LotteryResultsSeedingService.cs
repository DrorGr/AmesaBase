using AmesaBackend.Data;
using AmesaBackend.Models;
using AmesaBackend.Services;
using Microsoft.EntityFrameworkCore;

namespace AmesaBackend.Services
{
    public static class LotteryResultsSeedingService
    {
        public static async Task SeedLotteryResultsAsync(AmesaDbContext context, IQRCodeService qrCodeService)
        {
            if (await context.LotteryResults.AnyAsync())
            {
                return; // Already seeded
            }

            // Get some existing houses and users for the sample data
            var houses = await context.Houses.Take(3).ToListAsync();
            var users = await context.Users.Take(5).ToListAsync();

            if (!houses.Any() || !users.Any())
            {
                return; // Need houses and users to create lottery results
            }

            var lotteryResults = new List<LotteryResult>();

            // Create sample lottery results for the first 3 houses
            for (int i = 0; i < Math.Min(3, houses.Count); i++)
            {
                var house = houses[i];
                var drawId = Guid.NewGuid(); // Mock draw ID

                // Create 3 winners per house (1st, 2nd, 3rd place)
                for (int prizePosition = 1; prizePosition <= 3; prizePosition++)
                {
                    var winnerId = users[i % users.Count].Id;
                    var ticketNumber = $"T{house.Id.ToString()[..8]}-{prizePosition:D3}";
                    
                    var lotteryResult = new LotteryResult
                    {
                        Id = Guid.NewGuid(),
                        LotteryId = house.Id,
                        DrawId = drawId,
                        WinnerTicketNumber = ticketNumber,
                        WinnerUserId = winnerId,
                        PrizePosition = prizePosition,
                        PrizeType = GetPrizeType(prizePosition),
                        PrizeValue = GetPrizeValue(prizePosition, house.Price),
                        PrizeDescription = GetPrizeDescription(prizePosition, house.Title),
                        IsVerified = true,
                        IsClaimed = prizePosition == 3, // Only 3rd place claimed for demo
                        ClaimedAt = prizePosition == 3 ? DateTime.UtcNow.AddDays(-5) : null,
                        ResultDate = DateTime.UtcNow.AddDays(-10 + i),
                        CreatedAt = DateTime.UtcNow.AddDays(-10 + i),
                        UpdatedAt = DateTime.UtcNow.AddDays(-10 + i)
                    };

                    // Generate QR code
                    lotteryResult.QRCodeData = await qrCodeService.GenerateQRCodeDataAsync(
                        lotteryResult.Id, 
                        ticketNumber, 
                        prizePosition
                    );
                    lotteryResult.QRCodeImageUrl = qrCodeService.GenerateQRCodeImageUrl(lotteryResult.QRCodeData);

                    lotteryResults.Add(lotteryResult);

                    // Add history entry
                    var historyEntry = new LotteryResultHistory
                    {
                        Id = Guid.NewGuid(),
                        LotteryResultId = lotteryResult.Id,
                        Action = "Created",
                        Details = $"Lottery result created for {GetPrizeDescription(prizePosition, house.Title)}",
                        PerformedBy = "System",
                        Timestamp = lotteryResult.CreatedAt,
                        IpAddress = "127.0.0.1",
                        UserAgent = "System Seeding Service"
                    };

                    context.LotteryResultHistory.Add(historyEntry);

                    // Add prize delivery for 2nd and 3rd place winners
                    if (prizePosition > 1)
                    {
                        var delivery = new PrizeDelivery
                        {
                            Id = Guid.NewGuid(),
                            LotteryResultId = lotteryResult.Id,
                            WinnerUserId = winnerId,
                            RecipientName = $"Winner {prizePosition}",
                            AddressLine1 = $"{100 + prizePosition} Main Street",
                            City = "Sample City",
                            State = "Sample State",
                            PostalCode = $"1234{prizePosition}",
                            Country = "Sample Country",
                            Phone = $"+1-555-000{prizePosition}",
                            Email = $"winner{prizePosition}@example.com",
                            DeliveryMethod = "Standard Shipping",
                            DeliveryStatus = prizePosition == 3 ? "Delivered" : "Pending",
                            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(7),
                            ActualDeliveryDate = prizePosition == 3 ? DateTime.UtcNow.AddDays(-2) : null,
                            ShippingCost = 25.00m,
                            DeliveryNotes = $"Prize delivery for {GetPrizeDescription(prizePosition, house.Title)}",
                            CreatedAt = DateTime.UtcNow.AddDays(-8 + i),
                            UpdatedAt = DateTime.UtcNow.AddDays(-8 + i)
                        };

                        context.PrizeDeliveries.Add(delivery);
                    }
                }
            }

            // Add sample scratch card results
            for (int i = 0; i < Math.Min(10, users.Count * 2); i++)
            {
                var scratchCard = new ScratchCardResult
                {
                    Id = Guid.NewGuid(),
                    UserId = users[i % users.Count].Id,
                    CardType = GetRandomCardType(),
                    CardNumber = $"SC{DateTime.UtcNow.Ticks + i}",
                    IsWinner = i % 3 == 0, // 1 in 3 wins
                    PrizeType = i % 3 == 0 ? GetRandomPrizeType() : null,
                    PrizeValue = i % 3 == 0 ? GetRandomPrizeValue() : 0,
                    PrizeDescription = i % 3 == 0 ? GetRandomPrizeDescription() : "No Prize",
                    CardImageUrl = $"https://example.com/cards/scratch-card-{i + 1}.jpg",
                    ScratchedImageUrl = i % 2 == 0 ? $"https://example.com/cards/scratch-card-{i + 1}-revealed.jpg" : null,
                    IsScratched = i % 2 == 0,
                    ScratchedAt = i % 2 == 0 ? DateTime.UtcNow.AddDays(-i) : null,
                    IsClaimed = i % 6 == 0, // Some are claimed
                    ClaimedAt = i % 6 == 0 ? DateTime.UtcNow.AddDays(-i + 1) : null,
                    CreatedAt = DateTime.UtcNow.AddDays(-15 + i)
                };

                context.ScratchCardResults.Add(scratchCard);
            }

            await context.LotteryResults.AddRangeAsync(lotteryResults);
            await context.SaveChangesAsync();
        }

        private static string GetPrizeType(int position)
        {
            return position switch
            {
                1 => "House",
                2 => "Cash",
                3 => "Voucher",
                _ => "Other"
            };
        }

        private static decimal GetPrizeValue(int position, decimal housePrice)
        {
            return position switch
            {
                1 => housePrice,
                2 => 10000m,
                3 => 1000m,
                _ => 0m
            };
        }

        private static string GetPrizeDescription(int position, string houseTitle)
        {
            return position switch
            {
                1 => $"Grand Prize: {houseTitle}",
                2 => "Second Prize: $10,000 Cash",
                3 => "Third Prize: $1,000 Shopping Voucher",
                _ => "Consolation Prize"
            };
        }

        private static string GetRandomCardType()
        {
            var types = new[] { "Lucky Stars", "Golden Ticket", "Diamond Rush", "Cash Blast", "Treasure Hunt" };
            return types[Random.Shared.Next(types.Length)];
        }

        private static string GetRandomPrizeType()
        {
            var types = new[] { "Cash", "Voucher", "Free Ticket", "Bonus Entry" };
            return types[Random.Shared.Next(types.Length)];
        }

        private static decimal GetRandomPrizeValue()
        {
            var values = new[] { 5m, 10m, 25m, 50m, 100m, 250m, 500m };
            return values[Random.Shared.Next(values.Length)];
        }

        private static string GetRandomPrizeDescription()
        {
            var descriptions = new[] 
            { 
                "Cash Prize", 
                "Shopping Voucher", 
                "Free Lottery Ticket", 
                "Bonus Entry to Next Draw",
                "Restaurant Gift Card",
                "Online Store Credit"
            };
            return descriptions[Random.Shared.Next(descriptions.Length)];
        }
    }
}
