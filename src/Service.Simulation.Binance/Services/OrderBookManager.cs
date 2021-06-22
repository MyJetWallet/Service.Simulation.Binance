using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MyJetWallet.Connector.Binance.Ws;
using MyJetWallet.Domain.ExternalMarketApi.Dto;
using MyJetWallet.Domain.ExternalMarketApi.Models;

namespace Service.Simulation.Binance.Services
{
    public class OrderBookManager : IDisposable
    {
        public static string Source { get; private set; }

        private readonly ILogger<OrderBookManager> _logger;

        private BinanceWsOrderBooks _client;

        private string[] _symbols = { };

        public OrderBookManager(ILogger<OrderBookManager> logger)
        {
            Source = $"Simulation-{Program.Settings.Name}";

            _logger = logger;

            _symbols = Program.Settings.InstrumentsOriginalSymbolToSymbol.Split(';').ToArray();
            _client = new BinanceWsOrderBooks(_logger, _symbols, true);
        }

        public List<string> GetSymbols()
        {
            return _symbols.ToList();
        }

        public bool HasSymbol(string symbol)
        {
            return _symbols.Contains(symbol);
        }

        public GetOrderBookResponse GetOrderBookAsync(string market)
        {
            var data = _client.GetOrderBook(market);

            if (data == null)
            {
                return new GetOrderBookResponse()
                {
                    OrderBook = null
                };
            }

            var resp = new GetOrderBookResponse();
            resp.OrderBook = new LeOrderBook();
            resp.OrderBook.Source = Source;
            resp.OrderBook.Symbol = data.Symbol;
            resp.OrderBook.Timestamp = data.Time;
            resp.OrderBook.Asks = data.Asks.OrderBy(e => e.Key)
                .Select(e => new LeOrderBookLevel((double) e.Key, (double) e.Value)).ToList();
            resp.OrderBook.Bids = data.Bids.OrderByDescending(e => e.Key)
                .Select(e => new LeOrderBookLevel((double) e.Key, (double) e.Value)).ToList();

            return resp;
        }

        public void Start()
        {
            _client.Start();
        }

        public void Stop()
        {
            _client.Stop();
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}