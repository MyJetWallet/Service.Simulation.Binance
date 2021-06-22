using System.ServiceModel;
using System.Threading.Tasks;
using Service.Simulation.Binance.Grpc.Models;

namespace Service.Simulation.Binance.Grpc
{
    [ServiceContract]
    public interface ISimulationTradingService
    {
        [OperationContract]
        Task<ExecuteMarketOrderResponse> ExecuteMarketOrderAsync(ExecuteMarketOrderRequest request);

        [OperationContract]
        Task<GetBalancesResponse> GetBalancesAsync();

        [OperationContract]
        Task<GetMarketInfoResponse> GetMarketInfoAsync(GetMarketInfoRequest request);

        [OperationContract]
        Task<GetMarketInfoListResponse> GetMarketInfoListAsync();

        [OperationContract]
        Task SetBalanceAsync(SetBalanceRequest request);
    }
}