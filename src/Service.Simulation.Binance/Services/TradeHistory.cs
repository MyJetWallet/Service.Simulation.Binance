using System.Collections.Generic;
using System.Linq;
using Service.Simulation.Grpc.Models;

namespace Service.Simulation.Binance.Services
{
    public class TradeHistory
    {
        private List<SimTrade> _data = new List<SimTrade>();

        public void AddTrade(SimTrade trade)
        {
            lock (_data)
            {
                _data.Insert(0, trade);

                while (_data.Count > 50)
                {
                    _data.RemoveAt(_data.Count - 1);
                }
            }
        }

        public List<SimTrade> GetTrades()
        {
            lock (_data)
            {
                return _data.ToList();
            }
        }
    }
}