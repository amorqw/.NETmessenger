using System.IO;
using Microsoft.Extensions.Hosting;
using NETmessenger.Application.Abstractions.Files;

namespace NETmessenger.Infrastructure.Services.Files;

public sealed class LocalVoiceStorage(IHostEnvironment environment) : IVoiceStorage
{
    private const string VoiceFolder = "wwwroot/voice";

    public async Task<StoredVoiceFile> SaveAsync(
        Stream audioStream,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var safeExtension = Path.GetExtension(originalFileName);
        var fileName = $"{Guid.NewGuid():N}{safeExtension}";

        var rootPath = environment.ContentRootPath;
        var targetDir = Path.Combine(rootPath, VoiceFolder);
        Directory.CreateDirectory(targetDir);

        var fullPath = Path.Combine(targetDir, fileName);
        await using (var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await audioStream.CopyToAsync(output, cancellationToken);
        }

        var fileInfo = new FileInfo(fullPath);
        var url = $"/voice/{fileName}";
        return new StoredVoiceFile(url, fileInfo.Length, contentType);
    }
}
