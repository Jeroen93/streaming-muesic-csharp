using Q42.HueApi.Models.Bridge;

namespace Streaming_Muesic_WPF.Output.Hue.dialog
{
    public class BridgeItem
    {
        public BridgeItem(LocatedBridge bridge)
        {
            Bridge = bridge;
            DisplayName = bridge.IpAddress;
        }

        public string DisplayName { get; }

        public LocatedBridge Bridge { get; }
    }
}
