namespace ZR.Infrastructure.Helper
{
    public static class MaskUtil
    {
        /// <summary>
        /// Phone number masking
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public static string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 7) return phone;
            return phone[..3] + "****" + phone.Substring(7);
        }

        /// <summary>
        /// ID card number masking
        /// </summary>
        /// <param name="idCard"></param>
        /// <returns></returns>
        public static string MaskIdCard(string idCard)
        {
            if (string.IsNullOrEmpty(idCard) || idCard.Length < 8) return idCard;
            return idCard.Substring(0, 4) + "********" + idCard.Substring(idCard.Length - 4);
        }

        /// <summary>
        /// Name masking
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string MaskName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            if (name.Length == 2) return name[..1] + "*";
            if (name.Length > 2) return name[..1] + new string('*', name.Length - 2) + name.Substring(name.Length - 1);
            return "*";
        }

        /// <summary>
        /// Mask IP address (supports IPv4 and IPv6)
        /// </summary>
        public static string MaskIp(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return ip;

            if (System.Net.IPAddress.TryParse(ip, out var ipAddress))
            {
                if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    // IPv4: 123.45.67.89 -> 123.45.*.*
                    var parts = ip.Split('.');
                    if (parts.Length == 4)
                    {
                        return $"{parts[0]}.{parts[1]}.*.*";
                    }
                }
                else if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    // IPv6: Keep the first 3 segments, replace others with ****
                    var parts = ip.Split(':');
                    for (int i = 3; i < parts.Length; i++)
                    {
                        parts[i] = "****";
                    }
                    return string.Join(":", parts);
                }
            }

            return "***.***.***.***"; // fallback
        }
    }
}

