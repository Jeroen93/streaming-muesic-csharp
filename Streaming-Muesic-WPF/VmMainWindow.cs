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
        private IProcessModule selectedProcessModule;
        private IOutputModule selectedOutputModule;

        public VmMainWindow(IEnumerable<IInputModule> inputs,
            IEnumerable<IProcessModule> processes, IEnumerable<IOutputModule> outputs)
        {
            InputModules = inputs.OrderBy(i => i.Name).ToList();
            ProcessModules = processes.OrderBy(p => p.Name).ToList();
            OutputModules = outputs.OrderBy(o => o.Name).ToList();

            if (InputModules.Count > 0)
            {
                SelectedInputModule = InputModules[0];
            }

            if (ProcessModules.Count > 0)
            {
                SelectedProcessModule = ProcessModules[0];
            }

            if (OutputModules.Count > 0)
            {
                SelectedOutputModule = OutputModules[0];
            }
        }

        public List<IInputModule> InputModules { get; }
        public List<IProcessModule> ProcessModules { get; }
        public List<IOutputModule> OutputModules { get; }

        public IInputModule SelectedInputModule
        {
            get => selectedInputModule;
            set
            {
                if (value != selectedInputModule)
                {
                    if (selectedInputModule != null && selectedProcessModule != null)
                    {
                        selectedInputModule.RemoveDataAvailableListener(selectedProcessModule.GetDataAvailableListener());
                    }

                    selectedInputModule?.Deactivate();
                    selectedInputModule = value;

                    if (selectedInputModule != null && selectedProcessModule != null)
                    {
                        selectedInputModule.AddDataAvailableListener(selectedProcessModule.GetDataAvailableListener());
                    }
                    
                    OnPropertyChanged(nameof(SelectedInputModule));
                    OnPropertyChanged(nameof(InputUI));
                }
            }
        }

        public IProcessModule SelectedProcessModule
        {
            get => selectedProcessModule;
            set
            {
                if (value != selectedProcessModule)
                {
                    if (selectedInputModule != null && selectedProcessModule != null)
                    {
                        selectedInputModule.RemoveDataAvailableListener(selectedProcessModule.GetDataAvailableListener());
                    }

                    selectedProcessModule?.Deactivate();
                    selectedProcessModule = value;
                    OnPropertyChanged(nameof(SelectedProcessModule));
                    OnPropertyChanged(nameof(ProcessUI));

                    if (selectedInputModule != null && selectedProcessModule != null)
                    {
                        selectedInputModule.AddDataAvailableListener(selectedProcessModule.GetDataAvailableListener());
                    }                    
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
        public UserControl ProcessUI => SelectedProcessModule?.UserInterface;
        public UserControl OutputUI => SelectedOutputModule?.UserInterface;
    }
}
