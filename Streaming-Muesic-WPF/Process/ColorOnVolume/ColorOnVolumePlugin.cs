using System.Windows.Controls;

namespace Streaming_Muesic_WPF.Process.ColorOnVolume
{
    class ColorOnVolumePlugin : ProcessModuleBase
    {
        public override string Name => "Color on Volume";

        protected override UserControl CreateView()
        {
            return new ColorOnVolumeView();
        }
    }
}
