using Microsoft.AspNetCore.Hosting;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;

namespace OnlineAuctionSystem.Infrastructure.Storage
{
    // Saves uploaded files to wwwroot/uploads on local disk and returns a
    // relative URL. ASP.NET Core serves that folder via app.UseStaticFiles()
    // (see ApplicationBuilderExtensions), so the returned URL works directly
    // as an <img src> — same pattern as the existing local product photos
    // under /images/lots.
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _uploadsRootPath;

        public LocalFileStorageService(IWebHostEnvironment env)
        {
            var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
            _uploadsRootPath = Path.Combine(webRoot, "uploads");
            Directory.CreateDirectory(_uploadsRootPath);
        }

        public async Task<string> SaveAsync(Stream content, string originalFileName, string contentType, CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(originalFileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = contentType switch
                {
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    _ => ".jpg"
                };
            }

            // GUID filename — never trust or reuse the client-supplied name
            // (path traversal risk, collisions between different sellers'
            // uploads with the same filename, etc.).
            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(_uploadsRootPath, fileName);

            await using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await content.CopyToAsync(fileStream, cancellationToken);
            }

            return $"/uploads/{fileName}";
        }
    }
}
