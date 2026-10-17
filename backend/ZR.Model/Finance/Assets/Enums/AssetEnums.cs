namespace ZR.Model.Finance.Assets.Enums
{
    /// <summary>
    /// Depreciation Method - طريقة الإهلاك
    /// </summary>
    public enum DepreciationMethod
    {
        /// <summary>
        /// Straight Line - القسط الثابت
        /// </summary>
        StraightLine = 0,

        /// <summary>
        /// Declining Balance - القسط المتناقص
        /// </summary>
        DecliningBalance = 1,

        /// <summary>
        /// No Depreciation - بدون إهلاك
        /// </summary>
        None = 2
    }

    /// <summary>
    /// Asset Status - حالة الأصل
    /// </summary>
    public enum AssetStatus
    {
        /// <summary>
        /// Active - نشط
        /// </summary>
        Active = 0,

        /// <summary>
        /// Sold - مباع
        /// </summary>
        Sold = 1,

        /// <summary>
        /// Disposed - مستبعد
        /// </summary>
        Disposed = 2,

        /// <summary>
        /// Fully Depreciated - مهلك بالكامل
        /// </summary>
        FullyDepreciated = 3
    }

    /// <summary>
    /// Asset Transaction Type - نوع معاملة الأصل
    /// </summary>
    public enum AssetTransactionType
    {
        /// <summary>
        /// Purchase - شراء
        /// </summary>
        Purchase = 0,

        /// <summary>
        /// Depreciation - إهلاك
        /// </summary>
        Depreciation = 1,

        /// <summary>
        /// Sale - بيع
        /// </summary>
        Sale = 2,

        /// <summary>
        /// Disposal - استبعاد
        /// </summary>
        Disposal = 3,

        /// <summary>
        /// Revaluation - إعادة تقييم
        /// </summary>
        Revaluation = 4
    }

    /// <summary>
    /// Current Asset Status - حالة الأصل المتداول
    /// </summary>
    public enum CurrentAssetStatus
    {
        /// <summary>
        /// Active - نشط
        /// </summary>
        Active = 0,

        /// <summary>
        /// Consumed - مستهلك
        /// </summary>
        Consumed = 1,

        /// <summary>
        /// Transferred - محول
        /// </summary>
        Transferred = 2,

        /// <summary>
        /// Written Off - مشطوب
        /// </summary>
        WrittenOff = 3
    }
}

