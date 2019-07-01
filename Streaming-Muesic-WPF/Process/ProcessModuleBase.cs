using Streaming_Muesic_WPF.Model;
using Streaming_Muesic_WPF.Utils;
using System;
using System.Windows.Controls;

namespace Streaming_Muesic_WPF.Process
{
    internal abstract class ProcessModuleBase : IProcessModule
    {
        private UserControl view;

        protected abstract UserControl CreateView();

        public abstract string Name { get; }

        public UserControl UserInterface => view ?? (view = CreateView());

        public void Deactivate()
        {
            if (view == null)
            {
                return;
            }

            var d = view.DataContext as IDisposable;
            d?.Dispose();
            view = null;
        }

        public EventHandler<DataAvailableEventArgs> GetDataAvailableListener()
        {
            var vm = UserInterface.DataContext as ProcessViewModelBase;

            return vm.GetDataAvailableListener();
        }
    }
}
