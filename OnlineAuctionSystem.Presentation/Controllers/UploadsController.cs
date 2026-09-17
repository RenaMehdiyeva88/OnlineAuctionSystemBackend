using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Contracts.Uploads;

namespace OnlineAuctionSystem.Presentation.Controllers
{
    // Sellers only — buyers have no reason to upload auction photos.
    [ApiController]
    [Authorize(Roles = "Seller")]
    [Route("api/uploads")]
    public class UploadsController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public UploadsController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpPost]
        [RequestSizeLimit(MaxFileSizeBytes)]
        public async Task<ActionResult<UploadResponse>> Upload(IFormFile file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { message = "No file was provided." });

            if (file.Length > MaxFileSizeBytes)
                return BadRequest(new { message = "File exceeds the 5 MB limit." });

            if (!AllowedContentTypes.Contains(file.ContentType))
                return BadRequest(new { message = "Only JPEG, PNG, or WEBP images are allowed." });

            await using var stream = file.OpenReadStream();
            var url = await _fileStorageService.SaveAsync(stream, file.FileName, file.ContentType, cancellationToken);

            return Ok(new UploadResponse(url));
        }
    }
}
