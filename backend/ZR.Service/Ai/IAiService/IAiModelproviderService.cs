using ZR.Model.Ai.Dto;
using ZR.Model.Ai;

namespace ZR.Service.Ai.IAiService
{
    /// <summary>
    /// Model Provider Tableservice接口
    /// </summary>
    public interface IAiModelproviderService : IBaseService<AiModelprovider>
    {
        PagedInfo<AiModelproviderDto> GetList(AiModelproviderQueryDto parm);

        AiModelprovider GetInfo(long Id);


        AiModelprovider AddAiModelprovider(AiModelprovider parm);
        int UpdateAiModelprovider(AiModelprovider parm);
        
        bool TruncateAiModelprovider();

        (string, object, object) ImportAiModelprovider(List<AiModelprovider> list);

        PagedInfo<AiModelproviderDto> ExportList(AiModelproviderQueryDto parm);
    }
}

