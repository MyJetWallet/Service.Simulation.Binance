﻿using System.Threading.Tasks;
using MyJetWallet.Domain.ExternalMarketApi;
using MyJetWallet.Domain.ExternalMarketApi.Dto;

namespace Service.Simulation.Binance.Services
{
    public class OrderBookSourceGrpc: IOrderBookSource
    {
        private readonly OrderBookManager _manager;

        public OrderBookSourceGrpc(OrderBookManager manager)
        {
            _manager = manager;
        }

        public Task<GetNameResult> GetNameAsync()
        {
            return Task.FromResult(new GetNameResult() { Name = OrderBookManager.Source });
        }

        public Task<GetSymbolResponse> GetSymbolsAsync()
        {
            return Task.FromResult(new GetSymbolResponse() {Symbols = _manager.GetSymbols()});
        }

        public Task<HasSymbolResponse> HasSymbolAsync(MarketRequest request)
        {
            return Task.FromResult(new HasSymbolResponse() {Result = _manager.HasSymbol(request.Market)});
        }

        public Task<GetOrderBookResponse> GetOrderBookAsync(MarketRequest request)
        {
            var result = _manager.GetOrderBookAsync(request.Market);

            return Task.FromResult(new GetOrderBookResponse() {OrderBook = result.OrderBook});
        }
    }
}