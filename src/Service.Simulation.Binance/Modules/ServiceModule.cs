using Autofac;
using Binance;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataWriter;
using Service.Simulation.Binance.NoSql;
using Service.Simulation.Binance.Services;
using Service.Simulation.Grpc;

namespace Service.Simulation.Binance.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {

            builder
                .RegisterType<TradeHistory>()
                .AsSelf()
                .SingleInstance();

            RegisterMyNoSqlWriter<BalancesNoSql>(builder, BalancesNoSql.TableName);

            builder
                .RegisterType<SimulationTradingService>()
                .As<ISimulationTradingService>();

            builder
                .RegisterType<OrderBookManager>()
                .AsSelf()
                .SingleInstance();
            
            
            var api = new BinanceApi();
            var user = new BinanceApiUser(Program.Settings.BinanceApiKey, Program.Settings.BinanceApiSecret);

            builder.RegisterInstance(api).AsSelf().SingleInstance();
            builder.RegisterInstance(user).AsSelf().SingleInstance();
            builder.RegisterType<OrderBookManager>().AsSelf().SingleInstance();

            builder
                .RegisterType<MarketCache>()
                .AsSelf()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
        }

        private void RegisterMyNoSqlWriter<TEntity>(ContainerBuilder builder, string table)
            where TEntity : IMyNoSqlDbEntity, new()
        {
            builder.Register(ctx =>
                    new MyNoSqlServerDataWriter<TEntity>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), table,
                        true))
                .As<IMyNoSqlServerDataWriter<TEntity>>()
                .SingleInstance();
        }
    }
}