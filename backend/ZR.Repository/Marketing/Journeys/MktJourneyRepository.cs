using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Marketing.Journeys;
using ZR.Repository;

namespace ZR.Repository.Marketing.Journeys
{
    /// <summary>
    /// Customer Journey Repository - مستودع رحلة العميل التسويقية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktCustomerJourneyRepository : BaseRepository<MktCustomerJourney>
    {
        #region Journey Queries

        /// <summary>
        /// Get journey with all stages - الحصول على الرحلة مع المراحل
        /// </summary>
        public MktCustomerJourney GetJourneyWithStages(long journeyId)
        {
            var journey = Context.Queryable<MktCustomerJourney>()
                .Where(j => j.JourneyId == journeyId)
                .First();

            if (journey != null)
            {
                journey.Stages = Context.Queryable<MktJourneyStage>()
                    .Where(s => s.JourneyId == journeyId)
                    .OrderBy(s => s.SortOrder)
                    .ToList();
            }

            return journey;
        }

        /// <summary>
        /// Get active journeys - الحصول على الرحلات النشطة
        /// </summary>
        public List<MktCustomerJourney> GetActiveJourneys()
        {
            return Context.Queryable<MktCustomerJourney>()
                .Where(j => j.Status == "active")
                .OrderByDescending(j => j.CurrentlyActive)
                .ToList();
        }

        /// <summary>
        /// Get journeys by type - الحصول على الرحلات حسب النوع
        /// </summary>
        public List<MktCustomerJourney> GetByJourneyType(string journeyType)
        {
            return Context.Queryable<MktCustomerJourney>()
                .Where(j => j.JourneyType == journeyType)
                .OrderByDescending(j => j.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get journey analytics summary - ملخص تحليلات الرحلات
        /// </summary>
        public List<MktCustomerJourney> GetJourneyAnalytics()
        {
            var journeys = Context.Queryable<MktCustomerJourney>()
                .Where(j => j.Status == "active" || j.Status == "completed")
                .OrderByDescending(j => j.TotalEnrolled)
                .ToList();

            if (journeys.Any())
            {
                var journeyIds = journeys.Select(j => j.JourneyId).ToList();
                var stages = Context.Queryable<MktJourneyStage>()
                    .Where(s => journeyIds.Contains(s.JourneyId))
                    .OrderBy(s => s.SortOrder)
                    .ToList();

                foreach (var journey in journeys)
                {
                    journey.Stages = stages.Where(s => s.JourneyId == journey.JourneyId).ToList();
                }
            }

            return journeys;
        }

        #endregion
    }

    /// <summary>
    /// Journey Stage Repository - مستودع مراحل رحلة العميل
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktJourneyStageRepository : BaseRepository<MktJourneyStage>
    {
        /// <summary>
        /// Get stages by journey - الحصول على مراحل الرحلة
        /// </summary>
        public List<MktJourneyStage> GetByJourneyId(long journeyId)
        {
            return Context.Queryable<MktJourneyStage>()
                .Where(s => s.JourneyId == journeyId)
                .OrderBy(s => s.SortOrder)
                .ToList();
        }

        /// <summary>
        /// Get active stages - الحصول على المراحل النشطة
        /// </summary>
        public List<MktJourneyStage> GetActiveStages(long journeyId)
        {
            return Context.Queryable<MktJourneyStage>()
                .Where(s => s.JourneyId == journeyId)
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ToList();
        }

        /// <summary>
        /// Get next stage in sequence - الحصول على المرحلة التالية
        /// </summary>
        public MktJourneyStage GetNextStage(long journeyId, int currentSortOrder)
        {
            return Context.Queryable<MktJourneyStage>()
                .Where(s => s.JourneyId == journeyId)
                .Where(s => s.SortOrder > currentSortOrder)
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .First();
        }
    }

    /// <summary>
    /// Journey Trigger Repository - مستودع محفزات رحلة العميل
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktJourneyTriggerRepository : BaseRepository<MktJourneyTrigger>
    {
        /// <summary>
        /// Get triggers by journey - الحصول على محفزات الرحلة
        /// </summary>
        public List<MktJourneyTrigger> GetByJourneyId(long journeyId)
        {
            return Context.Queryable<MktJourneyTrigger>()
                .Where(t => t.JourneyId == journeyId)
                .Where(t => t.IsActive)
                .OrderBy(t => t.Priority)
                .ToList();
        }

        /// <summary>
        /// Get triggers by event type - الحصول على المحفزات حسب نوع الحدث
        /// </summary>
        public List<MktJourneyTrigger> GetByEventType(string eventType)
        {
            return Context.Queryable<MktJourneyTrigger>()
                .Where(t => t.EventType == eventType)
                .Where(t => t.IsActive)
                .OrderBy(t => t.Priority)
                .ToList();
        }
    }

    /// <summary>
    /// Journey Execution Repository - مستودع تنفيذ رحلة العميل
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktJourneyExecutionRepository : BaseRepository<MktJourneyExecution>
    {
        /// <summary>
        /// Get member execution in a journey - الحصول على تنفيذ العضو في الرحلة
        /// </summary>
        public MktJourneyExecution GetMemberExecution(long journeyId, long memberId)
        {
            return Context.Queryable<MktJourneyExecution>()
                .Where(e => e.JourneyId == journeyId && e.MemberId == memberId)
                .OrderByDescending(e => e.EnrolledAt)
                .First();
        }

        /// <summary>
        /// Get active executions for a journey - الحصول على التنفيذات النشطة للرحلة
        /// </summary>
        public List<MktJourneyExecution> GetActiveExecutions(long journeyId)
        {
            return Context.Queryable<MktJourneyExecution>()
                .Where(e => e.JourneyId == journeyId)
                .Where(e => e.Status == "active")
                .OrderByDescending(e => e.EnrolledAt)
                .ToList();
        }

        /// <summary>
        /// Get executions at a specific stage - الحصول على التنفيذات في مرحلة محددة
        /// </summary>
        public List<MktJourneyExecution> GetByStage(long journeyId, long stageId)
        {
            return Context.Queryable<MktJourneyExecution>()
                .Where(e => e.JourneyId == journeyId)
                .Where(e => e.CurrentStageId == stageId)
                .Where(e => e.Status == "active")
                .ToList();
        }

        /// <summary>
        /// Get converted executions - الحصول على التنفيذات المحولة
        /// </summary>
        public List<MktJourneyExecution> GetConvertedExecutions(long journeyId)
        {
            return Context.Queryable<MktJourneyExecution>()
                .Where(e => e.JourneyId == journeyId)
                .Where(e => e.IsConverted)
                .OrderByDescending(e => e.ConvertedAt)
                .ToList();
        }
    }

    /// <summary>
    /// Dynamic Content Repository - مستودع المحتوى الديناميكي
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktDynamicContentRepository : BaseRepository<MktDynamicContent>
    {
        /// <summary>
        /// Get active dynamic content - الحصول على المحتوى الديناميكي النشط
        /// </summary>
        public List<MktDynamicContent> GetActiveContent()
        {
            return Context.Queryable<MktDynamicContent>()
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.TimesServed)
                .ToList();
        }

        /// <summary>
        /// Get content by type - الحصول على المحتوى حسب النوع
        /// </summary>
        public List<MktDynamicContent> GetByContentType(string contentType)
        {
            return Context.Queryable<MktDynamicContent>()
                .Where(c => c.ContentType == contentType)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.EngagementRate)
                .ToList();
        }
    }
}
