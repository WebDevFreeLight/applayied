namespace ZR.Model.System
{
    /// <summary>
    /// Parameter configuration, data entity object
    ///
    /// @author mr.zhao
    /// @date 2021-09-29
    /// </summary>
    [SugarTable("sys_config", "Configuration table")]
    [Tenant("0")]
    public class SysConfig : SysBase, IMainDbEntity
    {
        /// <summary>
        /// Configuration ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int ConfigId { get; set; }
        /// <summary>
        /// Parameter name
        /// </summary>
        [SugarColumn(Length = 100)]
        public string ConfigName { get; set; }
        /// <summary>
        /// Parameter key name
        /// </summary>
        [SugarColumn(Length = 100)]
        public string ConfigKey { get; set; }
        /// <summary>
        /// Parameter key value
        /// </summary>
        [SugarColumn(Length = 500)]
        public string ConfigValue { get; set; }
        /// <summary>
        /// System built-in (Y=Yes N=No)
        /// </summary>
        [SugarColumn(Length = 1)]
        public string ConfigType { get; set; }

    }
}

