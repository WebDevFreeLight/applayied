

namespace ZR.CodeGenerator
{
    /// <summary>
    /// Code generation constants
    /// </summary>
    public class GenConstants
    {
        public static string Gen_author = "codeGen:author";
        public static string CodeGenDbConfig;

        /// <summary>
        /// InputDto input entity does not contain fields
        /// </summary>
        public static readonly string[] inputDtoNoField = new string[] { "Create_time", "Update_time", "addtime", "create_time", "update_time", "create_by", "update_by" };
        /// <summary>
        /// Image fields
        /// </summary>
        public static readonly string[] imageFiled = new string[] { "icon", "img", "image", "url", "pic", "photo", "avatar" };
        /// <summary>
        /// Dropdown fields
        /// </summary>
        public static readonly string[] selectFiled = new string[] { "status", "type", "state", "sex", "gender" };
        /// <summary>
        /// Radio button fields
        /// </summary>
        public static readonly string[] radioFiled = new string[] { "status", "state", "is" };

        /// <summary>
        /// Single table (CRUD)
        /// </summary>
        public static string TPL_CRUD = "crud";

        /// <summary>
        /// Tree table (CRUD)
        /// </summary>
        public static string TPL_TREE = "tree";

        /// <summary>
        /// Master-detail table (CRUD)
        /// </summary>
        public static string TPL_SUB = "sub";

        /// <summary>
        /// Tree code field
        /// </summary>
        public static string TREE_CODE = "treeCode";

        /// <summary>
        /// Tree parent code field
        /// </summary>
        public static string TREE_PARENT_CODE = "treeParentCode";

        /// <summary>
        /// Tree name field
        /// </summary>
        public static string TREE_NAME = "treeName";

        /// <summary>
        /// Parent menu ID field
        /// </summary>
        public static string PARENT_MENU_ID = "parentMenuId";

        /// <summary>
        /// Parent menu name field
        /// </summary>
        public static string PARENT_MENU_NAME = "parentMenuName";

        /// <summary>
        /// Database string type
        /// </summary>
        public static string[] COLUMNTYPE_STR = { "char", "varchar", "nvarchar", "varchar2" };

        /// <summary>
        /// Database text type
        /// </summary>
        public static string[] COLUMNTYPE_TEXT = { "tinytext", "text", "mediumtext", "longtext" };

        /// <summary>
        /// Database time type
        /// </summary>
        public static string[] COLUMNTYPE_TIME = { "datetime", "time", "date", "timestamp" };

        /// <summary>
        /// Page fields that don't need editing
        /// </summary>
        public static string[] COLUMNNAME_NOT_EDIT = { "id", "create_by", "create_time", "delFlag" };

        /// <summary>
        /// Page list fields that don't need to be displayed
        /// </summary>
        public static string[] COLUMNNAME_NOT_LIST = { "create_by", "create_time", "delFlag", "update_by",
            "update_time" , "password"};

        /// <summary>
        /// Page fields that don't need querying
        /// </summary>
        public static string[] COLUMNNAME_NOT_QUERY = { "id", "create_by", "create_time", "delFlag", "update_by",
            "update_time", "remark" };

        /// <summary>
        /// Entity base class fields
        /// </summary>
        public static string[] BASE_ENTITY = { "Create_by", "Create_time", "Update_by", "Update_time", "remark" };

        /// <summary>
        /// Tree base class fields
        /// </summary>
        public static string[] TREE_ENTITY = { "parentName", "parentId", "orderNum", "ancestors", "children" };

        /// <summary>
        /// Text box
        /// </summary>
        public static string HTML_INPUT = "input";
        /// <summary>
        /// Number box
        /// </summary>
        public static string HTML_INPUT_NUMBER = "inputNumber";

        /// <summary>
        /// Text area
        /// </summary>
        public static string HTML_TEXTAREA = "textarea";

        /** Dropdown */
        public static string HTML_SELECT = "select";
        /// <summary>
        /// Multi-select dropdown
        /// </summary>
        public static string HTML_SELECT_MULTI = "selectMulti";

        /// <summary>
        /// Radio button
        /// </summary>
        public static string HTML_RADIO = "radio";

        /// <summary>
        /// Checkbox
        /// </summary>
        public static string HTML_CHECKBOX = "checkbox";

        /// <summary>
        /// Date control
        /// </summary>
        public static string HTML_DATETIME = "datetime";

        /// <summary>
        /// Image upload control
        /// </summary>
        public static string HTML_IMAGE_UPLOAD = "imageUpload";

        /// <summary>
        /// File upload control
        /// </summary>
        public static string HTML_FILE_UPLOAD = "fileUpload";

        /// <summary>
        /// Rich text control
        /// </summary>
        public static string HTML_EDITOR = "editor";
        /// <summary>
        /// Custom input box
        /// </summary>
        public static string HTML_CUSTOM_INPUT = "customInput";
        /// <summary>
        /// Color picker
        /// </summary>
        public static string HTML_COLORPICKER = "colorPicker";

        /// <summary>
        /// Fuzzy query
        /// </summary>
        public static string QUERY_LIKE = "LIKE";

        /// <summary>
        /// Required
        /// </summary>
        public static string REQUIRE = "1";
        /// <summary>
        /// Time type
        /// </summary>
        public static string TYPE_DATE = "DateTime";
    }
}
