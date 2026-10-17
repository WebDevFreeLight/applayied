using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;

namespace ZR.Service.Boulevard.IBoulevardService
{
    /// <summary>
    /// CustomerRegistrationFormservice接口
    /// </summary>
    public interface IBlvCustomerregistrationformService : IBaseService<BlvCustomerregistrationform>
    {
        PagedInfo<BlvCustomerregistrationformDto> GetList(BlvCustomerregistrationformQueryDto parm);

        BlvCustomerregistrationform GetInfo(long Id);


        BlvCustomerregistrationform AddBlvCustomerregistrationform(BlvCustomerregistrationform parm);
        int UpdateBlvCustomerregistrationform(BlvCustomerregistrationform parm);
        
        bool TruncateBlvCustomerregistrationform();

        (string, object, object) ImportBlvCustomerregistrationform(List<BlvCustomerregistrationform> list);

        PagedInfo<BlvCustomerregistrationformDto> ExportList(BlvCustomerregistrationformQueryDto parm);
    }
}

