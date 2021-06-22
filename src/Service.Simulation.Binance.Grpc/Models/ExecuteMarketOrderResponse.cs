using System.Runtime.Serialization;

namespace Service.Simulation.Binance.Grpc.Models
{
    [DataContract]
    public class ExecuteMarketOrderResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }

        [DataMember(Order = 2)] public SimTrade Trade { get; set; }
    }
}