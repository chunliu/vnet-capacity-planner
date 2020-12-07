using System;
using System.Collections.Generic;
using System.Net;

namespace vnet_capacity_planner.Models
{
    public class IPRange
    {
        private IPNetwork _ipNetwork = null;

        public string StartIP 
        { 
            get 
            { 
                return _ipNetwork?.Network.ToString() ?? string.Empty; 
            }
            set
            {
                _ipNetwork = IPNetwork.Parse(value, Convert.ToByte(Cidr));
            }
        }

        public string AddressSpace 
        { 
            get { return _ipNetwork?.ToString() ?? string.Empty; }
        }

        public string AddressCount
        {
            get { return _ipNetwork?.Total.ToString() ?? string.Empty; }
        }

        public int Cidr
        {
            get { return _ipNetwork?.Cidr ?? 29; }
        }

        public string AddressRange
        {
            get { return _ipNetwork == null ? string.Empty : $"{_ipNetwork.Network} - {_ipNetwork.Broadcast}"; }
        }

        public IPNetwork IPNetwork
        {
            get { return _ipNetwork; }
        }

        public void WideSubnet(List<Subnet> subnets)
        {
            var smallest = IPNetwork.Parse(StartIP, Convert.ToByte(29));

            List<IPNetwork> all = new List<IPNetwork>
            {
                smallest
            };

            foreach (var subnet in subnets)
            {
                all.Add(subnet.Network);
            }

            _ipNetwork = IPNetwork.WideSubnet(all.ToArray());
        }
    }
}
