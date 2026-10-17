using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Marketing.ContentMgmt;
using ZR.Model.Marketing.Dto;

namespace ZR.Service.Marketing.IMarketingService
{
    /// <summary>
    /// Content Management Service Interface - خدمة إدارة المحتوى
    /// </summary>
    public interface IContentManagementService : IBaseService<MktContent>
    {
        PagedInfo<ContentOutputDto> GetContent(ContentQueryDto query);
        ContentOutputDto GetContentDetail(long contentId);
        long AddContent(ContentDto dto);
        int UpdateContent(ContentDto dto);
        int DeleteContent(long[] ids);
        int UpdateContentStatus(long contentId, string status);
        int PublishContent(long contentId);
        PagedInfo<AiContentGenerationDto> GetAiGenerations(AiContentGenerationQueryDto query);
        AiContentGenerationDto GenerateAiContent(GenerateContentRequest request);
        int ApproveAiContent(ApproveContentRequest dto);
        ContentAnalyticsDto GetContentAnalytics(DateTime? dateFrom, DateTime? dateTo);
    }
}
