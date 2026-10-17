using ZR.Model.AI.Dto;
using ZR.Model.AI;

namespace ZR.Service.AI.IAIService
{
    /// <summary>
    /// Team Mode Tableservice接口
    /// </summary>
    public interface IAiTeamModeService : IBaseService<AiTeamMode>
    {
        PagedInfo<AiTeamModeDto> GetList(AiTeamModeQueryDto parm);

        AiTeamMode GetInfo(long Id);


        AiTeamMode AddAiTeamMode(AiTeamMode parm);
        int UpdateAiTeamMode(AiTeamMode parm);
        
        bool TruncateAiTeamMode();

        (string, object, object) ImportAiTeamMode(List<AiTeamMode> list);

        PagedInfo<AiTeamModeDto> ExportList(AiTeamModeQueryDto parm);
    }
}

