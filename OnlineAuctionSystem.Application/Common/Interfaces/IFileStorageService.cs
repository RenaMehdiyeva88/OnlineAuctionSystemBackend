namespace OnlineAuctionSystem.Application.Common.Interfaces.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(Stream content, string originalFileName, string contentType, CancellationToken cancellationToken = default);
    }
}
