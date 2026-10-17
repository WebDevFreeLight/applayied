using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Customers.DataMine;
using ZR.Model.Customers.DataMine.Dto;

namespace ZR.Service.IService.Customers.DataMine
{
    public interface IDmContactService
    {
        Task<PagedInfo<DmContact>> GetListAsync(DmContactQueryDto query);
        Task<DmContact> GetByIdAsync(long contactId);
        Task<DmContactImportPreviewDto> PreviewExcelImportAsync(string filePath);
        Task<DmContactImportResultDto> ImportFromExcelAsync(string filePath, DmContactImportRequestDto request);
        Task<PullToLeadsResultDto> PullToLeadsAsync(PullToLeadsRequestDto request, long currentUserId);
        Task<int> DeleteAsync(long[] contactIds);
        Task<object> GetStatisticsAsync();
    }
}
