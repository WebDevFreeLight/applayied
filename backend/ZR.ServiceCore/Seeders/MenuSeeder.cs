using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Infrastructure;
using SqlSugar;
using ZR.Model.System;

namespace ZR.ServiceCore.Seeders
{
    /// <summary>
    /// Menu and Permission Seeder
    /// Populates sys_menu table from JSON seeder files
    /// </summary>
    public class MenuSeeder
    {
        private readonly ISqlSugarClient _db;
        private readonly string _seederPath;
        private readonly string _scriptsPath;

        public MenuSeeder(ISqlSugarClient db, string seederPath = null)
        {
            _db = db;
            _seederPath = seederPath ?? Path.Combine(AppContext.BaseDirectory, "Database", "Seeders");
            // Scripts folder is at the backend root level
            _scriptsPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Scripts");
        }

        /// <summary>
        /// Execute the menu seeding process using All_Permissions_Complete.json
        /// </summary>
        public async Task<SeederResult> SeedAsync(bool truncateFirst = false)
        {
            var result = new SeederResult();

            try
            {
                // First try to read from All_Permissions_Complete.json (new format with fixed IDs)
                // Check in Database/Seeders folder first (same as other seeder files)
                var completeJsonPath = Path.Combine(_seederPath, "All_Permissions_Complete.json");

                // Fallback paths to find the JSON file
                if (!File.Exists(completeJsonPath))
                {
                    completeJsonPath = Path.Combine(_scriptsPath, "All_Permissions_Complete.json");
                }
                if (!File.Exists(completeJsonPath))
                {
                    completeJsonPath = Path.Combine(AppContext.BaseDirectory, "Scripts", "All_Permissions_Complete.json");
                }
                if (!File.Exists(completeJsonPath))
                {
                    // Try relative to project root
                    var baseDir = AppContext.BaseDirectory;
                    var projectRoot = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.Parent?.FullName;
                    if (projectRoot != null)
                    {
                        completeJsonPath = Path.Combine(projectRoot, "Scripts", "All_Permissions_Complete.json");
                    }
                }

                if (File.Exists(completeJsonPath))
                {
                    return await SeedFromCompleteJsonAsync(completeJsonPath, truncateFirst);
                }

                // Fallback to old menus.seeder.json format
                var menusJson = await File.ReadAllTextAsync(Path.Combine(_seederPath, "menus.seeder.json"));
                var menuItems = JsonSerializer.Deserialize<List<MenuSeederItem>>(menusJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (menuItems == null || menuItems.Count == 0)
                {
                    result.Success = false;
                    result.Message = "No menu items found in seeder file";
                    return result;
                }

                // Truncate existing menus if requested
                if (truncateFirst)
                {
                    await _db.Deleteable<SysMenu>().ExecuteCommandAsync();
                    result.DeletedCount = await _db.Queryable<SysMenu>().CountAsync();
                }

                // Insert menus recursively
                result.InsertedCount = await InsertMenuHierarchy(menuItems, 0);

                result.Success = true;
                result.Message = $"Successfully seeded {result.InsertedCount} menu items";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Seeding failed: {ex.Message}";
                result.Exception = ex;
            }

            return result;
        }

        /// <summary>
        /// Seed from All_Permissions_Complete.json format (with fixed menuIds)
        /// </summary>
        private async Task<SeederResult> SeedFromCompleteJsonAsync(string jsonPath, bool truncateFirst)
        {
            var result = new SeederResult();
            var insertedMenuIds = new List<long>();

            try
            {
                var json = await File.ReadAllTextAsync(jsonPath);
                var completeData = JsonSerializer.Deserialize<CompletePermissionsFile>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (completeData?.Modules == null || completeData.Modules.Count == 0)
                {
                    result.Success = false;
                    result.Message = "No modules found in All_Permissions_Complete.json";
                    return result;
                }

                // Truncate existing menus and role-menu assignments if requested
                if (truncateFirst)
                {
                    var existingCount = await _db.Queryable<SysMenu>().CountAsync();
                    await _db.Deleteable<SysRoleMenu>().ExecuteCommandAsync();
                    await _db.Deleteable<SysMenu>().ExecuteCommandAsync();
                    result.DeletedCount = existingCount;
                }

                // Insert all modules and collect menu IDs
                foreach (var module in completeData.Modules)
                {
                    result.InsertedCount += await InsertCompleteMenuItem(module, insertedMenuIds);
                }

                // Assign all menus to admin role (role_id = 1)
                var roleMenuAssignments = await AssignMenusToAdminRole(insertedMenuIds);

                result.Success = true;
                result.Message = $"Successfully seeded {result.InsertedCount} menu items and assigned {roleMenuAssignments} role-menu mappings from All_Permissions_Complete.json";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Seeding failed: {ex.Message}";
                result.Exception = ex;
            }

            return result;
        }

        /// <summary>
        /// Assign ALL existing menus in sys_menu to admin role (role_id = 1)
        /// Use this to fix the case where menus exist but are not assigned to any role
        /// </summary>
        public async Task<SeederResult> AssignAllMenusToAdminRoleAsync()
        {
            var result = new SeederResult();

            try
            {
                // Get all existing menu IDs
                var allMenuIds = await _db.Queryable<SysMenu>()
                    .Select(m => m.MenuId)
                    .ToListAsync();

                if (allMenuIds == null || allMenuIds.Count == 0)
                {
                    result.Success = false;
                    result.Message = "No menus found in sys_menu table";
                    return result;
                }

                var assignedCount = await AssignMenusToAdminRole(allMenuIds);

                result.Success = true;
                result.InsertedCount = assignedCount;
                result.Message = $"Successfully assigned {assignedCount} menus to admin role (total menus: {allMenuIds.Count})";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Failed to assign menus to admin role: {ex.Message}";
                result.Exception = ex;
            }

            return result;
        }

        /// <summary>
        /// Assign all menu IDs to admin role (role_id = 1)
        /// </summary>
        private async Task<int> AssignMenusToAdminRole(List<long> menuIds)
        {
            const long adminRoleId = 1;
            int assignedCount = 0;

            // Get existing role-menu assignments for admin
            var existingMenuIds = await _db.Queryable<SysRoleMenu>()
                .Where(rm => rm.Role_id == adminRoleId)
                .Select(rm => rm.Menu_id)
                .ToListAsync();

            // Insert only new assignments
            foreach (var menuId in menuIds.Distinct())
            {
                if (!existingMenuIds.Contains(menuId))
                {
                    try
                    {
                        await _db.Insertable(new SysRoleMenu
                        {
                            Role_id = adminRoleId,
                            Menu_id = menuId
                        }).ExecuteCommandAsync();
                        assignedCount++;
                    }
                    catch (Exception)
                    {
                        // Skip if already exists (concurrent insert or constraint)
                    }
                }
            }

            return assignedCount;
        }

        /// <summary>
        /// Recursively insert menu from Complete JSON format
        /// </summary>
        private async Task<int> InsertCompleteMenuItem(CompleteMenuItem item, List<long> insertedMenuIds = null)
        {
            int insertedCount = 0;

            // Create SysMenu entity with the fixed menuId from JSON
            var menu = new SysMenu
            {
                MenuId = item.MenuId,
                ParentId = item.ParentId,
                MenuName = item.MenuName,
                MenuNameKey = item.MenuNameAr ?? item.MenuName, // Use Arabic name as key
                Path = item.Path ?? "",
                Component = item.Component,
                MenuType = item.MenuType ?? "M",
                Visible = "0",
                Status = "0",
                Perms = item.Perms ?? "",
                Icon = item.Icon ?? "#",
                OrderNum = item.OrderNum,
                IsFrame = "1",
                IsCache = "0",
                Remark = item.MenuNameAr ?? item.MenuName,
                Create_by = "admin",
                Create_time = DateTime.Now
            };

            // Insert menu (ignore if already exists)
            try
            {
                await _db.Insertable(menu).ExecuteCommandAsync();
                insertedCount++;
                insertedMenuIds?.Add(item.MenuId);
            }
            catch (Exception)
            {
                // Menu might already exist, try update instead
                await _db.Updateable(menu).ExecuteCommandAsync();
                insertedCount++;
                insertedMenuIds?.Add(item.MenuId);
            }

            // Insert children recursively
            if (item.Children != null && item.Children.Count > 0)
            {
                foreach (var child in item.Children)
                {
                    insertedCount += await InsertCompleteMenuItem(child, insertedMenuIds);
                }
            }

            return insertedCount;
        }

        /// <summary>
        /// Recursively insert menu hierarchy (old format)
        /// </summary>
        private async Task<int> InsertMenuHierarchy(List<MenuSeederItem> items, long parentId, int currentOrder = 0)
        {
            int insertedCount = 0;

            foreach (var item in items)
            {
                // Create SysMenu entity
                var menu = new SysMenu
                {
                    MenuId = SnowFlakeSingle.Instance.NextId(),
                    ParentId = parentId,
                    MenuName = item.MenuName,
                    MenuNameKey = item.MenuNameKey,
                    Path = item.Path,
                    Component = item.Component,
                    MenuType = item.MenuType,
                    Visible = item.Visible ?? "0",
                    Status = item.Status ?? "0",
                    Perms = item.Perms,
                    Icon = item.Icon ?? "#",
                    OrderNum = item.OrderNum > 0 ? item.OrderNum : currentOrder,
                    IsFrame = "1", // Default to not iframe (string)
                    IsCache = "0", // Default to cache (string)
                    Remark = item.MenuNameKey // Store translation key in remark
                };

                // Insert menu
                await _db.Insertable(menu).ExecuteCommandAsync();
                insertedCount++;

                // Insert children recursively
                if (item.Children != null && item.Children.Count > 0)
                {
                    insertedCount += await InsertMenuHierarchy(item.Children, menu.MenuId);
                }

                currentOrder++;
            }

            return insertedCount;
        }

        /// <summary>
        /// Get all permissions from permissions.seeder.json
        /// </summary>
        public async Task<List<PermissionModule>> GetPermissionsAsync()
        {
            var permissionsJson = await File.ReadAllTextAsync(Path.Combine(_seederPath, "permissions.seeder.json"));
            return JsonSerializer.Deserialize<List<PermissionModule>>(permissionsJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        /// <summary>
        /// Get mapping review data
        /// </summary>
        public async Task<MappingReview> GetMappingReviewAsync()
        {
            var mappingJson = await File.ReadAllTextAsync(Path.Combine(_seederPath, "mapping.review.json"));
            return JsonSerializer.Deserialize<MappingReview>(mappingJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }

    #region Seeder Models

    public class MenuSeederItem
    {
        public string MenuName { get; set; }
        public string MenuNameKey { get; set; }
        public string Path { get; set; }
        public string Component { get; set; }
        public string MenuType { get; set; }
        public string Icon { get; set; }
        public string Visible { get; set; }
        public string Status { get; set; }
        public int OrderNum { get; set; }
        public string Perms { get; set; }
        public List<MenuSeederItem> Children { get; set; }
    }

    public class PermissionModule
    {
        public string Module { get; set; }
        public string Description { get; set; }
        public List<string> Permissions { get; set; }
    }

    public class MappingReview
    {
        public Dictionary<string, object> Metadata { get; set; }
        public Dictionary<string, RouteMapping> Mappings { get; set; }
        public Dictionary<string, string> Assumptions { get; set; }
        public List<AmbiguousCase> AmbiguousCases { get; set; }
        public List<string> Recommendations { get; set; }
    }

    public class RouteMapping
    {
        public string MenuType { get; set; }
        public string MenuPermission { get; set; }
        public List<string> ActionPermissions { get; set; }
        public string Confidence { get; set; }
        public string Notes { get; set; }
    }

    public class AmbiguousCase
    {
        public string Route { get; set; }
        public string Issue { get; set; }
        public string Resolution { get; set; }
        public string Risk { get; set; }
    }

    public class SeederResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int InsertedCount { get; set; }
        public int DeletedCount { get; set; }
        public Exception Exception { get; set; }

        public override string ToString()
        {
            return Success
                ? $"✅ {Message} (Inserted: {InsertedCount}, Deleted: {DeletedCount})"
                : $"❌ {Message}";
        }
    }

    /// <summary>
    /// Complete permissions file structure (All_Permissions_Complete.json)
    /// </summary>
    public class CompletePermissionsFile
    {
        public PermissionsMetadata Metadata { get; set; }
        public List<CompleteMenuItem> Modules { get; set; }
    }

    public class PermissionsMetadata
    {
        public string Version { get; set; }
        public string GeneratedDate { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> MenuTypes { get; set; }
    }

    /// <summary>
    /// Menu item from All_Permissions_Complete.json
    /// </summary>
    public class CompleteMenuItem
    {
        public long MenuId { get; set; }
        public string MenuName { get; set; }
        public string MenuNameAr { get; set; }
        public long ParentId { get; set; }
        public int OrderNum { get; set; }
        public string Path { get; set; }
        public string Component { get; set; }
        public string MenuType { get; set; }
        public string Perms { get; set; }
        public string Icon { get; set; }
        public List<CompleteMenuItem> Children { get; set; }
    }

    #endregion
}
