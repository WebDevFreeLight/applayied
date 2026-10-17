using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Marketing.AiEngine;
using ZR.Repository;

namespace ZR.Repository.Marketing.AiEngine
{
    /// <summary>
    /// AI Audience Segment Repository - مستودع شرائح الجمهور الذكية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktAudienceSegmentRepository : BaseRepository<MktAudienceSegment>
    {
        /// <summary>
        /// Get active segments - الحصول على الشرائح النشطة
        /// </summary>
        public List<MktAudienceSegment> GetActiveSegments()
        {
            return Context.Queryable<MktAudienceSegment>()
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.MemberCount)
                .ToList();
        }

        /// <summary>
        /// Get segments by type - الحصول على الشرائح حسب النوع
        /// </summary>
        public List<MktAudienceSegment> GetBySegmentType(string segmentType)
        {
            return Context.Queryable<MktAudienceSegment>()
                .Where(s => s.SegmentType == segmentType)
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.AiQualityScore)
                .ToList();
        }

        /// <summary>
        /// Get segments needing refresh - الحصول على الشرائح التي تحتاج تحديث
        /// </summary>
        public List<MktAudienceSegment> GetSegmentsNeedingRefresh(int hoursThreshold = 24)
        {
            var threshold = DateTime.Now.AddHours(-hoursThreshold);
            return Context.Queryable<MktAudienceSegment>()
                .Where(s => s.IsActive && s.IsDynamic)
                .Where(s => s.LastRefreshedAt == null || s.LastRefreshedAt < threshold)
                .ToList();
        }
    }

    /// <summary>
    /// Segment Member Repository - مستودع أعضاء الشريحة
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktSegmentMemberRepository : BaseRepository<MktSegmentMember>
    {
        /// <summary>
        /// Get members by segment ID - الحصول على أعضاء الشريحة
        /// </summary>
        public List<MktSegmentMember> GetSegmentMembers(long segmentId)
        {
            return Context.Queryable<MktSegmentMember>()
                .Where(m => m.SegmentId == segmentId)
                .OrderByDescending(m => m.EngagementScore)
                .ToList();
        }

        /// <summary>
        /// Get member by segment and member ID - الحصول على عضو محدد في شريحة
        /// </summary>
        public MktSegmentMember GetMember(long segmentId, long memberId)
        {
            return Context.Queryable<MktSegmentMember>()
                .Where(m => m.SegmentId == segmentId && m.MemberId == memberId)
                .First();
        }

        /// <summary>
        /// Get high-value members - الحصول على الأعضاء ذوي القيمة العالية
        /// </summary>
        public List<MktSegmentMember> GetHighValueMembers(long segmentId, decimal minPurchaseProbability = 0.7m)
        {
            return Context.Queryable<MktSegmentMember>()
                .Where(m => m.SegmentId == segmentId)
                .Where(m => m.PurchaseProbability >= minPurchaseProbability)
                .OrderByDescending(m => m.PurchaseProbability)
                .ToList();
        }
    }

    /// <summary>
    /// Predictive Model Repository - مستودع النماذج التنبؤية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktPredictiveModelRepository : BaseRepository<MktPredictiveModel>
    {
        /// <summary>
        /// Get active models - الحصول على النماذج النشطة
        /// </summary>
        public List<MktPredictiveModel> GetActiveModels()
        {
            return Context.Queryable<MktPredictiveModel>()
                .Where(m => m.IsActive && m.Status == "active")
                .OrderByDescending(m => m.Accuracy)
                .ToList();
        }

        /// <summary>
        /// Get models by type - الحصول على النماذج حسب النوع
        /// </summary>
        public List<MktPredictiveModel> GetByModelType(string modelType)
        {
            return Context.Queryable<MktPredictiveModel>()
                .Where(m => m.ModelType == modelType)
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.Accuracy)
                .ToList();
        }

        /// <summary>
        /// Get models due for retraining - الحصول على النماذج التي تحتاج إعادة تدريب
        /// </summary>
        public List<MktPredictiveModel> GetModelsDueForTraining()
        {
            return Context.Queryable<MktPredictiveModel>()
                .Where(m => m.IsActive)
                .Where(m => m.NextTrainingAt != null && m.NextTrainingAt <= DateTime.Now)
                .ToList();
        }
    }

    /// <summary>
    /// Behavior Prediction Repository - مستودع التنبؤات السلوكية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktBehaviorPredictionRepository : BaseRepository<MktBehaviorPrediction>
    {
        /// <summary>
        /// Get active predictions - الحصول على التنبؤات النشطة
        /// </summary>
        public List<MktBehaviorPrediction> GetActivePredictions()
        {
            return Context.Queryable<MktBehaviorPrediction>()
                .Where(p => p.Status == "pending")
                .Where(p => p.TargetDate >= DateTime.Now)
                .OrderByDescending(p => p.ConfidenceLevel)
                .ToList();
        }

        /// <summary>
        /// Get predictions by target - الحصول على التنبؤات حسب الهدف
        /// </summary>
        public List<MktBehaviorPrediction> GetByTarget(string targetType, long targetId)
        {
            return Context.Queryable<MktBehaviorPrediction>()
                .Where(p => p.TargetType == targetType && p.TargetId == targetId)
                .OrderByDescending(p => p.PredictionDate)
                .ToList();
        }

        /// <summary>
        /// Get predictions by model - الحصول على التنبؤات حسب النموذج
        /// </summary>
        public List<MktBehaviorPrediction> GetByModel(long modelId, DateTime? from, DateTime? to)
        {
            return Context.Queryable<MktBehaviorPrediction>()
                .Where(p => p.ModelId == modelId)
                .WhereIF(from.HasValue, p => p.PredictionDate >= from.Value)
                .WhereIF(to.HasValue, p => p.PredictionDate <= to.Value)
                .OrderByDescending(p => p.PredictionDate)
                .ToList();
        }
    }

    /// <summary>
    /// AI Agent Repository - مستودع وكلاء الذكاء الاصطناعي
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktAiAgentRepository : BaseRepository<MktAiAgent>
    {
        /// <summary>
        /// Get active agents - الحصول على الوكلاء النشطين
        /// </summary>
        public List<MktAiAgent> GetActiveAgents()
        {
            return Context.Queryable<MktAiAgent>()
                .Where(a => a.IsActive && a.Status == "active")
                .OrderBy(a => a.AgentRole)
                .ToList();
        }

        /// <summary>
        /// Get agents by role - الحصول على الوكلاء حسب الدور
        /// </summary>
        public List<MktAiAgent> GetByRole(string agentRole)
        {
            return Context.Queryable<MktAiAgent>()
                .Where(a => a.AgentRole == agentRole)
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.SuccessRate)
                .ToList();
        }
    }

    /// <summary>
    /// AI Agent Task Repository - مستودع مهام وكيل الذكاء الاصطناعي
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktAiAgentTaskRepository : BaseRepository<MktAiAgentTask>
    {
        /// <summary>
        /// Get tasks by agent and status - الحصول على مهام الوكيل حسب الحالة
        /// </summary>
        public List<MktAiAgentTask> GetAgentTasks(long agentId, string status)
        {
            var query = Context.Queryable<MktAiAgentTask>()
                .Where(t => t.AgentId == agentId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }

            return query.OrderByDescending(t => t.CreatedAt).ToList();
        }

        /// <summary>
        /// Get queued tasks - الحصول على المهام في الانتظار
        /// </summary>
        public List<MktAiAgentTask> GetQueuedTasks()
        {
            return Context.Queryable<MktAiAgentTask>()
                .Where(t => t.Status == "queued")
                .OrderBy(t => t.CreatedAt)
                .ToList();
        }

        /// <summary>
        /// Get running tasks - الحصول على المهام الجارية
        /// </summary>
        public List<MktAiAgentTask> GetRunningTasks()
        {
            return Context.Queryable<MktAiAgentTask>()
                .Where(t => t.Status == "running")
                .OrderBy(t => t.StartedAt)
                .ToList();
        }
    }

    /// <summary>
    /// Competitor Analysis Repository - مستودع تحليل المنافسين
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktCompetitorAnalysisRepository : BaseRepository<MktCompetitorAnalysis>
    {
        /// <summary>
        /// Get active competitor analyses - الحصول على تحليلات المنافسين النشطة
        /// </summary>
        public List<MktCompetitorAnalysis> GetActiveAnalyses()
        {
            return Context.Queryable<MktCompetitorAnalysis>()
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.OverallScore)
                .ToList();
        }

        /// <summary>
        /// Get analyses by threat level - الحصول على التحليلات حسب مستوى التهديد
        /// </summary>
        public List<MktCompetitorAnalysis> GetByThreatLevel(string threatLevel)
        {
            return Context.Queryable<MktCompetitorAnalysis>()
                .Where(c => c.ThreatLevel == threatLevel)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.OverallScore)
                .ToList();
        }

        /// <summary>
        /// Get analyses needing refresh - الحصول على التحليلات التي تحتاج تحديث
        /// </summary>
        public List<MktCompetitorAnalysis> GetAnalysesNeedingRefresh()
        {
            return Context.Queryable<MktCompetitorAnalysis>()
                .Where(c => c.IsActive)
                .Where(c => c.LastAnalyzedAt == null ||
                    (c.AnalysisFrequency == "daily" && c.LastAnalyzedAt < DateTime.Now.AddDays(-1)) ||
                    (c.AnalysisFrequency == "weekly" && c.LastAnalyzedAt < DateTime.Now.AddDays(-7)) ||
                    (c.AnalysisFrequency == "monthly" && c.LastAnalyzedAt < DateTime.Now.AddDays(-30)))
                .ToList();
        }
    }

    /// <summary>
    /// Budget Optimization Repository - مستودع تحسين الميزانية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktBudgetOptimizationRepository : BaseRepository<MktBudgetOptimization>
    {
        /// <summary>
        /// Get pending optimizations - الحصول على التحسينات المعلقة
        /// </summary>
        public List<MktBudgetOptimization> GetPendingOptimizations()
        {
            return Context.Queryable<MktBudgetOptimization>()
                .Where(o => o.Status == "pending")
                .OrderByDescending(o => o.Confidence)
                .ToList();
        }

        /// <summary>
        /// Get optimizations by campaign - الحصول على التحسينات حسب الحملة
        /// </summary>
        public List<MktBudgetOptimization> GetByCampaignId(long campaignId)
        {
            return Context.Queryable<MktBudgetOptimization>()
                .Where(o => o.CampaignId == campaignId)
                .OrderByDescending(o => o.OptimizationDate)
                .ToList();
        }

        /// <summary>
        /// Get optimizations by scope and status - الحصول على التحسينات حسب النطاق والحالة
        /// </summary>
        public List<MktBudgetOptimization> GetByScope(string scope, string status = null)
        {
            var query = Context.Queryable<MktBudgetOptimization>()
                .Where(o => o.Scope == scope);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            return query.OrderByDescending(o => o.OptimizationDate).ToList();
        }
    }
}
