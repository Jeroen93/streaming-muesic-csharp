using System.Windows.Controls;

namespace Streaming_Muesic_WPF.Model
{
    interface IModule
    {
        string Name { get; }
        UserControl UserInterface { get; }
        void Deactivate();
    }
}
