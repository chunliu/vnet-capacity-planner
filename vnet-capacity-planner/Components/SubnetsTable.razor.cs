using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Net;
using vnet_capacity_planner.Models;

namespace vnet_capacity_planner.Components
{
    public partial class SubnetsTable
    {
        bool modalVisible = false;
        Form<Subnet> subnetForm;
        Subnet subnet = null;
        Table<Subnet> subnetTable;
        bool startIpError = false;
        string errorMessage = string.Empty;

        [Inject]
        private VirtualNetwork _vnet { get; set; }

        protected override void OnInitialized()
        {
            _vnet.OnSubnetChange += SubnetHasChanged;

            base.OnInitialized();
        }

        public void Dispose() => _vnet.OnSubnetChange -= SubnetHasChanged;

        private void SubnetHasChanged() => subnetTable.ReloadData();

        private void HandleOnSelectedItemChanged(ServiceSpec selectedService)
        {
            subnet.Service = selectedService;
            subnet.ServiceName = selectedService.Name;
            subnet.Name = selectedService.SubnetName;
            subnet.IpPerInstance = selectedService.IpPerInstance;
            subnet.AdditionalIps = selectedService.AdditionalIps;
        }

        private void StartIpBlur(FocusEventArgs e)
        {
            if (!IPAddress.TryParse(subnet.StartIP, out IPAddress ipAddress))
            {
                startIpError = true;
                errorMessage = "The start ip is not valid.";
                return;
            }

            var id = GetIpRangeId(IPNetwork.Parse($"{subnet.StartIP}/31"));
            if (id < 0)
            {
                startIpError = true;
                errorMessage = "The start ip is not contained in the virtual network's address spaces.";
                return;
            }

            startIpError = false;
            errorMessage = string.Empty;
            subnet.IPRangeId = id;
        }

        private int GetIpRangeId(IPNetwork network)
        {
            Console.WriteLine(network);
            for (int i = 0; i < _vnet.IPRanges.Length; i++)
            {
                bool wideResult = IPNetwork.TryWideSubnet(
                    new IPNetwork[]
                    {
                    _vnet.IPRanges[i].IPNetwork,
                    network
                    }, out IPNetwork widedNetwork);

                Console.WriteLine(widedNetwork);

                if (wideResult && Equals(_vnet.IPRanges[i].IPNetwork.Network, widedNetwork.Network))
                {
                    return i;
                }
            }

            return -1;
        }

        private void AddSubnetClick()
        {
            subnet = new Subnet
            {
                ServiceName = string.Empty,
                VirtualNetwork = _vnet
            };

            modalVisible = true;
        }

        private void HandleOk(MouseEventArgs e)
        {
            if (!subnetForm.Validate())
                return;

            _vnet.AddSubnet(subnet.Clone());

            modalVisible = false;
            subnet = null;
        }

        private void HandleCancel(MouseEventArgs e)
        {
            modalVisible = false;
            subnet = null;
        }

        private void DeleteSubnet(string subnetName) => _vnet.DeleteSubnet(subnetName);
    }
}
