using System.ComponentModel;

namespace ZR.Model.System.Enums
{
    /// <summary>
    /// M=Directory, C=Menu, F=Button, L=Link
    /// </summary>
    public enum MenuType
    {
        [Description("Directory")]
        M,
        [Description("Menu")]
        C,
        [Description("Button")]
        F,
        [Description("Link")]
        L
    }
}

