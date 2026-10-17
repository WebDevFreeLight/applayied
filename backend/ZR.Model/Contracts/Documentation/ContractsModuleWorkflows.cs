namespace ZR.Model.Contracts.Documentation
{
    /// <summary>
    /// Contracts Module Workflow Diagrams - مخططات سير عمل وحدة العقود
    ///
    /// This file documents all workflow diagrams for the Contracts Module.
    /// </summary>
    public static class ContractsModuleWorkflows
    {
        /*
        ╔══════════════════════════════════════════════════════════════════════════════╗
        ║                     CONTRACTS MODULE WORKFLOW DIAGRAMS                        ║
        ║                     مخططات سير عمل وحدة العقود                                ║
        ╚══════════════════════════════════════════════════════════════════════════════╝

        ============================================================================
        1. CONTRACT LIFECYCLE WORKFLOW - سير حياة العقد
        ============================================================================

        ┌─────────┐
        │  START  │
        └────┬────┘
             │
             ▼
        ┌─────────────┐
        │   DRAFT     │ ◄──────────────────────────────────────┐
        │  (مسودة)    │                                        │
        └──────┬──────┘                                        │
               │                                               │
               │ Activate                                      │
               ▼                                               │
        ┌─────────────┐    Suspend     ┌─────────────┐         │
        │   ACTIVE    │ ─────────────► │  SUSPENDED  │         │
        │   (نشط)     │                │  (معلق)     │         │
        └──────┬──────┘ ◄───────────── └─────────────┘         │
               │           Resume                              │
               │                                               │
               │ Terminate or Expire                           │
               ▼                                               │
        ┌─────────────┐                                        │
        │ TERMINATED  │                                        │
        │  (منتهي)    │                                        │
        └──────┬──────┘                                        │
               │                                               │
               │ (Cannot reactivate)                           │
               ▼                                               │
        ┌─────────────┐                                        │
        │    END      │                                        │
        └─────────────┘                                        │
                                                               │
        Alternative Flow: Renewal                              │
        ┌─────────────┐                                        │
        │   ACTIVE    │ ──── Renewal Executed ────────────────►│
        │   (نشط)     │      (End Date Extended)               │
        └─────────────┘

        ============================================================================
        2. CONTRACT CREATION WORKFLOW - سير إنشاء العقد
        ============================================================================

                                    ┌─────────────────┐
                                    │  User initiates │
                                    │ contract create │
                                    └────────┬────────┘
                                             │
                                             ▼
                                    ┌─────────────────┐
                                    │ Select Customer │
                                    │ (Business Partner)
                                    └────────┬────────┘
                                             │
                                             ▼
                                    ┌─────────────────┐
                                    │  Enter Contract │
                                    │    Details      │
                                    │ ─────────────── │
                                    │ - Type          │
                                    │ - Dates         │
                                    │ - Value         │
                                    │ - Terms         │
                                    └────────┬────────┘
                                             │
                                             ▼
                              ┌──────────────────────────────┐
                              │      Add Contract Units?      │
                              └──────────────┬───────────────┘
                                    Yes │           │ No
                                        ▼           │
                              ┌─────────────────┐   │
                              │  Add Units      │   │
                              │ (Properties/    │   │
                              │  Equipment)     │   │
                              └────────┬────────┘   │
                                       │            │
                                       ▼            │
                              ┌─────────────────┐   │
                              │Add Pricing Rules│   │
                              │ (Optional)      │   │
                              └────────┬────────┘   │
                                       │◄───────────┘
                                       ▼
                              ┌─────────────────┐
                              │  Save as Draft  │
                              └────────┬────────┘
                                       │
                                       ▼
                              ┌─────────────────┐
                              │ Review & Verify │
                              └────────┬────────┘
                                       │
                                       ▼
                              ┌─────────────────┐
                              │    Activate     │
                              └────────┬────────┘
                                       │
                                       ▼
                              ┌─────────────────┐
                              │Generate Billing │
                              │   Schedules     │
                              └────────┬────────┘
                                       │
                                       ▼
                              ┌─────────────────┐
                              │  Contract Live  │
                              └─────────────────┘

        ============================================================================
        3. BILLING SCHEDULE GENERATION WORKFLOW - سير توليد جدول الفوترة
        ============================================================================

        ┌───────────────────┐
        │ Trigger:          │
        │ - Contract Active │
        │ - Manual Generate │
        │ - Renewal Execute │
        └─────────┬─────────┘
                  │
                  ▼
        ┌───────────────────┐
        │ Get Contract      │
        │ Parameters:       │
        │ - Start/End Date  │
        │ - Billing Freq    │
        │ - Payment Terms   │
        └─────────┬─────────┘
                  │
                  ▼
        ┌───────────────────┐
        │ Calculate Billing │
        │ Periods based on  │
        │ Frequency         │
        └─────────┬─────────┘
                  │
                  ▼
        ┌─────────────────────────────────────────────┐
        │              FOR EACH PERIOD                 │
        │ ┌─────────────────────────────────────────┐ │
        │ │ 1. Get Active Units for Period          │ │
        │ │ 2. Get Active Pricing Rules for Period  │ │
        │ │ 3. Calculate Base Amount from Units     │ │
        │ │ 4. Apply Pricing Rules (Adjustments)    │ │
        │ │ 5. Calculate Tax Amount                 │ │
        │ │ 6. Set Due Date (PeriodEnd + PayTerms)  │ │
        │ │ 7. Create Schedule Record               │ │
        │ │ 8. Create Line Items                    │ │
        │ └─────────────────────────────────────────┘ │
        └─────────────────────────────────────────────┘
                  │
                  ▼
        ┌───────────────────┐
        │ Return Generated  │
        │ Schedules Summary │
        └───────────────────┘

        ============================================================================
        4. INVOICE GENERATION WORKFLOW - سير توليد الفاتورة
        ============================================================================

        ┌─────────────────────┐
        │ Billing Schedule    │
        │ Status: Pending     │
        │ Due Date: Reached   │
        └──────────┬──────────┘
                   │
                   ▼
        ┌─────────────────────┐         ┌─────────────────────┐
        │ Manual Trigger      │   OR    │ Bulk Invoice Job    │
        │ (Single Invoice)    │         │ (Multiple Invoices) │
        └──────────┬──────────┘         └──────────┬──────────┘
                   │                               │
                   └───────────┬───────────────────┘
                               │
                               ▼
                   ┌─────────────────────┐
                   │ Validate Schedule   │
                   │ - Not already inv.  │
                   │ - Not cancelled     │
                   └──────────┬──────────┘
                              │
                              ▼
                   ┌─────────────────────┐
                   │ Create AR Invoice   │
                   │ Header with:        │
                   │ - Contract Fields   │
                   │ - Customer Info     │
                   │ - Period Info       │
                   └──────────┬──────────┘
                              │
                              ▼
                   ┌─────────────────────┐
                   │ Create AR Invoice   │
                   │ Lines from Schedule │
                   │ Lines with:         │
                   │ - Unit Info         │
                   │ - Pricing Rule Info │
                   └──────────┬──────────┘
                              │
                              ▼
                   ┌─────────────────────┐
                   │ Calculate Tax       │
                   │ (ZATCA Compliance)  │
                   └──────────┬──────────┘
                              │
                              ▼
                   ┌─────────────────────┐
                   │ Save Invoice        │
                   └──────────┬──────────┘
                              │
                              ▼
                   ┌─────────────────────┐
                   │ Update Schedule:    │
                   │ - Status = Invoiced │
                   │ - InvoiceId = X     │
                   │ - InvoiceDate = Now │
                   └──────────┬──────────┘
                              │
                              ▼
                   ┌─────────────────────┐
                   │ Return Invoice ID   │
                   └─────────────────────┘

        ============================================================================
        5. AMENDMENT WORKFLOW - سير التعديلات
        ============================================================================

        ┌────────────────┐
        │ Create         │
        │ Amendment      │
        │ (Type-specific)│
        └───────┬────────┘
                │
                ▼
        ┌────────────────┐
        │ Status: DRAFT  │
        └───────┬────────┘
                │
                │ Submit for Approval
                ▼
        ┌────────────────┐
        │ Status: PENDING│
        │    APPROVAL    │
        └───────┬────────┘
                │
        ┌───────┴───────────────────┐
        │                           │
        ▼                           ▼
   ┌─────────────┐          ┌─────────────┐
   │  Approve    │          │   Reject    │
   └──────┬──────┘          └──────┬──────┘
          │                        │
          ▼                        ▼
   ┌─────────────┐          ┌─────────────┐
   │ Status:     │          │ Status:     │
   │  APPROVED   │          │  REJECTED   │
   └──────┬──────┘          └─────────────┘
          │
          │ Apply Amendment
          ▼
   ┌─────────────────────────────────────────────┐
   │               APPLY AMENDMENT                │
   │ ┌─────────────────────────────────────────┐ │
   │ │ 1. Store Pre-Amendment Snapshot         │ │
   │ │ 2. Apply changes based on type:         │ │
   │ │    - Price Change: Update amounts       │ │
   │ │    - Duration: Extend end date          │ │
   │ │    - Unit Add: Add units                │ │
   │ │    - Unit Remove: Deactivate units      │ │
   │ │    - Discount: Add pricing rule         │ │
   │ │    - Terms: Update payment/billing      │ │
   │ │ 3. Recalculate Pending Schedules        │ │
   │ │ 4. Update Contract Master Values        │ │
   │ │ 5. Mark Amendment as Applied            │ │
   │ └─────────────────────────────────────────┘ │
   └─────────────────────────────────────────────┘
          │
          ▼
   ┌─────────────┐
   │ Status:     │
   │  APPLIED    │
   └─────────────┘

        Optional: Revert (if no invoiced schedules affected)
        ┌─────────────┐
        │  APPLIED    │
        └──────┬──────┘
               │ Revert
               ▼
        ┌─────────────┐
        │  REVERTED   │
        └─────────────┘

        ============================================================================
        6. RENEWAL WORKFLOW - سير التجديد
        ============================================================================

                                    ┌─────────────────┐
                                    │ Contract Near   │
                                    │ Expiry OR       │
                                    │ Auto-Renew Due  │
                                    └────────┬────────┘
                                             │
                            ┌────────────────┴────────────────┐
                            │                                 │
                            ▼                                 ▼
                   ┌─────────────────┐               ┌─────────────────┐
                   │ Manual Renewal  │               │ Auto-Renewal    │
                   │ Creation        │               │ (Scheduled Job) │
                   └────────┬────────┘               └────────┬────────┘
                            │                                 │
                            └────────────────┬────────────────┘
                                             │
                                             ▼
                                    ┌─────────────────┐
                                    │ Status: PENDING │
                                    └────────┬────────┘
                                             │
                                             │ Send Offer (Optional)
                                             ▼
                                    ┌─────────────────┐
                                    │ Status: OFFERED │
                                    └────────┬────────┘
                                             │
                                    ┌────────┴────────┐
                                    │                 │
                                    ▼                 ▼
                           ┌────────────┐     ┌────────────┐
                           │ Customer   │     │ Customer   │
                           │ Accepts    │     │ Declines   │
                           └─────┬──────┘     └─────┬──────┘
                                 │                  │
                                 ▼                  ▼
                           ┌────────────┐     ┌────────────┐
                           │ APPROVED   │     │ DECLINED   │
                           └─────┬──────┘     └─────┬──────┘
                                 │                  │
                                 │                  │ Create Termination?
                                 │                  ▼
                                 │           ┌────────────┐
                                 │           │ Termination│
                                 │           │ for Non-   │
                                 │           │ Renewal    │
                                 │           └────────────┘
                                 │
                                 ▼
                   ┌─────────────────────────────────────────────┐
                   │               EXECUTE RENEWAL                │
                   │ ┌─────────────────────────────────────────┐ │
                   │ │ 1. Calculate New End Date               │ │
                   │ │ 2. Calculate New Base Amount            │ │
                   │ │    (with escalation if applicable)      │ │
                   │ │ 3. Update Contract Master:              │ │
                   │ │    - EndDate                            │ │
                   │ │    - BaseAmount                         │ │
                   │ │    - TotalRenewals++                    │ │
                   │ │ 4. Apply Terms Changes (if any)         │ │
                   │ │ 5. Generate New Billing Schedules       │ │
                   │ │ 6. Create Amendment Record (optional)   │ │
                   │ └─────────────────────────────────────────┘ │
                   └─────────────────────────────────────────────┘
                                 │
                                 ▼
                           ┌────────────┐
                           │  EXECUTED  │
                           └────────────┘

        ============================================================================
        7. TERMINATION WORKFLOW - سير الإنهاء
        ============================================================================

        ┌────────────────────┐
        │ Termination Request│
        │ - Early Termination│
        │ - Non-Renewal      │
        │ - Breach           │
        │ - Mutual Agreement │
        └─────────┬──────────┘
                  │
                  ▼
        ┌────────────────────┐
        │ Status: DRAFT      │
        │ Calculate:         │
        │ - Early Term Days  │
        │ - Penalty Amount   │
        │ - Settlement Est.  │
        └─────────┬──────────┘
                  │
                  │ Submit Notice
                  ▼
        ┌────────────────────┐
        │ Status: NOTICE_SENT│
        └─────────┬──────────┘
                  │
                  │ (If requires approval)
                  ▼
        ┌────────────────────┐
        │ Status: PENDING    │
        │ APPROVAL           │
        └─────────┬──────────┘
                  │
         ┌────────┴────────┐
         │                 │
         ▼                 ▼
   ┌───────────┐     ┌───────────┐
   │ Approved  │     │ Rejected  │
   └─────┬─────┘     └───────────┘
         │
         ▼
   ┌───────────┐
   │ APPROVED  │
   └─────┬─────┘
         │
         │ Process Termination
         ▼
   ┌─────────────────────────────────────────────┐
   │           PROCESS TERMINATION               │
   │ ┌─────────────────────────────────────────┐ │
   │ │ 1. Cancel Pending Billing Schedules     │ │
   │ │    after Effective Date                 │ │
   │ │ 2. Prorate Final Billing Period         │ │
   │ │ 3. Generate Final Invoice               │ │
   │ │ 4. Generate Penalty Invoice (if any)    │ │
   │ │ 5. Process Unit Returns                 │ │
   │ │ 6. Calculate Settlement Amount          │ │
   │ │ 7. Update Contract Status to TERMINATED │ │
   │ │ 8. Create GL Journal Entries            │ │
   │ └─────────────────────────────────────────┘ │
   └─────────────────────────────────────────────┘
         │
         ▼
   ┌────────────────────┐
   │ Status: PENDING    │
   │ SETTLEMENT or      │
   │ PENDING_HANDOVER   │
   └─────────┬──────────┘
             │
   ┌─────────┴─────────┐
   │                   │
   ▼                   ▼
   ┌───────────┐   ┌───────────────────┐
   │ Process   │   │ Record Handover   │
   │ Security  │   │ Inspection        │
   │ Deposit   │   └─────────┬─────────┘
   └─────┬─────┘             │
         │                   │
         └─────────┬─────────┘
                   │
                   ▼
         ┌─────────────────┐
         │ Complete        │
         │ Settlement      │
         └────────┬────────┘
                  │
                  ▼
         ┌─────────────────┐
         │ Status:         │
         │ COMPLETED       │
         └─────────────────┘

        ============================================================================
        8. BILLING DUE NOTIFICATION WORKFLOW - سير إشعارات الاستحقاق
        ============================================================================

        ┌──────────────────────────────┐
        │ Scheduled Job: Daily 7:00 AM │
        └──────────────┬───────────────┘
                       │
                       ▼
        ┌──────────────────────────────┐
        │ Get Billing Schedules Where: │
        │ - Status = Pending           │
        │ - DueDate within threshold   │
        │   (configurable days)        │
        └──────────────┬───────────────┘
                       │
                       ▼
        ┌──────────────────────────────┐
        │ FOR EACH Due Schedule:       │
        │ ┌──────────────────────────┐ │
        │ │ 1. Check if notification │ │
        │ │    already sent          │ │
        │ │ 2. If not, send:         │ │
        │ │    - Email to customer   │ │
        │ │    - Email to manager    │ │
        │ │    - System notification │ │
        │ │ 3. Record notification   │ │
        │ └──────────────────────────┘ │
        └──────────────────────────────┘

        ============================================================================
        9. CONTRACT EXPIRY NOTIFICATION WORKFLOW - سير إشعارات انتهاء العقد
        ============================================================================

        ┌──────────────────────────────┐
        │ Scheduled Job: Daily 6:00 AM │
        └──────────────┬───────────────┘
                       │
                       ▼
        ┌──────────────────────────────┐
        │ Notification Thresholds:     │
        │ - 90 days before expiry      │
        │ - 60 days before expiry      │
        │ - 30 days before expiry      │
        │ - 14 days before expiry      │
        │ - 7 days before expiry       │
        └──────────────┬───────────────┘
                       │
                       ▼
        ┌──────────────────────────────┐
        │ FOR EACH Threshold:          │
        │ ┌──────────────────────────┐ │
        │ │ Get contracts expiring   │ │
        │ │ exactly at threshold     │ │
        │ │                          │ │
        │ │ FOR EACH Contract:       │ │
        │ │   - Check if notified    │ │
        │ │   - If Auto-Renew:       │ │
        │ │     Notify about renew   │ │
        │ │   - If Not Auto-Renew:   │ │
        │ │     Notify about expiry  │ │
        │ │   - Record notification  │ │
        │ └──────────────────────────┘ │
        └──────────────────────────────┘

        ============================================================================
        10. AUTO-RENEWAL PROCESSING WORKFLOW - سير معالجة التجديد التلقائي
        ============================================================================

        ┌──────────────────────────────┐
        │ Scheduled Job: Daily 1:00 AM │
        └──────────────┬───────────────┘
                       │
                       ▼
        ┌──────────────────────────────┐
        │ Get Contracts Where:         │
        │ - Status = Active            │
        │ - AutoRenew = true           │
        │ - EndDate within notice pd   │
        │ - No pending renewal exists  │
        └──────────────┬───────────────┘
                       │
                       ▼
        ┌──────────────────────────────┐
        │ FOR EACH Contract:           │
        │ ┌──────────────────────────┐ │
        │ │ 1. Create Renewal Record │ │
        │ │    using contract's      │ │
        │ │    default renewal terms │ │
        │ │                          │ │
        │ │ 2. If AutoExecute = true │ │
        │ │    Execute immediately   │ │
        │ │ Else                     │ │
        │ │    Leave pending for     │ │
        │ │    manual approval       │ │
        │ │                          │ │
        │ │ 3. Send notification     │ │
        │ │    to relevant parties   │ │
        │ └──────────────────────────┘ │
        └──────────────────────────────┘

        */
    }
}
