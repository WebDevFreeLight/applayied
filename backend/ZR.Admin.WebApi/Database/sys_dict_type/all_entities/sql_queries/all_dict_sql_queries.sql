-- ========================================================
-- Auto-Generated Dictionary SQL Queries for All Entities
-- Generated: 2025-10-20 17:21:32
-- ========================================================

-- Table: ai_agent_mode (AiAgentMode)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_ai_agent_mode' AS dictType
FROM ai_agent_mode
ORDER BY id;

-- Table: ai_modelprovider (AiModelprovider)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_ai_modelprovider' AS dictType
FROM ai_modelprovider
ORDER BY id;

-- Table: ai_team_mode (AiTeamMode)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_ai_team_mode' AS dictType
FROM ai_team_mode
ORDER BY id;

-- Table: ai_tooltype (AiTooltype)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_ai_tooltype' AS dictType
FROM ai_tooltype
ORDER BY id;

-- Table: set_admin_settings (admin_settings)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_set_admin_settings' AS dictType
FROM set_admin_settings
ORDER BY id;

-- Table: blv_boulevard_seasons_Shops (BlvBoulevardSeasonShops)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_blv_boulevard_seasons_Shops' AS dictType
FROM blv_boulevard_seasons_Shops
ORDER BY id;

-- Table: blv_boulevard_seasons_shops (BlvBoulevardSeasonsShops)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_blv_boulevard_seasons_shops' AS dictType
FROM blv_boulevard_seasons_shops
ORDER BY id;

-- Table: blv_boulevard_season_zones (BlvBoulevardSeasonZones)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_blv_boulevard_season_zones' AS dictType
FROM blv_boulevard_season_zones
ORDER BY id;

-- Table: blv_boulevard_shop_category (BlvBoulevardShopCategory)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_blv_boulevard_shop_category' AS dictType
FROM blv_boulevard_shop_category
ORDER BY id;

-- Table: blv_boulevard_shops (BlvBoulevardShops)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_blv_boulevard_shops' AS dictType
FROM blv_boulevard_shops
ORDER BY id;

-- Table: blv_boulevard_shop_status (BlvBoulevardShopStatus)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_blv_boulevard_shop_status' AS dictType
FROM blv_boulevard_shop_status
ORDER BY id;

-- Table: blv_customerregistrationform (BlvCustomerregistrationform)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_blv_customerregistrationform' AS dictType
FROM blv_customerregistrationform
ORDER BY id;

-- Table: hr_documenttype (HrDocumenttype)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_documenttype' AS dictType
FROM hr_documenttype
ORDER BY id;

-- Table: hr_insurance_plan (HrInsurancePlan)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_insurance_plan' AS dictType
FROM hr_insurance_plan
ORDER BY id;

-- Table: hr_performance_review (HrPerformanceReview)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_performance_review' AS dictType
FROM hr_performance_review
ORDER BY id;

-- Table: hr_reviewtype (HrReviewtype)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_reviewtype' AS dictType
FROM hr_reviewtype
ORDER BY id;

-- Table: organizations (Organizations)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_organizations' AS dictType
FROM organizations
ORDER BY id;

-- Table: organizationtype (Organizationtype)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_organizationtype' AS dictType
FROM organizationtype
ORDER BY id;

-- Table: bp_businesspartner (BpBusinesspartner)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_bp_businesspartner' AS dictType
FROM bp_businesspartner
ORDER BY id;

-- Table: article (Article)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_article' AS dictType
FROM article
ORDER BY id;

-- Table: article_browsing_log (ArticleBrowsingLog)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_article_browsing_log' AS dictType
FROM article_browsing_log
ORDER BY id;

-- Table: articleCategory (ArticleCategory)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_articleCategory' AS dictType
FROM articleCategory
ORDER BY id;

-- Table: article_comment (ArticleComment)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_article_comment' AS dictType
FROM article_comment
ORDER BY id;

-- Table: article_praise (ArticlePraise)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_article_praise' AS dictType
FROM article_praise
ORDER BY id;

-- Table: article_topic (ArticleTopic)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_article_topic' AS dictType
FROM article_topic
ORDER BY id;

-- Table: article_userCircles (ArticleUserCircles)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_article_userCircles' AS dictType
FROM article_userCircles
ORDER BY id;

-- Table: event_booth_types (BoothType)
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_booth_types' AS dictType
FROM event_booth_types
ORDER BY typecode;

-- Table: event_events (Event)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_event_events' AS dictType
FROM event_events
ORDER BY id;

-- Table: event_booths (EventBooth)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_event_booths' AS dictType
FROM event_booths
ORDER BY id;

-- Table: event_contracts (EventContract)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_event_contracts' AS dictType
FROM event_contracts
ORDER BY id;

-- Table: event_gates (EventGate)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_event_gates' AS dictType
FROM event_gates
ORDER BY id;

-- Table: event_gate_types (EventGateType)
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_gate_types' AS dictType
FROM event_gate_types
ORDER BY typecode;

-- Table: event_incidents (EventIncident)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_event_incidents' AS dictType
FROM event_incidents
ORDER BY id;

-- Table: event_invoices (EventInvoice)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_event_invoices' AS dictType
FROM event_invoices
ORDER BY id;

-- Table: event_locations (EventLocation)
SELECT
    locationid AS dictValue,
    CONCAT(locationid, ' - ', locationname, ' - ', city) AS dictLabel,
    address AS remark,
    'sql_event_locations' AS dictType
FROM event_locations
ORDER BY locationid;

-- Table: event_payments (EventPayment)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_event_payments' AS dictType
FROM event_payments
ORDER BY id;

-- Table: event_permits (EventPermit)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_event_permits' AS dictType
FROM event_permits
ORDER BY id;

-- Table: event_status (EventStatus)
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_status' AS dictType
FROM event_status
ORDER BY typecode;

-- Table: event_tenants (EventTenant)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_event_tenants' AS dictType
FROM event_tenants
ORDER BY id;

-- Table: event_tickets (EventTicket)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_event_tickets' AS dictType
FROM event_tickets
ORDER BY id;

-- Table: event_types (EventType)
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_types' AS dictType
FROM event_types
ORDER BY typecode;

-- Table: event_visitors (EventVisitor)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_event_visitors' AS dictType
FROM event_visitors
ORDER BY id;

-- Table: event_zones (EventZone)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_event_zones' AS dictType
FROM event_zones
ORDER BY id;

-- Table: event_zone_types (EventZoneType)
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_zone_types' AS dictType
FROM event_zone_types
ORDER BY typecode;

-- Table: event_incident_types (IncidentType)
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_incident_types' AS dictType
FROM event_incident_types
ORDER BY typecode;

-- Table: event_payment_methods (PaymentMethod)
SELECT
    methodcode AS dictValue,
    CONCAT(methodcode, ' - ', methodname, ' - ', methodnamear) AS dictLabel,
    remark,
    'sql_event_payment_methods' AS dictType
FROM event_payment_methods
ORDER BY methodcode;

-- Table: event_permit_types (PermitType)
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_permit_types' AS dictType
FROM event_permit_types
ORDER BY typecode;

-- Table: event_ticket_types (TicketType)
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_ticket_types' AS dictType
FROM event_ticket_types
ORDER BY typecode;

-- Table: event_visitor_types (VisitorType)
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_visitor_types' AS dictType
FROM event_visitor_types
ORDER BY typecode;

-- Table: hr_analytics_report (HrAnalyticsReport)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_analytics_report' AS dictType
FROM hr_analytics_report
ORDER BY id;

-- Table: hr_attendance (HrAttendance)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_attendance' AS dictType
FROM hr_attendance
ORDER BY id;

-- Table: hr_attendance_device (HrAttendanceDevice)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_attendance_device' AS dictType
FROM hr_attendance_device
ORDER BY id;

-- Table: hr_attendance_log (HrAttendanceLog)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_attendance_log' AS dictType
FROM hr_attendance_log
ORDER BY id;

-- Table: hr_attendance_shift (HrAttendanceShift)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_attendance_shift' AS dictType
FROM hr_attendance_shift
ORDER BY id;

-- Table: hr_benefit_plan (HrBenefitPlan)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_benefit_plan' AS dictType
FROM hr_benefit_plan
ORDER BY id;

-- Table: hr_manager_report (HrManagerDashboard)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_manager_report' AS dictType
FROM hr_manager_report
ORDER BY id;

-- Table: hr_document (HrDocument)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_document' AS dictType
FROM hr_document
ORDER BY id;

-- Table: hr_employee (HrEmployee)
SELECT
    employeeid AS dictValue,
    CONCAT(firstname, ' ', lastname, ' ', namear) AS dictLabel,
    remark,
    'sql_hr_employee' AS dictType
FROM hr_employee
WHERE status = 'active'
ORDER BY employeeid;

-- Table: hr_employee_document (HrEmployeeDocument)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_employee_document' AS dictType
FROM hr_employee_document
ORDER BY id;

-- Table: hr_employee_shift (HrEmployeeShift)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_employee_shift' AS dictType
FROM hr_employee_shift
ORDER BY id;

-- Table: hr_holiday (HrHoliday)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_holiday' AS dictType
FROM hr_holiday
ORDER BY id;

-- Table: hr_insurance_enrollment (HrInsuranceEnrollment)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_insurance_enrollment' AS dictType
FROM hr_insurance_enrollment
ORDER BY id;

-- Table: hr_insurance_enrollment_status (HrInsuranceEnrollmentStatus)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_insurance_enrollment_status' AS dictType
FROM hr_insurance_enrollment_status
ORDER BY id;

-- Table: hr_kpi (HrKpi)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_kpi' AS dictType
FROM hr_kpi
ORDER BY id;

-- Table: hr_leave (HrLeave)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_leave' AS dictType
FROM hr_leave
ORDER BY id;

-- Table: hr_leave_balance (HrLeaveBalance)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_leave_balance' AS dictType
FROM hr_leave_balance
ORDER BY id;

-- Table: hr_leave_request (HrLeaveRequest)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_leave_request' AS dictType
FROM hr_leave_request
ORDER BY id;

-- Table: hr_loan (HrLoan)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_loan' AS dictType
FROM hr_loan
ORDER BY id;

-- Table: hr_notification (HrNotification)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_notification' AS dictType
FROM hr_notification
ORDER BY id;

-- Table: hr_hybrid_schedule (HrHybridSchedule)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_hybrid_schedule' AS dictType
FROM hr_hybrid_schedule
ORDER BY id;

-- Table: hr_payroll (HrPayroll)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_payroll' AS dictType
FROM hr_payroll
ORDER BY id;

-- Table: hr_performance_review (HrPerformanceReview)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_performance_review' AS dictType
FROM hr_performance_review
ORDER BY id;

-- Table: hr_job_requisition (HrJobRequisition)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_job_requisition' AS dictType
FROM hr_job_requisition
ORDER BY id;

-- Table: hr_recruitment (HrRecruitment)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_recruitment' AS dictType
FROM hr_recruitment
ORDER BY id;

-- Table: hr_salary_compliance (HrSalaryCompliance)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_salary_compliance' AS dictType
FROM hr_salary_compliance
ORDER BY id;

-- Table: hr_emergency_contact (HrEmployeeProfile)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_emergency_contact' AS dictType
FROM hr_emergency_contact
ORDER BY id;

-- Table: hr_skills_matrix (HrSkillsMatrix)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_skills_matrix' AS dictType
FROM hr_skills_matrix
ORDER BY id;

-- Table: hr_training_course (HrTrainingCourse)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_training_course' AS dictType
FROM hr_training_course
ORDER BY id;

-- Table: hr_workflow (HrWorkflow)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_workflow' AS dictType
FROM hr_workflow
ORDER BY id;

-- Table: hr_employee_dependent_relationship (HrEmployeeDependentRelationship)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_employee_dependent_relationship' AS dictType
FROM hr_employee_dependent_relationship
ORDER BY id;

-- Table: hr_insurance_coveragelevel (HrInsuranceCoveragelevel)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_insurance_coveragelevel' AS dictType
FROM hr_insurance_coveragelevel
ORDER BY id;

-- Table: hr_insurance_plan_ype (HrInsurancePlanYpe)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_insurance_plan_ype' AS dictType
FROM hr_insurance_plan_ype
ORDER BY id;

-- Table: ai_ToolType (ToolType)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_ai_ToolType' AS dictType
FROM ai_ToolType
ORDER BY id;

-- Table: hr_holidaytype (HrHolidaytype)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_holidaytype' AS dictType
FROM hr_holidaytype
ORDER BY id;

-- Table: hr_leave_type (HrLeaveType)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_hr_leave_type' AS dictType
FROM hr_leave_type
ORDER BY id;

-- Table: bp_businesspartnerdoctype (BpBusinesspartnerdoctype)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_bp_businesspartnerdoctype' AS dictType
FROM bp_businesspartnerdoctype
ORDER BY id;

-- Table: bp_businesspartnerkind (BpBusinesspartnerkind)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_bp_businesspartnerkind' AS dictType
FROM bp_businesspartnerkind
ORDER BY id;

-- Table: bp_businesspartnerstatus (BpBusinesspartnerstatus)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_bp_businesspartnerstatus' AS dictType
FROM bp_businesspartnerstatus
ORDER BY id;

-- Table: bp_businesspartnertype (BpBusinesspartnertype)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_bp_businesspartnertype' AS dictType
FROM bp_businesspartnertype
ORDER BY id;

-- Table: bp_compliancestatus (BpCompliancestatus)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_bp_compliancestatus' AS dictType
FROM bp_compliancestatus
ORDER BY id;

-- Table: bp_contactmethod (BpContactmethod)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_bp_contactmethod' AS dictType
FROM bp_contactmethod
ORDER BY id;

-- Table: bp_partnerdocsments (BpPartnerdocsments)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_bp_partnerdocsments' AS dictType
FROM bp_partnerdocsments
ORDER BY id;

-- Table: banner_config (BannerConfig)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_banner_config' AS dictType
FROM banner_config
ORDER BY id;

-- Table: social_fans (SocialFans)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_social_fans' AS dictType
FROM social_fans
ORDER BY id;

-- Table: social_fans_info (SocialFansInfo)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_social_fans_info' AS dictType
FROM social_fans_info
ORDER BY id;

-- Table: sys_common_lang (CommonLang)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_common_lang' AS dictType
FROM sys_common_lang
ORDER BY id;

-- Table: sys_post_dto (SysPostDto)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_post_dto' AS dictType
FROM sys_post_dto
ORDER BY id;

-- Table: email_log (EmailLog)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_email_log' AS dictType
FROM email_log
ORDER BY id;

-- Table: emailTpl (EmailTpl)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_emailTpl' AS dictType
FROM emailTpl
ORDER BY id;

-- Table: gen_table (GenTable)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_gen_table' AS dictType
FROM gen_table
ORDER BY id;

-- Table: gen_table_column (GenTableColumn)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_gen_table_column' AS dictType
FROM gen_table_column
ORDER BY id;

-- Table: smsCode_log (SmsCodeLog)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_smsCode_log' AS dictType
FROM smsCode_log
ORDER BY id;

-- Table: sqlDiffLog (SqlDiffLog)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sqlDiffLog' AS dictType
FROM sqlDiffLog
ORDER BY id;

-- Table: sys_config (SysConfig)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_config' AS dictType
FROM sys_config
ORDER BY id;

-- Table: sys_dept (SysDept)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_dept' AS dictType
FROM sys_dept
ORDER BY id;

-- Table: sys_dict_data (SysDictData)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_dict_data' AS dictType
FROM sys_dict_data
ORDER BY id;

-- Table: sys_dict_type (SysDictType)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_dict_type' AS dictType
FROM sys_dict_type
ORDER BY id;

-- Table: sys_file (SysFile)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_file' AS dictType
FROM sys_file
ORDER BY id;

-- Table: sys_file_group (SysFileGroup)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_file_group' AS dictType
FROM sys_file_group
ORDER BY id;

-- Table: sys_logininfor (SysLogininfor)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_logininfor' AS dictType
FROM sys_logininfor
ORDER BY id;

-- Table: sys_menu (SysMenu)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_menu' AS dictType
FROM sys_menu
ORDER BY id;

-- Table: sys_notice (SysNotice)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_notice' AS dictType
FROM sys_notice
ORDER BY id;

-- Table: sys_oper_log (SysOperLog)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_oper_log' AS dictType
FROM sys_oper_log
ORDER BY id;

-- Table: sys_post (SysPost)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_post' AS dictType
FROM sys_post
ORDER BY id;

-- Table: sys_role (SysRole)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_role' AS dictType
FROM sys_role
ORDER BY id;

-- Table: sys_role_dept (SysRoleDept)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_role_dept' AS dictType
FROM sys_role_dept
ORDER BY id;

-- Table: sys_role_menu (SysRoleMenu)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_role_menu' AS dictType
FROM sys_role_menu
ORDER BY id;

-- Table: sys_tasks (SysTasks)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_tasks' AS dictType
FROM sys_tasks
ORDER BY id;

-- Table: sys_tasks_log (SysTasksLog)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_tasks_log' AS dictType
FROM sys_tasks_log
ORDER BY id;

-- Table: sys_user (SysUser)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_user' AS dictType
FROM sys_user
ORDER BY id;

-- Table: sys_user_msg (SysUserMsg)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_user_msg' AS dictType
FROM sys_user_msg
ORDER BY id;

-- Table: sys_user_post (SysUserPost)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_user_post' AS dictType
FROM sys_user_post
ORDER BY id;

-- Table: sys_user_role (SysUserRole)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_sys_user_role' AS dictType
FROM sys_user_role
ORDER BY id;

-- Table: userOnlineLog (UserOnlineLog)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_userOnlineLog' AS dictType
FROM userOnlineLog
ORDER BY id;

