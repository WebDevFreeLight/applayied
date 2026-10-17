using Infrastructure;
using MiniExcelLibs;
using SqlSugar.IOC;
using ZR.Common;
using ZR.Model.Content;
using ZR.Model.System;
using ZR.Model.EventManagement;
using ZR.Model.Boulevard;
using ZR.Model.Business;
using ZR.Model.BusinessPartners;
using ZR.Model.HR.Hrtypes;
using ZR.Model.HR;
using System.Text.Json;
using System.Text.Encodings.Web;
using ZR.Model.HR.Attendance;
using ZR.Model.HR.Recruitment;
using ZR.Model.HR.Payroll;
using ZR.Model.HR.Performance;
using ZR.Model.HR.Business;
using ZR.Model.HR.Training;
using ZR.Model.HR.Attendance.Configuration;
using ZR.Model.HR.Attendance.AttendanceCapture;
using ZR.Model.HR.Attendance.LeaveManagement;

namespace ZR.ServiceCore.Services
{
    /// <summary>
    /// ?????? ???????? ???????
    /// </summary>
    public class SeedDataService
    {
        /// <summary>
        /// ????? ?????? ????????
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public (string, object, object) InitUserData(List<SysUser> data)
        {
            try
            {
                data.ForEach(x =>
                {
                    x.Password = "E10ADC3949BA59ABBE56E057F20F883E";
                });
                var db = DbScoped.SugarScope;
                db.Ado.BeginTran();
                //db.Ado.ExecuteCommand("SET IDENTITY_INSERT sys_user ON");
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())//???? ??? ??? ??? ??? ???? ??? where? ????? ????? ??? ???? ???????? ????? ?????? ??? ?? ???? ??? ???????? ???? ???? ?? ??? ?? ???
                    .SplitError(x => x.Item.UserName.IsEmpty(), "??? ???????? ?? ???? ?? ???? ??????")
                    .SplitError(x => !Tools.CheckUserName(x.Item.UserName), "??? ???????? ?? ?????? ?? ????????")
                    .WhereColumns(it => it.UserId)//??? ?? ??? ??????? ???????? ???? ????? ??? ???? ??????? (???? ?????? it=>new{it.x1,it.x2})
                    .ToStorage();
                var result = x.AsInsertable.OffIdentity().ExecuteCommand();//????? ????? ?????? ???????;
                //db.Ado.ExecuteCommand("SET IDENTITY_INSERT sys_user OFF");
                db.Ado.CommitTran();

                string msg = $"[?????? ????????] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[?????? ???????? - User Data] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ?????? ???????
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public (string, object, object) InitMenuData(List<SysMenu> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.MenuId)//??? ?? ??? ??????? ???????? ???? ????? ??? ???? ??????? (???? ?????? it=>new{it.x1,it.x2})
                    .ToStorage();
                var result = x.AsInsertable.OffIdentity().ExecuteCommand();//????? ????? ?????? ???????;

                string msg = $"[?????? ???????] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[?????? ??????? - Menu Data] ??? - Error: {ex.Message}", null, null);
            }
        }
        /// <summary>
        /// ?????? ????? ???????
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public (string, object, object) InitRoleMenuData(List<SysRoleMenu> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => new { it.Menu_id, it.Role_id })
                    .ToStorage();
                var result = x.AsInsertable.ExecuteCommand();//????? ????? ?????? ???????;

                string msg = $"[????? ???????] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ??????? - Role Menu] ??? - Error: {ex.Message}", null, null);
            }
        }
        /// <summary>
        /// ????? ?????? ???????
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public (string, object, object) InitDeptData(List<SysDept> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.DeptId)
                    .ToStorage();
                var result = x.AsInsertable.OffIdentity().ExecuteCommand();

                string msg = $"[?????? ???????] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[?????? ??????? - Dept Data] ??? - Error: {ex.Message}", null, null);
            }
        }

        public (string, object, object) InitPostData(List<SysPost> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.PostCode)
                    .ToStorage();
                var result = x.AsInsertable.ExecuteCommand();

                string msg = $"[?????? ???????] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[?????? ??????? - Post Data] ??? - Error: {ex.Message}", null, null);
            }
        }

        public (string, object, object) InitRoleData(List<SysRole> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.RoleKey)
                    .ToStorage();
                var result = x.AsInsertable.OffIdentity().ExecuteCommand();

                string msg = $"[?????? ???????] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[?????? ??????? - Role Data] ??? - Error: {ex.Message}", null, null);
            }
        }

        public (string, object, object) InitUserRoleData(List<SysUserRole> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => new { it.RoleId, it.UserId })
                    .ToStorage();
                var result = x.AsInsertable.ExecuteCommand();

                string msg = $"[????? ????????] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ???????? - User Role] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ??????? ??????
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public (string, object, object) InitConfigData(List<SysConfig> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.ConfigKey)
                    .ToStorage();
                var result = x.AsInsertable.ExecuteCommand();

                string msg = $"[??????? ??????] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[??????? ?????? - Config Data] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public (string, object, object) InitDictType(List<SysDictType> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.DictType)
                    .ToStorage();
                var result = x.AsInsertable.ExecuteCommand();

                string msg = $"[????? ???????] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ??????? - Dict Type] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ?????? ???????
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public (string, object, object) InitDictData(List<SysDictData> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => new { it.DictType, it.DictValue })
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[?????? ???????] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[?????? ??????? - Dict Data] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ???? ????????
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public (string, object, object) InitArticleCategoryData(List<ArticleCategory> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.Name)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[???? ????????] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[???? ???????? - Article Category] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ?????? ????????
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public (string, object, object) InitArticleTopicData(List<ArticleTopic> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.TopicName)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[?????? ????????] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[?????? ???????? - Article Topic] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public (string, object, object) InitTaskData(List<SysTasks> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.Name)
                    .ToStorage();
                var result = x.AsInsertable.ExecuteCommand();

                string msg = $"[?????? ??????] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[?????? ?????? - Task Data] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ?????? ?????????
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public (string, object, object) InitNoticeData(List<SysNotice> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => new { it.NoticeId })
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[?????? ????????? ??????????] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[?????? ????????? ?????????? - Notice Data] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ???????? ???????
        /// </summary>
        /// <param name="path"></param>
        /// <param name="clean"></param>
        /// <returns></returns>
        public List<string> InitSeedData(string path, string WebRootPath, bool clean)
        {
            List<string> result = new();
            var db = DbScoped.SugarScope;
            if (clean)
            {
                try { db.DbMaintenance.TruncateTable<SysRoleDept>(); } catch (Exception ex) { result.Add($"[????? - Clean] SysRoleDept: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<SysRoleMenu>(); } catch (Exception ex) { result.Add($"[????? - Clean] SysRoleMenu: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<SysMenu>(); } catch (Exception ex) { result.Add($"[????? - Clean] SysMenu: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<SysRole>(); } catch (Exception ex) { result.Add($"[????? - Clean] SysRole: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<SysUser>(); } catch (Exception ex) { result.Add($"[????? - Clean] SysUser: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<SysDept>(); } catch (Exception ex) { result.Add($"[????? - Clean] SysDept: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<SysPost>(); } catch (Exception ex) { result.Add($"[????? - Clean] SysPost: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<SysDictType>(); } catch (Exception ex) { result.Add($"[????? - Clean] SysDictType: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<SysDictData>(); } catch (Exception ex) { result.Add($"[????? - Clean] SysDictData: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<SysNotice>(); } catch (Exception ex) { result.Add($"[????? - Clean] SysNotice: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<SysUserRole>(); } catch (Exception ex) { result.Add($"[????? - Clean] SysUserRole: {ex.Message}"); }
            }


            var sysUser = MiniExcel.Query<SysUser>(path, sheetName: "user").ToList();
            var result1 = InitUserData(sysUser);
            result.Add(result1.Item1);

            var sysPost = MiniExcel.Query<SysPost>(path, sheetName: "post").ToList();
            var result2 = InitPostData(sysPost);
            result.Add(result2.Item1);

            var sysRole = MiniExcel.Query<SysRole>(path, sheetName: "role").ToList();
            var result3 = InitRoleData(sysRole);
            result.Add(result3.Item1);

            var sysUserRole = MiniExcel.Query<SysUserRole>(path, sheetName: "user_role").ToList();
            var result4 = InitUserRoleData(sysUserRole);
            result.Add(result4.Item1);

            var sysMenu = MiniExcel.Query<SysMenu>(path, sheetName: "menu").ToList();
            var result5 = InitMenuData(sysMenu);
            result.Add(result5.Item1);

            var sysConfig = MiniExcel.Query<SysConfig>(path, sheetName: "config").ToList();
            var result6 = InitConfigData(sysConfig);
            result.Add(result6.Item1);

            var sysRoleMenu = MiniExcel.Query<SysRoleMenu>(path, sheetName: "role_menu").ToList();
            var result7 = InitRoleMenuData(sysRoleMenu);
            result.Add(result7.Item1);

            var sysDict = MiniExcel.Query<SysDictType>(path, sheetName: "dict_type").ToList();
            var result8 = InitDictType(sysDict);
            result.Add(result8.Item1);

            var sysDictData = MiniExcel.Query<SysDictData>(path, sheetName: "dict_data").ToList();
            var result9 = InitDictData(sysDictData);
            result.Add(result9.Item1);

            var sysDept = MiniExcel.Query<SysDept>(path, sheetName: "dept").ToList();
            var result10 = InitDeptData(sysDept);
            result.Add(result10.Item1);

            var sysArticleCategory = MiniExcel.Query<ArticleCategory>(path, sheetName: "article_category").ToList();
            var result11 = InitArticleCategoryData(sysArticleCategory);
            result.Add(result11.Item1);

            var sysArticleTopic = MiniExcel.Query<ArticleTopic>(path, sheetName: "article_topic").ToList();
            var result13 = InitArticleTopicData(sysArticleTopic);
            result.Add(result13.Item1);

            var sysNotice = MiniExcel.Query<SysNotice>(path, sheetName: "notice").ToList();
            var result12 = InitNoticeData(sysNotice);
            result.Add(result12.Item1);


          var alldata=  InitEventManagementSeedData(path, WebRootPath, clean);


            if (alldata is not null &&alldata.Count>0)
            {
                result.AddRange(alldata);

            }

            // Initialize Boulevard Seed Data
            var boulevardData = InitBoulevardSeedData(path, WebRootPath, clean);
            if (boulevardData is not null && boulevardData.Count > 0)
            {
                result.AddRange(boulevardData);
            }

            // Initialize Business Seed Data
            var businessData = InitBusinessSeedData(path, WebRootPath, clean);
            if (businessData is not null && businessData.Count > 0)
            {
                result.AddRange(businessData);
            }

            // Initialize BusinessPartnerData Seed Data
            var businessPartnerData = InitBusinessPartnerDataSeedData(path, WebRootPath, clean);
            if (businessPartnerData is not null && businessPartnerData.Count > 0)
            {
                result.AddRange(businessPartnerData);
            }

            // Initialize Partnertype Seed Data
            var partnertypeData = InitPartnertypeSeedData(path, WebRootPath, clean);
            if (partnertypeData is not null && partnertypeData.Count > 0)
            {
                result.AddRange(partnertypeData);
            }

            // Initialize Hrtypes Seed Data
            var hrtypesData = InitHrtypesSeedData(path, WebRootPath, clean);
            if (hrtypesData is not null && hrtypesData.Count > 0)
            {
                result.AddRange(hrtypesData);
            }

            // Initialize HR Seed Data
            var hrData = InitHrSeedData(path, WebRootPath, clean);
            if (hrData is not null && hrData.Count > 0)
            {
                result.AddRange(hrData);
            }

            // Initialize Comprehensive HR Seed Data (All 90+ Models)
            var allHrData = InitAllHrSeedData(path, WebRootPath, clean);
            if (allHrData is not null && allHrData.Count > 0)
            {
                result.AddRange(allHrData);
            }

            // Initialize ERP Seed Data (Gym/Fitness Center Management)
            var erpSeedService = new ErpSeedDataService();
            var erpData = erpSeedService.InitErpSeedData(WebRootPath);
            if (erpData is not null && erpData.Count > 0)
            {
                result.AddRange(erpData);
            }

            return result;
        }

        #region Event Management Seed Data Methods

        /// <summary>
        /// ????? ???????? ?? ??? JSON - Load data from JSON file
        /// </summary>
        public static List<T> LoadFromJson<T>(string jsonFilePath)
        {
            try
            {
                if (!File.Exists(jsonFilePath))
                {
                    return new List<T>();
                }

                var jsonString = File.ReadAllText(jsonFilePath);
                var data = System.Text.Json.JsonSerializer.Deserialize<List<T>>(jsonString, new JsonSerializerOptions
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
        /// ??? ???????? ??? ??? JSON - Save data to JSON file
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

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var jsonString = System.Text.Json.JsonSerializer.Serialize(data, options);
                File.WriteAllText(jsonFilePath, jsonString);
            }
            catch (Exception)
            {
                // Silent fail - JSON export is optional
            }
        }

        /// <summary>
        /// ????? ?????? ????? ????????? - Event Locations
        /// </summary>
        public (string, object, object) InitEventLocationData(List<EventLocation> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.Locationid)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[????? ????????? - Event Locations] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ????????? - Event Locations] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ????? ????????? - Event Types
        /// </summary>
        public (string, object, object) InitEventTypeData(List<EventType> data)
        {
            try
            {
                var db = DbScoped.SugarScope;

                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.Typecode)
                    .ToStorage();
                //var result = x.AsInsertable.ExecuteCommand();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[????? ????????? - Event Types] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ????????? - Event Types] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ????? ??????? - Zone Types
        /// </summary>
        public (string, object, object) InitZoneTypeData(List<EventZoneType> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.Typecode)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[????? ??????? - Zone Types] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ??????? - Zone Types] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ????? ???????? - Gate Types
        /// </summary>
        public (string, object, object) InitGateTypeData(List<EventGateType> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.Typecode)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[????? ???????? - Gate Types] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ???????? - Gate Types] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ????? ?????? - Visitor Types
        /// </summary>
        public (string, object, object) InitVisitorTypeData(List<VisitorType> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.Typecode)
                    .ToStorage();
                var result = x.AsInsertable.ExecuteCommand();

                string msg = $"[????? ?????? - Visitor Types] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ?????? - Visitor Types] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ????? ??????? - Ticket Types
        /// </summary>
        public (string, object, object) InitTicketTypeData(List<TicketType> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.Typecode)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[????? ??????? - Ticket Types] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ??????? - Ticket Types] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ????? ???????? - Permit Types
        /// </summary>
        public (string, object, object) InitPermitTypeData(List<PermitType> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.Typecode)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[????? ???????? - Permit Types] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ???????? - Permit Types] ??? - Error: {ex.Message}", null, null);
            }
        }
        /// <summary>
        /// ????? ?????? ????? ??????? - Booth Types
        /// </summary>
        public (string, object, object) InitBoothTypeData(List<BoothType> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.Typecode)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ??????? - Booth Types] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ??????? - Booth Types] ??? - Error: {ex.Message}", null, null);
            }
        }
        /// <summary>
        /// ????? ?????? ????? ??????? - Incident Types
        /// </summary>
        public (string, object, object) InitIncidentTypeData(List<IncidentType> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.Typecode)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ??????? - Incident Types] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ??????? - Incident Types] ??? - Error: {ex.Message}", null, null);
            }
        }


        /// <summary>
        /// ????? ?????? ????? ????????? - Event Status
        /// </summary>
        public (string, object, object) InitEventStatusData(List<EventStatus> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.Typecode)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ????????? - Event Status] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ????????? - Event Status] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ????????? - Events
        /// </summary>
        public (string, object, object) InitEventData(List<Event> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.Eventcode)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????????? - Events] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????????? - Events] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ??????? - Event Zones
        /// </summary>
        public (string, object, object) InitEventZoneData(List<EventZone> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => new { it.EventId, it.ZoneCode })
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[??????? - Event Zones] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[??????? - Event Zones] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ???????? - Event Gates
        /// </summary>
        public (string, object, object) InitEventGateData(List<EventGate> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => new { it.Eventid, it.Gatecode })
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[???????? - Event Gates] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[???????? - Event Gates] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ?????? - Event Visitors
        /// </summary>
        public (string, object, object) InitEventVisitorData(List<EventVisitor> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => new { it.VisitorCode, it.Email })
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[?????? - Event Visitors] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[?????? - Event Visitors] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ??????? - Event Tickets
        /// </summary>
        public (string, object, object) InitEventTicketData(List<EventTicket> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.TicketNumber)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[??????? - Event Tickets] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[??????? - Event Tickets] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ???????? - Event Permits
        /// </summary>
        public (string, object, object) InitEventPermitData(List<EventPermit> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.PermitNumber)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[???????? - Event Permits] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[???????? - Event Permits] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ??????? - Event Booths
        /// </summary>
        public (string, object, object) InitEventBoothData(List<EventBooth> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => new { it.EventId, it.BoothNumber })
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[??????? - Event Booths] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[??????? - Event Booths] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ?????????? - Event Tenants
        /// </summary>
        public (string, object, object) InitEventTenantData(List<EventTenant> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.TenantCode)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[?????????? - Event Tenants] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[?????????? - Event Tenants] ??? - Error: {ex.Message}", null, null);
            }
        }


        /// <summary>
        /// ????? ?????? ??????? - Event Incidents
        /// </summary>
        public (string, object, object) InitEventIncidentData(List<EventIncident> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    //.SplitInsert(it => it.NotAny())
                    .WhereColumns(it => it.IncidentNumber)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[??????? - Event Incidents] ?? ??????? {x.InsertList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[??????? - Event Incidents] ??? - Error: {ex.Message}", null, null);
            }
        }


        /// <summary>
        /// ????? ???? ?????? ???? ????? ????????? - Initialize All Event Management Data
        /// </summary>
        /// <param name="path">???? ??? Excel ?? ???? JSON</param>
        /// <param name="clean">?? ???? ??? ???????? ???????</param>
        /// <returns></returns>
        public List<string> InitEventManagementSeedData(string path, string WebRootPath, bool clean)
        {
            List<string> result = new();
            var db = DbScoped.SugarScope;
            string jsonBasePath = Path.Combine(WebRootPath,"Database", "SeedData", "EventManagement");

            if (clean)
            {
                // ??? ???????? ???????? ????? (???? ???????? ????????)
                try { db.DbMaintenance.TruncateTable<EventIncident>(); } catch (Exception ex) { result.Add($"[????? - Clean] EventIncident: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<EventBooth>(); } catch (Exception ex) { result.Add($"[????? - Clean] EventBooth: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<EventTenant>(); } catch (Exception ex) { result.Add($"[????? - Clean] EventTenant: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<EventPermit>(); } catch (Exception ex) { result.Add($"[????? - Clean] EventPermit: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<EventTicket>(); } catch (Exception ex) { result.Add($"[????? - Clean] EventTicket: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<EventVisitor>(); } catch (Exception ex) { result.Add($"[????? - Clean] EventVisitor: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<EventGate>(); } catch (Exception ex) { result.Add($"[????? - Clean] EventGate: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<EventZone>(); } catch (Exception ex) { result.Add($"[????? - Clean] EventZone: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<Event>(); } catch (Exception ex) { result.Add($"[????? - Clean] Event: {ex.Message}"); }

                // ??? ????? ??????? ????????
                try { db.DbMaintenance.TruncateTable<EventLocation>(); } catch (Exception ex) { result.Add($"[????? - Clean] EventLocation: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<EventStatus>(); } catch (Exception ex) { result.Add($"[????? - Clean] EventStatus: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<IncidentType>(); } catch (Exception ex) { result.Add($"[????? - Clean] IncidentType: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<BoothType>(); } catch (Exception ex) { result.Add($"[????? - Clean] BoothType: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<PermitType>(); } catch (Exception ex) { result.Add($"[????? - Clean] PermitType: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<TicketType>(); } catch (Exception ex) { result.Add($"[????? - Clean] TicketType: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<VisitorType>(); } catch (Exception ex) { result.Add($"[????? - Clean] VisitorType: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<EventGateType>(); } catch (Exception ex) { result.Add($"[????? - Clean] EventGateType: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<EventZoneType>(); } catch (Exception ex) { result.Add($"[????? - Clean] EventZoneType: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<EventType>(); } catch (Exception ex) { result.Add($"[????? - Clean] EventType: {ex.Message}"); }
            }

            // ????? ?????? ?????? ??????? ?????
            try
            {
                var eventTypes = MiniExcel.Query<EventType>(path, sheetName: "event_types").ToList();
                SaveToJson(eventTypes, Path.Combine(jsonBasePath, "event_types.json"));
                if (eventTypes.Any())
                {
                    var result1 = InitEventTypeData(eventTypes);
                    result.Add(result1.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<EventType>(Path.Combine(jsonBasePath, "event_types.json"));
                if (jsonData.Any())
                {
                    var result1 = InitEventTypeData(jsonData);
                    result.Add(result1.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ????????? - Event Types] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            try
            {
                var eventStatus = MiniExcel.Query<EventStatus>(path, sheetName: "event_status").ToList();
                SaveToJson(eventStatus, Path.Combine(jsonBasePath, "event_status.json"));
                if (eventStatus.Any())
                {
                    var resultStatus = InitEventStatusData(eventStatus);
                    result.Add(resultStatus.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<EventStatus>(Path.Combine(jsonBasePath, "event_status.json"));

                if (jsonData.Any())
                {
                    var resultStatus = InitEventStatusData(jsonData);
                    result.Add(resultStatus.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ????????? - Event Status] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            try
            {
                var zoneTypes = MiniExcel.Query<EventZoneType>(path, sheetName: "zone_types").ToList();
                SaveToJson(zoneTypes, Path.Combine(jsonBasePath, "zone_types.json"));
                var result2 = InitZoneTypeData(zoneTypes);
                result.Add(result2.Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<EventZoneType>(Path.Combine(jsonBasePath, "zone_types.json"));
                if (jsonData.Any())
                {
                    var result2 = InitZoneTypeData(jsonData);
                    result.Add(result2.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ??????? - Zone Types] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            try
            {
                var gateTypes = MiniExcel.Query<EventGateType>(path, sheetName: "gate_types").ToList();
                SaveToJson(gateTypes, Path.Combine(jsonBasePath, "gate_types.json"));
                var result3 = InitGateTypeData(gateTypes);
                result.Add(result3.Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<EventGateType>(Path.Combine(jsonBasePath, "gate_types.json"));
                if (jsonData.Any())
                {
                    var result3 = InitGateTypeData(jsonData);
                    result.Add(result3.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ???????? - Gate Types] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            try
            {
                var visitorTypes = MiniExcel.Query<VisitorType>(path, sheetName: "visitor_types").ToList();
                SaveToJson(visitorTypes, Path.Combine(jsonBasePath, "visitor_types.json"));
                var result4 = InitVisitorTypeData(visitorTypes);
                result.Add(result4.Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<VisitorType>(Path.Combine(jsonBasePath, "visitor_types.json"));
                if (jsonData.Any())
                {
                    var result4 = InitVisitorTypeData(jsonData);
                    result.Add(result4.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ?????? - Visitor Types] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            try
            {
                var ticketTypes = MiniExcel.Query<TicketType>(path, sheetName: "ticket_types").ToList();
                SaveToJson(ticketTypes, Path.Combine(jsonBasePath, "ticket_types.json"));
                var result5 = InitTicketTypeData(ticketTypes);
                result.Add(result5.Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<TicketType>(Path.Combine(jsonBasePath, "ticket_types.json"));
                if (jsonData.Any())
                {
                    var result5 = InitTicketTypeData(jsonData);
                    result.Add(result5.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ??????? - Ticket Types] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            try
            {
                var permitTypes = MiniExcel.Query<PermitType>(path, sheetName: "permit_types").ToList();
                SaveToJson(permitTypes, Path.Combine(jsonBasePath, "permit_types.json"));
                var result6 = InitPermitTypeData(permitTypes);
                result.Add(result6.Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<PermitType>(Path.Combine(jsonBasePath, "permit_types.json"));
                if (jsonData.Any())
                {
                    var result6 = InitPermitTypeData(jsonData);
                    result.Add(result6.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ???????? - Permit Types] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            try
            {
                var boothTypes = MiniExcel.Query<BoothType>(path, sheetName: "booth_types").ToList();
                SaveToJson(boothTypes, Path.Combine(jsonBasePath, "booth_types.json"));
                var result7 = InitBoothTypeData(boothTypes);
                result.Add(result7.Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<BoothType>(Path.Combine(jsonBasePath, "booth_types.json"));
                if (jsonData.Any())
                {
                    var result7 = InitBoothTypeData(jsonData);
                    result.Add(result7.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ??????? - Booth Types] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            try
            {
                var incidentTypes = MiniExcel.Query<IncidentType>(path, sheetName: "incident_types").ToList();
                SaveToJson(incidentTypes, Path.Combine(jsonBasePath, "incident_types.json"));
                var result8 = InitIncidentTypeData(incidentTypes);
                result.Add(result8.Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<IncidentType>(Path.Combine(jsonBasePath, "incident_types.json"));
                if (jsonData.Any())
                {
                    var result8 = InitIncidentTypeData(jsonData);
                    result.Add(result8.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ??????? - Incident Types] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // ????? ?????? ?????? ???????
            try
            {
                var eventLocations = MiniExcel.Query<EventLocation>(path, sheetName: "event_locations").ToList();
                SaveToJson(eventLocations, Path.Combine(jsonBasePath, "event_locations.json"));
                if (eventLocations.Any())
                {
                    var resultLocations = InitEventLocationData(eventLocations);
                    result.Add(resultLocations.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<EventLocation>(Path.Combine(jsonBasePath, "event_locations.json"));
                if (jsonData.Any())
                {
                    var resultLocations = InitEventLocationData(jsonData);
                    result.Add(resultLocations.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ????????? - Event Locations] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // ????? ?????? ???????? ????????
            try
            {
                var events = MiniExcel.Query<Event>(path, sheetName: "events").ToList();
                SaveToJson(events, Path.Combine(jsonBasePath, "events.json"));
                var result10 = InitEventData(events);
                result.Add(result10.Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<Event>(Path.Combine(jsonBasePath, "events.json"));
                if (jsonData.Any())
                {
                    var result10 = InitEventData(jsonData);
                    result.Add(result10.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????????? - Events] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            try
            {
                var zones = MiniExcel.Query<EventZone>(path, sheetName: "event_zones").ToList();
                SaveToJson(zones, Path.Combine(jsonBasePath, "event_zones.json"));
                var result11 = InitEventZoneData(zones);
                result.Add(result11.Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<EventZone>(Path.Combine(jsonBasePath, "event_zones.json"));
                if (jsonData.Any())
                {
                    var result11 = InitEventZoneData(jsonData);
                    result.Add(result11.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[??????? - Event Zones] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            try
            {
                var gates = MiniExcel.Query<EventGate>(path, sheetName: "event_gates").ToList();
                SaveToJson(gates, Path.Combine(jsonBasePath, "event_gates.json"));
                var result12 = InitEventGateData(gates);
                result.Add(result12.Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<EventGate>(Path.Combine(jsonBasePath, "event_gates.json"));
                if (jsonData.Any())
                {
                    var result12 = InitEventGateData(jsonData);
                    result.Add(result12.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[???????? - Event Gates] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            try
            {
                var tenants = MiniExcel.Query<EventTenant>(path, sheetName: "event_tenants").ToList();
                SaveToJson(tenants, Path.Combine(jsonBasePath, "event_tenants.json"));
                var result13 = InitEventTenantData(tenants);
                result.Add(result13.Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<EventTenant>(Path.Combine(jsonBasePath, "event_tenants.json"));
                if (jsonData.Any())
                {
                    var result13 = InitEventTenantData(jsonData);
                    result.Add(result13.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[?????????? - Event Tenants] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            try
            {
                var booths = MiniExcel.Query<EventBooth>(path, sheetName: "event_booths").ToList();
                SaveToJson(booths, Path.Combine(jsonBasePath, "event_booths.json"));
                var result14 = InitEventBoothData(booths);
                result.Add(result14.Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<EventBooth>(Path.Combine(jsonBasePath, "event_booths.json"));
                if (jsonData.Any())
                {
                    var result14 = InitEventBoothData(jsonData);
                    result.Add(result14.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[??????? - Event Booths] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            try
            {
                var visitors = MiniExcel.Query<EventVisitor>(path, sheetName: "event_visitors").ToList();
                SaveToJson(visitors, Path.Combine(jsonBasePath, "event_visitors.json"));
                var result15 = InitEventVisitorData(visitors);
                result.Add(result15.Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<EventVisitor>(Path.Combine(jsonBasePath, "event_visitors.json"));
                if (jsonData.Any())
                {
                    var result15 = InitEventVisitorData(jsonData);
                    result.Add(result15.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[?????? - Event Visitors] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            try
            {
                var tickets = MiniExcel.Query<EventTicket>(path, sheetName: "event_tickets").ToList();
                SaveToJson(tickets, Path.Combine(jsonBasePath, "event_tickets.json"));
                var result16 = InitEventTicketData(tickets);
                result.Add(result16.Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<EventTicket>(Path.Combine(jsonBasePath, "event_tickets.json"));
                if (jsonData.Any())
                {
                    var result16 = InitEventTicketData(jsonData);
                    result.Add(result16.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[??????? - Event Tickets] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            try
            {
                var permits = MiniExcel.Query<EventPermit>(path, sheetName: "event_permits").ToList();
                SaveToJson(permits, Path.Combine(jsonBasePath, "event_permits.json"));
                var result17 = InitEventPermitData(permits);
                result.Add(result17.Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<EventPermit>(Path.Combine(jsonBasePath, "event_permits.json"));
                if (jsonData.Any())
                {
                    var result17 = InitEventPermitData(jsonData);
                    result.Add(result17.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[???????? - Event Permits] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            try
            {
                var incidents = MiniExcel.Query<EventIncident>(path, sheetName: "event_incidents").ToList();
                SaveToJson(incidents, Path.Combine(jsonBasePath, "event_incidents.json"));
                var result19 = InitEventIncidentData(incidents);
                result.Add(result19.Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<EventIncident>(Path.Combine(jsonBasePath, "event_incidents.json"));
                if (jsonData.Any())
                {
                    var result19 = InitEventIncidentData(jsonData);
                    result.Add(result19.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[??????? - Event Incidents] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Generate and save all dictionary data from Event Management types
            try
            {
                //var dictResults = GenerateAndSaveAllDictionaries(WebRootPath);
                //result.AddRange(dictResults);
            }
            catch (Exception ex)
            {
                result.Add($"[Dictionary Generation] ??? ????? ???????? - Failed to generate dictionaries: {ex.Message}");
            }

            return result;
        }

        #endregion

        #region Dictionary Generation from Event Management Types

        /// <summary>
        /// ????? ?????? sys_dict_type ?? ????? ????? ?????????
        /// Generate sys_dict_type entries for Event Management types
        /// </summary>
        public List<SysDictType> GenerateEventManagementDictTypes()
        {
            var dictTypes = new List<SysDictType>
            {
                new SysDictType
                {
                    DictName = "????? ????????? - Event Types",
                    DictType = "event_type",
                    Status = "0",
                    Remark = "Generated from EventType table",
                    Create_by = "system"
                },
                new SysDictType
                {
                    DictName = "????? ??????? - Zone Types",
                    DictType = "zone_type",
                    Status = "0",
                    Remark = "Generated from EventZoneType table",
                    Create_by = "system"
                },
                new SysDictType
                {
                    DictName = "????? ???????? - Gate Types",
                    DictType = "gate_type",
                    Status = "0",
                    Remark = "Generated from EventGateType table",
                    Create_by = "system"
                },
                new SysDictType
                {
                    DictName = "????? ?????? - Visitor Types",
                    DictType = "visitor_type",
                    Status = "0",
                    Remark = "Generated from VisitorType table",
                    Create_by = "system"
                },
                new SysDictType
                {
                    DictName = "????? ??????? - Ticket Types",
                    DictType = "ticket_type",
                    Status = "0",
                    Remark = "Generated from TicketType table",
                    Create_by = "system"
                },
                new SysDictType
                {
                    DictName = "????? ???????? - Permit Types",
                    DictType = "permit_type",
                    Status = "0",
                    Remark = "Generated from PermitType table",
                    Create_by = "system"
                },
                new SysDictType
                {
                    DictName = "????? ??????? - Booth Types",
                    DictType = "booth_type",
                    Status = "0",
                    Remark = "Generated from BoothType table",
                    Create_by = "system"
                },
                new SysDictType
                {
                    DictName = "????? ??????? - Incident Types",
                    DictType = "incident_type",
                    Status = "0",
                    Remark = "Generated from IncidentType table",
                    Create_by = "system"
                },
                new SysDictType
                {
                    DictName = "????? ????????? - Event Status",
                    DictType = "event_status",
                    Status = "0",
                    Remark = "Generated from EventStatus table",
                    Create_by = "system"
                },
                new SysDictType
                {
                    DictName = "????? ????????? - Event Locations",
                    DictType = "event_location",
                    Status = "0",
                    Remark = "Generated from EventLocation table",
                    Create_by = "system"
                },
                new SysDictType
                {
                    DictName = "???????? - Employees",
                    DictType = "hr_employee",
                    Status = "0",
                    Remark = "Generated from HrEmployee table",
                    Create_by = "system"
                }
            };

            return dictTypes;
        }

        /// <summary>
        /// ????? ?????? sys_dict_data ?? EventType
        /// Generate sys_dict_data from EventType table
        /// </summary>
        public List<SysDictData> GenerateDictDataFromEventTypes()
        {
            var db = DbScoped.SugarScope;
            var eventTypes = db.Queryable<EventType>().ToList();

            return eventTypes.Select((et, index) => new SysDictData
            {
                DictType = "event_type",
                DictLabel = $"{et.Typecode} - {et.Typename} - {et.Typenamear}",
                DictValue = et.Typecode,
                DictSort = index + 1,
                Remark = et.Remark,
                IsDefault = "N",
                Status = "0",
                Create_by = "system"
            }).ToList();
        }

        /// <summary>
        /// ????? ?????? sys_dict_data ?? EventZoneType
        /// Generate sys_dict_data from EventZoneType table
        /// </summary>
        public List<SysDictData> GenerateDictDataFromZoneTypes()
        {
            var db = DbScoped.SugarScope;
            var zoneTypes = db.Queryable<EventZoneType>().ToList();

            return zoneTypes.Select((zt, index) => new SysDictData
            {
                DictType = "zone_type",
                DictLabel = $"{zt.Typecode} - {zt.Typename} - {zt.Typenamear}",
                DictValue = zt.Typecode,
                DictSort = index + 1,
                Remark = zt.Remark,
                IsDefault = "N",
                Status = "0",
                Create_by = "system"
            }).ToList();
        }

        /// <summary>
        /// ????? ?????? sys_dict_data ?? EventGateType
        /// Generate sys_dict_data from EventGateType table
        /// </summary>
        public List<SysDictData> GenerateDictDataFromGateTypes()
        {
            var db = DbScoped.SugarScope;
            var gateTypes = db.Queryable<EventGateType>().ToList();

            return gateTypes.Select((gt, index) => new SysDictData
            {
                DictType = "gate_type",
                DictLabel = $"{gt.Typecode} - {gt.Typename} - {gt.Typenamear}",
                DictValue = gt.Typecode,
                DictSort = index + 1,
                Remark = gt.Remark,
                IsDefault = "N",
                Status = "0",
                Create_by = "system"
            }).ToList();
        }

        /// <summary>
        /// ????? ?????? sys_dict_data ?? VisitorType
        /// Generate sys_dict_data from VisitorType table
        /// </summary>
        public List<SysDictData> GenerateDictDataFromVisitorTypes()
        {
            var db = DbScoped.SugarScope;
            var visitorTypes = db.Queryable<VisitorType>().ToList();

            return visitorTypes.Select((vt, index) => new SysDictData
            {
                DictType = "visitor_type",
                DictLabel = $"{vt.Typecode} - {vt.Typename} - {vt.Typenamear}",
                DictValue = vt.Typecode,
                DictSort = index + 1,
                Remark = vt.Remark,
                IsDefault = "N",
                Status = "0",
                Create_by = "system"
            }).ToList();
        }

        /// <summary>
        /// ????? ?????? sys_dict_data ?? TicketType
        /// Generate sys_dict_data from TicketType table
        /// </summary>
        public List<SysDictData> GenerateDictDataFromTicketTypes()
        {
            var db = DbScoped.SugarScope;
            var ticketTypes = db.Queryable<TicketType>().ToList();

            return ticketTypes.Select((tt, index) => new SysDictData
            {
                DictType = "ticket_type",
                DictLabel = $"{tt.Typecode} - {tt.Typename} - {tt.Typenamear}",
                DictValue = tt.Typecode,
                DictSort = index + 1,
                Remark = tt.Remark,
                IsDefault = "N",
                Status = "0",
                Create_by = "system"
            }).ToList();
        }

        /// <summary>
        /// ????? ?????? sys_dict_data ?? PermitType
        /// Generate sys_dict_data from PermitType table
        /// </summary>
        public List<SysDictData> GenerateDictDataFromPermitTypes()
        {
            var db = DbScoped.SugarScope;
            var permitTypes = db.Queryable<PermitType>().ToList();

            return permitTypes.Select((pt, index) => new SysDictData
            {
                DictType = "permit_type",
                DictLabel = $"{pt.Typecode} - {pt.Typename} - {pt.Typenamear}",
                DictValue = pt.Typecode,
                DictSort = index + 1,
                Remark = pt.Remark,
                IsDefault = "N",
                Status = "0",
                Create_by = "system"
            }).ToList();
        }

        /// <summary>
        /// ????? ?????? sys_dict_data ?? BoothType
        /// Generate sys_dict_data from BoothType table
        /// </summary>
        public List<SysDictData> GenerateDictDataFromBoothTypes()
        {
            var db = DbScoped.SugarScope;
            var boothTypes = db.Queryable<BoothType>().ToList();

            return boothTypes.Select((bt, index) => new SysDictData
            {
                DictType = "booth_type",
                DictLabel = $"{bt.Typecode} - {bt.Typename} - {bt.Typenamear}",
                DictValue = bt.Typecode,
                DictSort = index + 1,
                Remark = bt.Remark,
                IsDefault = "N",
                Status = "0",
                Create_by = "system"
            }).ToList();
        }

        /// <summary>
        /// ????? ?????? sys_dict_data ?? IncidentType
        /// Generate sys_dict_data from IncidentType table
        /// </summary>
        public List<SysDictData> GenerateDictDataFromIncidentTypes()
        {
            var db = DbScoped.SugarScope;
            var incidentTypes = db.Queryable<IncidentType>().ToList();

            return incidentTypes.Select((it, index) => new SysDictData
            {
                DictType = "incident_type",
                DictLabel = $"{it.Typecode} - {it.Typename} - {it.Typenamear}",
                DictValue = it.Typecode,
                DictSort = index + 1,
                Remark = it.Remark,
                IsDefault = "N",
                Status = "0",
                Create_by = "system"
            }).ToList();
        }

        /// <summary>
        /// ????? ?????? sys_dict_data ?? EventStatus
        /// Generate sys_dict_data from EventStatus table
        /// </summary>
        public List<SysDictData> GenerateDictDataFromEventStatus()
        {
            var db = DbScoped.SugarScope;
            var eventStatuses = db.Queryable<EventStatus>().ToList();

            return eventStatuses.Select((es, index) => new SysDictData
            {
                DictType = "event_status",
                DictLabel = $"{es.Typecode} - {es.Typename} - {es.Typenamear}",
                DictValue = es.Typecode,
                DictSort = index + 1,
                Remark = es.Remark,
                IsDefault = "N",
                Status = "0",
                Create_by = "system"
            }).ToList();
        }

        /// <summary>
        /// ????? ?????? sys_dict_data ?? EventLocation
        /// Generate sys_dict_data from EventLocation table
        /// </summary>
        public List<SysDictData> GenerateDictDataFromEventLocations()
        {
            var db = DbScoped.SugarScope;
            var eventLocations = db.Queryable<EventLocation>().ToList();

            return eventLocations.Select((el, index) => new SysDictData
            {
                DictType = "event_location",
                DictLabel = $"{el.Locationid} - {el.Locationname} - {el.City}",
                DictValue = el.Locationid.ToString(),
                DictSort = index + 1,
                Remark = el.Address,
                IsDefault = "N",
                Status = "0",
                Create_by = "system"
            }).ToList();
        }

        /// <summary>
        /// ????? ?????? sys_dict_data ?? HrEmployee
        /// Generate sys_dict_data from HrEmployee table - equivalent to:
        /// SELECT employeeid AS dictValue, CONCAT(firstname, ' ', lastname, ' ', namear) AS dictLabel, remark FROM hr_employee
        /// </summary>
        public List<SysDictData> GenerateDictDataFromHrEmployees()
        {
            // NOTE: This method needs to be updated to use EmployeeApiMappingService
            // For now, returning empty list to avoid compilation errors
            // TODO: Implement proper employee dictionary generation using new EmployeeCore structure
            return new List<SysDictData>();
        }

        /// <summary>
        /// ????? ???? ????? ???????? ?????? ?????????
        /// Initialize all Event Management dictionary types
        /// </summary>
        public (string, object, object) InitEventManagementDictTypes(List<SysDictType> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.DictType)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ?????? ????? ????????? - Event Management Dict Types] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ?????? ????? ????????? - Event Management Dict Types] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ???? ?????? ???????? ???????
        /// Initialize all generated dictionary data
        /// </summary>
        public (string, object, object) InitGeneratedDictData(List<SysDictData> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => new { it.DictType, it.DictValue })
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[?????? ???????? ??????? - Generated Dict Data] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[?????? ???????? ??????? - Generated Dict Data] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ???? ???? ?????? ???????? ?? ????? ????????? ???????? ???????
        /// Generate and save all dictionary data from Event Management and HR
        /// </summary>
        public List<string> GenerateAndSaveAllDictionaries(string WebRootPath)
        {
            List<string> result = new();
            string jsonBasePath = Path.Combine(WebRootPath, "Database", "SeedData","Saveddata", "sys_dict_type", "EventManagement");

            try
            {
                // Generate and save dict types
                var dictTypes = GenerateEventManagementDictTypes();
                SaveToJson(dictTypes, Path.Combine(jsonBasePath, "sys_dict_type_event_management.json"));
                var resultDictTypes = InitEventManagementDictTypes(dictTypes);
                result.Add(resultDictTypes.Item1);

                // Generate and save all dict data
                var allDictData = new List<SysDictData>();
                // Event Types 
                var eventTypeData = GenerateDictDataFromEventTypes();
                allDictData.AddRange(eventTypeData);
                SaveToJson(eventTypeData, Path.Combine(jsonBasePath, "dict_data_event_types.json"));

                // Zone Types
                var zoneTypeData = GenerateDictDataFromZoneTypes();
                allDictData.AddRange(zoneTypeData);
                SaveToJson(zoneTypeData, Path.Combine(jsonBasePath, "dict_data_zone_types.json"));

                // Gate Types
                var gateTypeData = GenerateDictDataFromGateTypes();
                allDictData.AddRange(gateTypeData);
                SaveToJson(gateTypeData, Path.Combine(jsonBasePath, "dict_data_gate_types.json"));

                // Visitor Types
                var visitorTypeData = GenerateDictDataFromVisitorTypes();
                allDictData.AddRange(visitorTypeData);
                SaveToJson(visitorTypeData, Path.Combine(jsonBasePath, "dict_data_visitor_types.json"));

                // Ticket Types
                var ticketTypeData = GenerateDictDataFromTicketTypes();
                allDictData.AddRange(ticketTypeData);
                SaveToJson(ticketTypeData, Path.Combine(jsonBasePath, "dict_data_ticket_types.json"));

                // Permit Types
                var permitTypeData = GenerateDictDataFromPermitTypes();
                allDictData.AddRange(permitTypeData);
                SaveToJson(permitTypeData, Path.Combine(jsonBasePath, "dict_data_permit_types.json"));

                // Booth Types
                var boothTypeData = GenerateDictDataFromBoothTypes();
                allDictData.AddRange(boothTypeData);
                SaveToJson(boothTypeData, Path.Combine(jsonBasePath, "dict_data_booth_types.json"));

                // Incident Types
                var incidentTypeData = GenerateDictDataFromIncidentTypes();
                allDictData.AddRange(incidentTypeData);
                SaveToJson(incidentTypeData, Path.Combine(jsonBasePath, "dict_data_incident_types.json"));

                // Event Status
                var eventStatusData = GenerateDictDataFromEventStatus();
                allDictData.AddRange(eventStatusData);
                SaveToJson(eventStatusData, Path.Combine(jsonBasePath, "dict_data_event_status.json"));

                // Event Locations
                var eventLocationData = GenerateDictDataFromEventLocations();
                allDictData.AddRange(eventLocationData);
                SaveToJson(eventLocationData, Path.Combine(jsonBasePath, "dict_data_event_locations.json"));

                // HR Employees
                var hrEmployeeData = GenerateDictDataFromHrEmployees();
                allDictData.AddRange(hrEmployeeData);
                SaveToJson(hrEmployeeData, Path.Combine(jsonBasePath, "dict_data_hr_employees.json"));

                // Save combined data
                SaveToJson(allDictData, Path.Combine(jsonBasePath, "dict_data_all_combined.json"));

                // Initialize all dict data
                var resultDictData = InitGeneratedDictData(allDictData);
                result.Add(resultDictData.Item1);

                result.Add($"[Dictionary Generation] ?? ????? {allDictData.Count} ??? ????? ????? - Generated {allDictData.Count} dictionary records successfully");
            }
            catch (Exception ex)
            {
                result.Add($"[Dictionary Generation Error] ???: {ex.Message}");
            }

            return result;
        }

        #endregion

        #region SQL Dictionary Generation from Attributes

        /// <summary>
        /// Generates and saves all SQL dictionaries from entities with SqlDictAttribute
        /// This method uses reflection to discover all entities marked with SqlDictAttribute
        /// and automatically generates SQL-based dictionary types
        /// </summary>
        public List<string> GenerateAndSaveSqlDictionaries()
        {
            var results = new List<string>();
            results.Add("=== Automatic SQL Dictionary Generation ===");
            results.Add("");

            try
            {
                var sqlDictGenerator = new SqlDictGeneratorService();
                var generationResults = sqlDictGenerator.GenerateAndSaveAllSqlDictionaries();
                results.AddRange(generationResults);
            }
            catch (Exception ex)
            {
                results.Add($"[SQL Dictionary Generation Error] ???: {ex.Message}");
                results.Add($"Stack Trace: {ex.StackTrace}");
            }

            return results;
        }

        /// <summary>
        /// Generates SQL dictionaries for specific entity types
        /// </summary>
        public List<string> GenerateSqlDictionariesForSpecificTypes(params Type[] entityTypes)
        {
            var results = new List<string>();
            results.Add($"=== SQL Dictionary Generation for {entityTypes.Length} specific types ===");
            results.Add("");

            try
            {
                var sqlDictGenerator = new SqlDictGeneratorService();
                var generationResults = sqlDictGenerator.GenerateSqlDictionariesForTypes(entityTypes);
                results.AddRange(generationResults);
            }
            catch (Exception ex)
            {
                results.Add($"[SQL Dictionary Generation Error] ???: {ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Automatically discovers ALL SugarTable entities and generates SQL dictionaries
        /// This method intelligently analyzes column names to determine dictValue and dictLabel
        /// </summary>
        public List<string> AutoGenerateAllSqlDictionaries()
        {
            var results = new List<string>();
            results.Add("=== Automatic SQL Dictionary Generation (All Entities) ===");
            results.Add("");

            try
            {
                var autoGenerator = new AutoSqlDictGeneratorService();
                var generationResults = autoGenerator.GenerateAndSaveAllDiscoveredDictionaries();
                results.AddRange(generationResults);
            }
            catch (Exception ex)
            {
                results.Add($"[Auto SQL Dictionary Generation Error] ???: {ex.Message}");
                results.Add($"Stack Trace: {ex.StackTrace}");
            }

            return results;
        }

        /// <summary>
        /// Analyzes all SugarTable entities and returns analysis results without saving
        /// </summary>
        public List<AutoSqlDictGeneratorService.DiscoveredEntity> AnalyzeAllSugarTableEntities()
        {
            try
            {
                var autoGenerator = new AutoSqlDictGeneratorService();
                return autoGenerator.AnalyzeAllEntities();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing entities: {ex.Message}");
                return new List<AutoSqlDictGeneratorService.DiscoveredEntity>();
            }
        }

        /// <summary>
        /// Exports entity analysis to JSON format
        /// </summary>
        public string ExportEntityAnalysisToJson()
        {
            try
            {
                var autoGenerator = new AutoSqlDictGeneratorService();
                return autoGenerator.ExportAnalysisToJson();
            }
            catch (Exception ex)
            {
                return $"{{\"error\": \"{ex.Message}\"}}";
            }
        }

        /// <summary>
        /// Add [SqlDict] attributes to all entity files that don't have them
        /// </summary>
        public List<string> AddSqlDictAttributesToAllEntityFiles(bool saveFiles = false)
        {
            var results = new List<string>();
            results.Add("=== Adding [SqlDict] Attributes to Entity Files ===");
            results.Add("");

            try
            {
                var adder = new SqlDictAttributeAdderService();
                var addResults = adder.AddSqlDictAttributesToAllEntities(saveFiles);
                results.AddRange(addResults);
            }
            catch (Exception ex)
            {
                results.Add($"[SqlDict Attribute Addition Error] ???: {ex.Message}");
                results.Add($"Stack Trace: {ex.StackTrace}");
            }

            return results;
        }

        /// <summary>
        /// Preview what [SqlDict] attributes would be added
        /// </summary>
        public List<string> PreviewSqlDictAttributeAddition()
        {
            try
            {
                var adder = new SqlDictAttributeAdderService();
                return adder.GeneratePreviewReport();
            }
            catch (Exception ex)
            {
                return new List<string> { $"Error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Add [SqlDict] attributes to a specific module only
        /// </summary>
        public List<string> AddSqlDictAttributesToModule(string moduleName, bool saveFiles = false)
        {
            try
            {
                var adder = new SqlDictAttributeAdderService();
                return adder.AddSqlDictAttributesToModule(moduleName, saveFiles);
            }
            catch (Exception ex)
            {
                return new List<string> { $"Error: {ex.Message}" };
            }
        }

        #endregion

        #region Business Seed Data Methods

        /// <summary>
        /// ????? ?????? ????? ???????? - Organization Types
        /// </summary>
        public (string, object, object) InitOrganizationtypeData(List<Organizationtype> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.Organizationtypeid)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ???????? - Organization Types] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ???????? - Organization Types] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ???????? - Organizations
        /// </summary>
        public (string, object, object) InitOrganizationsData(List<Organizations> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.Organizationsid)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[???????? - Organizations] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[???????? - Organizations] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ????? ????????? - Document Types
        /// </summary>
        public (string, object, object) InitHrDocumenttypeData(List<HrDocumenttype> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.Documenttypeid)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ????????? - Document Types] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ????????? - Document Types] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ??? ??????? - Insurance Plans
        /// </summary>
        public (string, object, object) InitHrInsurancePlanData(List<HrInsurancePlan> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.Planid)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[??? ??????? - Insurance Plans] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[??? ??????? - Insurance Plans] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ????? ????????? - Review Types
        /// </summary>
        public (string, object, object) InitHrReviewtypeData(List<HrReviewtype> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.Reviewtypeid)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ????????? - Review Types] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ????????? - Review Types] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ??????? ?????? - Performance Reviews
        /// </summary>
        public (string, object, object) InitHrPerformanceReviewData(List<HrPerformanceReview> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.ReviewId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[??????? ?????? - Performance Reviews] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[??????? ?????? - Performance Reviews] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ???? ?????? Business - Initialize All Business Data
        /// </summary>
        public List<string> InitBusinessSeedData(string path, string WebRootPath, bool clean)
        {
            List<string> result = new();
            var db = DbScoped.SugarScope;
            string jsonBasePath = Path.Combine(WebRootPath, "Database", "SeedData", "Business");

            if (clean)
            {
                try { db.DbMaintenance.TruncateTable<HrPerformanceReview>(); } catch (Exception ex) { result.Add($"[????? - Clean] HrPerformanceReview: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<HrReviewtype>(); } catch (Exception ex) { result.Add($"[????? - Clean] HrReviewtype: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<HrInsurancePlan>(); } catch (Exception ex) { result.Add($"[????? - Clean] HrInsurancePlan: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<HrDocumenttype>(); } catch (Exception ex) { result.Add($"[????? - Clean] HrDocumenttype: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<Organizations>(); } catch (Exception ex) { result.Add($"[????? - Clean] Organizations: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<Organizationtype>(); } catch (Exception ex) { result.Add($"[????? - Clean] Organizationtype: {ex.Message}"); }
            }

            // Load and initialize Organization Types
            try
            {
                var orgTypes = MiniExcel.Query<Organizationtype>(path, sheetName: "organization_types").ToList();
                SaveToJson(orgTypes, Path.Combine(jsonBasePath, "organization_types.json"));
                if (orgTypes.Any())
                {
                    var result1 = InitOrganizationtypeData(orgTypes);
                    result.Add(result1.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<Organizationtype>(Path.Combine(jsonBasePath, "organization_types.json"));
                if (jsonData.Any())
                {
                    var result1 = InitOrganizationtypeData(jsonData);
                    result.Add(result1.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ???????? - Organization Types] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Load and initialize Organizations
            try
            {
                var orgs = MiniExcel.Query<Organizations>(path, sheetName: "organizations").ToList();
                SaveToJson(orgs, Path.Combine(jsonBasePath, "organizations.json"));
                if (orgs.Any())
                {
                    var result2 = InitOrganizationsData(orgs);
                    result.Add(result2.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<Organizations>(Path.Combine(jsonBasePath, "organizations.json"));
                if (jsonData.Any())
                {
                    var result2 = InitOrganizationsData(jsonData);
                    result.Add(result2.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[???????? - Organizations] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Load and initialize Document Types
            try
            {
                var docTypes = MiniExcel.Query<HrDocumenttype>(path, sheetName: "document_types").ToList();
                SaveToJson(docTypes, Path.Combine(jsonBasePath, "document_types.json"));
                if (docTypes.Any())
                {
                    var result3 = InitHrDocumenttypeData(docTypes);
                    result.Add(result3.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<HrDocumenttype>(Path.Combine(jsonBasePath, "document_types.json"));
                if (jsonData.Any())
                {
                    var result3 = InitHrDocumenttypeData(jsonData);
                    result.Add(result3.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ????????? - Document Types] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Load and initialize Insurance Plans
            try
            {
                var insurancePlans = MiniExcel.Query<HrInsurancePlan>(path, sheetName: "insurance_plans").ToList();
                SaveToJson(insurancePlans, Path.Combine(jsonBasePath, "insurance_plans.json"));
                if (insurancePlans.Any())
                {
                    var result4 = InitHrInsurancePlanData(insurancePlans);
                    result.Add(result4.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<HrInsurancePlan>(Path.Combine(jsonBasePath, "insurance_plans.json"));
                if (jsonData.Any())
                {
                    var result4 = InitHrInsurancePlanData(jsonData);
                    result.Add(result4.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[??? ??????? - Insurance Plans] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Load and initialize Review Types
            try
            {
                var reviewTypes = MiniExcel.Query<HrReviewtype>(path, sheetName: "review_types").ToList();
                SaveToJson(reviewTypes, Path.Combine(jsonBasePath, "review_types.json"));
                if (reviewTypes.Any())
                {
                    var result5 = InitHrReviewtypeData(reviewTypes);
                    result.Add(result5.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<HrReviewtype>(Path.Combine(jsonBasePath, "review_types.json"));
                if (jsonData.Any())
                {
                    var result5 = InitHrReviewtypeData(jsonData);
                    result.Add(result5.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ????????? - Review Types] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Load and initialize Performance Reviews
            try
            {
                var perfReviews = MiniExcel.Query<HrPerformanceReview>(path, sheetName: "performance_reviews").ToList();
                SaveToJson(perfReviews, Path.Combine(jsonBasePath, "performance_reviews.json"));
                if (perfReviews.Any())
                {
                    var result6 = InitHrPerformanceReviewData(perfReviews);
                    result.Add(result6.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<HrPerformanceReview>(Path.Combine(jsonBasePath, "performance_reviews.json"));
                if (jsonData.Any())
                {
                    var result6 = InitHrPerformanceReviewData(jsonData);
                    result.Add(result6.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[??????? ?????? - Performance Reviews] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            return result;
        }

        #endregion

        #region BusinessPartnerData Seed Data Methods

        /// <summary>
        /// ????? ?????? ??????? ????????? - Business Partners
        /// </summary>
        public (string, object, object) InitBpBusinesspartnerData(List<BpBusinesspartner> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.Partnerid)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[??????? ????????? - Business Partners] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[??????? ????????? - Business Partners] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ???? ?????? BusinessPartnerData - Initialize All BusinessPartnerData
        /// </summary>
        public List<string> InitBusinessPartnerDataSeedData(string path, string WebRootPath, bool clean)
        {
            List<string> result = new();
            var db = DbScoped.SugarScope;
            string jsonBasePath = Path.Combine(WebRootPath, "Database", "SeedData", "BusinessPartnerData");

            if (clean)
            {
                try { db.DbMaintenance.TruncateTable<BpBusinesspartner>(); } catch (Exception ex) { result.Add($"[????? - Clean] BpBusinesspartner: {ex.Message}"); }
            }

            // Load and initialize Business Partners
            try
            {
                var partners = MiniExcel.Query<BpBusinesspartner>(path, sheetName: "business_partners").ToList();
                SaveToJson(partners, Path.Combine(jsonBasePath, "business_partners.json"));
                if (partners.Any())
                {
                    var result1 = InitBpBusinesspartnerData(partners);
                    result.Add(result1.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<BpBusinesspartner>(Path.Combine(jsonBasePath, "business_partners.json"));
                if (jsonData.Any())
                {
                    var result1 = InitBpBusinesspartnerData(jsonData);
                    result.Add(result1.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[??????? ????????? - Business Partners] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            return result;
        }

        #endregion

        #region Partnertype Seed Data Methods

        /// <summary>
        /// ????? ?????? ????? ??????? - Business Partner Types
        /// </summary>
        public (string, object, object) InitBpBusinesspartnertypeData(List<BpBusinesspartnertype> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.Typeid)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ??????? - Business Partner Types] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ??????? - Business Partner Types] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ????? ??????? - Business Partner Status
        /// </summary>
        public (string, object, object) InitBpBusinesspartnerstatusData(List<BpBusinesspartnerstatus> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.Statusid)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ??????? - Business Partner Status] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ??????? - Business Partner Status] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ????? ??????? (?????) - Business Partner Kind
        /// </summary>
        public (string, object, object) InitBpBusinesspartnerkindData(List<BpBusinesspartnerkind> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.Kindid)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ??????? (?????) - Business Partner Kind] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ??????? (?????) - Business Partner Kind] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ??? ??????? - Contact Methods
        /// </summary>
        public (string, object, object) InitBpContactmethodData(List<BpContactmethod> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.Contactmethodenumid)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[??? ??????? - Contact Methods] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[??? ??????? - Contact Methods] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ????? ???????? - Compliance Status
        /// </summary>
        public (string, object, object) InitBpCompliancestatusData(List<BpCompliancestatus> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.Id)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ???????? - Compliance Status] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ???????? - Compliance Status] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ????? ????????? - Partner Document Types
        /// </summary>
        public (string, object, object) InitBpBusinesspartnerdoctypeData(List<BpBusinesspartnerdoctype> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.Id)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ??????? ??????? - Partner Document Types] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ??????? ??????? - Partner Document Types] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ??????? ??????? - Partner Documents
        /// </summary>
        public (string, object, object) InitBpPartnerdocsementsData(List<BpPartnerdocsments> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.Id)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[??????? ??????? - Partner Documents] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[??????? ??????? - Partner Documents] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ???? ?????? Partnertype - Initialize All Partnertype Data
        /// </summary>
        public List<string> InitPartnertypeSeedData(string path, string WebRootPath, bool clean)
        {
            List<string> result = new();
            var db = DbScoped.SugarScope;
            string jsonBasePath = Path.Combine(WebRootPath, "Database", "SeedData", "Partnertype");

            if (clean)
            {
                try { db.DbMaintenance.TruncateTable<BpPartnerdocsments>(); } catch (Exception ex) { result.Add($"[????? - Clean] BpPartnerdocsments: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<BpBusinesspartnerdoctype>(); } catch (Exception ex) { result.Add($"[????? - Clean] BpBusinesspartnerdoctype: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<BpCompliancestatus>(); } catch (Exception ex) { result.Add($"[????? - Clean] BpCompliancestatus: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<BpContactmethod>(); } catch (Exception ex) { result.Add($"[????? - Clean] BpContactmethod: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<BpBusinesspartnerkind>(); } catch (Exception ex) { result.Add($"[????? - Clean] BpBusinesspartnerkind: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<BpBusinesspartnerstatus>(); } catch (Exception ex) { result.Add($"[????? - Clean] BpBusinesspartnerstatus: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<BpBusinesspartnertype>(); } catch (Exception ex) { result.Add($"[????? - Clean] BpBusinesspartnertype: {ex.Message}"); }
            }

            // Load and initialize Business Partner Types
            try
            {
                var partnerTypes = MiniExcel.Query<BpBusinesspartnertype>(path, sheetName: "partner_types").ToList();
                SaveToJson(partnerTypes, Path.Combine(jsonBasePath, "partner_types.json"));
                if (partnerTypes.Any())
                {
                    var result1 = InitBpBusinesspartnertypeData(partnerTypes);
                    result.Add(result1.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<BpBusinesspartnertype>(Path.Combine(jsonBasePath, "partner_types.json"));
                if (jsonData.Any())
                {
                    var result1 = InitBpBusinesspartnertypeData(jsonData);
                    result.Add(result1.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ??????? - Business Partner Types] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Load and initialize Business Partner Status
            try
            {
                var partnerStatus = MiniExcel.Query<BpBusinesspartnerstatus>(path, sheetName: "partner_status").ToList();
                SaveToJson(partnerStatus, Path.Combine(jsonBasePath, "partner_status.json"));
                if (partnerStatus.Any())
                {
                    var result2 = InitBpBusinesspartnerstatusData(partnerStatus);
                    result.Add(result2.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<BpBusinesspartnerstatus>(Path.Combine(jsonBasePath, "partner_status.json"));
                if (jsonData.Any())
                {
                    var result2 = InitBpBusinesspartnerstatusData(jsonData);
                    result.Add(result2.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ??????? - Business Partner Status] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Load and initialize Business Partner Kind
            try
            {
                var partnerKind = MiniExcel.Query<BpBusinesspartnerkind>(path, sheetName: "partner_kind").ToList();
                SaveToJson(partnerKind, Path.Combine(jsonBasePath, "partner_kind.json"));
                if (partnerKind.Any())
                {
                    var result3 = InitBpBusinesspartnerkindData(partnerKind);
                    result.Add(result3.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<BpBusinesspartnerkind>(Path.Combine(jsonBasePath, "partner_kind.json"));
                if (jsonData.Any())
                {
                    var result3 = InitBpBusinesspartnerkindData(jsonData);
                    result.Add(result3.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ??????? (?????) - Business Partner Kind] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Load and initialize Contact Methods
            try
            {
                var contactMethods = MiniExcel.Query<BpContactmethod>(path, sheetName: "contact_methods").ToList();
                SaveToJson(contactMethods, Path.Combine(jsonBasePath, "contact_methods.json"));
                if (contactMethods.Any())
                {
                    var result4 = InitBpContactmethodData(contactMethods);
                    result.Add(result4.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<BpContactmethod>(Path.Combine(jsonBasePath, "contact_methods.json"));
                if (jsonData.Any())
                {
                    var result4 = InitBpContactmethodData(jsonData);
                    result.Add(result4.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[??? ??????? - Contact Methods] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Load and initialize Compliance Status
            try
            {
                var complianceStatus = MiniExcel.Query<BpCompliancestatus>(path, sheetName: "compliance_status").ToList();
                SaveToJson(complianceStatus, Path.Combine(jsonBasePath, "compliance_status.json"));
                if (complianceStatus.Any())
                {
                    var result5 = InitBpCompliancestatusData(complianceStatus);
                    result.Add(result5.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<BpCompliancestatus>(Path.Combine(jsonBasePath, "compliance_status.json"));
                if (jsonData.Any())
                {
                    var result5 = InitBpCompliancestatusData(jsonData);
                    result.Add(result5.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ???????? - Compliance Status] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Load and initialize Partner Document Types
            try
            {
                var docTypes = MiniExcel.Query<BpBusinesspartnerdoctype>(path, sheetName: "partner_doc_types").ToList();
                SaveToJson(docTypes, Path.Combine(jsonBasePath, "partner_doc_types.json"));
                if (docTypes.Any())
                {
                    var result6 = InitBpBusinesspartnerdoctypeData(docTypes);
                    result.Add(result6.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<BpBusinesspartnerdoctype>(Path.Combine(jsonBasePath, "partner_doc_types.json"));
                if (jsonData.Any())
                {
                    var result6 = InitBpBusinesspartnerdoctypeData(jsonData);
                    result.Add(result6.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ??????? ??????? - Partner Document Types] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Load and initialize Partner Documents
            try
            {
                var partnerDocs = MiniExcel.Query<BpPartnerdocsments>(path, sheetName: "partner_documents").ToList();
                SaveToJson(partnerDocs, Path.Combine(jsonBasePath, "partner_documents.json"));
                if (partnerDocs.Any())
                {
                    var result7 = InitBpPartnerdocsementsData(partnerDocs);
                    result.Add(result7.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<BpPartnerdocsments>(Path.Combine(jsonBasePath, "partner_documents.json"));
                if (jsonData.Any())
                {
                    var result7 = InitBpPartnerdocsementsData(jsonData);
                    result.Add(result7.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[??????? ??????? - Partner Documents] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            return result;
        }

        #endregion

        #region Hrtypes & HR Seed Data Methods

        /// <summary>
        /// ????? ?????? ????? ???????? - Leave Type Data
        /// </summary>
        public (string, object, object) InitHrLeaveTypeData(List<ZR.Model.HR.Attendance.LeaveManagement.HrLeaveType> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.LeaveTypeId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ???????? - Leave Types] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ???????? - Leave Types] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ????? ??????? - Holiday Type Data
        /// </summary>
        public (string, object, object) InitHrHolidaytypeData(List<HrHolidaytype> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.Id)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ??????? - Holiday Types] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ??????? - Holiday Types] ??? - Error: {ex.Message}", null, null);
            }
        }

        // TEMPORARILY COMMENTED OUT - BUILD FIX
   
        /// <summary>
        /// ????? ?????? ??????? - Holiday Data
        /// </summary>
        public (string, object, object) InitHrHolidayData(List<HrHoliday> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.Holidayid)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[??????? - Holidays] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[??????? - Holidays] ??? - Error: {ex.Message}", null, null);
            }
        }
      

        /// <summary>
        /// ????? ?????? ?????? ??????? - Attendance Data
        /// COMMENTED OUT - Old model HrAttendanceShift no longer exists
        /// </summary>
        /*
        public (string, object, object) InitHrAttendanceShiftData(List<HrAttendanceShift> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.ShiftId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ????? - Attendance Shifts] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ????? - Attendance Shifts] ??? - Error: {ex.Message}", null, null);
            }
        }
        */

        /// <summary>
        /// ????? ?????? ???????? - Leave Request Data
        /// </summary>
        public (string, object, object) InitHrLeaveData(List<HrLeave> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.LeaveId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ???????? - Leave Requests] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ???????? - Leave Requests] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ???? ???????? - Leave Balance Data
        /// </summary>
        public (string, object, object) InitHrLeaveBalanceData(List<HrLeaveBalance> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.BalanceId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();

                string msg = $"[????? ???????? - Leave Balances] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ???????? - Leave Balances] ??? - Error: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// ????? ?????? ????? ??????? ??????? - Hrtypes Seed Data
        /// </summary>
        public List<string> InitHrtypesSeedData(string path, string WebRootPath, bool clean)
        {
            List<string> result = new();
            string jsonBasePath = Path.Combine(WebRootPath, "Database", "SeedData", "Hrtypes");

            try
            {
                // Initialize Leave Types
                try
                {
                    var leaveTypes = MiniExcel.Query<ZR.Model.HR.Attendance.LeaveManagement.HrLeaveType>(path, sheetName: "leave_type").ToList();
                    if (leaveTypes.Any())
                    {
                        var result1 = InitHrLeaveTypeData(leaveTypes);
                        result.Add(result1.Item1);
                    }
                }
                catch
                {
                    var jsonData = LoadFromJson<ZR.Model.HR.Attendance.LeaveManagement.HrLeaveType>(Path.Combine(jsonBasePath, "hr_leave_type.json"));
                    if (jsonData.Any())
                    {
                        var result1 = InitHrLeaveTypeData(jsonData);
                        result.Add(result1.Item1 + " (?? JSON)");
                    }
                    else
                    {
                        result.Add("[????? ???????? - Leave Types] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                    }
                }

                // Initialize Holiday Types
                try
                {
                    var holidayTypes = MiniExcel.Query<HrHolidaytype>(path, sheetName: "holiday_type").ToList();
                    if (holidayTypes.Any())
                    {
                        var result2 = InitHrHolidaytypeData(holidayTypes);
                        result.Add(result2.Item1);
                    }
                }
                catch
                {
                    var jsonData = LoadFromJson<HrHolidaytype>(Path.Combine(jsonBasePath, "hr_holidaytype.json"));
                    if (jsonData.Any())
                    {
                        var result2 = InitHrHolidaytypeData(jsonData);
                        result.Add(result2.Item1 + " (?? JSON)");
                    }
                    else
                    {
                        result.Add("[????? ??????? - Holiday Types] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Add($"[?????? ????? ??????? ??????? - Hrtypes] ???: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// ????? ?????? ??????? ??????? - HR Seed Data
        /// </summary>
        public List<string> InitHrSeedData(string path, string WebRootPath, bool clean)
        {
            List<string> result = new();
            string jsonBasePath = Path.Combine(WebRootPath, "Database", "SeedData", "HR");

            try
            {
                // Initialize Holidays
                // TEMPORARILY COMMENTED OUT - BUILD FIX
                /*
                try
                {
                    var holidays = MiniExcel.Query<HrHoliday>(path, sheetName: "holiday").ToList();
                    if (holidays.Any())
                    {
                        var result1 = InitHrHolidayData(holidays);
                        result.Add(result1.Item1);
                    }
                }
                catch
                {
                    var jsonData = LoadFromJson<HrHoliday>(Path.Combine(jsonBasePath, "hr_holiday.json"));
                    if (jsonData.Any())
                    {
                        var result1 = InitHrHolidayData(jsonData);
                        result.Add(result1.Item1 + " (?? JSON)");
                    }
                    else
                    {
                        result.Add("[??????? - Holidays] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                    }
                }
                */

                // COMMENTED OUT: Old HrAttendanceShift model no longer exists
                /*
                // Initialize Attendance Shifts
                try
                {
                    var shifts = MiniExcel.Query<HrAttendanceShift>(path, sheetName: "attendance_shift").ToList();
                    if (shifts.Any())
                    {
                        var result2 = InitHrAttendanceShiftData(shifts);
                        result.Add(result2.Item1);
                    }
                }
                catch
                {
                    var jsonData = LoadFromJson<HrAttendanceShift>(Path.Combine(jsonBasePath, "hr_attendance_shift.json"));
                    if (jsonData.Any())
                    {
                        var result2 = InitHrAttendanceShiftData(jsonData);
                        result.Add(result2.Item1 + " (?? JSON)");
                    }
                    else
                    {
                        result.Add("[????? ????? - Attendance Shifts] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                    }
                }
                */

                // Initialize Leave Requests
                try
                {
                    var leaves = MiniExcel.Query<HrLeave>(path, sheetName: "leave").ToList();
                    if (leaves.Any())
                    {
                        var result3 = InitHrLeaveData(leaves);
                        result.Add(result3.Item1);
                    }
                }
                catch
                {
                    var jsonData = LoadFromJson<HrLeave>(Path.Combine(jsonBasePath, "hr_leave.json"));
                    if (jsonData.Any())
                    {
                        var result3 = InitHrLeaveData(jsonData);
                        result.Add(result3.Item1 + " (?? JSON)");
                    }
                    else
                    {
                        result.Add("[????? ???????? - Leave Requests] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                    }
                }

                // Initialize Leave Balances
                try
                {
                    var balances = MiniExcel.Query<HrLeaveBalance>(path, sheetName: "leave_balance").ToList();
                    if (balances.Any())
                    {
                        var result4 = InitHrLeaveBalanceData(balances);
                        result.Add(result4.Item1);
                    }
                }
                catch
                {
                    var jsonData = LoadFromJson<HrLeaveBalance>(Path.Combine(jsonBasePath, "hr_leave_balance.json"));
                    if (jsonData.Any())
                    {
                        var result4 = InitHrLeaveBalanceData(jsonData);
                        result.Add(result4.Item1 + " (?? JSON)");
                    }
                    else
                    {
                        result.Add("[????? ???????? - Leave Balances] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Add($"[?????? ??????? ??????? - HR Data] ???: {ex.Message}");
            }

            return result;
        }

        #endregion

        #region Boulevard Seed Data Methods

        /// <summary>
        /// ????? ?????? ???? ??????? - Boulevard Shop Categories
        /// </summary>
        public (string, object, object) InitBlvShopCategoryData(List<BlvBoulevardShopCategory> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.Id)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();

            string msg = $"[???? ??????? - Boulevard Shop Categories] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        /// <summary>
        /// ????? ?????? ????? ??????? - Boulevard Shop Status
        /// </summary>
        public (string, object, object) InitBlvShopStatusData(List<BlvBoulevardShopStatus> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.Id)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();

            string msg = $"[????? ??????? - Boulevard Shop Status] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        /// <summary>
        /// ????? ?????? ????? ?????? - Boulevard Season Zones
        /// </summary>
        public (string, object, object) InitBlvSeasonZonesData(List<BlvBoulevardSeasonZones> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.Id)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();

            string msg = $"[????? ?????? - Boulevard Season Zones] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        /// <summary>
        /// ????? ?????? ?????? - Boulevard Seasons
        /// </summary>
        public (string, object, object) InitBlvSeasonsData(List<BlvBoulevardSeasons> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.Id)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();

            string msg = $"[?????? - Boulevard Seasons] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        /// <summary>
        /// ????? ?????? ??????? - Boulevard Shops
        /// </summary>
        public (string, object, object) InitBlvShopsData(List<BlvBoulevardShops> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.Id)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();

            string msg = $"[??????? - Boulevard Shops] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        /// <summary>
        /// ????? ?????? ????? ????? ??????? - Customer Registration Forms
        /// </summary>
        public (string, object, object) InitBlvCustomerRegistrationData(List<BlvCustomerregistrationform> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.Id)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();

            string msg = $"[????? ????? ??????? - Customer Registrations] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count} ????? {x.ErrorList.Count} ??????? {x.TotalList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        /// <summary>
        /// ????? ???? ?????? ????????? - Initialize All Boulevard Data
        /// </summary>
        public List<string> InitBoulevardSeedData(string path, string WebRootPath, bool clean)
        {
            List<string> result = new();
            var db = DbScoped.SugarScope;
            string jsonBasePath = Path.Combine(WebRootPath, "Database", "SeedData", "Boulevard");

            if (clean)
            {
                // ??? ???????? ???????? ?????? (???? ???????? ????????)
                try { db.DbMaintenance.TruncateTable<BlvBoulevardShops>(); } catch (Exception ex) { result.Add($"[????? - Clean] BlvBoulevardShops: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<BlvBoulevardSeasonZones>(); } catch (Exception ex) { result.Add($"[????? - Clean] BlvBoulevardSeasonZones: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<BlvBoulevardSeasons>(); } catch (Exception ex) { result.Add($"[????? - Clean] BlvBoulevardSeasons: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<BlvBoulevardShopCategory>(); } catch (Exception ex) { result.Add($"[????? - Clean] BlvBoulevardShopCategory: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<BlvBoulevardShopStatus>(); } catch (Exception ex) { result.Add($"[????? - Clean] BlvBoulevardShopStatus: {ex.Message}"); }
                try { db.DbMaintenance.TruncateTable<BlvCustomerregistrationform>(); } catch (Exception ex) { result.Add($"[????? - Clean] BlvCustomerregistrationform: {ex.Message}"); }
            }

            // Load and initialize Shop Categories
            try
            {
                var shopCategories = MiniExcel.Query<BlvBoulevardShopCategory>(path, sheetName: "boulevard_shop_categories").ToList();
                SaveToJson(shopCategories, Path.Combine(jsonBasePath, "boulevard_shop_categories.json"));
                if (shopCategories.Any())
                {
                    var result1 = InitBlvShopCategoryData(shopCategories);
                    result.Add(result1.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<BlvBoulevardShopCategory>(Path.Combine(jsonBasePath, "boulevard_shop_categories.json"));
                if (jsonData.Any())
                {
                    var result1 = InitBlvShopCategoryData(jsonData);
                    result.Add(result1.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[???? ??????? - Boulevard Shop Categories] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Load and initialize Shop Status
            try
            {
                var shopStatus = MiniExcel.Query<BlvBoulevardShopStatus>(path, sheetName: "boulevard_shop_status").ToList();
                SaveToJson(shopStatus, Path.Combine(jsonBasePath, "boulevard_shop_status.json"));
                if (shopStatus.Any())
                {
                    var result2 = InitBlvShopStatusData(shopStatus);
                    result.Add(result2.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<BlvBoulevardShopStatus>(Path.Combine(jsonBasePath, "boulevard_shop_status.json"));
                if (jsonData.Any())
                {
                    var result2 = InitBlvShopStatusData(jsonData);
                    result.Add(result2.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ??????? - Boulevard Shop Status] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Load and initialize Seasons
            try
            {
                var seasons = MiniExcel.Query<BlvBoulevardSeasons>(path, sheetName: "boulevard_seasons").ToList();
                SaveToJson(seasons, Path.Combine(jsonBasePath, "boulevard_seasons.json"));
                if (seasons.Any())
                {
                    var result3 = InitBlvSeasonsData(seasons);
                    result.Add(result3.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<BlvBoulevardSeasons>(Path.Combine(jsonBasePath, "boulevard_seasons.json"));
                if (jsonData.Any())
                {
                    var result3 = InitBlvSeasonsData(jsonData);
                    result.Add(result3.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[?????? - Boulevard Seasons] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Load and initialize Season Zones
            try
            {
                var seasonZones = MiniExcel.Query<BlvBoulevardSeasonZones>(path, sheetName: "boulevard_season_zones").ToList();
                SaveToJson(seasonZones, Path.Combine(jsonBasePath, "boulevard_season_zones.json"));
                if (seasonZones.Any())
                {
                    var result4 = InitBlvSeasonZonesData(seasonZones);
                    result.Add(result4.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<BlvBoulevardSeasonZones>(Path.Combine(jsonBasePath, "boulevard_season_zones.json"));
                if (jsonData.Any())
                {
                    var result4 = InitBlvSeasonZonesData(jsonData);
                    result.Add(result4.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ?????? - Boulevard Season Zones] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Load and initialize Shops
            try
            {
                var shops = MiniExcel.Query<BlvBoulevardShops>(path, sheetName: "boulevard_shops").ToList();
                SaveToJson(shops, Path.Combine(jsonBasePath, "boulevard_shops.json"));
                if (shops.Any())
                {
                    var result5 = InitBlvShopsData(shops);
                    result.Add(result5.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<BlvBoulevardShops>(Path.Combine(jsonBasePath, "boulevard_shops.json"));
                if (jsonData.Any())
                {
                    var result5 = InitBlvShopsData(jsonData);
                    result.Add(result5.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[??????? - Boulevard Shops] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            // Load and initialize Customer Registrations
            try
            {
                var customerRegs = MiniExcel.Query<BlvCustomerregistrationform>(path, sheetName: "boulevard_customer_registrations").ToList();
                SaveToJson(customerRegs, Path.Combine(jsonBasePath, "boulevard_customer_registrations.json"));
                if (customerRegs.Any())
                {
                    var result6 = InitBlvCustomerRegistrationData(customerRegs);
                    result.Add(result6.Item1);
                }
            }
            catch
            {
                var jsonData = LoadFromJson<BlvCustomerregistrationform>(Path.Combine(jsonBasePath, "boulevard_customer_registrations.json"));
                if (jsonData.Any())
                {
                    var result6 = InitBlvCustomerRegistrationData(jsonData);
                    result.Add(result6.Item1 + " (?? JSON)");
                }
                else
                {
                    result.Add("[????? ????? ??????? - Boulevard Customer Registrations] ?? ??? ?????? ??? ?????? ?? ??? JSON");
                }
            }

            return result;
        }

        #endregion

        #region Comprehensive HR Seed Data Methods (All 90+ Models)

        /// <summary>
        /// COMPREHENSIVE HR INITIALIZATION - All Models
        /// ????? ????? ????? ????? ??????? ??????? - 90+ Entity Classes
        /// </summary>

        // ==================== CORE HR DATA METHODS ====================

        // NOTE: InitHrEmployeeData removed - old HrEmployee model deleted
        // Use EmployeeCoreOrchestrationService.CreateCompleteEmployeeAsync instead

        // ==================== ATTENDANCE & SHIFT MANAGEMENT ====================

        // COMMENTED OUT: Old attendance model no longer exists after restructuring
        /*
        public (string, object, object) InitHrAttendanceData(List<HrAttendance> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.AttendanceId)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();
            string msg = $"[?????? - Attendance] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }
        */

        public (string, object, object) InitHrAttendanceDeviceData(List<HrAttendanceDevice> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.DeviceId)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();
            string msg = $"[????? ?????? - Attendance Devices] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        public (string, object, object) InitHrAttendanceLogData(List<HrAttendanceLog> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.LogId)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            string msg = $"[????? ?????? - Attendance Logs] ?? ??????? {x.InsertList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        // COMMENTED OUT: Old employee shift model no longer exists after restructuring
        /*
        public (string, object, object) InitHrEmployeeShiftData(List<HrEmployeeShift> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.EmployeeShiftId)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();
            string msg = $"[????? ???????? - Employee Shifts] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }
        */

        // ==================== LEAVE & HOLIDAY MANAGEMENT ====================

        public (string, object, object) InitHrLeaveRequestData(List<HrLeaveRequest> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.RequestId)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();
            string msg = $"[????? ???????? - Leave Requests] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        // ==================== PERFORMANCE MANAGEMENT ====================

        public (string, object, object) InitHrKpiData(List<HrKpi> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.KpiId)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();
            string msg = $"[?????? ?????? - KPIs] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        public (string, object, object) InitHrEmployeeKpiData(List<HrEmployeeKpi> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.AssignmentId)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();
            string msg = $"[KPIs ?????? - Employee KPIs] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        public (string, object, object) InitHrPerformanceGoalData(List<HrPerformanceGoal> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.GoalId)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();
            string msg = $"[????? ?????? - Performance Goals] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        // ==================== RECRUITMENT ====================

        public (string, object, object) InitHrJobRequisitionData(List<HrJobRequisition> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.RequisitionId)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();
            string msg = $"[????? ??????? - Job Requisitions] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        public (string, object, object) InitHrCandidateData(List<HrCandidate> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.CandidateId)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();
            string msg = $"[???????? - Candidates] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        public (string, object, object) InitHrInterviewData(List<HrInterview> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.InterviewId)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();
            string msg = $"[????????? - Interviews] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        public (string, object, object) InitHrJobOfferData(List<HrJobOffer> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.OfferId)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();
            string msg = $"[???? ????? - Job Offers] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        // ==================== TRAINING & DEVELOPMENT ====================

        public (string, object, object) InitHrTrainingCourseData(List<HrTrainingCourse> data)
        {
            var db = DbScoped.SugarScope;
            var x = db.Storageable(data)
                .WhereColumns(it => it.CourseId)
                .ToStorage();
            x.AsInsertable.ExecuteCommand();
            x.AsUpdateable.ExecuteCommand();
            string msg = $"[??????? ????????? - Training Courses] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
            return (msg, x.ErrorList, x.IgnoreList);
        }

        public (string, object, object) InitHrTrainingSessionData(List<HrTrainingSession> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.SessionId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[????? ??????? - Training Sessions] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ??????? - Training Sessions] ??? - Error: {ex.Message}", null, null);
            }
        }

        public (string, object, object) InitHrTrainingEnrollmentData(List<HrTrainingEnrollment> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.EnrollmentId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[????????? ????????? - Training Enrollments] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????????? ????????? - Training Enrollments] ??? - Error: {ex.Message}", null, null);
            }
        }

        public (string, object, object) InitHrDevelopmentPlanData(List<HrDevelopmentPlan> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.PlanId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[??? ??????? - Development Plans] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[??? ??????? - Development Plans] ??? - Error: {ex.Message}", null, null);
            }
        }

        // ==================== BENEFITS & INSURANCE ====================

        public (string, object, object) InitHrBenefitPlanData(List<HrBenefitPlan> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.PlanId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[??? ??????? - Benefit Plans] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[??? ??????? - Benefit Plans] ??? - Error: {ex.Message}", null, null);
            }
        }

        public (string, object, object) InitHrBenefitEnrollmentData(List<HrBenefitEnrollment> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.EnrollmentId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[????? ??????? - Benefit Enrollments] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ??????? - Benefit Enrollments] ??? - Error: {ex.Message}", null, null);
            }
        }

        public (string, object, object) InitHrBenefitClaimData(List<HrBenefitClaim> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.ClaimId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[??????? ??????? - Benefit Claims] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[??????? ??????? - Benefit Claims] ??? - Error: {ex.Message}", null, null);
            }
        }

        public (string, object, object) InitHrWellnessProgramData(List<HrWellnessProgram> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.ProgramId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[????? ????? ???????? - Wellness Programs] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????? ????? ???????? - Wellness Programs] ??? - Error: {ex.Message}", null, null);
            }
        }

        // ==================== PAYROLL ====================

        public (string, object, object) InitHrPayrollData(List<HrPayroll> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.PayrollId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[??????? - Payroll] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[??????? - Payroll] ??? - Error: {ex.Message}", null, null);
            }
        }

        public (string, object, object) InitHrLoanData(List<HrLoan> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.LoanId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[?????? - Loans] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[?????? - Loans] ??? - Error: {ex.Message}", null, null);
            }
        }

        // ==================== DOCUMENTS & NOTIFICATIONS ====================

        // NOTE: InitHrEmployeeDocumentData removed - old HrEmployeeDocument model deleted

        public (string, object, object) InitHrNotificationData(List<HrNotification> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.NotificationId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[????????? - Notifications] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[????????? - Notifications] ??? - Error: {ex.Message}", null, null);
            }
        }

        public (string, object, object) InitHrWorkflowData(List<HrWorkflow> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.WorkflowId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[??? ????? - Workflows] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[??? ????? - Workflows] ??? - Error: {ex.Message}", null, null);
            }
        }

        public (string, object, object) InitHrWorkflowTaskData(List<HrWorkflowTask> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.TaskId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[???? ??? ????? - Workflow Tasks] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[???? ??? ????? - Workflow Tasks] ??? - Error: {ex.Message}", null, null);
            }
        }

        // ==================== ANALYTICS & REPORTING ====================

        public (string, object, object) InitHrAnalyticsReportData(List<HrAnalyticsReport> data)
        {
            try
            {
                var db = DbScoped.SugarScope;
                var x = db.Storageable(data)
                    .WhereColumns(it => it.ReportId)
                    .ToStorage();
                x.AsInsertable.ExecuteCommand();
                x.AsUpdateable.ExecuteCommand();
                string msg = $"[?????? ????????? - Analytics Reports] ?? ??????? {x.InsertList.Count} ?? ??????? {x.UpdateList.Count}";
                return (msg, x.ErrorList, x.IgnoreList);
            }
            catch (Exception ex)
            {
                return ($"[?????? ????????? - Analytics Reports] ??? - Error: {ex.Message}", null, null);
            }
        }

        // ==================== ORCHESTRATION METHODS BY CATEGORY ====================

        public List<string> InitHrAttendanceSeedData(string path, string WebRootPath, bool clean)
        {
            List<string> result = new();
            string jsonBasePath = Path.Combine(WebRootPath, "Database", "SeedData", "HR");

            // COMMENTED OUT: Old HrAttendance model no longer exists
            /*
            // Load Attendance
            try
            {
                var attendance = MiniExcel.Query<HrAttendance>(path, sheetName: "attendance").ToList();
                if (attendance.Any())
                    result.Add(InitHrAttendanceData(attendance).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrAttendance>(Path.Combine(jsonBasePath, "hr_attendance.json"));
                if (jsonData.Any())
                    result.Add(InitHrAttendanceData(jsonData).Item1 + " (?? JSON)");
            }
            */

            // Load Attendance Devices
            try
            {
                var devices = MiniExcel.Query<HrAttendanceDevice>(path, sheetName: "attendance_device").ToList();
                if (devices.Any())
                    result.Add(InitHrAttendanceDeviceData(devices).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrAttendanceDevice>(Path.Combine(jsonBasePath, "hr_attendance_device.json"));
                if (jsonData.Any())
                    result.Add(InitHrAttendanceDeviceData(jsonData).Item1 + " (?? JSON)");
            }

            // Load Attendance Logs
            try
            {
                var logs = MiniExcel.Query<HrAttendanceLog>(path, sheetName: "attendance_log").ToList();
                if (logs.Any())
                    result.Add(InitHrAttendanceLogData(logs).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrAttendanceLog>(Path.Combine(jsonBasePath, "hr_attendance_log.json"));
                if (jsonData.Any())
                    result.Add(InitHrAttendanceLogData(jsonData).Item1 + " (?? JSON)");
            }

            // COMMENTED OUT: Old HrEmployeeShift model no longer exists
            /*
            // Load Employee Shifts
            try
            {
                var shifts = MiniExcel.Query<HrEmployeeShift>(path, sheetName: "employee_shift").ToList();
                if (shifts.Any())
                    result.Add(InitHrEmployeeShiftData(shifts).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrEmployeeShift>(Path.Combine(jsonBasePath, "hr_employee_shift.json"));
                if (jsonData.Any())
                    result.Add(InitHrEmployeeShiftData(jsonData).Item1 + " (?? JSON)");
            }
            */

            // COMMENTED OUT: Old HrAttendanceShift model no longer exists
            /*
            // Load Shifts (already implemented)
            try
            {
                var shifts = MiniExcel.Query<HrAttendanceShift>(path, sheetName: "attendance_shift").ToList();
                if (shifts.Any())
                    result.Add(InitHrAttendanceShiftData(shifts).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrAttendanceShift>(Path.Combine(jsonBasePath, "hr_attendance_shift.json"));
                if (jsonData.Any())
                    result.Add(InitHrAttendanceShiftData(jsonData).Item1 + " (?? JSON)");
            }
            */

            return result;
        }

        public List<string> InitHrLeaveSeedData(string path, string WebRootPath, bool clean)
        {
            List<string> result = new();
            string jsonBasePath = Path.Combine(WebRootPath, "Database", "SeedData", "HR");

            // Load Leave Types (from Hrtypes)
            try
            {
                var leaveTypes = MiniExcel.Query<ZR.Model.HR.Attendance.LeaveManagement.HrLeaveType>(path, sheetName: "leave_type").ToList();
                if (leaveTypes.Any())
                    result.Add(InitHrLeaveTypeData(leaveTypes).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<ZR.Model.HR.Attendance.LeaveManagement.HrLeaveType>(Path.Combine(WebRootPath, "Database", "SeedData", "Hrtypes", "hr_leave_type.json"));
                if (jsonData.Any())
                    result.Add(InitHrLeaveTypeData(jsonData).Item1 + " (?? JSON)");
            }

            // Load Holiday Types (from Hrtypes)
            try
            {
                var holidayTypes = MiniExcel.Query<HrHolidaytype>(path, sheetName: "holiday_type").ToList();
                if (holidayTypes.Any())
                    result.Add(InitHrHolidaytypeData(holidayTypes).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrHolidaytype>(Path.Combine(WebRootPath, "Database", "SeedData", "Hrtypes", "hr_holidaytype.json"));
                if (jsonData.Any())
                    result.Add(InitHrHolidaytypeData(jsonData).Item1 + " (?? JSON)");
            }

            // TEMPORARILY COMMENTED OUT - BUILD FIX
            /*
            // Load Holidays
            try
            {
                var holidays = MiniExcel.Query<HrHoliday>(path, sheetName: "holiday").ToList();
                if (holidays.Any())
                    result.Add(InitHrHolidayData(holidays).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrHoliday>(Path.Combine(jsonBasePath, "hr_holiday.json"));
                if (jsonData.Any())
                    result.Add(InitHrHolidayData(jsonData).Item1 + " (?? JSON)");
            }
            */

            // Load Leave Requests
            try
            {
                var leaves = MiniExcel.Query<HrLeaveRequest>(path, sheetName: "leave_request").ToList();
                if (leaves.Any())
                    result.Add(InitHrLeaveRequestData(leaves).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrLeaveRequest>(Path.Combine(jsonBasePath, "hr_leave_request.json"));
                if (jsonData.Any())
                    result.Add(InitHrLeaveRequestData(jsonData).Item1 + " (?? JSON)");
            }

            return result;
        }

        public List<string> InitHrPerformanceSeedData(string path, string WebRootPath, bool clean)
        {
            List<string> result = new();
            string jsonBasePath = Path.Combine(WebRootPath, "Database", "SeedData", "HR");

            // Load KPIs
            try
            {
                var kpis = MiniExcel.Query<HrKpi>(path, sheetName: "kpi").ToList();
                if (kpis.Any())
                    result.Add(InitHrKpiData(kpis).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrKpi>(Path.Combine(jsonBasePath, "hr_kpi.json"));
                if (jsonData.Any())
                    result.Add(InitHrKpiData(jsonData).Item1 + " (?? JSON)");
            }

            // Load Employee KPIs
            try
            {
                var empKpis = MiniExcel.Query<HrEmployeeKpi>(path, sheetName: "employee_kpi").ToList();
                if (empKpis.Any())
                    result.Add(InitHrEmployeeKpiData(empKpis).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrEmployeeKpi>(Path.Combine(jsonBasePath, "hr_employee_kpi.json"));
                if (jsonData.Any())
                    result.Add(InitHrEmployeeKpiData(jsonData).Item1 + " (?? JSON)");
            }

            // Load Performance Reviews
            try
            {
                var reviews = MiniExcel.Query<HrPerformanceReview>(path, sheetName: "performance_review").ToList();
                if (reviews.Any())
                    result.Add(InitHrPerformanceReviewData(reviews).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrPerformanceReview>(Path.Combine(jsonBasePath, "hr_performance_review.json"));
                if (jsonData.Any())
                    result.Add(InitHrPerformanceReviewData(jsonData).Item1 + " (?? JSON)");
            }

            // Load Performance Goals
            try
            {
                var goals = MiniExcel.Query<HrPerformanceGoal>(path, sheetName: "performance_goal").ToList();
                if (goals.Any())
                    result.Add(InitHrPerformanceGoalData(goals).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrPerformanceGoal>(Path.Combine(jsonBasePath, "hr_performance_goal.json"));
                if (jsonData.Any())
                    result.Add(InitHrPerformanceGoalData(jsonData).Item1 + " (?? JSON)");
            }

            return result;
        }

        public List<string> InitHrRecruitmentSeedData(string path, string WebRootPath, bool clean)
        {
            List<string> result = new();
            string jsonBasePath = Path.Combine(WebRootPath, "Database", "SeedData", "HR");

            // Load Job Requisitions
            try
            {
                var requisitions = MiniExcel.Query<HrJobRequisition>(path, sheetName: "job_requisition").ToList();
                if (requisitions.Any())
                    result.Add(InitHrJobRequisitionData(requisitions).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrJobRequisition>(Path.Combine(jsonBasePath, "hr_job_requisition.json"));
                if (jsonData.Any())
                    result.Add(InitHrJobRequisitionData(jsonData).Item1 + " (?? JSON)");
            }

            // Load Candidates
            try
            {
                var candidates = MiniExcel.Query<HrCandidate>(path, sheetName: "candidate").ToList();
                if (candidates.Any())
                    result.Add(InitHrCandidateData(candidates).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrCandidate>(Path.Combine(jsonBasePath, "hr_candidate.json"));
                if (jsonData.Any())
                    result.Add(InitHrCandidateData(jsonData).Item1 + " (?? JSON)");
            }

            // Load Interviews
            try
            {
                var interviews = MiniExcel.Query<HrInterview>(path, sheetName: "interview").ToList();
                if (interviews.Any())
                    result.Add(InitHrInterviewData(interviews).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrInterview>(Path.Combine(jsonBasePath, "hr_interview.json"));
                if (jsonData.Any())
                    result.Add(InitHrInterviewData(jsonData).Item1 + " (?? JSON)");
            }

            // Load Job Offers
            try
            {
                var offers = MiniExcel.Query<HrJobOffer>(path, sheetName: "job_offer").ToList();
                if (offers.Any())
                    result.Add(InitHrJobOfferData(offers).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrJobOffer>(Path.Combine(jsonBasePath, "hr_job_offer.json"));
                if (jsonData.Any())
                    result.Add(InitHrJobOfferData(jsonData).Item1 + " (?? JSON)");
            }

            return result;
        }

        public List<string> InitHrTrainingSeedData(string path, string WebRootPath, bool clean)
        {
            List<string> result = new();
            string jsonBasePath = Path.Combine(WebRootPath, "Database", "SeedData", "HR");

            // Load Training Courses
            try
            {
                var courses = MiniExcel.Query<HrTrainingCourse>(path, sheetName: "training_course").ToList();
                if (courses.Any())
                    result.Add(InitHrTrainingCourseData(courses).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrTrainingCourse>(Path.Combine(jsonBasePath, "hr_training_course.json"));
                if (jsonData.Any())
                    result.Add(InitHrTrainingCourseData(jsonData).Item1 + " (?? JSON)");
            }

            // Load Training Sessions
            try
            {
                var sessions = MiniExcel.Query<HrTrainingSession>(path, sheetName: "training_session").ToList();
                if (sessions.Any())
                    result.Add(InitHrTrainingSessionData(sessions).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrTrainingSession>(Path.Combine(jsonBasePath, "hr_training_session.json"));
                if (jsonData.Any())
                    result.Add(InitHrTrainingSessionData(jsonData).Item1 + " (?? JSON)");
            }

            // Load Training Enrollments
            try
            {
                var enrollments = MiniExcel.Query<HrTrainingEnrollment>(path, sheetName: "training_enrollment").ToList();
                if (enrollments.Any())
                    result.Add(InitHrTrainingEnrollmentData(enrollments).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrTrainingEnrollment>(Path.Combine(jsonBasePath, "hr_training_enrollment.json"));
                if (jsonData.Any())
                    result.Add(InitHrTrainingEnrollmentData(jsonData).Item1 + " (?? JSON)");
            }

            // Load Development Plans
            try
            {
                var plans = MiniExcel.Query<HrDevelopmentPlan>(path, sheetName: "development_plan").ToList();
                if (plans.Any())
                    result.Add(InitHrDevelopmentPlanData(plans).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrDevelopmentPlan>(Path.Combine(jsonBasePath, "hr_development_plan.json"));
                if (jsonData.Any())
                    result.Add(InitHrDevelopmentPlanData(jsonData).Item1 + " (?? JSON)");
            }

            return result;
        }

        public List<string> InitHrPayrollSeedData(string path, string WebRootPath, bool clean)
        {
            List<string> result = new();
            string jsonBasePath = Path.Combine(WebRootPath, "Database", "SeedData", "HR");

            // Load Benefit Plans
            try
            {
                var plans = MiniExcel.Query<HrBenefitPlan>(path, sheetName: "benefit_plan").ToList();
                if (plans.Any())
                    result.Add(InitHrBenefitPlanData(plans).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrBenefitPlan>(Path.Combine(jsonBasePath, "hr_benefit_plan.json"));
                if (jsonData.Any())
                    result.Add(InitHrBenefitPlanData(jsonData).Item1 + " (?? JSON)");
            }

            // Load Payroll
            try
            {
                var payroll = MiniExcel.Query<HrPayroll>(path, sheetName: "payroll").ToList();
                if (payroll.Any())
                    result.Add(InitHrPayrollData(payroll).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrPayroll>(Path.Combine(jsonBasePath, "hr_payroll.json"));
                if (jsonData.Any())
                    result.Add(InitHrPayrollData(jsonData).Item1 + " (?? JSON)");
            }

            // Load Loans
            try
            {
                var loans = MiniExcel.Query<HrLoan>(path, sheetName: "loan").ToList();
                if (loans.Any())
                    result.Add(InitHrLoanData(loans).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrLoan>(Path.Combine(jsonBasePath, "hr_loan.json"));
                if (jsonData.Any())
                    result.Add(InitHrLoanData(jsonData).Item1 + " (?? JSON)");
            }

            // Load Notifications
            try
            {
                var notifications = MiniExcel.Query<HrNotification>(path, sheetName: "notification").ToList();
                if (notifications.Any())
                    result.Add(InitHrNotificationData(notifications).Item1);
            }
            catch
            {
                var jsonData = LoadFromJson<HrNotification>(Path.Combine(jsonBasePath, "hr_notification.json"));
                if (jsonData.Any())
                    result.Add(InitHrNotificationData(jsonData).Item1 + " (?? JSON)");
            }

            return result;
        }

        /// <summary>
        /// ????? ????? ????? ?????? ??????? ???????
        /// Initialize ALL HR Seed Data (90+ Models)
        /// </summary>
        public List<string> InitAllHrSeedData(string path, string WebRootPath, bool clean)
        {
            List<string> result = new();
            result.Add("=== ????? ????? ??????? ??????? ??????? ===");
            result.Add("=== Comprehensive HR Seed Data Initialization ===");
            result.Add("");

            try
            {
                // Initialize by category
                var attendanceResult = InitHrAttendanceSeedData(path, WebRootPath, clean);
                result.AddRange(attendanceResult);

                var leaveResult = InitHrLeaveSeedData(path, WebRootPath, clean);
                result.AddRange(leaveResult);

                var performanceResult = InitHrPerformanceSeedData(path, WebRootPath, clean);
                result.AddRange(performanceResult);

                var recruitmentResult = InitHrRecruitmentSeedData(path, WebRootPath, clean);
                result.AddRange(recruitmentResult);

                var trainingResult = InitHrTrainingSeedData(path, WebRootPath, clean);
                result.AddRange(trainingResult);

                var payrollResult = InitHrPayrollSeedData(path, WebRootPath, clean);
                result.AddRange(payrollResult);

                result.Add("");
                result.Add($"[?????? - Total] ?? ????? ?????? ??????? ??????? ?????");
            }
            catch (Exception ex)
            {
                result.Add($"[??? - Error] {ex.Message}");
            }

            return result;
        }

        #endregion
    }
}

