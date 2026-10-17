using System;
using SqlSugar;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Invoice Integration Fields - حقول تكامل فواتير العقود
    ///
    /// IMPORTANT: These fields should be added to the existing ARInvoice and ARInvoiceLine models
    /// in ZR.Model\ERP\Finance\ARAP\ to enable contract integration.
    ///
    /// This file documents the integration without modifying the Finance module directly.
    /// The database migration should add these columns to the existing tables.
    /// </summary>
    public static class ContractInvoiceIntegration
    {
        /*
        ============================================================================
        FIELDS TO ADD TO ARInvoice (arap_ar_invoice table)
        ============================================================================

        Add these fields after the existing ZATCA fields region:

        #region Contract Integration Fields - حقول تكامل العقود

        /// <summary>
        /// Contract ID - معرف العقد
        /// Foreign key to ErpContract (nullable - not all invoices are from contracts)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "contract_id")]
        public Guid? ContractId { get; set; }

        /// <summary>
        /// Contract Schedule ID - معرف جدول العقد
        /// Foreign key to ErpContractBillingSchedule (nullable)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "contract_schedule_id")]
        public Guid? ContractScheduleId { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// Denormalized for quick reference (avoids joins for display)
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "contract_number")]
        public string ContractNumber { get; set; }

        /// <summary>
        /// Billing Period Start - بداية فترة الفوترة
        /// Start date of the billing period (for contract invoices)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "billing_period_start")]
        public DateTime? BillingPeriodStart { get; set; }

        /// <summary>
        /// Billing Period End - نهاية فترة الفوترة
        /// End date of the billing period (for contract invoices)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "billing_period_end")]
        public DateTime? BillingPeriodEnd { get; set; }

        /// <summary>
        /// Is Recurring Invoice - فاتورة متكررة
        /// True if this invoice was generated from a contract schedule
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "is_recurring")]
        public bool? IsRecurring { get; set; }

        #endregion

        ============================================================================
        FIELDS TO ADD TO ARInvoiceLine (arap_ar_invoice_line table)
        ============================================================================

        Add these fields after the existing ZATCA fields region:

        #region Contract Integration Fields - حقول تكامل العقود

        /// <summary>
        /// Contract Unit ID - معرف وحدة العقد
        /// Foreign key to ErpContractUnit (nullable)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "contract_unit_id")]
        public Guid? ContractUnitId { get; set; }

        /// <summary>
        /// Contract Pricing Rule ID - معرف قاعدة تسعير العقد
        /// Foreign key to ErpContractPricingRule (nullable - for adjustment lines)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "contract_pricing_rule_id")]
        public Guid? ContractPricingRuleId { get; set; }

        /// <summary>
        /// Unit Code - رمز الوحدة
        /// Denormalized for quick reference
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "contract_unit_code")]
        public string ContractUnitCode { get; set; }

        #endregion

        ============================================================================
        DATABASE MIGRATION SQL (PostgreSQL)
        ============================================================================

        -- Add contract integration columns to AR Invoice
        ALTER TABLE arap_ar_invoice
            ADD COLUMN IF NOT EXISTS contract_id UUID NULL,
            ADD COLUMN IF NOT EXISTS contract_schedule_id UUID NULL,
            ADD COLUMN IF NOT EXISTS contract_number VARCHAR(50) NULL,
            ADD COLUMN IF NOT EXISTS billing_period_start TIMESTAMP NULL,
            ADD COLUMN IF NOT EXISTS billing_period_end TIMESTAMP NULL,
            ADD COLUMN IF NOT EXISTS is_recurring BOOLEAN NULL;

        -- Add indexes for contract lookups
        CREATE INDEX IF NOT EXISTS ix_ar_invoice_contract
            ON arap_ar_invoice(contract_id) WHERE contract_id IS NOT NULL;
        CREATE INDEX IF NOT EXISTS ix_ar_invoice_schedule
            ON arap_ar_invoice(contract_schedule_id) WHERE contract_schedule_id IS NOT NULL;

        -- Add contract integration columns to AR Invoice Line
        ALTER TABLE arap_ar_invoice_line
            ADD COLUMN IF NOT EXISTS contract_unit_id UUID NULL,
            ADD COLUMN IF NOT EXISTS contract_pricing_rule_id UUID NULL,
            ADD COLUMN IF NOT EXISTS contract_unit_code VARCHAR(50) NULL;

        -- Add index for unit lookups
        CREATE INDEX IF NOT EXISTS ix_ar_invoice_line_unit
            ON arap_ar_invoice_line(contract_unit_id) WHERE contract_unit_id IS NOT NULL;

        ============================================================================
        */
    }

    /// <summary>
    /// Contract Invoice View Model - نموذج عرض فاتورة العقد
    /// Extended view model for invoices generated from contracts
    /// Used in queries that join ARInvoice with Contract data
    /// </summary>
    public class ContractInvoiceViewModel
    {
        // From ARInvoice
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string Currency { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }

        // Contract Integration Fields
        public Guid? ContractId { get; set; }
        public string ContractNumber { get; set; }
        public Guid? ContractScheduleId { get; set; }
        public int? ScheduleNumber { get; set; }
        public DateTime? BillingPeriodStart { get; set; }
        public DateTime? BillingPeriodEnd { get; set; }
        public bool IsRecurring { get; set; }

        // Contract Details
        public string ContractTitle { get; set; }
        public int? ContractType { get; set; }
        public string ContractTypeName { get; set; }
        public int? ContractStatus { get; set; }
        public string ContractStatusName { get; set; }

        /// <summary>
        /// Period Display - عرض الفترة
        /// </summary>
        public string PeriodDisplay => BillingPeriodStart.HasValue && BillingPeriodEnd.HasValue
            ? $"{BillingPeriodStart:MMM dd} - {BillingPeriodEnd:MMM dd, yyyy}"
            : null;
    }
}
