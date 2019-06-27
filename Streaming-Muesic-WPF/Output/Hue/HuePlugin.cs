using System.Windows.Controls;

namespace Streaming_Muesic_WPF.Output.Hue
{
    internal class HuePlugin : OutputModuleBase
    {
        public override string Name => "Philips Hue";

        protected override UserControl CreateView()
        {
            return new HueView();
        }
    }
}
