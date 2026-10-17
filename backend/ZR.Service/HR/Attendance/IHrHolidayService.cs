using ZR.Model.Hr.Dto;
using ZR.Model.HR.Attendance.Configuration;

namespace ZR.Service.Hr.IHrService
{
    /// <summary>
    /// Holiday Tableservice接口
    /// </summary>
    public interface IHrHolidayService : IBaseService<HrHoliday>
    {
        PagedInfo<HrHolidayDto> GetList(HrHolidayQueryDto parm);

        HrHoliday GetInfo(long Holidayid);


        HrHoliday AddHrHoliday(HrHoliday parm);
        int UpdateHrHoliday(HrHoliday parm);
        
        bool TruncateHrHoliday();

        (string, object, object) ImportHrHoliday(List<HrHoliday> list);

        PagedInfo<HrHolidayDto> ExportList(HrHolidayQueryDto parm);
    }
}

