using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.Simulation.Binance.NoSql;
using Service.Simulation.Grpc;
using Service.Simulation.Grpc.Models;

namespace Service.Simulation.Binance.Services
{
    internal class SimulationTradingService : ISimulationTradingService
    {
        private readonly ILogger<SimulationTradingService> _logger;
        private readonly MarketCache _marketCache;
        private readonly OrderBookManager _orderBookManager;
        private readonly IMyNoSqlServerDataWriter<BalancesNoSql> _balanceWriter;
        private readonly TradeHistory _history;

        public SimulationTradingService(ILogger<SimulationTradingService> logger, MarketCache marketCache,
            OrderBookManager orderBookManager, IMyNoSqlServerDataWriter<BalancesNoSql> balanceWriter,
            TradeHistory history)
        {
            _logger = logger;
            _marketCache = marketCache;
            _orderBookManager = orderBookManager;
            _balanceWriter = balanceWriter;
            _history = history;
        }

        public async Task<ExecuteMarketOrderResponse> ExecuteMarketOrderAsync(ExecuteMarketOrderRequest request)
        {
            _logger.LogInformation("ExecuteMarketOrderAsync Request: {tradeText}",
                JsonConvert.SerializeObject(request));

            var marketResp = await GetMarketInfoAsync(new GetMarketInfoRequest() {Market = request.Market});

            if (marketResp.Info == null)
            {
                _logger.LogError("Cannot execute market order: {jsonText}", JsonConvert.SerializeObject(request));
                return new ExecuteMarketOrderResponse()
                {
                    Success = false,
                };
            }

            var market = marketResp.Info;

            var orderBookResp = _orderBookManager.GetOrderBookAsync(request.Market);
            if (orderBookResp.OrderBook == null)
            {
                _logger.LogError("Cannot execute market order, order book do not found. Request: {jsonText}",
                    JsonConvert.SerializeObject(request));
                return new ExecuteMarketOrderResponse()
                {
                    Success = false,
                };
            }

            var balances = (await GetBalancesAsync()).Balances;

            var baseBalance = balances.FirstOrDefault(e => e.Symbol == market.BaseAsset);
            var quoteBalance = balances.FirstOrDefault(e => e.Symbol == market.QuoteAsset);

            if (request.Side == SimulationOrderSide.Buy)
            {
                var remindVolume = request.Size;
                var quoteVolume = 0.0;

                var levels = orderBookResp.OrderBook.Asks.OrderBy(e => e.Price);
                foreach (var level in levels)
                {
                    if (level.Price >= remindVolume)
                    {
                        quoteVolume += remindVolume * level.Price;
                        remindVolume = 0;
                        break;
                    }

                    quoteVolume += level.Price * level.Volume;
                    remindVolume -= level.Volume;
                }

                if (remindVolume != 0)
                {
                    _logger.LogError(
                        "Cannot execute market order, not enough liquidity. RemindVolume: {remindNumber}. Request: {jsonText}",
                        remindVolume, JsonConvert.SerializeObject(request));
                    return new ExecuteMarketOrderResponse()
                    {
                        Success = false,
                    };
                }


                if (quoteBalance == null || quoteBalance.Amount < quoteVolume)
                {
                    _logger.LogError(
                        "Cannot execute market order, not enough balance. Required: {requiredNumber}; Exist: {existNumber}. Request: {jsonText}",
                        quoteBalance, quoteBalance?.Amount ?? 0.0, JsonConvert.SerializeObject(request));
                    return new ExecuteMarketOrderResponse()
                    {
                        Success = false,
                    };
                }

                quoteBalance.Amount -= quoteVolume;
                if (baseBalance == null)
                    baseBalance = new GetBalancesResponse.Balance() {Symbol = market.BaseAsset, Amount = 0};
                baseBalance.Amount += request.Size;

                await SetBalanceAsync(new SetBalanceRequest()
                    {Symbol = quoteBalance.Symbol, Amount = quoteBalance.Amount});
                await SetBalanceAsync(
                    new SetBalanceRequest() {Symbol = baseBalance.Symbol, Amount = baseBalance.Amount});

                var price = quoteVolume / request.Size;

                var trade = new SimTrade()
                {
                    Market = request.Market,
                    ClientId = request.ClientId,
                    Id = Guid.NewGuid().ToString("N"),
                    Side = request.Side,
                    Size = request.Size,
                    Price = price,
                    Timestamp = DateTime.UtcNow
                };

                _history.AddTrade(trade);

                var resp = new ExecuteMarketOrderResponse()
                {
                    Success = true,
                    Trade = trade
                };

                _logger.LogInformation("Trade: {tradeText}", JsonConvert.SerializeObject(resp));

                return resp;
            }
            else
            {
                var remindVolume = request.Size;
                var quoteVolume = 0.0;

                var levels = orderBookResp.OrderBook.Bids.OrderByDescending(e => e.Price);
                foreach (var level in levels)
                {
                    if (level.Volume >= remindVolume)
                    {
                        quoteVolume += remindVolume * level.Price;
                        remindVolume = 0;
                        break;
                    }

                    quoteVolume += level.Price * level.Volume;
                    remindVolume -= level.Volume;
                }

                if (remindVolume != 0)
                {
                    _logger.LogError(
                        "Cannot execute market order, not enough liquidity. RemindVolume: {remindNumber}. Request: {jsonText}",
                        remindVolume, JsonConvert.SerializeObject(request));
                    return new ExecuteMarketOrderResponse()
                    {
                        Success = false,
                    };
                }


                if (baseBalance == null || baseBalance.Amount < request.Size)
                {
                    _logger.LogError(
                        "Cannot execute market order, not enough balance. Required: {requiredNumber}; Exist: {existNumber}. Request: {jsonText}",
                        request.Side, baseBalance?.Amount ?? 0.0, JsonConvert.SerializeObject(request));
                    return new ExecuteMarketOrderResponse()
                    {
                        Success = false,
                    };
                }

                baseBalance.Amount -= request.Size;

                if (quoteBalance == null)
                    quoteBalance = new GetBalancesResponse.Balance() {Symbol = market.QuoteAsset, Amount = 0};
                quoteBalance.Amount += quoteVolume;

                await SetBalanceAsync(
                    new SetBalanceRequest() {Symbol = baseBalance.Symbol, Amount = baseBalance.Amount});
                await SetBalanceAsync(new SetBalanceRequest()
                    {Symbol = quoteBalance.Symbol, Amount = quoteBalance.Amount});

                var price = quoteVolume / request.Size;

                var trade = new SimTrade()
                {
                    Market = request.Market,
                    ClientId = request.ClientId,
                    Id = Guid.NewGuid().ToString("N"),
                    Side = request.Side,
                    Size = request.Size,
                    Price = price,
                    Timestamp = DateTime.UtcNow
                };

                _history.AddTrade(trade);

                var resp = new ExecuteMarketOrderResponse()
                {
                    Success = true,
                    Trade = trade
                };

                _logger.LogInformation("Trade: {tradeText}", JsonConvert.SerializeObject(resp));

                return resp;
            }
        }

        public async Task<GetBalancesResponse> GetBalancesAsync()
        {
            var data = await _balanceWriter.GetAsync(BalancesNoSql.GeneratePartitionKey(Program.Settings.Name));
            return new GetBalancesResponse()
            {
                Balances = data.Select(e => e.Balance).ToList()
            };
        }

        public async Task<GetMarketInfoResponse> GetMarketInfoAsync(GetMarketInfoRequest request)
        {
            var data = _marketCache.GetMarkets().Find(e => e.Market == request.Market);

            if (data == null)
            {
                _logger.LogError("Cannot found market {market}", request.Market);
                return new GetMarketInfoResponse();
            }

            var resp = new GetMarketInfoResponse()
            {
                Info = ReadMarket(data)
            };

            return resp;
        }

        public async Task<GetMarketInfoListResponse> GetMarketInfoListAsync()
        {
            var data = _marketCache.GetMarkets();

            var resp = new GetMarketInfoListResponse()
            {
                Info = new List<MarketInfo>()
            };

            foreach (var market in data)
            {
                resp.Info.Add(ReadMarket(market));
            }

            return resp;
        }

        private MarketInfo ReadMarket(ExchangeMarketInfo market)
        {
            var result = new MarketInfo()
            {
                Market = market.Market,
                BaseAsset = market.BaseAsset,
                QuoteAsset = market.QuoteAsset,
                MinVolume = (double) market.MinVolume
            };

            //var volumeParams = market.sizeIncrement.ToString(CultureInfo.InvariantCulture).Split('.');
            //var priceParams = market.priceIncrement.ToString(CultureInfo.InvariantCulture).Split('.');
            result.BaseAccuracy = 8; //volumeParams.Length == 2 ? volumeParams.Length : 0;
            result.PriceAccuracy = 8; // priceParams.Length == 2 ? priceParams.Length : 0;

            return result;
        }

        public async Task SetBalanceAsync(SetBalanceRequest request)
        {
            var item = BalancesNoSql.Create(Program.Settings.Name, new GetBalancesResponse.Balance()
            {
                Symbol = request.Symbol,
                Amount = request.Amount
            });

            await _balanceWriter.InsertOrReplaceAsync(item);
        }
    }
}