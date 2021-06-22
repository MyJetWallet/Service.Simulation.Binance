using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Simulation.Binance.Settings
{
    public class SettingsModel
    {
        [YamlProperty("SimulationBinance.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("SimulationBinance.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("SimulationBinance.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
        
        [YamlProperty("SimulationBinance.Name")]
        public string Name { get; set; }

        [YamlProperty("SimulationBinance.MyNoSqlWriterUrl")]
        public string MyNoSqlWriterUrl { get; set; }

        [YamlProperty("SimulationBinance.InstrumentsOriginalSymbolToSymbol")]
        public string InstrumentsOriginalSymbolToSymbol { get; set; }

        [YamlProperty("SimulationBinance.RefreshBalanceIntervalSec")]
        public int RefreshBalanceIntervalSec { get; set; }

        [YamlProperty("SimulationBinance.ApiKey")]
        public string BinanceApiKey { get; set; }

        [YamlProperty("SimulationBinance.ApiSecret")]
        public string BinanceApiSecret { get; set; }
    }
}
