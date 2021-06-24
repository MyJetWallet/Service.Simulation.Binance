using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using JetBrains.Annotations;
using MyJetWallet.Sdk.GrpcMetrics;
using ProtoBuf.Grpc.Client;
using Service.Simulation.Grpc;

namespace Service.Simulation.Binance.Client
{
    [UsedImplicitly]
    public class SimulationBinanceClientFactory
    {
        private readonly CallInvoker _channel;
        public SimulationBinanceClientFactory(string grpcServiceUrl)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(grpcServiceUrl);
            _channel = channel.Intercept(new PrometheusMetricsInterceptor());
        }
        
        public ISimulationTradingService GetSimulationBinanceTradingService() => _channel.CreateGrpcService<ISimulationTradingService>();
        public ISimulationTradeHistoryService GetSimulationBinanceTradeHistoryService() => _channel.CreateGrpcService<ISimulationTradeHistoryService>();

    }
}