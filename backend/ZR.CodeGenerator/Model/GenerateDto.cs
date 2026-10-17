using System.Text.Json.Serialization;
using Infrastructure.Converter;
using System.Collections.Generic;
using ZR.Model.System.Generate;
using SqlSugar;

namespace ZR.CodeGenerator.Model
{
    public class GenerateDto
    {
        /// <summary>
        /// Vue version
        /// </summary>
        public int VueVersion { get; set; }
        //[JsonConverter(typeof(ValueToStringConverter))]
        public long TableId { get; set; }
        /// <summary>
        /// Whether to preview code
        /// </summary>
        public bool IsPreview { get; set; }
        /// <summary>
        /// Database type for code generation: 0=MySQL, 1=SQL Server
        /// </summary>
        public int DbType { get; set; }
        /// <summary>
        /// Generated button functions
        /// </summary>
        public int[] CheckedBtn { get; set; } = System.Array.Empty<int>();
        public GenTable GenTable { get; set; }
        /// <summary>
        /// Main table properties
        /// </summary>
        public CodeGenerateOption GenOptions { get; set; }
        /// <summary>
        /// Sub-table properties
        /// </summary>
        public CodeGenerateOption SubTableOptions { get; set; }
        #region Storage paths
        /// <summary>
        /// Code template preview storage path
        /// </summary>
        public List<GenCode> GenCodes { get; set; } = new List<GenCode>();
        /// <summary>
        /// Code generation path
        /// </summary>
        public string GenCodePath { get; set; } = string.Empty;
        /// <summary>
        /// Code generation zip package path
        /// </summary>
        public string ZipPath { get; set; }
        /// <summary>
        /// Code generation zip package name
        /// </summary>
        public string ZipFileName { get; set; }
        /// <summary>
        /// Code generation method (0=zip package, 1=custom path)
        /// </summary>
        public string GenType { get; set; }
        public string GenPath { get; set; } = "";
        /// <summary>
        /// Vue storage path
        /// </summary>
        public string VueParentPath { get; set; }
        /// <summary>
        /// UniApp storage path
        /// </summary>
        public string AppVuePath { get; set; } = "maham-ain-app";
        #endregion
        public ReplaceDto ReplaceDto { get; set; }
    }

    public class GenCode
    {
        public int Type { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }

        public GenCode(int type, string title, string path, string content)
        {
            Type = type;
            Title = title;
            Path = path;
            Content = content;
        }
    }
}

