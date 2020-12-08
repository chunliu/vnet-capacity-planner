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
        public IPRange[] IPRanges { get; set; }
        public List<Subnet> Subnets { get; set; }

        public List<ServiceSpec> ServiceSpecs { get; set; }

        public VirtualNetwork(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task Initialize()
        {
            IPRanges = new IPRange[]
            {
                new()
                {
                    StartIP = "10.0.0.0",
                }
            };

            Subnets = new List<Subnet>();

            var services = await _httpClient.GetFromJsonAsync<ServiceSpec[]>("data/services.json");
            ServiceSpecs = services.ToList();
        }

        public IPNetwork IPNetwork => IPRanges[0].IPNetwork;

        public event Action OnVnetStartIpChange;
        private void NotifyVnetStartIpChange() => OnVnetStartIpChange?.Invoke();

        public event Action OnSubnetChange;
        private void NotifySubnetChange() => OnSubnetChange?.Invoke();

        public string GetVnetStartIp(int index = 0) => IPRanges[index].StartIP;

        public void SetVnetStartIp(int index, string ip)
        {
            if (IPRanges[index].StartIP != ip)
            {
                IPRanges[index].StartIP = ip;
                NotifyVnetStartIpChange();
            }
        }

        public void AddSubnet(Subnet subnet)
        {
            var network = Subnets.Where(s => s.Name.Equals(subnet.Name)).FirstOrDefault();
            if (network == null)
                Subnets.Add(subnet);
            else
                network = subnet;

            IPRanges[0].WideSubnet(Subnets);

            NotifySubnetChange();
        }

        public void DeleteSubnet(string subnetName)
        {
            var network = Subnets.Where(s => s.Name.Equals(subnetName)).FirstOrDefault();
            if (network != null)
            {
                Subnets.Remove(network);
                IPRanges[0].WideSubnet(Subnets);

                NotifySubnetChange();
            }
        }

        public void ResetSubnets()
        {
            Subnets.Clear();
            IPRanges[0].WideSubnet(Subnets);

            NotifySubnetChange();
        }
    }
}
