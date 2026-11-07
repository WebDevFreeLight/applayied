using Microsoft.Extensions.Options;
using VibeVoice.Application.Abstractions;

namespace VibeVoice.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string Section = "Storage";

    /// <summary>Local (files on disk, default) or Database (bytes in the stored_files table).</summary>
    public string Provider { get; set; } = "Local";

    /// <summary>Root folder for uploads; relative paths are resolved against the content root.</summary>
    public string RootPath { get; set; } = "storage";
}

/// <summary>Stores files as <c>{category}/{yyyy}/{MM}/{guid}.{ext}</c> under the configured root.</summary>
public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _root;
    private readonly TimeProvider _clock;

    public LocalFileStorage(IOptions<StorageOptions> options, string contentRoot, TimeProvider clock)
    {
        var configured = options.Value.RootPath;
        _root = Path.GetFullPath(Path.IsPathRooted(configured) ? configured : Path.Combine(contentRoot, configured));
        Directory.CreateDirectory(_root);
        _clock = clock;
    }

    public string Root => _root;

    public async Task<string> SaveAsync(Stream content, string category, string extension, CancellationToken ct)
    {
        if (category.IndexOfAny(['/', '\\', '.']) >= 0) throw new ArgumentException("Invalid storage category.", nameof(category));
        var ext = extension.TrimStart('.').ToLowerInvariant();
        if (ext.Length is 0 or > 5 || !ext.All(char.IsLetterOrDigit)) throw new ArgumentException("Invalid file extension.", nameof(extension));

        var now = _clock.GetUtcNow();
        var relative = $"{category}/{now:yyyy}/{now:MM}/{Guid.NewGuid():N}.{ext}";
        var full = Resolve(relative)!;
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);

        await using var file = new FileStream(full, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await content.CopyToAsync(file, ct);
        return relative;
    }

    public Task<Stream?> OpenReadAsync(string relativePath, CancellationToken ct)
    {
        var full = Resolve(relativePath);
        if (full is null || !File.Exists(full)) return Task.FromResult<Stream?>(null);
        Stream stream = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct)
    {
        var full = Resolve(relativePath);
        if (full is not null && File.Exists(full)) File.Delete(full);
        return Task.CompletedTask;
    }

    /// <summary>Maps a relative path to disk, refusing anything that escapes the root (e.g. "../../etc/passwd").</summary>
    private string? Resolve(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath)) return null;
        var full = Path.GetFullPath(Path.Combine(_root, relativePath.Replace('\\', '/')));
        return full.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ? full : null;
    }
}
