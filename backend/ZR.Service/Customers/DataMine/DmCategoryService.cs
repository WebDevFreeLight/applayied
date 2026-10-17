using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Customers.DataMine;
using ZR.Service.IService.Customers.DataMine;

namespace ZR.Service.Customers.DataMine
{
    [AppService(ServiceType = typeof(IDmCategoryService), ServiceLifetime = LifeTime.Transient)]
    public class DmCategoryService : BaseService<DmCategory>, IDmCategoryService
    {
        public async Task<List<DmCategory>> GetAllAsync()
        {
            return await Context.Queryable<DmCategory>()
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
        }

        public async Task<DmCategory> AddAsync(string name, string nameAr)
        {
            var maxSort = await Context.Queryable<DmCategory>()
                .MaxAsync(c => c.SortOrder);

            var category = new DmCategory
            {
                CategoryId = SnowFlakeSingle.Instance.NextId(),
                Name = name,
                NameAr = nameAr,
                IsActive = true,
                SortOrder = maxSort + 1,
                Create_time = DateTime.Now
            };

            Add(category);
            return category;
        }
    }
}
