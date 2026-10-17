using System;

namespace Infrastructure.Attribute
{
    /// <summary>
    /// Reference: https://www.cnblogs.com/kelelipeng/p/10643556.html
    /// Mark service
    /// How to use?
    /// 1. If the service is itself, use [AppService] directly on the class
    /// 2. If the service is an interface, use [AppService(ServiceType = typeof(implementing interface))] on the class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AppServiceAttribute : System.Attribute
    {
        /// <summary>
        /// Service lifetime
        /// If no default value is provided, the registration is AddSingleton
        /// </summary>
        public LifeTime ServiceLifetime { get; set; } = LifeTime.Scoped;
        /// <summary>
        /// Specify service type
        /// </summary>
        public Type ServiceType { get; set; }
        /// <summary>
        /// Whether service type can be obtained from the first interface
        /// </summary>
        public bool InterfaceServiceType { get; set; }
    }

    public enum LifeTime
    {
        Transient, Scoped, Singleton
    }
}

