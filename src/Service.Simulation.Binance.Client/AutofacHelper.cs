// ReSharper disable UnusedMember.Global

using Autofac;
using Service.Simulation.Grpc;

namespace Service.Simulation.Binance.Client
{
    public static class AutofacHelper
    {
        public static void RegisterSimulationBinanceClient(this ContainerBuilder builder,
            string simulationFtxGrpcServiceUrl)
        {
            var factory = new SimulationBinanceClientFactory(simulationFtxGrpcServiceUrl);

            builder.RegisterInstance(factory.GetSimulationBinanceTradingService()).As<ISimulationTradingService>()
                .SingleInstance();
            builder.RegisterInstance(factory.GetSimulationBinanceTradeHistoryService()).As<ISimulationTradeHistoryService>()
                .SingleInstance();
        }
    }
}