using System;
using System.Runtime.Serialization;

namespace Service.Simulation.Binance.Grpc.Models
{
    [DataContract]
    public class SimTrade
    {
        [DataMember(Order = 1)] public string Id { get; set; }
        [DataMember(Order = 2)] public string ClientId { get; set; }
        [DataMember(Order = 3)] public string Market { get; set; }
        [DataMember(Order = 4)] public SimulationOrderSide Side { get; set; }
        [DataMember(Order = 5)] public double Size { get; set; }
        [DataMember(Order = 6)] public double Price { get; set; }
        [DataMember(Order = 7)] public DateTime Timestamp { get; set; }
    }
}