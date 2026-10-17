using Infrastructure;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Repository;

namespace ZR.ServiceCore
{
    /// <summary>
    /// Base service definition
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseService<T> : BaseRepository<T> where T : class, new()
    {





        //protected readonly IPdfRenderer _pdfRenderer;
        //protected IPdfRenderer _pdfRenderer => App.PdfRenderer;

        //public BaseService()
        //{
        //    try
        //    {
        //        _pdfRenderer = App.PdfRenderer;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception or handle initialization failure
        //        throw new InvalidOperationException($"Failed to initialize PDF renderer in {typeof(T).Name} service", ex);
        //    }
        //}

        // Example method using the PDF renderer
        //public async Task<byte[]> GeneratePdfReport(string htmlContent)
        //{
        //    try
        //    {
        //        if (_pdfRenderer == null)
        //        {
        //            throw new InvalidOperationException("PDF renderer is not available");
        //        }

        //        return  await _pdfRenderer.GetPdfByHtmlAsync(htmlContent);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle PDF generation errors
        //        throw new InvalidOperationException("Failed to generate PDF report", ex);
        //    }
        //}

        //// Another example method for generating PDF from data
        //public async Task<byte[]> GeneratePdfFromData(T data)
        //{
        //    try
        //    {
        //        if (_pdfRenderer == null)
        //        {
        //            throw new InvalidOperationException("PDF renderer is not available");
        //        }

        //        // Convert data to HTML or PDF format
        //        string htmlContent = ConvertToHtml(data);
        //        return await _pdfRenderer.GetPdfByHtmlAsync(htmlContent);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new InvalidOperationException($"Failed to generate PDF from {typeof(T).Name} data", ex);
        //    }
        //}

        private string ConvertToHtml(T data)
        {
            // Implement conversion logic here
            // This could use reflection, templates, or other methods
            return $"<html><body><h1>Report for {typeof(T).Name}</h1></body></html>";
        }



        //protected ISqlSugarClient Context2 { get; }

        //protected BaseService(ISqlSugarClient context)
        //{
        //    Context2 = context;
        //}

        //public BaseService()
        //{
        //    Context2 = this.Context;

        //}

        /// <summary>
        /// Get entity by ID
        /// </summary>
        public virtual async Task<T> GetByIdAsync(object id)
        {
            return await Context.Queryable<T>().InSingleAsync(id);
        }

        /// <summary>
        /// Get all entities
        /// </summary>
        public virtual async Task<List<T>> GetAllAsync()
        {
            return await Context.Queryable<T>().ToListAsync();
        }

        /// <summary>
        /// Get entities by condition
        /// </summary>
        public virtual async Task<List<T>> GetListAsync(Expression<Func<T, bool>> whereExpression)
        {
            return await Context.Queryable<T>().Where(whereExpression).ToListAsync();
        }

        /// <summary>
        /// Get paged list
        /// </summary>
        public virtual async Task<PagedInfo<T>> GetPagedListAsync(Expression<Func<T, bool>> whereExpression, int pageNum, int pageSize)
        {
            RefAsync<int> totalCount = 0;
            var list = await Context.Queryable<T>()
                .Where(whereExpression)
                .ToPageListAsync(pageNum, pageSize, totalCount);

            return new PagedInfo<T>()
            {
                PageIndex = pageNum,
                PageSize = pageSize,
                TotalNum = totalCount,
                Result = list
            };
        }

        /// <summary>
        /// Insert entity
        /// </summary>
        public virtual async Task<long> InsertAsync(T entity)
        {
            return await Context.Insertable(entity).ExecuteReturnBigIdentityAsync();
        }

        /// <summary>
        /// Insert entities
        /// </summary>
        public virtual async Task<int> InsertRangeAsync(List<T> entities)
        {
            return await Context.Insertable(entities).ExecuteCommandAsync();
        }

        /// <summary>
        /// Update entity
        /// </summary>
        public virtual async Task<bool> UpdateAsync(T entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Update entities
        /// </summary>
        public virtual async Task<int> UpdateRangeAsync(List<T> entities)
        {
            return await Context.Updateable(entities).ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete entity
        /// </summary>
        public virtual async Task<bool> DeleteAsync(T entity)
        {
            return await Context.Deleteable(entity).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Delete by ID
        /// </summary>
        public virtual async Task<bool> DeleteByIdAsync(object id)
        {
            return await Context.Deleteable<T>().In(id).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Delete by IDs
        /// </summary>
        public virtual async Task<bool> DeleteByIdsAsync(object[] ids)
        {
            return await Context.Deleteable<T>().In(ids).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Check if exists
        /// </summary>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> whereExpression)
        {
            return await Context.Queryable<T>().Where(whereExpression).AnyAsync();
        }

        /// <summary>
        /// Get count
        /// </summary>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> whereExpression = null)
        {
            return whereExpression == null
                ? await Context.Queryable<T>().CountAsync()
                : await Context.Queryable<T>().Where(whereExpression).CountAsync();
        }


    }
}

