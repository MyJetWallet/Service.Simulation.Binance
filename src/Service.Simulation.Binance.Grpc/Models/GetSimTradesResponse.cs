using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Simulation.Binance.Grpc.Models
{
    [DataContract]
    public class GetSimTradesResponse
    {
        [DataMember(Order = 1)] public List<SimTrade> Trades { get; set; }
    }
}