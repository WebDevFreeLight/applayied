using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using ZR.Model.CustomerAssets;
using ZR.Model.CustomerAssets.Dto;
using ZR.Service.IService.CustomerAssets;

namespace ZR.Service.CustomerAssets
{
    [AppService(ServiceType = typeof(ICustomerContractFileService), ServiceLifetime = LifeTime.Transient)]
    public class CustomerContractFileService : BaseService<CustomerContractFile>, ICustomerContractFileService
    {
        public List<ContractFileListDto> GetByAssignment(Guid assignmentId)
        {
            return Queryable()
                .Where(x => x.AssignmentId == assignmentId)
                .OrderByDescending(x => x.Create_time)
                .Select(x => new ContractFileListDto
                {
                    Id = x.Id,
                    AssignmentId = x.AssignmentId,
                    FileType = x.FileType,
                    FileName = x.FileName,
                    FileUrl = x.FileUrl,
                    FileSize = x.FileSize,
                    UploadedBy = x.UploadedBy,
                    Notes = x.Notes,
                    Create_time = x.Create_time
                }).ToList();
        }

        public CustomerContractFile Create(CreateContractFileDto dto, string userName)
        {
            var model = new CustomerContractFile
            {
                Id = Guid.NewGuid(),
                AssignmentId = dto.AssignmentId,
                FileType = dto.FileType,
                FileName = dto.FileName,
                FileUrl = dto.FileUrl,
                FileSize = dto.FileSize,
                UploadedBy = userName,
                Notes = dto.Notes,
                Create_by = userName
            };
            Insert(model);
            return model;
        }

        public int Delete(Guid id)
        {
            return base.Delete(id);
        }
    }
}
