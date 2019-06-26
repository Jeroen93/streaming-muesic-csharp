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

        public VmMainWindow(IEnumerable<IInputModule> inputs)
        {
            InputModules = inputs.OrderBy(i => i.Name).ToList();

            if (InputModules.Count > 0)
            {
                SelectedInputModule = InputModules[0];
            }
        }

        public List<IInputModule> InputModules { get; }

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

        public UserControl InputUI => SelectedInputModule?.UserInterface;
    }
}
