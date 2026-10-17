using ZR.Model.HR.Hrtypes.Dto;
using ZR.Model.HR.Hrtypes;

namespace ZR.Service.HR.Hrtypes.IHrtypesService
{
    /// <summary>
    /// HolidayType Tableservice接口
    /// </summary>
    public interface IHrHolidaytypeService : IBaseService<HrHolidaytype>
    {
        PagedInfo<HrHolidaytypeDto> GetList(HrHolidaytypeQueryDto parm);

        HrHolidaytype GetInfo(long Id);


        HrHolidaytype AddHrHolidaytype(HrHolidaytype parm);
        int UpdateHrHolidaytype(HrHolidaytype parm);
        
        bool TruncateHrHolidaytype();

        (string, object, object) ImportHrHolidaytype(List<HrHolidaytype> list);

        PagedInfo<HrHolidaytypeDto> ExportList(HrHolidaytypeQueryDto parm);
    }
}

