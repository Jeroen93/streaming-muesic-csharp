using Streaming_Muesic_WPF.Model;
using Streaming_Muesic_WPF.Utils;
using System;
using System.Windows.Controls;

namespace Streaming_Muesic_WPF.Input
{
    internal abstract class InputModuleBase : IInputModule
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

        public void AddDataAvailableListener(EventHandler<DataAvailableEventArgs> handler)
        {
            var vm = view.DataContext as BaseInputVM;
            vm?.AddDataAvailableListener(handler);
        }

        public void RemoveDataAvailableListener(EventHandler<DataAvailableEventArgs> handler)
        {
            var vm = view.DataContext as BaseInputVM;
            vm?.RemoveDataAvailableListener(handler);
        }
    }
}
