using System.Linq.Expressions;
using VibeVoice.Application.Dtos;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application.Abstractions;

/// <summary>Generic data access over SqlSugar. Expressions are translated to provider-neutral SQL.</summary>
public interface IRepository<T> where T : class, new()
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct);
    Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct);
    Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> orderBy, bool descending, int take, CancellationToken ct);
    Task<(List<T> Items, int Total)> PageAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> orderBy, bool descending, int page, int pageSize, CancellationToken ct);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct);
    Task InsertAsync(T entity, CancellationToken ct);
    Task InsertRangeAsync(IReadOnlyCollection<T> entities, CancellationToken ct);
    Task UpdateAsync(T entity, CancellationToken ct);
    Task UpdateRangeAsync(IReadOnlyCollection<T> entities, CancellationToken ct);
    Task<int> DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken ct);
}

/// <summary>Filtered/paged queries that need joins or sub-queries.</summary>
public interface INoteRepository : IRepository<Note>
{
    Task<(List<Note> Items, int Total)> QueryAsync(Guid userId, NoteQuery query, CancellationToken ct);
}

public interface IMemoryRepository : IRepository<Memory>
{
    Task<(List<Memory> Items, int Total)> QueryAsync(Guid userId, MemoryQuery query, CancellationToken ct);
}

public interface IUnitOfWork
{
    /// <summary>Runs <paramref name="work"/> in a database transaction (commit on success, rollback on exception).</summary>
    Task ExecuteInTransactionAsync(Func<Task> work, CancellationToken ct);
}

/// <summary>Blob storage. Paths are always relative ("images/2026/11/{guid}.jpg") so S3/Azure can be swapped in.</summary>
public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string category, string extension, CancellationToken ct);
    Task<Stream?> OpenReadAsync(string relativePath, CancellationToken ct);
    Task DeleteAsync(string relativePath, CancellationToken ct);
}

public sealed record ProcessedImage(Stream Image, Stream Thumbnail, int Width, int Height, string Extension, string MimeType) : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await Image.DisposeAsync();
        await Thumbnail.DisposeAsync();
    }
}

public interface IImageProcessor
{
    /// <summary>Auto-orients, strips EXIF/GPS/XMP metadata and renders a thumbnail.</summary>
    Task<ProcessedImage> ProcessAsync(Stream input, CancellationToken ct);
}

public sealed record VoiceHealth(string? TtsEngine, string? AsrEngine, string? Device, bool ModelLoaded, string? ArabicTtsEngine);

public sealed record TtsResult(byte[] Audio, string ContentType, string? Voice, string? Engine, string? Language, double? DurationSeconds);

public sealed record AsrResult(string Text, string? Language, IReadOnlyList<SegmentDto> Segments, string? Engine);

/// <summary>Typed client for the Python FastAPI service that wraps Microsoft VibeVoice.</summary>
public interface IVoiceServiceClient
{
    /// <summary>Returns null when the service is unreachable.</summary>
    Task<VoiceHealth?> GetHealthAsync(CancellationToken ct);
    Task<IReadOnlyList<VoiceDto>> GetVoicesAsync(CancellationToken ct);
    Task<TtsResult> SynthesizeAsync(string text, string? voice, string? language, CancellationToken ct);
    Task<AsrResult> TranscribeAsync(Stream audio, string fileName, string contentType, string? language, CancellationToken ct);
}

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}

public interface ICurrentUser
{
    Guid UserId { get; }
    bool IsAdmin { get; }
}
