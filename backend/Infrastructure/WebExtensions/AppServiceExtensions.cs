using Infrastructure.Attribute;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Infrastructure
{
    /// <summary>
    /// App Service Registration ffggghj dsdffdgsdf
    /// </summary>
    public static class AppServiceExtensions
    {
        /// <summary>
        /// Register services for all classes marked with AppService in the application domain
        /// </summary>
        /// <param name="services"></param>
        public static void AddAppService(this IServiceCollection services)
        {
            var cls = AppSettings.Get<string[]>("InjectClass");
            if (cls == null || cls.Length <= 0)
            {
                throw new Exception("Please update appsettings class");
            }
            foreach (var item in cls)
            {
                Register(services, item);
            }
        }

        private static void Register(IServiceCollection services, string item)
        {

            try
            {
                Assembly assembly = Assembly.Load(item);
                foreach (var type in assembly.GetTypes())
                {
                    var serviceAttribute = type.GetCustomAttribute<AppServiceAttribute>();

                    if (serviceAttribute != null)
                    {
                        var serviceType = serviceAttribute.ServiceType;
                        //Case 1: Suitable for dependency abstraction programming, note that only the first one is retrieved here
                        if (serviceType == null && serviceAttribute.InterfaceServiceType)
                        {
                            serviceType = type.GetInterfaces().FirstOrDefault();
                        }
                        //Case 2: ServiceType is only specified in uncommon special cases, which is troublesome to write
                        if (serviceType == null)
                        {
                            serviceType = type;
                        }

                        switch (serviceAttribute.ServiceLifetime)
                        {
                            case LifeTime.Singleton:
                                services.AddSingleton(serviceType, type);
                                break;
                            case LifeTime.Scoped:
                                services.AddScoped(serviceType, type);
                                break;
                            case LifeTime.Transient:
                                services.AddTransient(serviceType, type);
                                break;
                            default:
                                services.AddTransient(serviceType, type);
                                break;
                        }
                        //System.Console.WriteLine($"Registered: {serviceType}");
                    }
                }


            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message );
            }
          
        
        }
    }
}

