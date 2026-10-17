using System.Collections.Generic;

namespace Infrastructure.Exceptions
{
    /// <summary>
    /// Metadata for exception codes - maps codes to default severity and HTTP status
    /// </summary>
    public static class ExceptionMetadata
    {
        private static readonly Dictionary<string, (ExceptionSeverity Severity, int HttpStatus)> _metadata = new()
        {
            // Common
            [ExceptionCode.VALIDATION_FAILED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.INVALID_PARAMETER] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.UNAUTHORIZED] = (ExceptionSeverity.Warning, 401),
            [ExceptionCode.FORBIDDEN] = (ExceptionSeverity.Warning, 403),
            [ExceptionCode.NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.CONFLICT] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.INTERNAL_ERROR] = (ExceptionSeverity.Critical, 500),
            [ExceptionCode.SERVICE_UNAVAILABLE] = (ExceptionSeverity.Error, 503),
            [ExceptionCode.DATABASE_ERROR] = (ExceptionSeverity.Critical, 500),
            [ExceptionCode.CONCURRENCY_CONFLICT] = (ExceptionSeverity.Warning, 409),

            // Finance - Invoices
            [ExceptionCode.FINANCE_INVOICE_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_INVOICE_DUPLICATE] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_INVOICE_ALREADY_POSTED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_INVOICE_CANNOT_DELETE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_INVOICE_INVALID_STATUS] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_INVOICE_INVALID_AMOUNT] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_INVOICE_INVALID_DATE] = (ExceptionSeverity.Warning, 400),

            // Finance - Payments
            [ExceptionCode.FINANCE_PAYMENT_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_PAYMENT_INSUFFICIENT_BALANCE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_PAYMENT_ALREADY_POSTED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_PAYMENT_INVALID_STATUS] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_PAYMENT_ALLOCATION_FAILED] = (ExceptionSeverity.Error, 500),

            // Finance - Budget
            [ExceptionCode.FINANCE_BUDGET_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_BUDGET_DUPLICATE_CODE] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_BUDGET_CANNOT_MODIFY] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_BUDGET_INSUFFICIENT_FUNDS] = (ExceptionSeverity.Warning, 400),

            // Finance - GL
            [ExceptionCode.FINANCE_GL_JOURNAL_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_GL_UNBALANCED_ENTRY] = (ExceptionSeverity.Error, 400),
            [ExceptionCode.FINANCE_GL_PERIOD_CLOSED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_PERIOD_LOCKED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_POSTING_FAILED] = (ExceptionSeverity.Error, 500),

            // Finance - Treasury
            [ExceptionCode.FINANCE_TREASURY_ACCOUNT_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_TREASURY_ACCOUNT_INACTIVE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_INSUFFICIENT_BALANCE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_GL_ACCOUNT_REQUIRED] = (ExceptionSeverity.Warning, 400),

            [ExceptionCode.FINANCE_TREASURY_BANK_ACCOUNT_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_TREASURY_BANK_ACCOUNT_NUMBER_DUPLICATE] = (ExceptionSeverity.Warning, 409),

            [ExceptionCode.FINANCE_TREASURY_CASH_ACCOUNT_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_TREASURY_CASH_ACCOUNT_CODE_DUPLICATE] = (ExceptionSeverity.Warning, 409),

            [ExceptionCode.FINANCE_TREASURY_HAS_RELATED_TRANSACTIONS] = (ExceptionSeverity.Warning, 409),

            [ExceptionCode.FINANCE_TREASURY_RECONCILIATION_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_TREASURY_RECONCILIATION_STATUS_INVALID] = (ExceptionSeverity.Warning, 400),

            [ExceptionCode.FINANCE_TREASURY_BANK_STATEMENT_PARSE_FAILED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_BANK_STATEMENT_VALIDATION_FAILED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_BANK_STATEMENT_LINE_NOT_FOUND] = (ExceptionSeverity.Info, 404),

            [ExceptionCode.FINANCE_TREASURY_OBSOLETE_OPERATION] = (ExceptionSeverity.Warning, 400),

            [ExceptionCode.FINANCE_TREASURY_CHEQUE_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_TREASURY_CHEQUE_CREATE_FAILED] = (ExceptionSeverity.Error, 500),
            [ExceptionCode.FINANCE_TREASURY_CHEQUE_ISSUE_FAILED] = (ExceptionSeverity.Error, 500),

            [ExceptionCode.FINANCE_TREASURY_PAYMENT_GL_JOURNAL_MISSING] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_PAYMENT_TYPE_UNSUPPORTED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_GL_JOURNAL_NOT_REQUIRED_FOR_CHEQUE_STATUS] = (ExceptionSeverity.Warning, 400),

            [ExceptionCode.FINANCE_TREASURY_TREASURY_ACCOUNT_GL_LINK_MISSING] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_TREASURY_ACCOUNT_NOT_FOUND_FOR_ID] = (ExceptionSeverity.Info, 404),

            [ExceptionCode.FINANCE_TREASURY_VALIDATION_FAILED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_IBAN_REQUIRED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_IBAN_INVALID_LENGTH] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_IBAN_INVALID_FORMAT] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_IBAN_INVALID_CHECKSUM] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_SWIFT_REQUIRED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_SWIFT_INVALID_FORMAT] = (ExceptionSeverity.Warning, 400),

            [ExceptionCode.FINANCE_TREASURY_NEGATIVE_BALANCE_NOT_ALLOWED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_INTERCOMPANY_JOURNAL_REQUIRED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_ACCOUNT_COMPANY_MISMATCH] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_PAYMENT_ALREADY_REVERSED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_FUTURE_DATE_NOT_ALLOWED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_CHEQUE_STATUS_TRANSITION_INVALID] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_CASH_ACCOUNT_DOES_NOT_ACCEPT_CHEQUES] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_RECONCILIATION_TOLERANCE_EXCEEDED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_APPROVAL_LIMIT_EXCEEDED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TREASURY_REQUIRED_FIELD_MISSING] = (ExceptionSeverity.Warning, 400),

            // Finance - Configuration
            [ExceptionCode.FINANCE_CONFIG_TAX_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_CONFIG_TAX_ALREADY_EXISTS] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_CONFIG_TAX_NOT_CONFIGURED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_CONFIG_TAX_INCOMPLETE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_CONFIG_TAX_SAVE_FAILED] = (ExceptionSeverity.Error, 500),
            [ExceptionCode.FINANCE_CONFIG_TAX_UPDATE_FAILED] = (ExceptionSeverity.Error, 500),

            [ExceptionCode.FINANCE_CONFIG_TREASURY_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_CONFIG_TREASURY_ALREADY_EXISTS] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_CONFIG_TREASURY_SAVE_FAILED] = (ExceptionSeverity.Error, 500),
            [ExceptionCode.FINANCE_CONFIG_TREASURY_UPDATE_FAILED] = (ExceptionSeverity.Error, 500),
            [ExceptionCode.FINANCE_CONFIG_TREASURY_INVALID_APPROVAL_THRESHOLD] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_CONFIG_TREASURY_INVALID_PETTY_CASH_LIMIT] = (ExceptionSeverity.Warning, 400),

            // Finance - Configuration (Fiscal Year)
            [ExceptionCode.FINANCE_CONFIG_FISCAL_YEAR_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_CONFIG_FISCAL_YEAR_INVALID_DATE_RANGE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_CONFIG_FISCAL_YEAR_OVERLAP] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_CONFIG_FISCAL_YEAR_CANNOT_DELETE_OPEN] = (ExceptionSeverity.Warning, 400),

            // Finance - Configuration (Currency)
            [ExceptionCode.FINANCE_CONFIG_CURRENCY_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_CONFIG_CURRENCY_CODE_ALREADY_EXISTS] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_CONFIG_CURRENCY_CANNOT_DELETE_BASE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_CONFIG_CURRENCY_BASE_MUST_BE_ACTIVE] = (ExceptionSeverity.Warning, 400),

            // Finance - Configuration (Exchange Rates)
            [ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_NOT_FOUND_FOR_DATE] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_INVALID_RATE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_CURRENCY_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_CURRENCY_CODE_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_CURRENCY_INACTIVE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_TOO_OLD] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_NOT_APPROVED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_DIVIDE_BY_ZERO] = (ExceptionSeverity.Warning, 400),

            // Finance - Configuration (System Config)
            [ExceptionCode.FINANCE_CONFIG_SYSTEM_KEY_ALREADY_EXISTS] = (ExceptionSeverity.Warning, 409),

            // Finance - Configuration (ERP Settings)
            [ExceptionCode.FINANCE_CONFIG_ERPSETTINGS_DEFAULT_CURRENCY_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_CONFIG_ERPSETTINGS_DEFAULT_CURRENCY_REQUIRED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_CONFIG_ERPSETTINGS_DEFAULT_TAXRULE_INVALID] = (ExceptionSeverity.Warning, 400),

            // Finance - Configuration (Auto Posting)
            [ExceptionCode.FINANCE_CONFIG_AUTOPOSTING_DEBIT_ACCOUNT_INVALID] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_CONFIG_AUTOPOSTING_CREDIT_ACCOUNT_INVALID] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_CONFIG_AUTOPOSTING_ACCOUNT_REQUIRED] = (ExceptionSeverity.Warning, 400),

            // Finance - Configuration (Tax Rule)
            [ExceptionCode.FINANCE_CONFIG_TAXRULE_GL_ACCOUNT_INVALID] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_CONFIG_TAXRULE_RATE_OUT_OF_RANGE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_CONFIG_TAXRULE_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_CONFIG_TAXRULE_INACTIVE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_CONFIG_TAXRULE_NOT_EFFECTIVE_YET] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_CONFIG_TAXRULE_EXPIRED] = (ExceptionSeverity.Warning, 400),

            // Finance - Cost Center
            [ExceptionCode.FINANCE_COSTCENTER_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_COSTCENTER_CODE_DUPLICATE] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_COSTCENTER_CIRCULAR_REFERENCE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_COSTCENTER_HAS_CHILDREN] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_COSTCENTER_MOVE_TO_DESCENDANT] = (ExceptionSeverity.Warning, 400),

            // Finance - Tax
            [ExceptionCode.FINANCE_TAX_CODE_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_TAX_CODE_DUPLICATE] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_TAX_CODE_IN_USE] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_TAX_RATE_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_TAX_RATE_OVERLAP] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_TAX_RATE_INVALID_DATE_RANGE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_TAX_RATE_IN_USE] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_TAX_GROUP_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_TAX_GROUP_CODE_DUPLICATE] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_TAX_GROUP_IN_USE] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_TAX_GROUP_LINE_ID_REQUIRED] = (ExceptionSeverity.Warning, 400),

            // Finance - Assets
            [ExceptionCode.FINANCE_ASSET_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_ASSET_CODE_DUPLICATE] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_ASSET_CREATE_FAILED] = (ExceptionSeverity.Error, 500),
            [ExceptionCode.FINANCE_ASSET_CANNOT_DEPRECIATE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ASSET_DEPRECIATION_EXISTS] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_ASSET_CATEGORY_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_ASSET_CATEGORY_CODE_DUPLICATE] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_ASSET_CATEGORY_HAS_ASSETS] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_ASSET_ACCOUNT_NOT_CONFIGURED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ASSET_DEPRECIATION_ACCOUNT_NOT_CONFIGURED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ASSET_ACCUMULATED_DEPRECIATION_ACCOUNT_NOT_CONFIGURED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ASSET_GAINLOSS_ACCOUNT_NOT_CONFIGURED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ASSET_REVALUATION_ACCOUNT_NOT_CONFIGURED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ASSET_SALE_FAILED] = (ExceptionSeverity.Error, 500),
            [ExceptionCode.FINANCE_ASSET_DISPOSAL_FAILED] = (ExceptionSeverity.Error, 500),
            [ExceptionCode.FINANCE_ASSET_REVALUATION_FAILED] = (ExceptionSeverity.Error, 500),
            [ExceptionCode.FINANCE_ASSET_GL_JOURNAL_FAILED] = (ExceptionSeverity.Error, 500),
            [ExceptionCode.FINANCE_ASSET_VALIDATION_FAILED] = (ExceptionSeverity.Warning, 400),

            // Finance - GL Extended
            [ExceptionCode.FINANCE_GL_ACCOUNT_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_GL_ACCOUNT_NUMBER_DUPLICATE] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_GL_PARENT_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_GL_PARENT_IS_LEAF] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_CIRCULAR_REFERENCE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_ACCOUNT_HAS_TRANSACTIONS] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_GL_ACCOUNT_HAS_CHILDREN] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_GL_CONCURRENCY_ERROR] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_GL_OPENING_BALANCE_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_GL_OPENING_BALANCE_EXISTS] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_GL_OPENING_BALANCE_LOCKED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_OPENING_BALANCE_ZERO] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_OPENING_BALANCE_ONLY_LEAF] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_PERIOD_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_GL_PERIOD_EXISTS] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_GL_PERIOD_ALREADY_OPEN] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_PERIOD_UNPOSTED_VOUCHERS] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_PERIOD_NOT_BALANCED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_PERIOD_NOT_CLOSED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_PERIOD_DELETE_ONLY_OPEN] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_PERIOD_STATUS_FAILED] = (ExceptionSeverity.Error, 500),
            [ExceptionCode.FINANCE_GL_JOURNAL_ALREADY_CANCELLED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_JOURNAL_CANNOT_CANCEL_POSTED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_JOURNAL_NO_ENTRIES] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_JOURNAL_ONLY_DRAFT_DELETE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_JOURNAL_ONLY_DRAFT_SUBMIT] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_JOURNAL_ONLY_SUBMITTED_APPROVE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_JOURNAL_ONLY_SUBMITTED_REJECT] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_VALIDATION_FAILED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_GL_FLAG_VALIDATION_FAILED] = (ExceptionSeverity.Warning, 400),

            // Finance - ARAP
            [ExceptionCode.FINANCE_ARAP_CUSTOMER_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_ARAP_VENDOR_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_ARAP_INVOICE_NO_LINES] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ARAP_GL_ACCOUNT_NOT_CONFIGURED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ARAP_VAT_OUTPUT_NOT_CONFIGURED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ARAP_VAT_INPUT_NOT_CONFIGURED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ARAP_NO_GL_JOURNAL_TO_REVERSE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ARAP_INVALID_DUE_DATE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ARAP_LINE_REQUIRED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ARAP_ONLY_DRAFT_EDIT] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ARAP_ONLY_DRAFT_DELETE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ARAP_HAS_PAYMENTS] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_ARAP_CUSTOMER_AND_VENDOR_CONFLICT] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ARAP_CUSTOMER_OR_VENDOR_REQUIRED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.FINANCE_ARAP_NOTE_NOT_FOUND] = (ExceptionSeverity.Info, 404),

            // Finance - Zatca
            [ExceptionCode.FINANCE_ZATCA_DEVICE_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.FINANCE_ZATCA_DEVICE_SERIAL_DUPLICATE] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.FINANCE_ZATCA_CERTIFICATE_LOAD_FAILED] = (ExceptionSeverity.Error, 500),
            [ExceptionCode.FINANCE_ZATCA_PRIVATE_KEY_LOAD_FAILED] = (ExceptionSeverity.Error, 500),
            [ExceptionCode.FINANCE_ZATCA_XML_CANONICALIZATION_FAILED] = (ExceptionSeverity.Error, 500),

            // HR
            [ExceptionCode.HR_EMPLOYEE_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.HR_EMPLOYEE_DUPLICATE] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.HR_EMPLOYEE_INVALID_ID] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_EMPLOYEE_REQUIRED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_EMPLOYEE_JOB_REQUIRED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_EMPLOYEE_NAME_REQUIRED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_EMPLOYEE_INVALID_EMAIL] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_EMPLOYEE_DELETE_NO_IDS] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_EMPLOYEE_DELETE_HAS_PENDING_LEAVE] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.HR_EMPLOYEE_DELETE_HAS_PENDING_PAYROLL] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.HR_EMPLOYEE_STATUS_REQUIRED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_EMPLOYEE_INVALID_STATUS] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_EMPLOYEE_IMPORT_EMPTY] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_EMPLOYEE_IMPORT_VALIDATION_FAILED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_EMPLOYEE_IMPORT_DUPLICATE_CODES_IN_FILE] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.HR_EMPLOYEE_IMPORT_CODES_ALREADY_EXIST] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.HR_ATTENDANCE_INVALID] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_LEAVE_INSUFFICIENT_BALANCE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_LEAVE_TYPE_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.HR_LEAVE_TYPE_INACTIVE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_LEAVE_CONFLICT] = (ExceptionSeverity.Warning, 409),
            [ExceptionCode.HR_LEAVE_INVALID_DATES] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_LEAVE_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.HR_LEAVE_INVALID_STATUS] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_LEAVE_BALANCE_DEDUCTION_FAILED] = (ExceptionSeverity.Error, 500),
            [ExceptionCode.HR_LEAVE_MIN_DAYS] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_LEAVE_MAX_DAYS] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_LEAVE_ADVANCE_NOTICE] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.HR_LEAVE_DOCUMENT_REQUIRED] = (ExceptionSeverity.Warning, 400),

            // Inventory
            [ExceptionCode.INVENTORY_ITEM_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.INVENTORY_INSUFFICIENT_STOCK] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.INVENTORY_LOCATION_NOT_FOUND] = (ExceptionSeverity.Info, 404),

            // Sales
            [ExceptionCode.SALES_ORDER_NOT_FOUND] = (ExceptionSeverity.Info, 404),
            [ExceptionCode.SALES_CUSTOMER_CREDIT_EXCEEDED] = (ExceptionSeverity.Warning, 400),
            [ExceptionCode.SALES_PRICE_NOT_FOUND] = (ExceptionSeverity.Warning, 404),
        };

        public static ExceptionSeverity GetSeverity(string code)
        {
            return _metadata.TryGetValue(code, out var metadata)
                ? metadata.Severity
                : ExceptionSeverity.Error;
        }

        public static int GetHttpStatusCode(string code)
        {
            return _metadata.TryGetValue(code, out var metadata)
                ? metadata.HttpStatus
                : 500;
        }

        /// <summary>
        /// Register metadata for a new exception code (for extensibility)
        /// </summary>
        public static void Register(string code, ExceptionSeverity severity, int httpStatus)
        {
            _metadata[code] = (severity, httpStatus);
        }
    }
}
