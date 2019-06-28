using Streaming_Muesic_WPF.Utils;
using Streaming_Muesic_WPF.ViewModel;
using System;

namespace Streaming_Muesic_WPF.Input
{
    internal class BaseInputVM : ViewModelBase
    {
        public SampleAggregator Aggregator { get; protected set; }

        public void AddDataAvailableListener(EventHandler<DataAvailableEventArgs> handler)
        {
            Aggregator.AddDataListener(handler);
        }

        public void RemoveDataAvailableListener(EventHandler<DataAvailableEventArgs> handler)
        {
            Aggregator.RemoveDataListener(handler);
        }
    }
}
