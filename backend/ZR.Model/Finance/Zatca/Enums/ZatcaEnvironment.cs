namespace ZR.Model.Finance.Zatca.Enums
{
    /// <summary>
    /// ZATCA Environment - بيئة زاتكا
    /// </summary>
    public enum ZatcaEnvironment
    {
        /// <summary>
        /// Sandbox - بيئة الاختبار
        /// For development and testing
        /// URL: https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal
        /// </summary>
        Sandbox = 0,

        /// <summary>
        /// Simulation - بيئة المحاكاة
        /// For integration testing before production
        /// URL: https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation
        /// </summary>
        Simulation = 1,

        /// <summary>
        /// Production - بيئة الإنتاج
        /// Live production environment
        /// URL: https://gw-fatoora.zatca.gov.sa/e-invoicing/core
        /// </summary>
        Production = 2
    }
}
