using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;

namespace Service.Simulation.Binance.Client
{
    [UsedImplicitly]
    public class BinanceClientFactory : MyGrpcClientFactory
    {
        public BinanceClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }
    }
}