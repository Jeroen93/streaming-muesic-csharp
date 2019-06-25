using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Bridge;
using Streaming_Muesic_WPF.Properties;

namespace Streaming_Muesic_WPF
{
    class HuePlugin
    {
        public string IpAddress { get; private set; }

        public event EventHandler BridgeConnected;

        private readonly Settings settings;
        private LocalHueClient client;        

        public HuePlugin()
        {
            settings = Settings.Default;            
        }

        public bool ConnectToLastKnownBridge()
        {
            var ip = settings.LastIPAddress;
            var key = settings.HueKey;

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

            IpAddress = ip;
            BridgeConnected?.Invoke(this, null); //Send list of lights in EventArgs?
            Console.WriteLine($"Client initialized on IP Address {IpAddress} and with key {key}");

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

        public void Connect(string address)
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
                var appKey = GetAppKeyAsync().Result;

                if (client.IsInitialized)
                {
                    settings.LastIPAddress = address;
                    settings.HueKey = appKey;
                    settings.Save();

                    BridgeConnected?.Invoke(null, null);
                }
            }
            catch (LinkButtonNotPressedException ex)
            {
                Console.WriteLine($"Link button was not pressed: {ex.Message}");
            }
        }

        private async Task<string> GetAppKeyAsync()
        {
            return await client.RegisterAsync("streamingmuesic", "mydevice").ConfigureAwait(false);
        }

        private async Task<string> GetOrFindIP()
        {
            Console.WriteLine("starting GetOrFindIP");
            var ip = settings.LastIPAddress;

            Console.WriteLine("ip: " + ip);

            if (!string.IsNullOrEmpty(ip))
            {
                return ip;
            }

            IBridgeLocator locator = new HttpBridgeLocator();
            Console.WriteLine("Searching for bridges");
            var bridgeIPs = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            Console.WriteLine("Searching done");

            if (!bridgeIPs.Any())
            {
                Console.WriteLine("Scan did not find a Hue Bridge. Try suppling a IP address for the bridge");
                return null;
            }

            if (bridgeIPs.Count() == 1)
            {
                ip = bridgeIPs.First().IpAddress;
                Console.WriteLine("Bridge found using IP address: " + ip);

                //Store the new IP address
                settings.LastIPAddress = ip;
                settings.Save();
            }
            else
            {
                Console.WriteLine("Found more than one bridge");

                var bridges = new ObservableCollection<BridgeItem>();
                foreach (var b in bridgeIPs)
                {
                    bridges.Add(new BridgeItem(b));
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var dialog = new ListDialogBox
                    {
                        Items = bridges
                    };
                    Console.WriteLine("showing dialog");
                    dialog.ShowDialog();

                    if (!dialog.IsCancelled)
                    {
                        var item = dialog.SelectedItem as BridgeItem;
                        ip = item.Bridge.IpAddress;

                        //Store the new IP address
                        settings.LastIPAddress = ip;
                        settings.Save();
                    }
                });
            }

            return ip;
        }

        private bool ClientNotInitialized()
        {
            return client == null || !client.IsInitialized;
        }
    }
}
