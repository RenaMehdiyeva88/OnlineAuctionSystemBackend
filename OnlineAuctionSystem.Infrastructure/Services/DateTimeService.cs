using OnlineAuctionSystem.Application.Common.Interfaces;

namespace OnlineAuctionSystem.Infrastructure.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
