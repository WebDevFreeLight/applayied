using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.Journeys
{
    /// <summary>
    /// Customer Journey - رحلة العميل التسويقية
    /// Hyper-personalized automated marketing journey with dynamic content
    /// </summary>
    [SugarTable("mkt_customer_journey")]
    public class MktCustomerJourney : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long JourneyId { get; set; }

        [SugarColumn(Length = 100)]
        public string JourneyCode { get; set; }

        [SugarColumn(Length = 300)]
        public string JourneyName { get; set; }

        [SugarColumn(Length = 300, IsNullable = true)]
        public string JourneyNameAr { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string Description { get; set; }

        /// <summary>
        /// Type: onboarding, nurture, conversion, retention, re_engagement, event_lifecycle, vip_experience, sponsor_onboarding
        /// </summary>
        [SugarColumn(Length = 50)]
        public string JourneyType { get; set; }

        /// <summary>
        /// Entry trigger: registration, ticket_purchase, event_checkin, form_submission,
        ///                 segment_entry, cart_abandonment, inactivity, api_trigger
        /// </summary>
        [SugarColumn(Length = 50)]
        public string EntryTrigger { get; set; }

        /// <summary>
        /// JSON: entry conditions that must be met
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string EntryConditionsJson { get; set; }

        /// <summary>
        /// JSON: exit conditions (journey completion criteria)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ExitConditionsJson { get; set; }

        /// <summary>
        /// Target audience segment ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? TargetSegmentId { get; set; }

        /// <summary>
        /// Status: draft, active, paused, completed, archived
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "draft";

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int TotalEnrolled { get; set; } = 0;
        public int TotalCompleted { get; set; } = 0;
        public int TotalDropped { get; set; } = 0;
        public int CurrentlyActive { get; set; } = 0;

        /// <summary>
        /// Overall conversion rate (0-100)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? ConversionRate { get; set; }

        /// <summary>
        /// Average journey duration in hours
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? AvgDurationHours { get; set; }

        /// <summary>
        /// AI personalization enabled
        /// </summary>
        public bool AiPersonalizationEnabled { get; set; } = true;

        /// <summary>
        /// AI can modify content dynamically
        /// </summary>
        public bool DynamicContentEnabled { get; set; } = true;

        /// <summary>
        /// JSON: journey visual flow definition (nodes and edges)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string FlowDefinitionJson { get; set; }

        /// <summary>
        /// Priority: low, medium, high
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Priority { get; set; } = "medium";

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }

        // Navigation
        [SugarColumn(IsIgnore = true)]
        public List<MktJourneyStage> Stages { get; set; }

        // Computed
        [SugarColumn(IsIgnore = true)]
        public decimal CompletionRate => TotalEnrolled > 0 ? ((decimal)TotalCompleted / TotalEnrolled) * 100 : 0;

        [SugarColumn(IsIgnore = true)]
        public decimal DropRate => TotalEnrolled > 0 ? ((decimal)TotalDropped / TotalEnrolled) * 100 : 0;
    }

    /// <summary>
    /// Journey Stage - مراحل رحلة العميل
    /// Individual steps within a customer journey
    /// </summary>
    [SugarTable("mkt_journey_stage")]
    public class MktJourneyStage : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long StageId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long JourneyId { get; set; }

        [SugarColumn(Length = 200)]
        public string StageName { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string StageNameAr { get; set; }

        /// <summary>
        /// Type: email, sms, whatsapp, push_notification, wait, condition, action,
        ///       ai_decision, webhook, internal_task, dynamic_content
        /// </summary>
        [SugarColumn(Length = 50)]
        public string StageType { get; set; }

        /// <summary>
        /// Order in journey flow
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// JSON: stage configuration (template, content, timing)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string ConfigJson { get; set; }

        /// <summary>
        /// Wait duration before executing (in minutes)
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? WaitMinutes { get; set; }

        /// <summary>
        /// Wait until specific time of day (HH:mm)
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = true)]
        public string WaitUntilTime { get; set; }

        /// <summary>
        /// JSON: condition for branching (if stage type is "condition")
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ConditionJson { get; set; }

        /// <summary>
        /// Next stage if condition is true
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? NextStageIfTrue { get; set; }

        /// <summary>
        /// Next stage if condition is false
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? NextStageIfFalse { get; set; }

        /// <summary>
        /// Default next stage (for non-conditional stages)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? NextStageId { get; set; }

        /// <summary>
        /// JSON: dynamic content variants based on user profile/behavior
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string DynamicContentJson { get; set; }

        /// <summary>
        /// JSON: A/B test configuration for this stage
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string AbTestJson { get; set; }

        public int TotalProcessed { get; set; } = 0;
        public int TotalSuccess { get; set; } = 0;
        public int TotalFailed { get; set; } = 0;

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? EngagementRate { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Journey Trigger - محفزات رحلة العميل
    /// Event-based triggers that can advance users through journeys
    /// </summary>
    [SugarTable("mkt_journey_trigger")]
    public class MktJourneyTrigger : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TriggerId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long JourneyId { get; set; }

        [SugarColumn(Length = 200)]
        public string TriggerName { get; set; }

        /// <summary>
        /// Event: page_view, button_click, form_submit, purchase, check_in, check_out,
        ///        zone_enter, zone_exit, app_open, push_interact, email_open, email_click,
        ///        feedback_submit, social_share, qr_scan, beacon_proximity
        /// </summary>
        [SugarColumn(Length = 50)]
        public string EventType { get; set; }

        /// <summary>
        /// JSON: conditions that must be met for trigger to fire
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ConditionsJson { get; set; }

        /// <summary>
        /// Action: advance_stage, skip_stage, exit_journey, send_notification, update_score, tag_user
        /// </summary>
        [SugarColumn(Length = 50)]
        public string ActionType { get; set; }

        /// <summary>
        /// JSON: action parameters
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ActionParamsJson { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? TargetStageId { get; set; }

        public int Priority { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public int TimesTriggered { get; set; } = 0;
    }

    /// <summary>
    /// Journey Execution - تنفيذ رحلة العميل
    /// Tracks individual user's progress through a journey
    /// </summary>
    [SugarTable("mkt_journey_execution")]
    public class MktJourneyExecution
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ExecutionId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long JourneyId { get; set; }

        /// <summary>
        /// Member type: attendee, exhibitor, sponsor, investor, merchant
        /// </summary>
        [SugarColumn(Length = 30)]
        public string MemberType { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CurrentStageId { get; set; }

        /// <summary>
        /// Status: active, completed, dropped, paused, error
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "active";

        public DateTime EnrolledAt { get; set; } = DateTime.Now;

        [SugarColumn(IsNullable = true)]
        public DateTime? CompletedAt { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? LastActivityAt { get; set; }

        /// <summary>
        /// Number of stages completed
        /// </summary>
        public int StagesCompleted { get; set; } = 0;

        /// <summary>
        /// JSON: user's interaction history within this journey
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string InteractionHistoryJson { get; set; }

        /// <summary>
        /// Engagement score (0-100) calculated from interactions
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? EngagementScore { get; set; }

        /// <summary>
        /// JSON: personalization profile built during journey
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string PersonalizationProfileJson { get; set; }

        /// <summary>
        /// Did user convert (reach goal)?
        /// </summary>
        public bool IsConverted { get; set; } = false;

        [SugarColumn(IsNullable = true)]
        public DateTime? ConvertedAt { get; set; }

        /// <summary>
        /// Revenue attributed to this journey execution
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? AttributedRevenue { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Dynamic Content - المحتوى الديناميكي
    /// Content variants that change based on user behavior and profile
    /// </summary>
    [SugarTable("mkt_dynamic_content")]
    public class MktDynamicContent : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ContentId { get; set; }

        [SugarColumn(Length = 200)]
        public string ContentName { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string ContentNameAr { get; set; }

        /// <summary>
        /// Type: email_body, sms_text, push_message, landing_page, banner, popup, whatsapp_template
        /// </summary>
        [SugarColumn(Length = 50)]
        public string ContentType { get; set; }

        /// <summary>
        /// Default/fallback content
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string DefaultContent { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string DefaultContentAr { get; set; }

        /// <summary>
        /// JSON: array of content variants with conditions
        /// [{"condition": {"segment": "vip"}, "content": "...", "contentAr": "..."},
        ///  {"condition": {"behavior": "frequent_buyer"}, "content": "...", "contentAr": "..."}]
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string VariantsJson { get; set; }

        /// <summary>
        /// JSON: template variables available (name, event_name, ticket_type, etc.)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string TemplateVarsJson { get; set; }

        /// <summary>
        /// Language handling: auto_detect, ar_only, en_only, bilingual
        /// </summary>
        [SugarColumn(Length = 20)]
        public string LanguageMode { get; set; } = "bilingual";

        /// <summary>
        /// AI-generated content optimization
        /// </summary>
        public bool AiOptimized { get; set; } = false;

        public int TimesServed { get; set; } = 0;

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? EngagementRate { get; set; }

        public bool IsActive { get; set; } = true;

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    public static class JourneyTypes
    {
        public const string ONBOARDING = "onboarding";
        public const string NURTURE = "nurture";
        public const string CONVERSION = "conversion";
        public const string RETENTION = "retention";
        public const string RE_ENGAGEMENT = "re_engagement";
        public const string EVENT_LIFECYCLE = "event_lifecycle";
        public const string VIP_EXPERIENCE = "vip_experience";
        public const string SPONSOR_ONBOARDING = "sponsor_onboarding";
    }
}
