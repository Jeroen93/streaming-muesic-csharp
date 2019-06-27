﻿using Streaming_Muesic_WPF.Model;
using System;
using System.Windows.Controls;

namespace Streaming_Muesic_WPF.Output
{
    internal abstract class OutputModuleBase : IOutputModule
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
    }
}