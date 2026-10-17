using System;
using System.Collections.Generic;
using ZR.Model.CustomerAssets;
using ZR.Model.CustomerAssets.Dto;

namespace ZR.Service.IService.CustomerAssets
{
    public interface ICustomerContractFileService : IBaseService<CustomerContractFile>
    {
        List<ContractFileListDto> GetByAssignment(Guid assignmentId);
        CustomerContractFile Create(CreateContractFileDto dto, string userName);
        int Delete(Guid id);
    }
}
