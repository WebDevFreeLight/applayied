using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Customers.DataMine;

namespace ZR.Service.IService.Customers.DataMine
{
    public interface IDmCategoryService
    {
        Task<List<DmCategory>> GetAllAsync();
        Task<DmCategory> AddAsync(string name, string nameAr);
    }
}
