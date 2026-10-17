using System.ComponentModel;

namespace ZR.Model.System.Enums
{
    /// <summary>
    /// Menu status (0=Normal, 1=Disabled)
    /// </summary>
    public enum MenuStatus
    {
        [Description("Normal")]
        Normal = 0,
        [Description("Disabled")]
        Disabled = 1,
    }
}

