using Streaming_Muesic_WPF.Model;
using Streaming_Muesic_WPF.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Streaming_Muesic_WPF
{
    internal class VmMainWindow : ViewModelBase
    {
        private IInputModule selectedInputModule;
        private IOutputModule selectedOutputModule;

        public VmMainWindow(IEnumerable<IInputModule> inputs, IEnumerable<IOutputModule> outputs)
        {
            InputModules = inputs.OrderBy(i => i.Name).ToList();
            OutputModules = outputs.OrderBy(o => o.Name).ToList();

            if (InputModules.Count > 0)
            {
                SelectedInputModule = InputModules[0];
            }

            if (OutputModules.Count > 0)
            {
                SelectedOutputModule = OutputModules[0];
            }
        }

        public List<IInputModule> InputModules { get; }
        public List<IOutputModule> OutputModules { get; }

        public IInputModule SelectedInputModule
        {
            get => selectedInputModule;
            set
            {
                if (value != selectedInputModule)
                {
                    selectedInputModule?.Deactivate();
                    selectedInputModule = value;
                    OnPropertyChanged(nameof(SelectedInputModule));
                    OnPropertyChanged(nameof(InputUI));
                }
            }
        }

        public IOutputModule SelectedOutputModule
        {
            get => selectedOutputModule;
            set
            {
                if (value != selectedOutputModule)
                {
                    selectedOutputModule?.Deactivate();
                    selectedOutputModule = value;
                    OnPropertyChanged(nameof(SelectedOutputModule));
                    OnPropertyChanged(nameof(OutputUI));
                }
            }
        }

        public UserControl InputUI => SelectedInputModule?.UserInterface;
        public UserControl OutputUI => SelectedOutputModule?.UserInterface;
    }
}
