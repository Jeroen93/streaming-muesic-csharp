using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private LocalHueClient client;        

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

            IpAddress = ip;
            BridgeConnected?.Invoke(this, null); //Send list of lights in EventArgs?
            Console.WriteLine($"Reconnected to client on IP Address {IpAddress} and with key {key}");

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

        public async void Connect(string address)
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
                    BridgeConnected?.Invoke(null, null);
                }
            }
            catch (LinkButtonNotPressedException ex)
            {
                Console.WriteLine($"Link button was not pressed: {ex.Message}");
            }
        }

        private bool ClientNotInitialized()
        {
            return client == null || !client.IsInitialized;
        }
    }
}
