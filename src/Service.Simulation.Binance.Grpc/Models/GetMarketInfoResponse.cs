using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Simulation.Binance.Grpc.Models
{
    [DataContract]
    public class GetMarketInfoResponse
    {
        [DataMember(Order = 1)] public MarketInfo Info { get; set; }
    }

    [DataContract]
    public class GetMarketInfoListResponse
    {
        [DataMember(Order = 1)] public List<MarketInfo> Info { get; set; }
    }

    [DataContract]
    public class MarketInfo
    {
        [DataMember(Order = 1)] public string Market { get; set; }
        [DataMember(Order = 2)] public string BaseAsset { get; set; }
        [DataMember(Order = 3)] public string QuoteAsset { get; set; }
        [DataMember(Order = 4)] public int BaseAccuracy { get; set; }
        [DataMember(Order = 5)] public int PriceAccuracy { get; set; }
        [DataMember(Order = 6)] public double MinVolume { get; set; }
    }
}