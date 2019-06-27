using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Bridge;
using Streaming_Muesic_WPF.Output.Hue.dialog;
using Streaming_Muesic_WPF.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Streaming_Muesic_WPF.Output.Hue
{
    /// <summary>
    /// Interaction logic for HueView.xaml
    /// </summary>
    public partial class HueView : UserControl
    {
        private string ipAddress;
        private LocalHueClient client;

        public HueView()
        {
            InitializeComponent();
        }

        private void Btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            if (ConnectToLastKnownBridge())
            {
                tbIP.Text = ipAddress;
                ShowLights();

                btnConnect.IsEnabled = false;
            }
        }

        private void ShowLights()
        {
            IEnumerable<Light> lights = GetLights();

            RunOnUIThread(new Action(() =>
            {
                foreach (var item in lights)
                {
                    lbLights.Items.Add(item.Name);
                }
            }));
        }

        private async void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            var bridges = ScanForBridges().ToArray();
            bool isConnected = false;

            if (!bridges.Any())
            {
                Console.WriteLine("Scan did not find a Hue Bridge. Try suppling a IP address for the bridge");
                return;
            }

            if (bridges.Length == 1)
            {
                string ip = bridges[0].IpAddress;
                Console.WriteLine("Bridge found using IP address: " + ip);

                isConnected = await ConnectToNewBridge(ip).ConfigureAwait(false);
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

                    isConnected = await ConnectToNewBridge(ip).ConfigureAwait(false);
                }
            }

            if (isConnected)
            {
                ShowLights();

                RunOnUIThread(new Action(() =>
                {
                    tbIP.Text = ipAddress;
                    btnScan.IsEnabled = false;
                    btnConnect.IsEnabled = false;
                }));
            }
        }

        private void RunOnUIThread(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        public bool ConnectToLastKnownBridge()
        {
            var ip = Settings.Default.LastIPAddress;
            var key = Settings.Default.HueKey;

            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(key))
            {
                return false;
            }

            client = new LocalHueClient(ip);
            client.Initialize(key);

            if (ClientNotInitialized())
            {
                Console.WriteLine("Could not connect to last known bridge");
                return false;
            }

            ipAddress = ip;
            Console.WriteLine($"Reconnected to client on IP Address {ip} and with key {key}");

            return true;
        }

        public IEnumerable<LocatedBridge> ScanForBridges()
        {
            return ScanNetworkAsync().Result;
        }

        public IEnumerable<Light> GetLights()
        {
            return GetLightsAsync().Result;
        }

        private async Task<IEnumerable<Light>> GetLightsAsync()
        {
            if (ClientNotInitialized())
            {
                throw new InvalidOperationException("Hue client is not initialized");
            }

            //For more info, tips and tricks on using async methods: https://stackoverflow.com/a/10351400
            IEnumerable<Light> lights = await client.GetLightsAsync().ConfigureAwait(false);

            if (lights == null)
            {
                Console.WriteLine("Could not enumerate list. Hue may not be reachable.");
                return Enumerable.Empty<Light>();
            }

            return lights;
        }

        private async Task<IEnumerable<LocatedBridge>> ScanNetworkAsync()
        {
            IBridgeLocator locator = new HttpBridgeLocator();
            Console.WriteLine("Searching for bridges");
            var bridgeIPs = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            Console.WriteLine("Searching done");

            return bridgeIPs;
        }

        public async Task<bool> ConnectToNewBridge(string address)
        {
            if (!ClientNotInitialized())
            {
                throw new InvalidOperationException("Already connected to a bridge");
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentNullException(nameof(address));
            }

            client = new LocalHueClient(address);

            try
            {
                var appKey = await client.RegisterAsync("streamingmuesic", "mydevice").ConfigureAwait(false);

                if (client.IsInitialized)
                {
                    Settings.Default.LastIPAddress = address;
                    Settings.Default.HueKey = appKey;
                    Settings.Default.Save();

                    Console.WriteLine($"Connected to new client at {address} with key {appKey}");
                    ipAddress = address;

                    return true;
                }

                return false;
            }
            catch (LinkButtonNotPressedException ex)
            {
                Console.WriteLine($"Link button was not pressed: {ex.Message}");

                return false;
            }
        }

        private bool ClientNotInitialized()
        {
            return client == null || !client.IsInitialized;
        }
    }
}
