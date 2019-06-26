using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Q42.HueApi;

namespace Streaming_Muesic_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HuePlugin huePlugin;

        public MainWindow()
        {
            InitializeComponent();

            huePlugin = new HuePlugin();
            huePlugin.BridgeConnected += Hue_BridgeConnected;
        }

        private void Btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            huePlugin.ConnectToLastKnownBridge();
        }

        private void Hue_BridgeConnected(object sender, EventArgs e)
        {
            IEnumerable<Light> lights = huePlugin.GetLights();

            RunOnUIThread(new Action(() =>
            {
                foreach (var item in lights)
                {
                    lbLights.Items.Add(item.Name);
                }
            }));
        }

        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            var bridges = huePlugin.ScanForBridges().ToArray();

            if (!bridges.Any())
            {
                Console.WriteLine("Scan did not find a Hue Bridge. Try suppling a IP address for the bridge");
                return;
            }

            if (bridges.Length == 1)
            {
                string ip = bridges[0].IpAddress;
                Console.WriteLine("Bridge found using IP address: " + ip);

                huePlugin.Connect(ip);
            }
            else
            {
                Console.WriteLine("Found more than one bridge");

                var bridgeItems = new ObservableCollection<BridgeItem>(bridges.Select(b => new BridgeItem(b)));

                var dialog = new ListDialogBox
                {
                    Items = bridgeItems
                };
                Console.WriteLine("showing dialog");
                dialog.ShowDialog();

                if (!dialog.IsCancelled)
                {
                    var item = dialog.SelectedItem as BridgeItem;
                    var ip = item.Bridge.IpAddress;
                    huePlugin.Connect(ip);
                }
            }
        }

        private void RunOnUIThread(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }
    }
}
