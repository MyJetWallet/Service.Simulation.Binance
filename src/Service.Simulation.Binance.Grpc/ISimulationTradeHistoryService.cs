using System.ServiceModel;
using System.Threading.Tasks;
using Service.Simulation.Binance.Grpc.Models;

namespace Service.Simulation.Binance.Grpc
{
    [ServiceContract]
    public interface ISimulationTradeHistoryService
    {
        [OperationContract]
        Task<GetSimTradesResponse> GetTradesAsync();
    }
}