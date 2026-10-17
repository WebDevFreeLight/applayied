using ZR.Repository;

namespace ZR.ServiceCore
{
    /// <summary>
    /// Base service definition
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBaseService<T> : IBaseRepository<T> where T : class, new()
    {
    }
}
