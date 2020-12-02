using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vnet_capacity_planner.Models
{
    public class VirtualNetwork
    {
        public IPRange[] IPRanges { get; set; }
        public Subnet[] Subnets { get; set; }

        public List<ServiceSpec> ServiceSpecs { get; set; }

        public VirtualNetwork()
        {
            IPRanges = new IPRange[]
            {
                new()
                {
                    StartIP = "10.0.0.0",
                }
            };

            Subnets = Array.Empty<Subnet>();


            ServiceSpecs = new List<ServiceSpec>
            {
                new()
                {
                    Name = "Azure Firewall",
                    FixedSubnetName = true,
                    SubnetName = "AzureFirewallSubnet",
                    FixedSubnetCidr = true,
                    SubnetCidr = 26,
                    MinInstances = 0,
                    MaxInstances = 0,
                    IpPerInstance = 0,
                    AdditionalIps = 0
                },
                new()
                {
                    Name = "API Management",
                    FixedSubnetName = false,
                    SubnetName = "",
                    FixedSubnetCidr = false,
                    SubnetCidr = 0,
                    MinInstances = 1,
                    MaxInstances = 10,
                    IpPerInstance = 2,
                    AdditionalIps = 1
                },
                new()
                {
                    Name = "Other",
                    FixedSubnetName = false,
                    SubnetName = "",
                    FixedSubnetCidr = false,
                    SubnetCidr = 0,
                    MinInstances = 0,
                    MaxInstances = 0,
                    IpPerInstance = 0,
                    AdditionalIps = 0
                }
            };
        }

        public event Action OnVnetStartIpChange;
        private void NotifyVnetStartIpChange() => OnVnetStartIpChange?.Invoke();
        
        public void SetVnetStartIp(int index, string ip)
        {
            if (IPRanges[index].StartIP != ip)
            {
                IPRanges[index].StartIP = ip;
                NotifyVnetStartIpChange();
            }
        }
    }
}
