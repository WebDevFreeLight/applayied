namespace Infrastructure.Enums
{
    /// <summary>
    /// Business operation type 0=Other,1=Insert,2=Update,3=Delete,4=Grant,5=Export,6=Import,7=Force,8=Generate code,9=Clear data
    /// </summary>
    public enum BusinessType
    {
        /// <summary>
        /// Other
        /// </summary>
        OTHER = 0,

        /// <summary>
        /// Insert
        /// </summary>
        INSERT = 1,

        /// <summary>
        /// Update
        /// </summary>
        UPDATE = 2,

        /// <summary>
        /// Delete
        /// </summary>
        DELETE = 3,

        /// <summary>
        /// Grant
        /// </summary>
        GRANT = 4,

        /// <summary>
        /// Export
        /// </summary>
        EXPORT = 5,

        /// <summary>
        /// Import
        /// </summary>
        IMPORT = 6,

        /// <summary>
        /// Force logout
        /// </summary>
        FORCE = 7,

        /// <summary>
        /// Generate code
        /// </summary>
        GENCODE = 8,

        /// <summary>
        /// Clear data
        /// </summary>
        CLEAN = 9,

        /// <summary>
        /// Download
        /// </summary>
        DOWNLOAD = 10,
    }
}

