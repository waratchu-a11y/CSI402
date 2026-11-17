using Microsoft.AspNetCore.Http;

namespace warat_0583_l005.Services;

public sealed class PrivateUploadStorage
{
    private readonly string _uploadDirectory;

    public PrivateUploadStorage(IWebHostEnvironment environment)
    {
        _uploadDirectory = Path.Combine(environment.ContentRootPath, "App_Data", "private-uploads");
    }

    public async Task<string?> SaveImageAsync(IFormFile? file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0 || file.Length > 5 * 1024 * 1024)
        {
            return null;
        }

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png"))
        {
            return null;
        }

        byte[] signature = new byte[8];
        await using (var stream = file.OpenReadStream())
        {
            int read = await stream.ReadAsync(signature.AsMemory(0, signature.Length), cancellationToken);
            if (!HasValidImageSignature(signature, read, extension))
            {
                return null;
            }
        }

        Directory.CreateDirectory(_uploadDirectory);
        string fileName = $"{Guid.NewGuid():N}{extension}";
        string destination = Path.Combine(_uploadDirectory, fileName);
        await using (var source = file.OpenReadStream())
        await using (var target = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await source.CopyToAsync(target, cancellationToken);
        }

        return fileName;
    }

    public FileStream? OpenRead(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) ||
            Path.GetFileName(fileName) != fileName ||
            fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            return null;
        }

        string path = Path.Combine(_uploadDirectory, fileName);
        return File.Exists(path)
            ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
            : null;
    }

    public static string? GetImageContentType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => null
        };

    private static bool HasValidImageSignature(byte[] signature, int bytesRead, string extension)
    {
        if (extension is ".jpg" or ".jpeg")
        {
            return bytesRead >= 3 && signature[0] == 0xFF && signature[1] == 0xD8 && signature[2] == 0xFF;
        }

        return bytesRead == 8 &&
            signature[0] == 0x89 && signature[1] == 0x50 && signature[2] == 0x4E && signature[3] == 0x47 &&
            signature[4] == 0x0D && signature[5] == 0x0A && signature[6] == 0x1A && signature[7] == 0x0A;
    }
}
