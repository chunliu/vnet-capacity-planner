using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace vnet_capacity_planner.Models
{
    public class VirtualNetwork
    {
        private readonly HttpClient _httpClient;
        public List<IPRange> IPRanges { get; set; }
        public List<Subnet> Subnets { get; set; }
        public List<ServiceSpec> ServiceSpecs { get; set; }
        public VirtualNetwork(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task Initialize()
        {
            IPRanges = new List<IPRange>()
            {
                new IPRange()
                {
                    Id = 0,
                    StartIP = "10.0.0.0",
                    StartIpHolder = "10.0.0.0",
                }
            };

            Subnets = new List<Subnet>();

            var services = await _httpClient.GetFromJsonAsync<ServiceSpec[]>("data/services.json");
            ServiceSpecs = services.ToList();
        }

        public event Action OnVnetStartIpChange;
        private void NotifyVnetStartIpChange() => OnVnetStartIpChange?.Invoke();

        public event Action OnSubnetChange;
        private void NotifySubnetChange() => OnSubnetChange?.Invoke();

        public void AddIpRange()
        {
            var ipRange = new IPRange()
            {
                Id = IPRanges.Count
            };
            IPRanges.Add(ipRange);
        }

        public void SetVnetStartIp(int irId, string ip)
        {
            if (string.IsNullOrEmpty(ip))
                return;

            var ipRange = IPRanges.Where(ir => ir.Id == irId).FirstOrDefault();
            if ((ipRange?.StartIP ?? null) != ip)
            {
                ipRange.StartIP = ip;
                NotifyVnetStartIpChange();
            }
        }

        private void WideIpRange(int ipRangeId, bool notify = true)
        {
            var ipRange = IPRanges.Where(ir => ir.Id == ipRangeId).FirstOrDefault();
            var subnets = Subnets.Where(s => s.IPRangeId == ipRangeId).ToList();
            if (ipRange != null && subnets?.Count > 0)
            {
                ipRange.WideSubnet(subnets);
                if (notify)
                    NotifySubnetChange();
            }
        }

        public void AddSubnet(Subnet subnet)
        {
            Subnets.Add(subnet);
            WideIpRange(subnet.IPRangeId);
        }

        public void DeleteSubnet(string subnetName)
        {
            var subnet = Subnets.Where(s => s.Name.Equals(subnetName)).FirstOrDefault();
            if (subnet != null)
            {
                Subnets.Remove(subnet);
                WideIpRange(subnet.IPRangeId);
            }
        }

        public void ResetSubnets(int ipRangeId)
        {
            Subnets.RemoveAll(s => s.IPRangeId == ipRangeId);
            WideIpRange(ipRangeId, false);

            NotifySubnetChange();
        }
    }
}
