using MyNoSqlServer.Abstractions;
using Service.Simulation.Grpc.Models;

namespace Service.Simulation.Binance.NoSql
{
    public class BalancesNoSql: MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-simulation-balances";

        public static string GeneratePartitionKey(string simulationName) => simulationName;
        public static string GenerateRowKey(string asset) => asset;

        public GetBalancesResponse.Balance Balance { get; set; }

        public static BalancesNoSql Create(string simulationName, GetBalancesResponse.Balance balance)
        {
            return new BalancesNoSql()
            {
                PartitionKey = GeneratePartitionKey(simulationName),
                RowKey = GenerateRowKey(balance.Symbol),
                Balance = balance
            };
        }
    }
}