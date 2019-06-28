using Streaming_Muesic_WPF.Utils;
using System;

namespace Streaming_Muesic_WPF.Model
{
    interface IProcessModule : IModule
    {
        EventHandler<DataAvailableEventArgs> GetDataAvailableListener();
    }
}
