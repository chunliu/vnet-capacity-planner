using System;
using System.Collections.Generic;
using System.Net;

namespace VnetCapacityPlanner.Models
{
    public class IPRange
    {
        private IPNetwork _ipNetwork = null;
        public int Id { get; set; }
        public string StartIP
        {
            get => _ipNetwork?.Network.ToString() ?? string.Empty;
            set => _ipNetwork = IPNetwork.Parse(value, Convert.ToByte(Cidr));
        }
        public string StartIpHolder { get; set; }
        public bool HolderIpInvalid { get; set; }
        public string IpInvalidMessage { get; set; }

        public string AddressSpace => _ipNetwork?.ToString() ?? string.Empty;

        public string AddressCount => _ipNetwork?.Total.ToString() ?? string.Empty;

        public int Cidr => _ipNetwork?.Cidr ?? 29;

        public string AddressRange => _ipNetwork == null ? string.Empty : $"{_ipNetwork.Network} - {_ipNetwork.Broadcast}";

        public IPNetwork IPNetwork => _ipNetwork;

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
