using System.Runtime.Serialization;

namespace Service.Simulation.Binance.Grpc.Models
{
    [DataContract]
    public class SetBalanceRequest
    {
        [DataMember(Order = 1)] public string Symbol { get; set; }
        [DataMember(Order = 2)] public double Amount { get; set; }
    }
}