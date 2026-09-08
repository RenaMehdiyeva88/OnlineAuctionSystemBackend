namespace OnlineAuctionSystem.Application.Common.Exceptions
{
    // Maps to HTTP 404 in the API layer.
    public class NotFoundException : Exception
    {
        public NotFoundException(string name, object key)
            : base($"Entity \"{name}\" ({key}) was not found.") { }
    }
}
