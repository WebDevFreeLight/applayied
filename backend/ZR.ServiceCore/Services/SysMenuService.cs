using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Model;
using Mapster;
using Microsoft.AspNetCore.Http;
using ZR.Admin.WebApi.Extensions;
using ZR.Common;
using ZR.Model.System;
using ZR.Model.System.Dto;
using ZR.Model.System.Enums;
using ZR.Model.System.Generate;
using ZR.Model.System.Vo;

namespace ZR.ServiceCore.Services
{
    /// <summary>
    /// Menu
    /// </summary>
    [AppService(ServiceType = typeof(ISysMenuService), ServiceLifetime = LifeTime.Transient)]
    public class SysMenuService : BaseService<SysMenu>, ISysMenuService
    {
        public ISysRoleService SysRoleService;

        public SysMenuService(ISysRoleService sysRoleService)
        {
            SysRoleService = sysRoleService;
        }

        /// <summary>
        /// Get all menu data (menu management)
        /// </summary>
        /// <returns></returns>
        public List<SysMenu> SelectTreeMenuList(MenuQueryDto menu, long userId)
        {
            if (menu.ParentId != null)
            {
                return GetMenusByMenuId(menu.ParentId.ParseToInt(), userId);
            }
            List<SysMenu> menuList = BuildMenuTree(SelectMenuList(menu, userId));
            return menuList;
        }

        /// <summary>
        /// Get all menu list
        /// </summary>
        /// <returns></returns>
        public List<SysMenu> SelectMenuList(MenuQueryDto menu, long userId)
        {
            // ✅ FIX: Always respect role-menu assignments, even for admin users
            var userRoles = SysRoleService.SelectUserRoles(userId);
            return SelectMenuListByRoles(menu, userRoles);
        }

        /// <summary>
        /// Get menu details
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        public SysMenu GetMenuByMenuId(long menuId)
        {
            return GetFirst(it => it.MenuId == menuId);
        }

        /// <summary>
        /// Get menu list by menu id
        /// </summary>
        /// <param name="menuId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<SysMenu> GetMenusByMenuId(long menuId, long userId)
        {
            var menuExpression = Expressionable.Create<SysMenu>();
            menuExpression.And(c => c.ParentId == menuId);

            if (!SysRoleService.IsAdmin(userId))
            {
                var userRoles = SysRoleService.SelectUserRoles(userId);
                var roleMenus = Context.Queryable<SysRoleMenu>()
                    .Where(r => userRoles.Contains(r.Role_id)).Select(s => s.Menu_id).ToList();

                menuExpression.And(c => roleMenus.Contains(c.MenuId));
            }
            var list = GetList(menuExpression.ToExpression()).OrderBy(f => f.OrderNum).ToList();
            Context.ThenMapper(list, item =>
            {
                item.SubNum = Context.Queryable<SysMenu>().SetContext(x => x.ParentId, () => item.MenuId, item).Count;
            });
            return list;
        }

        /// <summary>
        /// Add menu
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public long AddMenu(SysMenu menu)
        {


            Insertable(menu).ExecuteReturnSnowflakeId();
            //Insertable(menu).ExecuteReturnEntity();

            long v = menu.MenuId;
            return v;
        }

        /// <summary>
        /// Edit menu
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public long EditMenu(SysMenu menu)
        {
            menu.Icon = string.IsNullOrEmpty(menu.Icon) ? "" : menu.Icon;
            return Update(menu, false);
        }

        /// <summary>
        /// Delete menu management information
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        public int DeleteMenuById(long menuId)
        {
            return Delete(menuId);
        }

        /// <summary>
        /// Delete all menus
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        public int DeleteAllMenuById(long menuId)
        {
            var childMenu = Queryable().ToChildList(x => x.ParentId, menuId).Select(x => x.MenuId);
            var result = UseTran(() =>
            {
                Delete(childMenu.ToArray(), "Delete menu");
                Context.Deleteable<SysRoleMenu>().Where(f => childMenu.Contains(f.Menu_id)).ExecuteCommand();
            });
            return result.IsSuccess ? 1 : 0;
        }

        /// <summary>
        /// Check if menu name is unique
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public string CheckMenuNameUnique(SysMenu menu)
        {
            long menuId = menu.MenuId == 0 ? -1 : menu.MenuId;
            SysMenu info = GetFirst(it => it.MenuName == menu.MenuName && it.ParentId == menu.ParentId);
            if (menu.MenuType == "C")
            {
                SysMenu info2 = GetFirst(it => it.Path == menu.Path);
                if (info2 != null)
                {
                    throw new CustomException($"Route path {menu.Path} already exists");
                }
            }

            //if (info != null && menuId != info.menuId && menu.menuName.Equals(info.menuName))
            //{
            //    return UserConstants.NOT_UNIQUE;
            //}
            if (info != null && info.MenuId != menu.MenuId)
            {
                return UserConstants.NOT_UNIQUE;
            }
            return UserConstants.UNIQUE;
        }

        /// <summary>
        /// Menu sorting
        /// </summary>
        /// <param name="menuDto"></param>
        /// <returns></returns>
        public int ChangeSortMenu(MenuDto menuDto)
        {
            return Update(new SysMenu() { MenuId = menuDto.MenuId, OrderNum = menuDto.OrderNum }, it => new { it.OrderNum });
        }

        /// <summary>
        /// Check if menu has child nodes
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        public bool HasChildByMenuId(long menuId)
        {
            return Count(it => it.ParentId == menuId) > 0;
        }

        /// <summary>
        /// Get left navigation menu tree
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<SysMenu> SelectMenuTreeByUserId(long userId)
        {
            MenuQueryDto dto = new() { Status = "0", MenuTypeIds = "M,C,L" };

            // ✅ FIX: Always respect role-menu assignments, even for admin users
            // This ensures the permission system works correctly
            List<long> roleIds = SysRoleService.SelectUserRoles(userId);
            return SelectTreeMenuListByRoles(dto, roleIds);
        }

        /// <summary>
        /// Query precise operation permissions for buttons
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<string> SelectMenuPermsByUserId(long userId)
        {
            var menus = Context.Queryable<SysMenu, SysRoleMenu, SysUserRole, SysRole>((m, rm, ur, r) => new JoinQueryInfos(
                JoinType.Left, m.MenuId == rm.Menu_id,
                JoinType.Left, rm.Role_id == ur.RoleId,
                JoinType.Left, ur.RoleId == r.RoleId
                ))
                .WithCache(60 * 10)
                .Where((m, rm, ur, r) => m.Status == "0" && r.Status == 0 && ur.UserId == userId)
                .Select((m, rm, ur, r) => m).ToList();
            var menuList = menus.Where(f => !string.IsNullOrEmpty(f.Perms));

            return menuList.Select(x => x.Perms).Distinct().ToList();
        }

        /// <summary>
        /// Query system menu list by user
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="roles">User role collection</param>
        /// <returns></returns>
        public List<SysMenu> SelectTreeMenuListByRoles(MenuQueryDto menu, List<long> roles)
        {
            // Get only explicitly assigned menu IDs for the user's roles
            var roleMenus = Context.Queryable<SysRoleMenu>()
                .Where(r => roles.Contains(r.Role_id))
                .Select(f => f.Menu_id).Distinct().ToList();

            // ✅ FIX: Get only assigned menus + their parent menus (needed for tree structure)
            // First, get all assigned menus
            var assignedMenus = Queryable()
                .Where(c => roleMenus.Contains(c.MenuId))
                .WhereIF(!string.IsNullOrEmpty(menu.MenuName), (c) => c.MenuName.Contains(menu.MenuName))
                .WhereIF(!string.IsNullOrEmpty(menu.Visible), (c) => c.Visible == menu.Visible)
                .WhereIF(!string.IsNullOrEmpty(menu.Status), (c) => c.Status == menu.Status)
                .WhereIF(!string.IsNullOrEmpty(menu.MenuTypeIds), c => menu.MenuTypeIdArr.Contains(c.MenuType))
                .ToList();

            // Get parent menu IDs needed for tree structure (but not explicitly assigned)
            var parentMenuIds = assignedMenus
                .Where(m => m.ParentId != 0)
                .Select(m => m.ParentId)
                .Distinct()
                .ToList();

            // Get all menu IDs to include (assigned + parents)
            var allMenuIds = roleMenus.Concat(parentMenuIds).Distinct().ToList();

            // Build tree with assigned menus + necessary parents
            return Queryable()
                .Where(c => allMenuIds.Contains(c.MenuId))
                .WhereIF(!string.IsNullOrEmpty(menu.MenuName), (c) => c.MenuName.Contains(menu.MenuName))
                .WhereIF(!string.IsNullOrEmpty(menu.Visible), (c) => c.Visible == menu.Visible)
                .WhereIF(!string.IsNullOrEmpty(menu.Status), (c) => c.Status == menu.Status)
                .WhereIF(!string.IsNullOrEmpty(menu.MenuTypeIds), c => menu.MenuTypeIdArr.Contains(c.MenuType))
                .OrderBy((c) => new { c.ParentId, c.OrderNum })
                .ToTree(it => it.Children, it => it.ParentId, 0);
        }

        /// <summary>
        /// Query system menu list by user
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="roleId">User role</param>
        /// <returns></returns>
        public List<RoleMenuExportDto> SelectRoleMenuListByRole(MenuQueryDto menu, int roleId)
        {
            var menuIds = Context.Queryable<SysRoleMenu>()
                .Where(r => r.Role_id == roleId)
                .Select(f => f.Menu_id).Distinct().ToList();

            return Context.Queryable<SysMenu>()
                .InnerJoin<SysMenu>((t1, t2) => t1.MenuId == t2.ParentId)
                .InnerJoin<SysMenu>((t1, t2, t3) => t2.MenuId == t3.ParentId)
                .Where((t1, t2, t3) => menuIds.Contains(t1.MenuId))
                .Select((t1, t2, t3) => new RoleMenuExportDto()
                {
                    MenuName = $"{t1.MenuName}->{t2.MenuName}->{t3.MenuName}",
                    Path = t2.Path,
                    Component = t2.Component,
                    Perms = t3.Perms,
                    MenuType = (MenuType)(object)t3.MenuType,
                    Status = (MenuStatus)(object)t3.Status
                }).ToList();
        }

        /// <summary>
        /// Get all menus
        /// </summary>
        /// <returns></returns>
        private List<SysMenu> SelectMenuList(MenuQueryDto menu)
        {
            var menuExp = Expressionable.Create<SysMenu>();
            menuExp.AndIF(!string.IsNullOrEmpty(menu.MenuName), it => it.MenuName.Contains(menu.MenuName));
            menuExp.AndIF(!string.IsNullOrEmpty(menu.Visible), it => it.Visible == menu.Visible);
            menuExp.AndIF(!string.IsNullOrEmpty(menu.Status), it => it.Status == menu.Status);
            menuExp.AndIF(!string.IsNullOrEmpty(menu.MenuTypeIds), it => menu.MenuTypeIdArr.Contains(it.MenuType));
            menuExp.AndIF(menu.ParentId != null, it => it.ParentId == menu.ParentId);

            return Queryable()
            .Where(menuExp.ToExpression())
            .OrderBy(it => new { it.ParentId, it.OrderNum })
            .ToList();
        }

        /// <summary>
        /// Query system menu list by user
        /// </summary>
        /// <param name="sysMenu"></param>
        /// <param name="roles">User role collection</param>
        /// <returns></returns>
        private List<SysMenu> SelectMenuListByRoles(MenuQueryDto sysMenu, List<long> roles)
        {
            var roleMenus = Context.Queryable<SysRoleMenu>()
                .Where(r => roles.Contains(r.Role_id));

            return Queryable()
                .InnerJoin(roleMenus, (c, j) => c.MenuId == j.Menu_id)
                .Where((c, j) => c.Status == "0")
                .WhereIF(!string.IsNullOrEmpty(sysMenu.MenuName), (c, j) => c.MenuName.Contains(sysMenu.MenuName))
                .WhereIF(!string.IsNullOrEmpty(sysMenu.Visible), (c, j) => c.Visible == sysMenu.Visible)
                .OrderBy((c, j) => new { c.ParentId, c.OrderNum })
                .Select(c => c)
                .ToList();
        }

        /// <summary>
        /// Get all menus (menu management)
        /// </summary>
        /// <returns></returns>
        public List<SysMenu> SelectTreeMenuList(MenuQueryDto menu)
        {
            long parentId = menu.ParentId != null ? (long)menu.ParentId : 0;

            var list = Queryable()
                //.WithCache(60 * 10)
                .WhereIF(!string.IsNullOrEmpty(menu.MenuName), it => it.MenuName.Contains(menu.MenuName))
                .WhereIF(!string.IsNullOrEmpty(menu.Visible), it => it.Visible == menu.Visible)
                .WhereIF(!string.IsNullOrEmpty(menu.Status), it => it.Status == menu.Status)
                .WhereIF(!string.IsNullOrEmpty(menu.MenuTypeIds), it => menu.MenuTypeIdArr.Contains(it.MenuType))
                .WhereIF(menu.ParentId != null, it => it.ParentId == menu.ParentId)
                .OrderBy(it => new { it.ParentId, it.OrderNum })
                .ToTree(it => it.Children, it => it.ParentId, parentId);

            return list;
        }
        #region Methods

        /// <summary>
        /// Recursive list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="t"></param>
        private void RecursionFn(List<SysMenu> list, SysMenu t)
        {
            //Get child node list
            List<SysMenu> childList = GetChildList(list, t);
            t.Children = childList;
            foreach (var item in childList)
            {
                if (GetChildList(list, item).Count() > 0)
                {
                    RecursionFn(list, item);
                }
            }
        }

        /// <summary>
        /// Recursively get child menus
        /// </summary>
        /// <param name="list">All menus</param>
        /// <param name="sysMenu"></param>
        /// <returns></returns>
        private List<SysMenu> GetChildList(List<SysMenu> list, SysMenu sysMenu)
        {
            return list.Where(p => p.ParentId == sysMenu.MenuId).ToList();
        }

        /// <summary>
        /// Get route sidebar, dynamically generated
        /// </summary>
        /// <param name="menus"></param>
        /// <returns></returns>
        public List<RouterVo> BuildMenus(List<SysMenu> menus)
        {
            List<RouterVo> routers = new();
            if (menus == null) return routers;

            foreach (var menu in menus)
            {
                RouterVo router = new()
                {
                    Query = menu.Query,
                    Hidden = "1".Equals(menu.Visible),
                    Name = GetRouteName(menu),
                    Path = GetRoutePath(menu),
                    Component = GetComponent(menu),
                    Meta = new Meta(menu.MenuName, menu.Icon, "1".Equals(menu.IsCache), menu.MenuNameKey, menu.Path, menu.Create_time)
                };

                List<SysMenu> cMenus = menu.Children;
                //Is directory and has child menus
                if (cMenus != null && cMenus.Count > 0 && UserConstants.TYPE_DIR.Equals(menu.MenuType))
                {
                    router.AlwaysShow = true;
                    router.Redirect = "noRedirect";
                    router.Children = BuildMenus(cMenus);
                }
                else if (IsMeunFrame(menu))
                {
                    router.Meta = null;
                    List<RouterVo> childrenList = [];
                    RouterVo children = new()
                    {
                        Query = menu.Query,
                        Path = menu.Path,
                        Component = menu.Component,
                        Name = menu.RouteName.IsNotEmpty() ? menu.RouteName : menu.Path.ToLower(),
                        Meta = new Meta(menu.MenuName, menu.Icon, "1".Equals(menu.IsCache), menu.MenuNameKey, menu.Path, menu.Create_time)
                    };
                    childrenList.Add(children);
                    router.Children = childrenList;
                }
                else if (menu.ParentId == 0 && IsInnerLink(menu))
                {
                    router.Meta = new Meta(menu.MenuName, menu.Icon);
                    router.Path = "/";
                    List<RouterVo> childrenList = new();
                    RouterVo children = new();
                    string routerPath = InnerLinkReplaceEach(menu.Path);
                    children.Query = menu.Query;
                    children.Path = routerPath;
                    children.Component = UserConstants.INNER_LINK;
                    children.Name = menu.RouteName.IsNotEmpty() ? menu.RouteName : routerPath.ToLower();
                    children.Meta = new Meta(menu.MenuName, menu.Icon, menu.Path);
                    childrenList.Add(children);
                    router.Children = childrenList;
                }
                routers.Add(router);
            }

            return routers;
        }

        /// <summary>
        /// Build dropdown tree structure required by frontend
        /// </summary>
        /// <param name="menus">Menu list</param>
        /// <returns>Dropdown tree structure list</returns>
        public List<SysMenu> BuildMenuTree(List<SysMenu> menus)
        {
            List<SysMenu> returnList = new();
            List<long> tempList = menus.Select(f => f.MenuId).ToList();

            foreach (var menu in menus)
            {
                // If it's a top-level node, traverse all child nodes of this parent node
                if (!tempList.Contains(menu.ParentId))
                {
                    var menuInfo = menus.Find(f => f.MenuId == menu.MenuId);
                    //Remove buttons that don't have parent
                    if (!tempList.Contains(menuInfo.ParentId) && menu.MenuType != "F")
                    {
                        RecursionFn(menus, menu);
                        returnList.Add(menu);
                    }
                }
            }
            if (!returnList.Any())
            {
                returnList = menus;
            }
            return returnList;
        }

        /// <summary>
        /// Build dropdown tree structure required by frontend
        /// </summary>
        /// <param name="menus"></param>
        /// <returns></returns>
        public List<TreeSelectVo> BuildMenuTreeSelect(List<SysMenu> menus)
        {
            List<SysMenu> menuTrees = BuildMenuTree(menus);
            List<TreeSelectVo> treeMenuVos = new List<TreeSelectVo>();
            foreach (var item in menuTrees)
            {
                treeMenuVos.Add(new TreeSelectVo(item));
            }
            return treeMenuVos;
        }

        /// <summary>
        /// Get route name
        /// </summary>
        /// <param name="menu">Menu information</param>
        /// <returns>Route name</returns>
        public string GetRouteName(SysMenu menu)
        {
            string routerName = menu.Path.ToLower();
            if (menu.RouteName.IsNotEmpty())
            {
                routerName = menu.RouteName;
            }
            // Non-external link and is first-level directory (type is directory)
            if (IsMeunFrame(menu))
            {
                routerName = string.Empty;
            }
            return routerName;
        }

        /// <summary>
        /// Get route path
        /// </summary>
        /// <param name="menu">Menu information</param>
        /// <returns>Route address</returns>
        public string GetRoutePath(SysMenu menu)
        {
            string routerPath = menu.Path;
            // Internal link opens external website
            if (menu.ParentId != 0 && IsInnerLink(menu))
            {
                routerPath = InnerLinkReplaceEach(routerPath);
            }
            // Non-external link and is first-level directory (type is directory)
            if (0 == menu.ParentId && UserConstants.TYPE_DIR.Equals(menu.MenuType)
                && UserConstants.NO_FRAME.Equals(menu.IsFrame))
            {
                routerPath = "/" + menu.Path;
            }
            else if (IsMeunFrame(menu))// Non-external link and is first-level directory (type is menu)
            {
                routerPath = "/";
            }
            return routerPath;
        }

        /// <summary>
        /// Get component name
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public string GetComponent(SysMenu menu)
        {
            string component = UserConstants.LAYOUT;
            if (!string.IsNullOrEmpty(menu.Component) && !IsMeunFrame(menu))
            {
                component = menu.Component;
            }
            else if (menu.Component.IsEmpty() && menu.ParentId != 0 && IsInnerLink(menu))
            {
                component = UserConstants.INNER_LINK;
            }
            else if (string.IsNullOrEmpty(menu.Component) && IsParentView(menu))
            {
                component = UserConstants.PARENT_VIEW;
            }
            return component;
        }

        /// <summary>
        /// Check if it's menu internal jump
        /// </summary>
        /// <param name="menu">Menu information</param>
        /// <returns></returns>
        public bool IsMeunFrame(SysMenu menu)
        {
            return menu.ParentId == 0 && UserConstants.TYPE_MENUS.Contains(menu.MenuType)
                && menu.IsFrame.Equals(UserConstants.NO_FRAME);
        }

        /// <summary>
        /// Check if it's internal link component
        /// </summary>
        /// <param name="menu">Menu information</param>
        /// <returns>Result</returns>
        public bool IsInnerLink(SysMenu menu)
        {
            return menu.IsFrame.Equals(UserConstants.NO_FRAME) && Tools.IsUrl(menu.Path);
        }

        /// <summary>
        /// Check if it's parent_view component
        /// </summary>
        /// <param name="menu">Menu information</param>
        /// <returns></returns>
        public bool IsParentView(SysMenu menu)
        {
            return menu.ParentId != 0 && UserConstants.TYPE_DIR.Equals(menu.MenuType);
        }

        /// <summary>
        /// Internal link domain special character replacement
        /// </summary>
        /// <param name = "path" ></ param >
        /// < returns ></ returns >
        public string InnerLinkReplaceEach(string path)
        {
            return path.IsNotEmpty() ? path
                .Replace(UserConstants.HTTP, "")
                .Replace(UserConstants.HTTPS, "")
                .Replace(UserConstants.WWW, "")
                .Replace(".", "/") : path;
        }
        #endregion


        public void AddSysMenu(GenTable genTableInfo, string permPrefix, bool showEdit, bool showExport, bool showImport)
        {
            var menu = GetFirst(f => f.MenuName == genTableInfo.FunctionName);
            if (menu is null)
            {
                menu = new()
                {
                    MenuName = genTableInfo.FunctionName,
                    ParentId = genTableInfo.Options.ParentMenuId,
                    OrderNum = 0,
                    Path = genTableInfo.BusinessName,
                    Component = $"{genTableInfo.ModuleName.FirstLowerCase()}/{genTableInfo.BusinessName}",
                    Perms = $"{permPrefix}:list",
                    IsCache = "1",
                    MenuType = "C",
                    Visible = "0",
                    Status = "0",
                    Icon = "icon1",
                    Create_by = "system",
                };
                menu.MenuId = AddMenu(menu);
            }
            else
            {

                menu.MenuName = genTableInfo.FunctionName;
                menu.ParentId = genTableInfo.Options.ParentMenuId;
                menu.Path = genTableInfo.BusinessName;
                menu.Component = $"{genTableInfo.ModuleName.FirstLowerCase()}/{genTableInfo.BusinessName}";
                menu.Perms = $"{permPrefix}:list";
                menu.IsCache = "1";
                menu.Visible = "0";
                menu.Status = "0";
                menu.Update_by = "system";
                menu.Update_time = DateTime.Now;
                EditMenu(menu);
            }

            List<SysMenu> menuList = new();

            SysMenu menuQuery = new()
            {
                MenuName = "Query",
                ParentId = menu.MenuId,
                OrderNum = 1,
                Perms = $"{permPrefix}:query",
                MenuType = "F",
                Visible = "0",
                Status = "0",
                Icon = "",
            };
            SysMenu menuAdd = new()
            {
                MenuName = "Add",
                ParentId = menu.MenuId,
                OrderNum = 2,
                Perms = $"{permPrefix}:add",
                MenuType = "F",
                Visible = "0",
                Status = "0",
                Icon = "",
            };
            SysMenu menuDel = new()
            {
                MenuName = "Delete",
                ParentId = menu.MenuId,
                OrderNum = 3,
                Perms = $"{permPrefix}:delete",
                MenuType = "F",
                Visible = "0",
                Status = "0",
                Icon = "",
            };

            SysMenu menuEdit = new()
            {
                MenuName = "Edit",
                ParentId = menu.MenuId,
                OrderNum = 4,
                Perms = $"{permPrefix}:edit",
                MenuType = "F",
                Visible = "0",
                Status = "0",
                Icon = "",
            };

            SysMenu menuExport = new()
            {
                MenuName = "Export",
                ParentId = menu.MenuId,
                OrderNum = 5,
                Perms = $"{permPrefix}:export",
                MenuType = "F",
                Visible = "0",
                Status = "0",
                Icon = "",
            };

            SysMenu menuImport = new()
            {
                
                MenuName = "Import",
                ParentId = menu.MenuId,
                OrderNum = 5,
                Perms = $"{permPrefix}:import",
                MenuType = "F",
                Visible = "0",
                Status = "0",
                Icon = "",
            };

            menuList.Add(menuQuery);
            menuList.Add(menuAdd);
            menuList.Add(menuDel);
            if (showEdit)
            {
                menuList.Add(menuEdit);
            }
            if (showExport)
            {
                menuList.Add(menuExport);
            }
            if (showImport)
            {
                menuList.Add(menuImport);
            }














                                var x = Context.Storageable(menuList)
                            .WhereColumns(it => new { it.MenuName, it.ParentId }) // Added Perms to make matching more specific
                            .ToStorage();


            x.AsInsertable.ExecuteReturnSnowflakeIdList(); // Insert new records
            x.AsUpdateable.ExecuteCommand(); // Update existing records






            //var x = Context.Storageable(menuList)
               
            //    //.SplitInsert(it => !it.Any(m => m.MenuName == it.Item.MenuName && m.Path == it.Item.Path))
            //    //.SplitUpdate(it => !it.Any(m => m.MenuName == it.Item.MenuName && m.Path == it.Item.Path))

            //    .WhereColumns(it => new { it.MenuName, it.ParentId, })
            //    .ToStorage();
            //x.AsInsertable.ExecuteCommand();//插入可插入部分;
            //x.AsUpdateable.ExecuteCommand();







        }

        /// <summary>
        /// Menu add action with validation and role assignment
        /// </summary>
        /// <param name="menuDto">Menu data transfer object</param>
        /// <param name="userId">User ID performing the action</param>
        /// <param name="createBy">User name creating the menu</param>
        /// <returns>Menu ID</returns>
        public long MenuAddAction(MenuDto menuDto, long userId, string createBy)
        {
            var config = new TypeAdapterConfig();
            // Mapping rules
            config.ForType<SysMenu, MenuDto>()
                .NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);

            var menu = menuDto.Adapt<SysMenu>(config);

            if (menu == null)
            {
                throw new CustomException("Request parameters are invalid");
            }

            // Check menu name uniqueness
            if (UserConstants.NOT_UNIQUE.Equals(CheckMenuNameUnique(menu)))
            {
                throw new CustomException($"Add menu '{menu.MenuName}' failed, menu name already exists");
            }

            // Validate frame URLs
            if (UserConstants.YES_FRAME.Equals(menu.IsFrame) && !menu.Path.StartsWith("http"))
            {
                throw new CustomException($"Add menu '{menu.MenuName}' failed, address must start with http(s)://");
            }

            // Check route name uniqueness
            if (menu.RouteName.IsNotEmpty())
            {
                var existRouteName = Any(f => f.RouteName == menu.RouteName);
                if (existRouteName)
                {
                    throw new CustomException($"Route name {menu.RouteName} already exists");
                }
            }

            menu.Create_by = createBy;
            menu.Create_time = DateTime.Now;

            long result = AddMenu(menu);

            // Auto-assign menu to user's first role if not admin
            if (result > 0)
            {
                var userRoles = SysRoleService.SelectUserRoles(userId);
                var roleId = userRoles.FirstOrDefault();

                if (!SysRoleService.IsAdmin(userId) && roleId > 0)
                {
                    SysRoleService.InsertRoleMenu(new SysRoleDto
                    {
                        Create_by = createBy,
                        RoleId = roleId,
                        MenuIds = [result]
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Sync missing permissions from ActionPermissionFilter attributes to menu system
        /// Handles both well-formed (module:resource:action) and undefined permissions
        /// </summary>
        /// <param name="createBy">User creating the menus</param>
        /// <returns>Number of menus added</returns>

        public int SyncMissingPermissionsToMenus(string createBy = "system")
        {
            var filterInfos = ActionPermissionFilterInfo.GetAllActionPermissionFiltersWithInfo();

            // Get all existing permissions from menu system (normalized for comparison)
            var existingPerms = Queryable()
                .Where(m => !string.IsNullOrEmpty(m.Perms))
                .Select(m => m.Perms.Trim().ToLower())
                .ToList();

            // Find permissions that don't exist in menu system
            var missingPermissions = new List<(string Permission, string ControllerName, string MethodName, bool IsClassLevel)>();

            foreach (var filterInfo in filterInfos)
            {
                if (filterInfo.IndividualPermissions != null)
                {
                    foreach (var perm in filterInfo.IndividualPermissions)
                    {
                        if (!string.IsNullOrWhiteSpace(perm))
                        {
                            var normalizedPerm = perm.Trim().ToLower();
                            if (!existingPerms.Contains(normalizedPerm))
                            {
                                missingPermissions.Add((perm.Trim(), filterInfo.ControllerName, filterInfo.MethodName, filterInfo.IsClassLevel));
                            }
                        }
                    }
                }
            }

            if (!missingPermissions.Any())
            {
                Console.WriteLine("No missing permissions found.");
                return 0;
            }

            // Separate well-formed permissions from undefined ones
            var wellFormedPermissions = missingPermissions
                .Where(p => p.Permission.Contains(':') && p.Permission.Split(':').Length >= 3)
                .ToList();

            var undefinedPermissions = missingPermissions
                .Where(p => !p.Permission.Contains(':') || p.Permission.Split(':').Length < 3)
                .ToList();

            int addedCount = 0;

            // Process well-formed permissions (module:resource:action)
            addedCount += ProcessWellFormedPermissions(wellFormedPermissions, createBy);

            // Process undefined permissions
            addedCount += ProcessUndefinedPermissions(undefinedPermissions, createBy);

            Console.WriteLine($"Sync completed. Total menus added: {addedCount}");
            return addedCount;
        }

        /// <summary>
        /// Process well-formed permissions with module:resource:action structure
        /// </summary>
        private int ProcessWellFormedPermissions(List<(string Permission, string ControllerName, string MethodName, bool IsClassLevel)> permissions, string createBy)
        {
            int addedCount = 0;

            // Group by module (first part)
            var groupedByModule = permissions
                .GroupBy(p => p.Permission.Split(':')[0])
                .ToList();

            foreach (var moduleGroup in groupedByModule)
            {
                var moduleName = moduleGroup.Key;
                var moduleTitle = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(moduleName);

                Console.WriteLine($"Processing module: {moduleName} with {moduleGroup.Count()} permissions");

                // Find or create parent menu
                var parentMenu = Queryable()
                    .Where(m => m.MenuType == "M" && m.MenuName.ToLower().Contains(moduleName.ToLower()))
                    .OrderBy(m => m.MenuId)
                    .First();

                long parentMenuId;

                if (parentMenu == null)
                {
                    try
                    {
                        var parentMenuDto = new MenuDto
                        {
                            MenuName = moduleTitle,
                            ParentId = 0,
                            OrderNum = GetNextOrderNum(0),
                            Path = $"/{moduleName.ToLower()}",
                            Component = "Layout",
                            MenuType = "M",
                            Visible = "0",
                            Status = "0",
                            Icon = "system",
                            IsFrame = 0,
                            IsCache = 1
                        };

                        parentMenuId = MenuAddAction(parentMenuDto, 1, createBy);
                        addedCount++;
                        Console.WriteLine($"Created parent menu: {moduleTitle}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to create parent menu {moduleTitle}: {ex.Message}");
                        continue;
                    }
                }
                else
                {
                    parentMenuId = parentMenu.MenuId;
                }

                // Group by resource (second part)
                var groupedByResource = moduleGroup
                    .GroupBy(p => p.Permission.Split(':')[1])
                    .ToList();

                foreach (var resourceGroup in groupedByResource)
                {
                    var resourceName = resourceGroup.Key;
                    var resourceTitle = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resourceName);

                    Console.WriteLine($"  Processing resource: {resourceName}");

                    // Find or create resource menu
                    var resourceMenu = Queryable()
                        .Where(m => m.ParentId == parentMenuId &&
                                   m.MenuType == "C" &&
                                   m.MenuName.ToLower().Contains(resourceName.ToLower()))
                        .OrderBy(m => m.MenuId)
                        .First();

                    long resourceMenuId;

                    if (resourceMenu == null)
                    {
                        try
                        {
                            var resourceMenuDto = new MenuDto
                            {
                                MenuName = resourceTitle,
                                ParentId = parentMenuId,
                                OrderNum = GetNextOrderNum(parentMenuId),
                                Path = resourceName.ToLower(),
                                Component = $"{moduleName.ToLower()}/{resourceName.ToLower()}/index",
                                Perms = $"{moduleName}:{resourceName}:list",
                                MenuType = "C",
                                Visible = "0",
                                Status = "0",
                                Icon = "tree",
                                IsFrame = 0,
                                IsCache = 1
                            };

                            resourceMenuId = MenuAddAction(resourceMenuDto, 1, createBy);
                            addedCount++;
                            Console.WriteLine($"    Created resource menu: {resourceTitle}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"    Failed to create resource menu {resourceTitle}: {ex.Message}");
                            continue;
                        }
                    }
                    else
                    {
                        resourceMenuId = resourceMenu.MenuId;
                    }

                    // Add button permissions
                    foreach (var perm in resourceGroup)
                    {
                        var actionName = perm.Permission.Split(':')[2];
                        var actionTitle = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(actionName);

                        // Check if already exists
                        var existing = Queryable()
                            .Where(m => m.ParentId == resourceMenuId &&
                                       m.MenuType == "F" &&
                                       m.Perms != null &&
                                       m.Perms.Trim().ToLower() == perm.Permission.ToLower())
                            .First();

                        if (existing != null)
                        {
                            Console.WriteLine($"      Button already exists: {perm.Permission}");
                            continue;
                        }

                        try
                        {
                            var buttonMenuDto = new MenuDto
                            {
                                MenuName = actionTitle,
                                ParentId = resourceMenuId,
                                OrderNum = GetNextOrderNum(resourceMenuId),
                                Perms = perm.Permission,
                                MenuType = "F",
                                Visible = "0",
                                Status = "0",
                                Icon = "",
                                IsFrame = 0,
                                IsCache = 0
                            };

                            MenuAddAction(buttonMenuDto, 1, createBy);
                            addedCount++;
                            Console.WriteLine($"      Added button: {perm.Permission}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"      Failed to add button {perm.Permission}: {ex.Message}");
                        }
                    }
                }
            }

            return addedCount;
        }

        /// <summary>
        /// Process undefined permissions (those without proper module:resource:action structure)
        /// </summary>
        private int ProcessUndefinedPermissions(List<(string Permission, string ControllerName, string MethodName, bool IsClassLevel)> permissions, string createBy)
        {
            if (!permissions.Any())
            {
                return 0;
            }

            int addedCount = 0;

            Console.WriteLine($"Processing {permissions.Count} undefined permissions");

            // Find or create "Undefined Permissions" parent menu
            var undefinedParent = Queryable()
                .Where(m => m.MenuType == "M" && m.MenuName == "Undefined Permissions")
                .First();

            long undefinedParentId;

            if (undefinedParent == null)
            {
                try
                {
                    var parentMenuDto = new MenuDto
                    {
                        MenuName = "Undefined Permissions",
                        ParentId = 0,
                        OrderNum = 9999,
                        Path = "/undefined",
                        Component = "Layout",
                        MenuType = "M",
                        Visible = "0",
                        Status = "0",
                        Icon = "question",
                        IsFrame = 0,
                        IsCache = 1
                    };

                    undefinedParentId = MenuAddAction(parentMenuDto, 1, createBy);
                    addedCount++;
                    Console.WriteLine("Created 'Undefined Permissions' parent menu");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create 'Undefined Permissions' parent: {ex.Message}");
                    return 0;
                }
            }
            else
            {
                undefinedParentId = undefinedParent.MenuId;
            }

            // Find or create "General" resource menu
            var generalResource = Queryable()
                .Where(m => m.ParentId == undefinedParentId && m.MenuType == "C" && m.MenuName == "General")
                .First();

            long generalResourceId;

            if (generalResource == null)
            {
                try
                {
                    var resourceMenuDto = new MenuDto
                    {
                        MenuName = "General",
                        ParentId = undefinedParentId,
                        OrderNum = 1,
                        Path = "general",
                        Component = "undefined/general/index",
                        Perms = "undefined:general:list",
                        MenuType = "C",
                        Visible = "0",
                        Status = "0",
                        Icon = "list",
                        IsFrame = 0,
                        IsCache =1
                    };

                    generalResourceId = MenuAddAction(resourceMenuDto, 1, createBy);
                    addedCount++;
                    Console.WriteLine("  Created 'General' resource menu");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Failed to create 'General' resource: {ex.Message}");
                    return addedCount;
                }
            }
            else
            {
                generalResourceId = generalResource.MenuId;
            }

            // Add all undefined permissions as buttons
            foreach (var perm in permissions)
            {
                var permName = perm.Permission.Replace(":", "_").Replace("/", "_");
                var actionTitle = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(permName);

                // Check if already exists
                var existing = Queryable()
                    .Where(m => m.ParentId == generalResourceId &&
                               m.MenuType == "F" &&
                               m.Perms != null &&
                               m.Perms.Trim().ToLower() == perm.Permission.ToLower())
                    .First();

                if (existing != null)
                {
                    Console.WriteLine($"    Button already exists: {perm.Permission}");
                    continue;
                }

                try
                {
                    var buttonMenuDto = new MenuDto
                    {
                        MenuName = actionTitle,
                        ParentId = generalResourceId,
                        OrderNum = GetNextOrderNum(generalResourceId),
                        Perms = perm.Permission,
                        MenuType = "F",
                        Visible = "0",
                        Status = "0",
                        Icon = "",
                        IsFrame = 0,
                        IsCache = 0
                    };

                    MenuAddAction(buttonMenuDto, 1, createBy);
                    addedCount++;
                    Console.WriteLine($"    Added undefined button: {perm.Permission}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"    Failed to add undefined button {perm.Permission}: {ex.Message}");
                }
            }

            return addedCount;
        }

        /// <summary>
        /// Get next order number for menu items under a parent
        /// </summary>
        private int GetNextOrderNum(long parentId)
        {
            var maxOrder = Queryable()
                .Where(m => m.ParentId == parentId)
                .Max(m => m.OrderNum);
            return (maxOrder   ) + 1;
        }

        /// <summary>
        /// Get mobile workbench menu
        /// </summary>
        /// <param name="perms"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public List<RouterVo> GetAppMenus(List<string> perms, int v)
        {
            var demoList = new List<RouterVo>(){
                new()
                {
                    Path = "/pages/demo/index",
                    Meta = new Meta("Feature Demo", "bookmark")
                },
                    new()
                {
                    Path = "/pages/demo/table/table",
                    Meta = new Meta("List Table", "grid")
                },
                    new()
                {
                    Path = "/pages/demo/table/table2",
                    Meta = new Meta("Horizontal Table", "list")
                }
            };
            if (v >= 1)
            {
                demoList.Add(new RouterVo
                {
                    Path = "/pages/dashboard/index",
                    Meta = new Meta("Console", "list")
                });
            }
            var router = new List<RouterVo>
            {
                new()
                {
                    Meta = new Meta("Demo Features", ""){ IconColor = "#2389da"},
                    Children = demoList
                },

                new()
                {
                    Meta = new Meta("System Management", ""){ IconColor = "#ff7d00"},
                    Children = new List<RouterVo>()
                {
                    new()
                    {
                        Path = "/pages/system/sysConfig/sysConfig",
                        Meta = new Meta("System Configuration", "setting"){ Permi = "system:config:list"},
                    },
                     new()
                    {
                        Path = "/pages/monitor/tasks/index",
                        Meta = new Meta("Task Management", "hourglass-half-fill"){Permi = "monitor:job:list"}
                    },
                     new()
                    {
                        Path = "/pages/monitor/onlineuser",
                        Meta = new Meta("Online Users", "grid"){ Permi = "admin"}
                    },
                      new()
                    {
                        Path = "/pages/system/user/user",
                        Meta = new Meta("User Management", "account"){ Permi = "system:user:list" }
                    },
                       new()
                    {
                        Path = "/pages/monitor/server",
                        Meta = new Meta("System Monitor", "play-right"){ Permi = "monitor:server:list"}
                    },
                       new()
                    {
                        Path = "/pages/notice/index",
                        Meta = new Meta("Notifications", "volume")
                    }
                }
                },

                new()
                {
                    Meta = new Meta("Log Management", ""){ IconColor = "#8a8a8a"},
                    Children = new List<RouterVo>()
                {
                    new()
                    {
                        Path = "/pages/monitor/logininfo",
                        Meta = new Meta("Login Logs", "clock")
                    },
                     new()
                    {
                        Path = "/pages/monitor/operlog",
                        Meta = new Meta("Operation Logs", "file-text"){ Permi = "monitor:operlog:list" }
                    }
                }
                },
            };
            if (perms.Contains(GlobalConstant.AdminPerm))
            {
                return router;
            }
            var newRouter = new List<RouterVo>();
            foreach (var item in router)
            {
                RouterVo routerVo = new()
                {
                    Children = new List<RouterVo>()
                };
                for (var i = 0; i < item.Children.Count; i++)
                {
                    var chd = item.Children[i];
                    if (chd.Meta.Permi != null && perms.Contains(chd.Meta.Permi))
                    {
                        routerVo.Children.Add(chd);
                    }
                    else if (chd.Meta.Permi == null)
                    {
                        routerVo.Children.Add(chd);
                    }
                }

                if (routerVo.Children != null && routerVo.Children.Count > 0)
                {
                    routerVo.Meta = item.Meta;
                    newRouter.Add(routerVo);
                }
            }
            return newRouter;
        }
    }
}

