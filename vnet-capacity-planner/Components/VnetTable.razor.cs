using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using vnet_capacity_planner.Models;

namespace vnet_capacity_planner.Components
{
    public partial class VnetTable
    {
        private string startIp = string.Empty;
        private bool showError = false;
        private string errorMessage = string.Empty;
        Table<IPRange> vnetTable;
        [Inject]
        private VirtualNetwork _vnet { get; set; }
        [Inject]
        private ConfirmService _confirmService { get; set; }


        protected override void OnInitialized()
        {
            startIp = _vnet.IPRanges[0].StartIP;

            _vnet.OnSubnetChange += SubnetHasChanged;
            _vnet.OnVnetStartIpChange += NetworkIpHasChanged;

            base.OnInitialized();
        }

        public void Dispose()
        {
            _vnet.OnSubnetChange -= SubnetHasChanged;
            _vnet.OnVnetStartIpChange -= NetworkIpHasChanged;
        }

        private void NetworkIpHasChanged() => vnetTable.ReloadData();

        private void SubnetHasChanged()
        {
            vnetTable.ReloadData();
            //if (Convert.ToInt32(_vnet.IPNetwork.Cidr) < 8)
            //{
            //    errorMessage = $"The virtual network is too large.";
            //    showError = true;
            //}
        }

        private async Task StartIpBlur(FocusEventArgs e)
        {
            bool validIp = IPAddress.TryParse(startIp, out IPAddress ipAddress);
            if (!validIp)
            {
                errorMessage = "Network IP address is not valid.";
                showError = !validIp;
                return;
            }

            // RFC1918
            IPNetwork network1 = IPNetwork.Parse("10.0.0.0/8");
            IPNetwork network2 = IPNetwork.Parse("172.16.0.0/12");
            IPNetwork network3 = IPNetwork.Parse("192.168.0.0/16");
            if (!network1.Contains(ipAddress) && !network2.Contains(ipAddress) && !network3.Contains(ipAddress))
            {
                errorMessage = $"Network IP address is not recommended. " +
                        $"<a href=\"https://docs.microsoft.com/en-us/azure/virtual-network/virtual-networks-faq#what-address-ranges-can-i-use-in-my-vnets\" target=\"_blank\">More info</a>";
                showError = true;
                return;
            }

            showError = false;
            errorMessage = string.Empty;

            if (_vnet.Subnets.Count > 0 && startIp != _vnet.GetVnetStartIp())
            {
                var content = "Chaning network IP will reset all its subnets!";
                var title = "Warning";
                var confirmResult = await _confirmService.Show(content, title, ConfirmButtons.OKCancel, ConfirmIcon.Warning,
                    new ConfirmButtonOptions()
                    {
                        Button1Props = new ButtonProps() 
                        {
                            Type = "primary",
                            Danger = true
                        }
                    });
                if (confirmResult == ConfirmResult.OK)
                {
                    _vnet.SetVnetStartIp(0, startIp);
                    _vnet.ResetSubnets();
                }
            }
            else
                _vnet.SetVnetStartIp(0, startIp);

            startIp = _vnet.GetVnetStartIp();
        }

        private int GetAvailableCount()
        {
            int total = int.Parse(_vnet.IPRanges[0].AddressCount);

            foreach (var subnet in _vnet.Subnets)
            {
                total -= subnet.UsedCount;
            }

            return total;
        }
    }
}
