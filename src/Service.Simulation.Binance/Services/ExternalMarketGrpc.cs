using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.ExternalMarketApi;
using MyJetWallet.Domain.ExternalMarketApi.Dto;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using MyJetWallet.Domain.Orders;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;
using Service.Simulation.Grpc;
using Service.Simulation.Grpc.Models;
using GetBalancesResponse = MyJetWallet.Domain.ExternalMarketApi.Dto.GetBalancesResponse;
using GetMarketInfoListResponse = MyJetWallet.Domain.ExternalMarketApi.Dto.GetMarketInfoListResponse;
using GetMarketInfoResponse = MyJetWallet.Domain.ExternalMarketApi.Dto.GetMarketInfoResponse;

namespace Service.Simulation.Binance.Services
{
    public class ExternalMarketGrpc: IExternalMarket
    {
        private static Dictionary<string, ExchangeMarketInfo> _marketInfoData = new Dictionary<string, ExchangeMarketInfo>();

        private readonly ILogger<ExternalMarketGrpc> _logger;
        private readonly ISimulationTradingService _service;
        private List<string> _symbolList;

        public ExternalMarketGrpc(
            ILogger<ExternalMarketGrpc> logger,
            ISimulationTradingService service)
        {
            _logger = logger;
            _service = service;

            _symbolList = !string.IsNullOrEmpty(Program.Settings.InstrumentsOriginalSymbolToSymbol)
                ? Program.Settings.InstrumentsOriginalSymbolToSymbol.Split(';').ToList()
                : new List<string>();
        }

        public Task<GetNameResult> GetNameAsync(GetNameRequest request)
        {
            return Task.FromResult(new GetNameResult() {Name = OrderBookManager.Source });
        }

        public async Task<GetBalancesResponse> GetBalancesAsync(GetBalancesRequest request)
        {
            using var activity = MyTelemetry.StartActivity("Get balance");

            var resp = await _service.GetBalancesAsync();
            var result = resp.Balances.Select(e => new ExchangeBalance()
                {Symbol = e.Symbol, Balance = (decimal)e.Amount, Free = (decimal)e.Amount}).ToList();   
            return new GetBalancesResponse(){Balances = result};
        }

        public async Task<GetMarketInfoResponse> GetMarketInfoAsync(MarketRequest request)
        {
            if (_marketInfoData!.Any() != true)
                await LoadMarketInfo();

            if (_marketInfoData.TryGetValue(request.Market, out var resp))
                return new GetMarketInfoResponse(){Info = resp};

            return new GetMarketInfoResponse() { Info = null };
        }

        public async Task<GetMarketInfoListResponse> GetMarketInfoListAsync(GetMarketInfoListRequest request)
        {
            if (_marketInfoData!.Any() != true)
                await LoadMarketInfo();

            return new GetMarketInfoListResponse() {Infos = _marketInfoData.Values.Where(e => _symbolList.Contains(e.Market)).ToList()};
        }

        public async Task<ExchangeTrade> MarketTrade(MarketTradeRequest request)
        {
            using var activity = MyTelemetry.StartActivity("MarketTrade");

            try
            {
                var tradeRequest = new ExecuteMarketOrderRequest()
                {
                    ClientId = request.ReferenceId,
                    Market = request.Market,
                    Side = request.Side == OrderSide.Buy ? SimulationOrderSide.Buy : SimulationOrderSide.Sell,
                    Size = request.Volume
                };

                request.AddToActivityAsJsonTag("request");

                var marketInfo = await GetMarketInfoAsync(new MarketRequest() {Market = request.Market });
                if (marketInfo?.Info == null)
                {
                    throw new Exception(
                        $"Cannot execute trade, market info do not found. Request: {JsonConvert.SerializeObject(request)}");
                }

                marketInfo.AddToActivityAsJsonTag("market-info");

                var resp = await _service.ExecuteMarketOrderAsync(tradeRequest);

                resp.AddToActivityAsJsonTag("response");

                if (!resp.Success)
                {
                    throw new Exception(
                        $"Cannot execute trade in simulation binance. Request: {JsonConvert.SerializeObject(request)}");
                }

                var result = new ExchangeTrade()
                {
                    Id = resp.Trade.Id,
                    Market = resp.Trade.Market,
                    Price = resp.Trade.Price,
                    Volume = resp.Trade.Size,
                    Timestamp = resp.Trade.Timestamp,
                    Side = resp.Trade.Side == SimulationOrderSide.Buy ? OrderSide.Buy : OrderSide.Sell,
                    OppositeVolume = (double) ((decimal) resp.Trade.Price * (decimal) resp.Trade.Size),
                    Source = (await GetNameAsync(null)).Name
                };

                result.AddToActivityAsJsonTag("result");

                return result;
            }
            catch(Exception ex)
            {
                ex.FailActivity();
                _logger.LogError(ex, "Cannot execute MarketTrade in simulation Binance");
                throw;
            }
        }

        private async Task LoadMarketInfo()
        {
            using var activity = MyTelemetry.StartActivity("Load market info");
            try
            {
                var data = await _service.GetMarketInfoListAsync();

                var result = new Dictionary<string, ExchangeMarketInfo>();

                foreach (var marketInfo in data.Info)
                {
                    var resp = new ExchangeMarketInfo()
                    {
                        Market = marketInfo.Market,
                        MinVolume = marketInfo.MinVolume,
                        PriceAccuracy = marketInfo.PriceAccuracy,
                        BaseAsset = marketInfo.BaseAsset,
                        QuoteAsset = marketInfo.QuoteAsset,
                        VolumeAccuracy = marketInfo.BaseAccuracy
                    };

                    result[resp.Market] = resp;
                }

                _marketInfoData = result;
            }
            catch (Exception ex)
            {
                ex.FailActivity();
                throw;
            }
        }
    }
}