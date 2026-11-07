using SqlSugar;
using VibeVoice.Application.Abstractions;

namespace VibeVoice.Infrastructure.Storage;

/// <summary>A stored upload's bytes, kept in the database when <c>Storage:Provider = Database</c>.</summary>
[SugarTable("stored_files")]
public class StoredFile
{
    [SugarColumn(IsPrimaryKey = true, Length = 300)]
    public string Path { get; set; } = "";

    public long SizeBytes { get; set; }

    /// <summary>bytea on PostgreSQL, BLOB on SQLite, varbinary(max) on SQL Server.</summary>
    public byte[] Data { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Keeps uploads inside the database instead of on disk. Meant for hosts whose disk is wiped on restart
/// (e.g. Render's free tier with a Neon Postgres): data and files then live and die together. Uses the same
/// <c>{category}/{yyyy}/{MM}/{guid}.{ext}</c> paths as <see cref="LocalFileStorage"/>, so rows stay compatible.
/// Files are read fully into memory, so pair it with modest upload limits.
/// </summary>
public sealed class DatabaseFileStorage(ISqlSugarClient db, TimeProvider clock) : IFileStorage
{
    public async Task<string> SaveAsync(Stream content, string category, string extension, CancellationToken ct)
    {
        if (category.IndexOfAny(['/', '\\', '.']) >= 0) throw new ArgumentException("Invalid storage category.", nameof(category));
        var ext = extension.TrimStart('.').ToLowerInvariant();
        if (ext.Length is 0 or > 5 || !ext.All(char.IsLetterOrDigit)) throw new ArgumentException("Invalid file extension.", nameof(extension));

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        var now = clock.GetUtcNow();
        var file = new StoredFile
        {
            Path = $"{category}/{now:yyyy}/{now:MM}/{Guid.NewGuid():N}.{ext}",
            Data = buffer.ToArray(),
            SizeBytes = buffer.Length,
        };
        await db.Insertable(file).ExecuteCommandAsync(ct);
        return file.Path;
    }

    public async Task<Stream?> OpenReadAsync(string relativePath, CancellationToken ct)
    {
        var file = await db.Queryable<StoredFile>().Where(f => f.Path == relativePath).FirstAsync(ct);
        // A seekable stream, so the files endpoint can still answer HTTP range requests (audio/video seeking).
        return file is null ? null : new MemoryStream(file.Data, writable: false);
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct) =>
        db.Deleteable<StoredFile>().Where(f => f.Path == relativePath).ExecuteCommandAsync(ct);
}
