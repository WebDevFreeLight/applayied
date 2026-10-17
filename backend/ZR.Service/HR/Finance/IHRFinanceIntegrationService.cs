using System.Threading.Tasks;

namespace ZR.Service.HR.IService
{
    public interface IHRFinanceIntegrationService
    {
        Task<int> FullReconcile();
    }
}















