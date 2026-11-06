using System.Linq.Expressions;
using SqlSugar;
using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Application.Services;
using VibeVoice.Domain;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Infrastructure.Persistence;

public class SqlSugarRepository<T>(ISqlSugarClient db) : IRepository<T> where T : class, new()
{
    protected ISqlSugarClient Db { get; } = db;

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await Db.Queryable<T>().InSingleAsync(id);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct) =>
        await Db.Queryable<T>().Where(predicate).FirstAsync(ct);

    public Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct) =>
        Db.Queryable<T>().Where(predicate).ToListAsync(ct);

    public Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> orderBy, bool descending, int take, CancellationToken ct) =>
        Db.Queryable<T>().Where(predicate).OrderBy(orderBy, descending ? OrderByType.Desc : OrderByType.Asc).Take(take).ToListAsync(ct);

    public async Task<(List<T> Items, int Total)> PageAsync(
        Expression<Func<T, bool>> predicate, Expression<Func<T, object>> orderBy, bool descending, int page, int pageSize, CancellationToken ct)
    {
        RefAsync<int> total = 0;
        var items = await Db.Queryable<T>().Where(predicate)
            .OrderBy(orderBy, descending ? OrderByType.Desc : OrderByType.Asc)
            .ToPageListAsync(page, pageSize, total, ct);
        return (items, total.Value);
    }

    public Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct) =>
        Db.Queryable<T>().Where(predicate).CountAsync(ct);

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct) =>
        Db.Queryable<T>().Where(predicate).AnyAsync(ct);

    public Task InsertAsync(T entity, CancellationToken ct) =>
        Db.Insertable(entity).ExecuteCommandAsync(ct);

    public Task InsertRangeAsync(IReadOnlyCollection<T> entities, CancellationToken ct) =>
        entities.Count == 0 ? Task.CompletedTask : Db.Insertable(entities.ToList()).ExecuteCommandAsync(ct);

    public Task UpdateAsync(T entity, CancellationToken ct) =>
        Db.Updateable(entity).ExecuteCommandAsync(ct);

    public Task UpdateRangeAsync(IReadOnlyCollection<T> entities, CancellationToken ct) =>
        entities.Count == 0 ? Task.CompletedTask : Db.Updateable(entities.ToList()).ExecuteCommandAsync(ct);

    public Task<int> DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken ct) =>
        Db.Deleteable<T>().Where(predicate).ExecuteCommandAsync(ct);
}

public sealed class NoteRepository(ISqlSugarClient db, TaggingService tagging) : SqlSugarRepository<Note>(db), INoteRepository
{
    public async Task<(List<Note> Items, int Total)> QueryAsync(Guid userId, NoteQuery q, CancellationToken ct)
    {
        var deleted = q.Deleted;
        Expression<Func<Note, bool>> where = n => n.UserId == userId && n.IsDeleted == deleted;
        if (!q.Deleted)
        {
            if (q.ArchivedOnly) where = where.And(n => n.IsArchived);
            else if (!q.IncludeArchived) where = where.And(n => !n.IsArchived);
        }

        var term = TextNormalizer.Normalize(q.Search);
        if (term.Length > 0) where = where.And(n => n.SearchText.Contains(term));

        if (!string.IsNullOrWhiteSpace(q.Tag))
        {
            var ids = await tagging.NoteIdsWithTagAsync(userId, q.Tag, ct);
            if (ids.Count == 0) return ([], 0);
            where = where.And(n => ids.Contains(n.Id));
        }

        if (q.From is { } from) where = where.And(n => n.UpdatedAt >= from);
        if (q.To is { } to) { var end = to.Date.AddDays(1); where = where.And(n => n.UpdatedAt < end); }

        var query = Db.Queryable<Note>().Where(where);
        query = q.HasAudio switch
        {
            true => query.Where(n => SqlFunc.Subqueryable<AudioClip>().Where(a => a.OwnerType == OwnerType.Note && a.OwnerId == n.Id).Any()),
            false => query.Where(n => SqlFunc.Subqueryable<AudioClip>().Where(a => a.OwnerType == OwnerType.Note && a.OwnerId == n.Id).NotAny()),
            _ => query,
        };
        query = q.HasImages switch
        {
            true => query.Where(n => SqlFunc.Subqueryable<ImageAttachment>().Where(i => i.OwnerType == OwnerType.Note && i.OwnerId == n.Id).Any()),
            false => query.Where(n => SqlFunc.Subqueryable<ImageAttachment>().Where(i => i.OwnerType == OwnerType.Note && i.OwnerId == n.Id).NotAny()),
            _ => query,
        };

        // Pinned notes always float to the top of the current sort.
        query = query.OrderBy(n => n.IsPinned, OrderByType.Desc);
        query = q.Sort switch
        {
            "updated_asc" => query.OrderBy(n => n.UpdatedAt, OrderByType.Asc),
            "created_desc" => query.OrderBy(n => n.CreatedAt, OrderByType.Desc),
            "created_asc" => query.OrderBy(n => n.CreatedAt, OrderByType.Asc),
            "title_asc" => query.OrderBy(n => n.Title, OrderByType.Asc),
            _ => query.OrderBy(n => n.UpdatedAt, OrderByType.Desc),
        };

        RefAsync<int> total = 0;
        var items = await query.ToPageListAsync(q.Page, q.PageSize, total, ct);
        return (items, total.Value);
    }
}

public sealed class MemoryRepository(ISqlSugarClient db, TaggingService tagging) : SqlSugarRepository<Memory>(db), IMemoryRepository
{
    public async Task<(List<Memory> Items, int Total)> QueryAsync(Guid userId, MemoryQuery q, CancellationToken ct)
    {
        var deleted = q.Deleted;
        Expression<Func<Memory, bool>> where = m => m.UserId == userId && m.IsDeleted == deleted;
        if (!q.Deleted)
        {
            if (q.ArchivedOnly) where = where.And(m => m.IsArchived);
            else if (!q.IncludeArchived) where = where.And(m => !m.IsArchived);
        }

        var term = TextNormalizer.Normalize(q.Search);
        if (term.Length > 0) where = where.And(m => m.SearchText.Contains(term));
        if (q.Mood is { } mood) where = where.And(m => m.Mood == mood);
        if (q.From is { } from) { var start = from.ToDateTime(); where = where.And(m => m.MemoryDate >= start); }
        if (q.To is { } to) { var end = to.ToDateTime(); where = where.And(m => m.MemoryDate <= end); }

        if (!string.IsNullOrWhiteSpace(q.Tag))
        {
            var ids = await tagging.MemoryIdsWithTagAsync(userId, q.Tag, ct);
            if (ids.Count == 0) return ([], 0);
            where = where.And(m => ids.Contains(m.Id));
        }
        if (!string.IsNullOrWhiteSpace(q.Person))
        {
            var ids = await tagging.MemoryIdsWithPersonAsync(userId, q.Person, ct);
            if (ids.Count == 0) return ([], 0);
            where = where.And(m => ids.Contains(m.Id));
        }

        var query = Db.Queryable<Memory>().Where(where);
        query = q.HasAudio switch
        {
            true => query.Where(m => SqlFunc.Subqueryable<AudioClip>().Where(a => a.OwnerType == OwnerType.Memory && a.OwnerId == m.Id).Any()),
            false => query.Where(m => SqlFunc.Subqueryable<AudioClip>().Where(a => a.OwnerType == OwnerType.Memory && a.OwnerId == m.Id).NotAny()),
            _ => query,
        };
        query = q.HasImages switch
        {
            true => query.Where(m => SqlFunc.Subqueryable<ImageAttachment>().Where(i => i.OwnerType == OwnerType.Memory && i.OwnerId == m.Id).Any()),
            false => query.Where(m => SqlFunc.Subqueryable<ImageAttachment>().Where(i => i.OwnerType == OwnerType.Memory && i.OwnerId == m.Id).NotAny()),
            _ => query,
        };

        query = query.OrderBy(m => m.IsPinned, OrderByType.Desc);
        query = q.Sort switch
        {
            "date_asc" => query.OrderBy(m => m.MemoryDate, OrderByType.Asc),
            "updated_desc" => query.OrderBy(m => m.UpdatedAt, OrderByType.Desc),
            "title_asc" => query.OrderBy(m => m.Title, OrderByType.Asc),
            _ => query.OrderBy(m => m.MemoryDate, OrderByType.Desc).OrderBy(m => m.CreatedAt, OrderByType.Desc),
        };

        RefAsync<int> total = 0;
        var items = await query.ToPageListAsync(q.Page, q.PageSize, total, ct);
        return (items, total.Value);
    }
}

public sealed class SqlSugarUnitOfWork(ISqlSugarClient db) : IUnitOfWork
{
    public async Task ExecuteInTransactionAsync(Func<Task> work, CancellationToken ct)
    {
        // Join an outer transaction instead of nesting one.
        if (db.Ado.Transaction is not null)
        {
            await work();
            return;
        }

        var result = await db.Ado.UseTranAsync(work);
        if (!result.IsSuccess)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(result.ErrorException).Throw();
    }
}
