using System.IO;

namespace NETmessenger.Application.Abstractions.Files;

public interface IVoiceStorage
{
    Task<StoredVoiceFile> SaveAsync(
        Stream audioStream,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken);
}

public record StoredVoiceFile(string Url, long SizeBytes, string ContentType);
