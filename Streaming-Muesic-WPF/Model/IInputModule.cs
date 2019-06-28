using Streaming_Muesic_WPF.Utils;
using System;

namespace Streaming_Muesic_WPF.Model
{
    interface IInputModule : IModule
    {
        void AddDataAvailableListener(EventHandler<DataAvailableEventArgs> handler);

        void RemoveDataAvailableListener(EventHandler<DataAvailableEventArgs> handler);
    }
}
