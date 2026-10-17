 
using Infrastructure.Converter;
using System.ComponentModel;
using ZR.Model.System;

namespace ZR.Model.Models
{
    /// <summary>
    /// Multi-language configuration, data entity object
    /// </summary>
    [Tenant("0")]
    [SugarTable("sys_common_lang", "Multi-language configuration table")]
    public class CommonLang : SysBase, IMainDbEntity
    {
        /// <summary>
        /// id
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true)] 
        public long Id { get; set; }

        /// <summary>
        /// Language code
        /// </summary>
        [DisplayName("Language code")]
        [SugarColumn(ColumnName = "lang_code", Length = 10, ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string LangCode { get; set; }

        /// <summary>
        /// Language key
        /// </summary>
        [DisplayName("Language key")]
        [SugarColumn(ColumnName = "lang_key", Length = 100, ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string LangKey { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DisplayName("Name")]
        [SugarColumn(ColumnName = "lang_name", Length = 2000, ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string LangName { get; set; }

        /// <summary>
        /// Add time
        /// </summary>
        [DisplayName("Add time")]
        public DateTime? Addtime { get; set; }
    }
}

