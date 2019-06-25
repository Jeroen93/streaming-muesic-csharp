﻿using Q42.HueApi.Models.Bridge;

namespace Streaming_Muesic_WPF
{
    public class BridgeItem
    {
        private readonly string displayName;
        private readonly LocatedBridge bridge;

        public BridgeItem(LocatedBridge bridge)
        {
            this.bridge = bridge;
            displayName = bridge.IpAddress;
        }

        public string DisplayName
        {   //Implement the property.  The implementer doesn't need
            //to provide an implementation for setting the property.
            get { return displayName; }
            set { }
        }

        public LocatedBridge Bridge
        {   //Implement the property.  The implementer doesn't need
            //to provide an implementation for setting the property.
            get { return bridge; }
            set { }
        }
    }
}
