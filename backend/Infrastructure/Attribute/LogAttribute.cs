using Infrastructure.Enums;

namespace Infrastructure.Attribute
{
    /// <summary>
    /// Custom operation log recording attribute
    /// </summary>
    public class LogAttribute : System.Attribute
    {
        public string Title { get; set; }
        public BusinessType BusinessType { get; set; }
        /// <summary>
        /// Whether to save request data
        /// </summary>
        public bool IsSaveRequestData { get; set; } = true;
        /// <summary>
        /// Whether to save response data
        /// </summary>
        public bool IsSaveResponseData { get; set; } = true;

        public LogAttribute() { }

        public LogAttribute(string name)
        {
            Title = name;
        }
        public LogAttribute(string name, BusinessType businessType, bool saveRequestData = true, bool saveResponseData = true)
        {
            Title = name;
            BusinessType = businessType;
            IsSaveRequestData = saveRequestData;
            IsSaveResponseData = saveResponseData;
        }
    }
}

