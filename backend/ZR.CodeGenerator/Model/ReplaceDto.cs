using System;

namespace ZR.CodeGenerator.Model
{
    public class ReplaceDto
    {
        /// <summary>
        /// Primary key field
        /// </summary>
        public string PKName { get; set; }
        /// <summary>
        /// Primary key with first letter lowercase
        /// </summary>
        public string FistLowerPk { get; set; }
        /// <summary>
        /// Primary key type
        /// </summary>
        public string PKType { get; set; }
        /// <summary>
        /// Controller permissions
        /// </summary>
        public string PermissionPrefix { get; set; }
        /// <summary>
        /// C# class name
        /// </summary>
        public string ModelTypeName { get; set; }
        //Vue, API
        //public string VueViewFormResetHtml { get; set; }
        /// <summary>
        /// Frontend list query HTML
        /// </summary>
        public string VueViewListHtml { get; set; }
        /// <summary>
        /// Frontend add/edit table HTML
        /// </summary>
        public string VueViewFormHtml { get; set; }
        /// <summary>
        /// Frontend search form HTML
        /// </summary>
        public string VueQueryFormHtml { get; set; }

        /// <summary>
        /// Query conditions
        /// </summary>
        public string QueryCondition { get; set; } = "";
        public bool ShowBtnExport { get; set; }
        public bool ShowBtnAdd { get; set; }
        public bool ShowBtnEdit { get; set; }
        public bool ShowBtnDelete { get; set; }
        public bool ShowBtnView { get; set; }
        public bool ShowBtnTruncate { get; set; }
        public bool ShowBtnMultiDel { get; set; }
        public bool ShowBtnImport { get; set; }
        /// <summary>
        /// Upload URL data
        /// </summary>
        //public string VueUploadUrl { get; set; }
        public int UploadFile { get; set; } = 0;
        /// <summary>
        /// Whether there are dropdown multi-select boxes
        /// </summary>
        public int SelectMulti { get; set; }
        public string Author { get; set; }
        public string AddTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
        /// <summary>
        /// Whether there is an editor
        /// </summary>
        public int ShowEditor { get; set; }
        /// <summary>
        /// Vue page filename
        /// </summary>
        public string ViewFileName { get; set; }
        /// <summary>
        /// Operation button style
        /// </summary>
        public int OperBtnStyle { get; set; }
        /// <summary>
        /// Whether to use snowflake ID
        /// </summary>
        public bool UseSnowflakeId { get; set; }
        public bool EnableLog { get; set; }
    }
}

