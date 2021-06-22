using System;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using MyJetWallet.Domain.ExternalMarketApi;
using MyJetWallet.Domain.ExternalMarketApi.Dto;
using MyJetWallet.Domain.Orders;
using MyJetWallet.Sdk.GrpcMetrics;
using ProtoBuf.Grpc.Client;
using Service.Simulation.Binance.Grpc;
using Service.Simulation.Binance.Grpc.Models;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {

            GrpcClientFactory.AllowUnencryptedHttp2 = true;

            Console.Write("Press enter to start");
            Console.ReadLine();

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress("http://localhost:95");
            var _channel = channel.Intercept(new PrometheusMetricsInterceptor());

            var orderBookClient = _channel.CreateGrpcService<IOrderBookSource>();

            var orderBook = await orderBookClient.GetOrderBookAsync(new MarketRequest {Market = "BTCBUSD"});

            Console.WriteLine(JsonSerializer.Serialize(orderBook, new JsonSerializerOptions {WriteIndented = true}));

            Console.WriteLine("***********************************************");

            var balancesClient = _channel.CreateGrpcService<ISimulationTradingService>();
            
            await balancesClient.SetBalanceAsync(new SetBalanceRequest() {Symbol = "BUSD", Amount = 100000});
            await balancesClient.SetBalanceAsync(new SetBalanceRequest() {Symbol = "BTC", Amount = 100});
            
            var externalMarketClient = _channel.CreateGrpcService<IExternalMarket>();
            
            Console.WriteLine(JsonSerializer.Serialize(await externalMarketClient.GetBalancesAsync(),
                new JsonSerializerOptions {WriteIndented = true}));

            Console.WriteLine("***********************************************");
            var order = await externalMarketClient.MarketTrade(new MarketTradeRequest
            {
                ReferenceId = "Order8", Market = "BTCBUSD", Side = OrderSide.Sell, Volume = 0.1,
            });

            Console.WriteLine(JsonSerializer.Serialize(order, new JsonSerializerOptions {WriteIndented = true}));

            Console.WriteLine("End");
            Console.ReadLine();
            
            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}