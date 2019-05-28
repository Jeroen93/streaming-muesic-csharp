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
        public string Key { get; private set; }
        public IEnumerable<Light> Lights { get; private set; }

        public event EventHandler BridgeConnected;

        private Settings settings;
        private LocalHueClient client;        

        public HuePlugin()
        {
            settings = Settings.Default;            
        }

        public void Connect()
        {
            var t = GetClient();
            t.Wait();

            if (ClientNotInitialized())
            {
                MessageBox.Show("Could not connect to a bridge with specified Ip Address and Key");
                return;
            }

            t = GetLights();
            t.Wait();
        }

        public IEnumerable<LocatedBridge> ScanForBridges()
        {
            return ScanNetworkAsync().Result;
        }

        private async Task GetClient()
        {
            //For more info, tips and tricks on using async methods: https://stackoverflow.com/a/10351400
            IpAddress = await GetOrFindIP().ConfigureAwait(false);

            if (string.IsNullOrEmpty(IpAddress))
            {
                return;
            }

            client = new LocalHueClient(IpAddress);

            Key = await GetOrRegisterKey().ConfigureAwait(false);

            client.Initialize(Key);

            if (ClientNotInitialized())
            {
                Console.WriteLine("Could not initialize client");
                return;
            }

            Console.WriteLine($"Client initialized on IP Address {IpAddress} and with key {Key}");            
        }

        private async Task GetLights()
        {
            if (ClientNotInitialized())
                throw new InvalidOperationException("Hue client is not initialized");

            Lights = await client.GetLightsAsync().ConfigureAwait(false);
            if (Lights == null)
            {
                Console.WriteLine("Could not enumerate list. Hue may not be reachable.");
                return;
            }

            BridgeConnected?.Invoke(this, null); //Send list of lights in EventArgs?
        }

        private async Task<IEnumerable<LocatedBridge>> ScanNetworkAsync()
        {
            var ip = settings.LastIPAddress;

            if (!string.IsNullOrEmpty(ip))
            {
                return Enumerable.Empty<LocatedBridge>();
            }

            IBridgeLocator locator = new HttpBridgeLocator();
            Console.WriteLine("Searching for bridges");
            var bridgeIPs = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            Console.WriteLine("Searching done");

            return bridgeIPs;
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

        private async Task<string> GetOrRegisterKey()
        {
            Console.WriteLine("Starting getting key");
            var key = settings.HueKey;

            if (string.IsNullOrEmpty(key))
            {
                Console.WriteLine("Starting registering key");
                key = await client.RegisterAsync("streamingmuesic", "6UiL4alABwlLXtoXqp7nlQHMAORbKEyoFjSHdLFw");
                Console.WriteLine("Done registering key: " + key);

                if (!string.IsNullOrEmpty(key))
                {
                    //Store the new key
                    settings.HueKey = key;
                    settings.Save();
                }
                else
                {
                    Console.WriteLine("Could not retrieve a key from the bridge");
                    return null;
                }                
            }

            Console.WriteLine("Key: " + key);
            return key;
        }

        private bool ClientNotInitialized()
        {
            return client == null || !client.IsInitialized;
        }
    }
}
