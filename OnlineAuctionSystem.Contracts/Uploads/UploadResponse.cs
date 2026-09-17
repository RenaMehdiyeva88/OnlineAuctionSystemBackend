namespace OnlineAuctionSystem.Contracts.Uploads
{
    // Url is a relative path (e.g. "/uploads/3f2a....jpg") — the frontend
    // combines it with the API base URL, same as it already does for the
    // local product photos under /images/lots.
    public record UploadResponse(string Url);
}
