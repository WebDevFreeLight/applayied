using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Marketing.PublicRelations;
using ZR.Model.Marketing.Dto;

namespace ZR.Service.Marketing.IMarketingService
{
    /// <summary>
    /// Public Relations Service Interface - خدمة العلاقات العامة
    /// </summary>
    public interface IPublicRelationsService : IBaseService<MktPressRelease>
    {
        #region Press Releases

        PagedInfo<PressReleaseOutputDto> GetPressReleases(PressReleaseQueryDto query);
        PressReleaseOutputDto GetPressReleaseDetail(long releaseId);
        long AddPressRelease(PressReleaseDto dto);
        int UpdatePressRelease(PressReleaseDto dto);
        int DeletePressReleases(long[] ids);
        int DistributePressRelease(long releaseId);
        int ApprovePressRelease(long releaseId);

        #endregion

        #region Media Contacts

        PagedInfo<MediaContactDto> GetMediaContacts(MediaContactQueryDto query);
        MediaContactDto GetMediaContactDetail(long contactId);
        long AddMediaContact(MediaContactDto dto);
        int UpdateMediaContact(MediaContactDto dto);
        int DeleteMediaContacts(long[] ids);

        #endregion

        #region Media Mentions

        PagedInfo<MediaMentionDto> GetMentions(MediaMentionQueryDto query);
        MediaMentionDto GetMentionDetail(long mentionId);
        long AddMention(MediaMentionDto dto);
        int UpdateMention(MediaMentionDto dto);
        int DeleteMentions(long[] ids);

        #endregion

        #region Sentiment & Analytics

        List<PrSentimentDto> GetSentiment(PrSentimentQueryDto query);
        PrAnalyticsDto GetPrAnalytics(DateTime? dateFrom, DateTime? dateTo);

        #endregion
    }
}
