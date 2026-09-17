using System.Globalization;

namespace OnlineAuctionSystem.Application.Common
{
    // Single source of truth for formatting money in notification text.
    // Previously CloseAuctionCommandHandler wrote "{amount}" (raw decimal,
    // no currency symbol at all) while AuctionAutoCloseService wrote
    // "{amount:C}" (server-locale currency — $, ₼, TL, etc. depending on
    // whatever regional settings the machine running the app happens to
    // have). Both auto-close and manual-close now produce identical wording
    // regardless of which code path closed the auction or what server it
    // runs on.
    public static class CurrencyFormatter
    {
        public static string Format(decimal amount) =>
            string.Format(CultureInfo.InvariantCulture, "{0:0.00} AZN", amount);
    }
}
