using Streaming_Muesic_WPF.Utils;
using Streaming_Muesic_WPF.ViewModel;
using System;

namespace Streaming_Muesic_WPF.Process
{
    internal abstract class ProcessViewModelBase : ViewModelBase
    {
        public EventHandler<DataAvailableEventArgs> GetDataAvailableListener()
        {
            return (s, dae) => {
                for (int i = 0; i < dae.Left.Length; i++)
                {
                    Calculate(dae.Left[i], dae.Right[i]);
                }
            };
        }

        public abstract void Calculate(float left, float right);
    }
}
