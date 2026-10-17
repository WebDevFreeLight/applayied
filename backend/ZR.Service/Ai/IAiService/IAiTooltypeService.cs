using ZR.Model.Ai.Dto;
using ZR.Model.Ai;

namespace ZR.Service.Ai.IAiService
{
    /// <summary>
    /// Tool Type Tableservice接口
    /// </summary>
    public interface IAiTooltypeService : IBaseService<AiTooltype>
    {
        PagedInfo<AiTooltypeDto> GetList(AiTooltypeQueryDto parm);

        AiTooltype GetInfo(long Id);


        AiTooltype AddAiTooltype(AiTooltype parm);
        int UpdateAiTooltype(AiTooltype parm);
        
        bool TruncateAiTooltype();

        (string, object, object) ImportAiTooltype(List<AiTooltype> list);

        PagedInfo<AiTooltypeDto> ExportList(AiTooltypeQueryDto parm);
    }
}

