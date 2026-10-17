using ZR.Model.Ai.Dto;
using ZR.Model.Ai;

namespace ZR.Service.Ai.IAiService
{
    /// <summary>
    /// Agent Mode Tableservice接口
    /// </summary>
    public interface IAiAgentModeService : IBaseService<AiAgentMode>
    {
        PagedInfo<AiAgentModeDto> GetList(AiAgentModeQueryDto parm);

        AiAgentMode GetInfo(long Id);


        AiAgentMode AddAiAgentMode(AiAgentMode parm);
        int UpdateAiAgentMode(AiAgentMode parm);
        
        bool TruncateAiAgentMode();

        (string, object, object) ImportAiAgentMode(List<AiAgentMode> list);

        PagedInfo<AiAgentModeDto> ExportList(AiAgentModeQueryDto parm);
    }
}

