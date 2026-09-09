namespace OnlineAuctionSystem.Contracts.Categories
{
    // NOTE: Description was removed — Category has no Description property on
    // the Domain entity, so it was always mapping to an empty string (dead field).
    public record CategoryDto(
        Guid Id,
        string Name
    );
}
