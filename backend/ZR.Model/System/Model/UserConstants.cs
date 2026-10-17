namespace ZR.Model.System
{
    public class UserConstants
    {
        /// <summary>
        /// Unique identifier for system users within the platform
        /// </summary>
        public static string SYS_USER = "SYS_USER";

        /// <summary>
        /// Normal status
        /// </summary>
        public static string NORMAL = "0";

        /// <summary>
        /// Exception status
        /// </summary>
        public static string EXCEPTION = "1";

        /// <summary>
        /// User disabled status
        /// </summary>
        public static string USER_DISABLE = "1";

        /// <summary>
        /// Role disabled status
        /// </summary>
        public static string ROLE_DISABLE = "1";

        /// <summary>
        /// Department normal status
        /// </summary>
        public static int DEPT_NORMAL = 0;

        /// <summary>
        /// Department disabled status
        /// </summary>
        public static string DEPT_DISABLE = "1";

        /// <summary>
        /// Dictionary normal status
        /// </summary>
        public static string DICT_NORMAL = "0";

        /// <summary>
        /// Is system default (Yes)
        /// </summary>
        public static string YES = "Y";

        /// <summary>
        /// Is menu external link (Yes)
        /// </summary>
        public static string YES_FRAME = "1";

        /// <summary>
        /// Is menu external link (No)
        /// </summary>
        public static string NO_FRAME = "0";

        /// <summary>
        /// Menu type (Directory)
        /// </summary>
        public static string TYPE_DIR = "M";

        /// <summary>
        /// Menu type (Menu)
        /// </summary>
        public static string TYPE_MENU = "C";
        public static string TYPE_MENUS = "C,L";

        /// <summary>
        /// Menu type (Button)
        /// </summary>
        public static string TYPE_BUTTON = "F";

        /// <summary>
        /// Menu type (Link)
        /// </summary>
        public static string TYPE_LINK = "L";

        /// <summary>
        /// Layout component identifier
        /// </summary>
        public static string LAYOUT = "Layout";

        /// <summary>
        /// ParentView component identifier
        /// </summary>
        public static string PARENT_VIEW = "ParentView";

        /// <summary>
        /// InnerLink component identifier
        /// </summary>
        public static string INNER_LINK = "InnerLink";

        /// <summary>
        /// Validation return result code
        /// </summary>
        public static string UNIQUE = "0";
        public static string NOT_UNIQUE = "1";

        /// <summary>
        /// HTTP request
        /// </summary>
        public static string HTTP = "http://";

        /// <summary>
        /// HTTPS request
        /// </summary>
        public static string HTTPS = "https://";
        /// <summary>
        /// WWW main domain
        /// </summary>
        public static string WWW = "www.";
    }
}

