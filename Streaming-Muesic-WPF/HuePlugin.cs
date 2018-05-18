using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.NET;
using Streaming_Muesic_WPF.Properties;

namespace Streaming_Muesic_WPF
{
    class HuePlugin
    {
        private Settings settings;
        private LocalHueClient client;

        public HuePlugin()
        {
            settings = Settings.Default;
            Task t = GetClient();
            t.Wait();
        }


        async Task GetClient()
        {
            Console.WriteLine("starting GetClient");
            string ip = await GetOrFindIP();

            if (String.IsNullOrEmpty(ip))
            {
                return;
            }

            string key = await GetOrRegisterKey();

            client = new LocalHueClient(ip);
            client.Initialize(key);

            if (!client.IsInitialized)
            {
                Console.WriteLine("Could not initialize client");
            }

        }

        async Task<string> GetOrFindIP()
        {
            Console.WriteLine("starting GetOrFindIP");
            string ip = settings.LastIPAddress;

            Console.WriteLine("ip: " + ip);
            if (String.IsNullOrEmpty(ip))
            {
                //IBridgeLocator locator = new HttpBridgeLocator();
                SSDPBridgeLocator locator = new SSDPBridgeLocator();
                Console.WriteLine("Searching for bridges");
                IEnumerable<Q42.HueApi.Models.Bridge.LocatedBridge> bridgeIPs = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(20));
                Console.WriteLine("Searching done");
                ////For Windows 8 and .NET45 projects you can use the SSDPBridgeLocator which actually scans your network. 
                ////See the included BridgeDiscoveryTests and the specific .NET and .WinRT projects

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
                    ListDialogBox dialog = new ListDialogBox();
                    ObservableCollection<BridgeItem> bridges = new ObservableCollection<BridgeItem>();
                    foreach (var b in bridgeIPs)
                    {
                        bridges.Add(new BridgeItem(b));
                    }
                    dialog.Items = bridges;
                    Console.WriteLine("showing dialog");
                    dialog.ShowDialog();

                    if (!dialog.IsCancelled)
                    {
                        BridgeItem item = dialog.SelectedItem as BridgeItem;
                        ip = item.Bridge.IpAddress;

                        //Store the new IP address
                        settings.LastIPAddress = ip;
                        settings.Save();
                    }
                }
            }

            return ip;
        }

        async Task<String> GetOrRegisterKey()
        {
            string key = settings.HueKey;

            if (String.IsNullOrEmpty(key))
            {
                key = await client.RegisterAsync("Streaming Muesic", "");

                if (!String.IsNullOrEmpty(key))
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

            return key;
        }
    }
}
