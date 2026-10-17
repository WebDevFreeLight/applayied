using Infrastructure;
using SqlSugar.IOC;
using ZR.Common;
using ZR.Model.Sales;
using ZR.Model.Inventory;
using ZR.Model.Subscriptions;
using ZR.Model.Marketing;
using ZR.Model.Finance.Treasury;
using ZR.Model.Operations;
using ZR.Model.Inventory.Suppliers;
using ZR.Model.Finance.ZATCA;
using ZR.Model.Settings;
using System.Text.Json;
using System.Text.Encodings.Web;
using ZR.Model.Finance.CostCenter;

namespace ZR.ServiceCore.Services
{
    /// <summary>
    /// ERP Seed Data Service - خدمة البيانات الأولية لنظام ERP
    /// </summary>
    public class ErpSeedDataService
    {
        #region Main Initialization - التهيئة الرئيسية

        /// <summary>
        /// Initialize all ERP seed data - تهيئة جميع البيانات الأولية لنظام ERP
        /// </summary>
        /// <param name="webRootPath">Path to wwwroot</param>
        /// <returns>List of result messages</returns>
        public List<string> InitErpSeedData(string webRootPath)
        {
            List<string> result = new();
            var jsonBasePath = Path.Combine(webRootPath, "Database", "SeedData", "ERP");

            result.Add("========== ERP Seed Data Initialization Started - بدء تهيئة بيانات ERP ==========");

            // 1. Settings Module - الإعدادات (must be first as other modules depend on it)
            var branches = LoadFromJson<ErpBranch>(Path.Combine(jsonBasePath, "erp_branches.json"));
            if (branches.Any()) result.Add(InitErpBranchData(branches).Item1);

            var paymentMethods = LoadFromJson<ErpPaymentMethod>(Path.Combine(jsonBasePath, "erp_payment_methods.json"));
            if (paymentMethods.Any()) result.Add(InitErpPaymentMethodData(paymentMethods).Item1);

            var expenseTypes = LoadFromJson<ErpExpenseType>(Path.Combine(jsonBasePath, "erp_expense_types.json"));
            if (expenseTypes.Any()) result.Add(InitErpExpenseTypeData(expenseTypes).Item1);

            var units = LoadFromJson<ErpUnitOfMeasure>(Path.Combine(jsonBasePath, "erp_units_of_measure.json"));
            if (units.Any()) result.Add(InitErpUnitOfMeasureData(units).Item1);

            var serviceCategories = LoadFromJson<ErpServiceCategory>(Path.Combine(jsonBasePath, "erp_service_categories.json"));
            if (serviceCategories.Any()) result.Add(InitErpServiceCategoryData(serviceCategories).Item1);

            // 2. HR Module - الموارد البشرية
            // NOTE: ErpDepartment and ErpPosition types don't exist - commented out
            // var departments = LoadFromJson<ErpDepartment>(Path.Combine(jsonBasePath, "erp_departments.json"));
            // if (departments.Any()) result.Add(InitErpDepartmentData(departments).Item1);

            // var positions = LoadFromJson<ErpPosition>(Path.Combine(jsonBasePath, "erp_positions.json"));
            // if (positions.Any()) result.Add(InitErpPositionData(positions).Item1);

            // 3. Accounts Module - المحاسبة
            // NOTE: ErpAccount type doesn't exist - use GLAccount instead if needed
            // var accounts = LoadFromJson<ErpAccount>(Path.Combine(jsonBasePath, "erp_accounts.json"));
            // if (accounts.Any()) result.Add(InitErpAccountData(accounts).Item1);

            var costCenters = LoadFromJson<CostCenter>(Path.Combine(jsonBasePath, "finance_cost_centers.json"));
            if (costCenters.Any()) result.Add(InitErpCostCenterData(costCenters).Item1);

            // NOTE: ErpFinancialPeriod type doesn't exist - use GLPeriod instead if needed
            // var financialPeriods = LoadFromJson<ErpFinancialPeriod>(Path.Combine(jsonBasePath, "erp_financial_periods.json"));
            // if (financialPeriods.Any()) result.Add(InitErpFinancialPeriodData(financialPeriods).Item1);

            // 4. Treasury Module - الخزائن
            var treasuries = LoadFromJson<ErpTreasury>(Path.Combine(jsonBasePath, "erp_treasuries.json"));
            if (treasuries.Any()) result.Add(InitErpTreasuryData(treasuries).Item1);

            // 5. Inventory Module - المخزون
            var warehouses = LoadFromJson<ErpWarehouse>(Path.Combine(jsonBasePath, "erp_warehouses.json"));
            if (warehouses.Any()) result.Add(InitErpWarehouseData(warehouses).Item1);

            // 6. Subscriptions Module - الاشتراكات
            var subscriptionPackages = LoadFromJson<ErpSubscriptionPackage>(Path.Combine(jsonBasePath, "erp_subscription_packages.json"));
            if (subscriptionPackages.Any()) result.Add(InitErpSubscriptionPackageData(subscriptionPackages).Item1);

            var resources = LoadFromJson<ErpResource>(Path.Combine(jsonBasePath, "erp_resources.json"));
            if (resources.Any()) result.Add(InitErpResourceData(resources).Item1);

            var activities = LoadFromJson<ErpActivity>(Path.Combine(jsonBasePath, "erp_activities.json"));
            if (activities.Any()) result.Add(InitErpActivityData(activities).Item1);

            var memberGroups = LoadFromJson<ErpMemberGroup>(Path.Combine(jsonBasePath, "erp_member_groups.json"));
            if (memberGroups.Any()) result.Add(InitErpMemberGroupData(memberGroups).Item1);

            result.Add("========== ERP Seed Data Initialization Completed - اكتملت تهيئة بيانات ERP ==========");

            return result;
        }

        #endregion

        #region Helper Methods - الدوال المساعدة

        /// <summary>
        /// Load data from JSON file - تحميل البيانات من ملف JSON
        /// </summary>
        private List<T> LoadFromJson<T>(string jsonFilePath)
        {
            try
            {
                if (!File.Exists(jsonFilePath))
                {
                    return new List<T>();
                }

                var jsonString = File.ReadAllText(jsonFilePath);
                var data = System.Text.Json.JsonSerializer.Deserialize<List<T>>(jsonString, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return data ?? new List<T>();
            }
            catch (Exception)
            {
                return new List<T>();
            }
        }

        /// <summary>
        /// Save data to JSON file - حفظ البيانات إلى ملف JSON
        /// </summary>
        private void SaveToJson<T>(List<T> data, string jsonFilePath)
        {
            try
            {
                var directory = Path.GetDirectoryName(jsonFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var jsonString = System.Text.Json.JsonSerializer.Serialize(data, options);
                File.WriteAllText(jsonFilePath, jsonString);
            }
            catch
            {
                // Silently ignore save errors
            }
        }

        #endregion

        #region Settings Module - الإعدادات

        /// <summary>
        /// Initialize Branches - تهيئة الفروع
        /// </summary>
        public (string, object, object) InitErpBranchData(List<ErpBranch> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.BranchID)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Branches - الفروع] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Branches] Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// Initialize Payment Methods - تهيئة طرق الدفع
        /// </summary>
        public (string, object, object) InitErpPaymentMethodData(List<ErpPaymentMethod> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.PaymentMethodId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Payment Methods - طرق الدفع] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Payment Methods] Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// Initialize Expense Types - تهيئة أنواع المصروفات
        /// </summary>
        public (string, object, object) InitErpExpenseTypeData(List<ErpExpenseType> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.ExpenseTypeId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Expense Types - أنواع المصروفات] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Expense Types] Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// Initialize Units of Measure - تهيئة وحدات القياس
        /// </summary>
        public (string, object, object) InitErpUnitOfMeasureData(List<ErpUnitOfMeasure> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.UnitId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Units - وحدات القياس] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Units] Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// Initialize Service Categories - تهيئة فئات الخدمات
        /// </summary>
        public (string, object, object) InitErpServiceCategoryData(List<ErpServiceCategory> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.CategoryId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Service Categories - فئات الخدمات] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Service Categories] Error: {ex.Message}", null, null);
            }
        }

        #endregion

        #region HR Module - الموارد البشرية

        // NOTE: ErpDepartment type doesn't exist - commented out for now
        // /// <summary>
        // /// Initialize Departments - تهيئة الأقسام
        // /// </summary>
        // public (string, object, object) InitErpDepartmentData(List<ErpDepartment> data)
        // {
        //     try
        //     {
        //         var db = DbScoped.SugarScope;
        //         var x = db.Storageable(data)
        //             .SplitInsert(it => it.NotAny())
        //             .WhereColumns(it => it.DepartmentId)
        //             .ToStorage();
        //         x.AsInsertable.ExecuteCommand();
        //         string msg = $"[ERP Departments - الأقسام] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
        //         return (msg, x.ErrorList, x.IgnoreList);
        //     }
        //     catch (Exception ex)
        //     {
        //         return ($"[ERP Departments] Error: {ex.Message}", null, null);
        //     }
        // }

        // NOTE: ErpPosition type doesn't exist - commented out for now
        // /// <summary>
        // /// Initialize Positions - تهيئة المناصب
        // /// </summary>
        // public (string, object, object) InitErpPositionData(List<ErpPosition> data)
        // {
        //     try
        //     {
        //         var db = DbScoped.SugarScope;
        //         var x = db.Storageable(data)
        //             .SplitInsert(it => it.NotAny())
        //             .WhereColumns(it => it.PositionId)
        //             .ToStorage();
        //         x.AsInsertable.ExecuteCommand();
        //         string msg = $"[ERP Positions - المناصب] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
        //         return (msg, x.ErrorList, x.IgnoreList);
        //     }
        //     catch (Exception ex)
        //     {
        //         return ($"[ERP Positions] Error: {ex.Message}", null, null);
        //     }
        // }

        // NOTE: ErpEmployee type doesn't exist - commented out for now
        // /// <summary>
        // /// Initialize Employees - تهيئة الموظفين
        // /// </summary>
        // public (string, object, object) InitErpEmployeeData(List<ErpEmployee> data)
        // {
        //     try
        //     {
        //         var db = DbScoped.SugarScope;
        //         var x = db.Storageable(data)
        //             .SplitInsert(it => it.NotAny())
        //             .WhereColumns(it => it.EmployeeId)
        //             .ToStorage();
        //         x.AsInsertable.ExecuteCommand();
        //         string msg = $"[ERP Employees - الموظفين] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
        //         return (msg, x.ErrorList, x.IgnoreList);
        //     }
        //     catch (Exception ex)
        //     {
        //         return ($"[ERP Employees] Error: {ex.Message}", null, null);
        //     }
        // }

        // NOTE: ErpCommissionTier type doesn't exist - commented out for now
        // /// <summary>
        // /// Initialize Commission Tiers - تهيئة مستويات العمولات
        // /// </summary>
        // public (string, object, object) InitErpCommissionTierData(List<ErpCommissionTier> data)
        // {
        //     try
        //     {
        //         var db = DbScoped.SugarScope;
        //         var x = db.Storageable(data)
        //             .SplitInsert(it => it.NotAny())
        //             .WhereColumns(it => it.TierId)
        //             .ToStorage();
        //         x.AsInsertable.ExecuteCommand();
        //         string msg = $"[ERP Commission Tiers - مستويات العمولات] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
        //         return (msg, x.ErrorList, x.IgnoreList);
        //     }
        //     catch (Exception ex)
        //     {
        //         return ($"[ERP Commission Tiers] Error: {ex.Message}", null, null);
        //     }
        // }

        #endregion

        #region Accounts Module - المحاسبة

        // NOTE: ErpAccount type doesn't exist - use GLAccount instead if needed
        // /// <summary>
        // /// Initialize Accounts - تهيئة شجرة الحسابات
        // /// </summary>
        // public (string, object, object) InitErpAccountData(List<ErpAccount> data)
        // {
        //     try
        //     {
        //         var db = DbScoped.SugarScope;
        //         var x = db.Storageable(data)
        //             .SplitInsert(it => it.NotAny())
        //             .WhereColumns(it => it.AccountId)
        //             .ToStorage();
        //         x.AsInsertable.ExecuteCommand();
        //         string msg = $"[ERP Accounts - الحسابات] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
        //         return (msg, x.ErrorList, x.IgnoreList);
        //     }
        //     catch (Exception ex)
        //     {
        //         return ($"[ERP Accounts] Error: {ex.Message}", null, null);
        //     }
        // }

        /// <summary>
        /// Initialize Cost Centers - تهيئة مراكز التكلفة
        /// </summary>
        public (string, object, object) InitErpCostCenterData(List<CostCenter> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.CostCenterId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Cost Centers - مراكز التكلفة] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Cost Centers] Error: {ex.Message}", null, null);
            }
        }

        // NOTE: ErpFinancialPeriod type doesn't exist - use GLPeriod instead if needed
        // /// <summary>
        // /// Initialize Financial Periods - تهيئة الفترات المالية
        // /// </summary>
        // public (string, object, object) InitErpFinancialPeriodData(List<ErpFinancialPeriod> data)
        // {
        //     try
        //     {
        //         var db = DbScoped.SugarScope;
        //         var x = db.Storageable(data)
        //             .SplitInsert(it => it.NotAny())
        //             .WhereColumns(it => it.PeriodId)
        //             .ToStorage();
        //         x.AsInsertable.ExecuteCommand();
        //         string msg = $"[ERP Financial Periods - الفترات المالية] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
        //         return (msg, x.ErrorList, x.IgnoreList);
        //     }
        //     catch (Exception ex)
        //     {
        //         return ($"[ERP Financial Periods] Error: {ex.Message}", null, null);
        //     }
        // }

        #endregion

        #region Treasury Module - الخزائن

        /// <summary>
        /// Initialize Treasuries - تهيئة الخزائن
        /// </summary>
        public (string, object, object) InitErpTreasuryData(List<ErpTreasury> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.TreasuryId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Treasuries - الخزائن] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Treasuries] Error: {ex.Message}", null, null);
            }
        }

        #endregion

        #region Inventory Module - المخزون

        /// <summary>
        /// Initialize Warehouses - تهيئة المستودعات
        /// </summary>
        public (string, object, object) InitErpWarehouseData(List<ErpWarehouse> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.WarehouseId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Warehouses - المستودعات] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Warehouses] Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// Initialize Products - تهيئة المنتجات
        /// </summary>
        public (string, object, object) InitErpProductData(List<ErpProduct> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.ProductId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Products - المنتجات] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Products] Error: {ex.Message}", null, null);
            }
        }

        #endregion

        #region Suppliers Module - الموردين

        /// <summary>
        /// Initialize Suppliers - تهيئة الموردين
        /// </summary>
        public (string, object, object) InitErpSupplierData(List<ErpSupplier> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.SupplierId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Suppliers - الموردين] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Suppliers] Error: {ex.Message}", null, null);
            }
        }

        #endregion

        #region Sales Module - المبيعات

        /// <summary>
        /// Initialize Customers - تهيئة العملاء
        /// </summary>
        public (string, object, object) InitErpCustomerData(List<ErpCustomer> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.CustomerId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Customers - العملاء] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Customers] Error: {ex.Message}", null, null);
            }
        }

        #endregion

        #region Subscriptions Module - الاشتراكات

        /// <summary>
        /// Initialize Subscription Packages - تهيئة باقات الاشتراك
        /// </summary>
        public (string, object, object) InitErpSubscriptionPackageData(List<ErpSubscriptionPackage> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.PackageId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Subscription Packages - باقات الاشتراك] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Subscription Packages] Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// Initialize Resources - تهيئة الموارد
        /// </summary>
        public (string, object, object) InitErpResourceData(List<ErpResource> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.ResourceId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Resources - الموارد] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Resources] Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// Initialize Activities - تهيئة الأنشطة
        /// </summary>
        public (string, object, object) InitErpActivityData(List<ErpActivity> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.ActivityId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Activities - الأنشطة] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Activities] Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// Initialize Member Groups - تهيئة مجموعات الأعضاء
        /// </summary>
        public (string, object, object) InitErpMemberGroupData(List<ErpMemberGroup> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.GroupId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Member Groups - مجموعات الأعضاء] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Member Groups] Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// Initialize Members - تهيئة الأعضاء
        /// </summary>
        public (string, object, object) InitErpMemberData(List<ErpMember> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.MemberId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Members - الأعضاء] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Members] Error: {ex.Message}", null, null);
            }
        }

        #endregion

        #region Marketing Module - التسويق

        /// <summary>
        /// Initialize Promo Codes - تهيئة أكواد الخصم
        /// </summary>
        public (string, object, object) InitErpPromoCodeData(List<ErpPromoCode> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.PromoCodeId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Promo Codes - أكواد الخصم] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Promo Codes] Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// Initialize Gift Cards - تهيئة بطاقات الهدايا
        /// </summary>
        public (string, object, object) InitErpGiftCardData(List<ErpGiftCard> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.GiftCardId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP Gift Cards - بطاقات الهدايا] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP Gift Cards] Error: {ex.Message}", null, null);
            }
        }

        #endregion

        #region ZATCA Module - هيئة الزكاة والضريبة

        /// <summary>
        /// Initialize ZATCA Settings - تهيئة إعدادات الزكاة والضريبة
        /// </summary>
        public (string, object, object) InitErpZatcaSettingsData(List<ErpZatcaSettings> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.SettingsId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                string msg = $"[ERP ZATCA Settings - إعدادات الزكاة] Inserted {x.InsertList.Count} Errors {x.ErrorList.Count} Total {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[ERP ZATCA Settings] Error: {ex.Message}", null, null);
            }
        }

        #endregion
    }
}

