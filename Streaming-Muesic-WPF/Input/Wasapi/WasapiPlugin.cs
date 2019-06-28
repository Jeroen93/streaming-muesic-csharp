using System.Windows.Controls;

namespace Streaming_Muesic_WPF.Input.Wasapi
{
    class WasapiPlugin : InputModuleBase
    {
        protected override UserControl CreateView()
        {
            return new WasapiView { DataContext = new WasapiViewModel() };
        }

        public override string Name => "WASAPI";
    }
}
