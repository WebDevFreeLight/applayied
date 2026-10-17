using Infrastructure;
using SqlSugar.IOC;
using ZR.ServiceCore.SqlSugar;

namespace ZR.ServiceCore
{
    public class DataPermiSevice
    {
        /// <summary>
        /// Your business database data permission filtering method
        /// </summary>
        /// <param name="configId"></param>
        public static void FilterData(int configId)
        {
            //Get current user information
            var user = JwtUtil.GetLoginUser(App.HttpContext);
            if (user == null) return;
            var db = DbScoped.SugarScope.GetConnectionScope(configId);

            foreach (var role in user.Roles.OrderBy(f => f.DataScope))
            {
                var dataScope = (DataPermiEnum)role.DataScope;
                if (DataPermiEnum.All.Equals(dataScope))//All permissions
                {
                    break;
                }
                else if (DataPermiEnum.CUSTOM.Equals(dataScope))//Custom data permissions
                {
                }
                else if (DataPermiEnum.DEPT.Equals(dataScope))//Department data
                {
                }
                else if (DataPermiEnum.DEPT_CHILD.Equals(dataScope))//Department and sub-department data
                {

                }
                else if (DataPermiEnum.SELF.Equals(dataScope))//Only personal data
                {
                }
            }
        }
    }
}

