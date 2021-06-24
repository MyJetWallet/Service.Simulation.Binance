using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using JetBrains.Annotations;
using MyJetWallet.Sdk.GrpcMetrics;
using ProtoBuf.Grpc.Client;
using Service.Simulation.Binance.Grpc;

namespace Service.Simulation.Binance.Client
{
    [UsedImplicitly]
    public class SimulationClientFactory
    {
        private readonly CallInvoker _channel;
        public SimulationClientFactory(string grpcServiceUrl)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(grpcServiceUrl);
            _channel = channel.Intercept(new PrometheusMetricsInterceptor());
        }
        
        public ISimulationTradingService GetSimulationFtxTradingService() => _channel.CreateGrpcService<ISimulationTradingService>();
        public ISimulationTradeHistoryService GetSimulationFtxTradeHistoryService() => _channel.CreateGrpcService<ISimulationTradeHistoryService>();

    }
}