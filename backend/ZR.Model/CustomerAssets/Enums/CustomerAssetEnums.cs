namespace ZR.Model.CustomerAssets.Enums
{
    /// <summary>
    /// Asset Condition - حالة الأصل
    /// </summary>
    public enum AssetCondition
    {
        New = 0,
        Good = 1,
        Fair = 2,
        Poor = 3,
        Damaged = 4
    }

    /// <summary>
    /// Inventory Asset Status - حالة الأصل في المخزون
    /// </summary>
    public enum InventoryAssetStatus
    {
        Available = 0,
        Assigned = 1,
        UnderMaintenance = 2,
        Retired = 3,
        Lost = 4,
        Damaged = 5
    }

    /// <summary>
    /// Assignment Status - حالة التعيين
    /// </summary>
    public enum AssignmentStatus
    {
        Active = 0,
        Returned = 1,
        Lost = 2,
        Damaged = 3
    }

    /// <summary>
    /// Contract File Type - نوع ملف العقد
    /// </summary>
    public enum ContractFileType
    {
        SignedContract = 0,
        DeliveryNote = 1,
        ReturnNote = 2,
        DamageReport = 3,
        Other = 4
    }
}
