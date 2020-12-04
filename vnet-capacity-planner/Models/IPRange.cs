using System;
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

        public void WideSubnet(IPNetwork subnet)
        {
            _ipNetwork = IPNetwork.WideSubnet(new IPNetwork[]
            {
                _ipNetwork,
                subnet
            });
        }
    }
}
