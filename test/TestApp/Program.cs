using System;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using MyJetWallet.Domain.ExternalMarketApi;
using MyJetWallet.Sdk.GrpcMetrics;
using ProtoBuf.Grpc.Client;
using Service.Simulation.Grpc;
using Service.Simulation.Grpc.Models;

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
            var channel = GrpcChannel.ForAddress("http://simulation-binance-1.services.svc.cluster.local:80");
            var _channel = channel.Intercept(new PrometheusMetricsInterceptor());
            
            // var orderBookClient = _channel.CreateGrpcService<IOrderBookSource>();
            //
            // var orderBook = await orderBookClient.GetOrderBookAsync(new MarketRequest {Market = "BTCBUSD"});
            //
            // Console.WriteLine(JsonSerializer.Serialize(orderBook, new JsonSerializerOptions {WriteIndented = true}));
            //
            // var markets = await orderBookClient.GetSymbolsAsync();
            //
            // Console.WriteLine(JsonSerializer.Serialize(markets, new JsonSerializerOptions {WriteIndented = true}));

            Console.WriteLine("***********************************************");
            
            var balancesClient = _channel.CreateGrpcService<ISimulationTradingService>();
            
            // await balancesClient.SetBalanceAsync(new SetBalanceRequest() {Symbol = "ALGO", Amount = 1000000});
            // await balancesClient.SetBalanceAsync(new SetBalanceRequest() {Symbol = "BCH", Amount = 1000});
            await balancesClient.SetBalanceAsync(new SetBalanceRequest() {Symbol = "EUR", Amount = 1000000});
            // await balancesClient.SetBalanceAsync(new SetBalanceRequest() {Symbol = "DASH", Amount = 1000000});
            // await balancesClient.SetBalanceAsync(new SetBalanceRequest() {Symbol = "BUSD", Amount = 100000});
            // await balancesClient.SetBalanceAsync(new SetBalanceRequest() {Symbol = "BTC", Amount = 100});
            // await balancesClient.SetBalanceAsync(new SetBalanceRequest() {Symbol = "EOS", Amount = 1000000});
            // await balancesClient.SetBalanceAsync(new SetBalanceRequest() {Symbol = "ETH", Amount = 10000});
            // await balancesClient.SetBalanceAsync(new SetBalanceRequest() {Symbol = "LTC", Amount = 10000});
            // await balancesClient.SetBalanceAsync(new SetBalanceRequest() {Symbol = "TRX", Amount = 100000});
            // await balancesClient.SetBalanceAsync(new SetBalanceRequest() {Symbol = "XLM", Amount = 100000});
            // await balancesClient.SetBalanceAsync(new SetBalanceRequest() {Symbol = "XRP", Amount = 1000000});
            
            var externalMarketClient = _channel.CreateGrpcService<IExternalMarket>();
            
            Console.WriteLine(JsonSerializer.Serialize(await externalMarketClient.GetBalancesAsync(null),
                new JsonSerializerOptions {WriteIndented = true}));
            
            // Console.WriteLine("***********************************************");
            // var order = await externalMarketClient.MarketTrade(new MarketTradeRequest
            // {
            //     ReferenceId = "Order8", Market = "BTCBUSD", Side = OrderSide.Sell, Volume = 0.1,
            // });
            //
            // Console.WriteLine(JsonSerializer.Serialize(order, new JsonSerializerOptions {WriteIndented = true}));
            //
            // Console.WriteLine("End");
            // Console.ReadLine();
            //
            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}