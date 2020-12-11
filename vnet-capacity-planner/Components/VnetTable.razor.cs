using AntDesign;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using vnet_capacity_planner.Models;

namespace vnet_capacity_planner.Components
{
    public partial class VnetTable
    {
        Table<IPRange> vnetTable;
        [Inject]
        private VirtualNetwork _vnet { get; set; }
        [Inject]
        private ConfirmService _confirmService { get; set; }

        private bool ipRangeOverlap = false;

        protected override void OnInitialized()
        {
            _vnet.OnSubnetChange += SubnetHasChanged;
            _vnet.OnVnetStartIpChange += NetworkIpHasChanged;

            base.OnInitialized();
        }

        public void Dispose()
        {
            _vnet.OnSubnetChange -= SubnetHasChanged;
            _vnet.OnVnetStartIpChange -= NetworkIpHasChanged;
        }

        private void NetworkIpHasChanged()
        {
            vnetTable.ReloadData();
            CheckIpRangeOverlap();
        }

        private void CheckIpRangeOverlap()
        {
            for (int i = 0; i < _vnet.IPRanges.Count; i++)
            {
                for (int j = i + 1; j < _vnet.IPRanges.Count; j++)
                {
                    if (_vnet.IPRanges[j].IPNetwork != null 
                        && _vnet.IPRanges[i].IPNetwork.Overlap(_vnet.IPRanges[j].IPNetwork))
                    {
                        ipRangeOverlap = true;
                        StateHasChanged();
                        return;
                    }
                }
            }
            ipRangeOverlap = false;
            StateHasChanged();
        }

        private void SubnetHasChanged()
        {
            vnetTable.ReloadData();
            CheckIpRangeOverlap();
        }

        private async Task StartIpBlur(IPRange ipRange)
        {
            bool validIp = IPAddress.TryParse(ipRange.StartIpHolder, out IPAddress ipAddress);
            if (!validIp)
            {
                ipRange.IpInvalidMessage = "Network IP address is not valid.";
                ipRange.HolderIpInvalid = !validIp;
                return;
            }

            // RFC1918
            IPNetwork network1 = IPNetwork.Parse("10.0.0.0/8");
            IPNetwork network2 = IPNetwork.Parse("172.16.0.0/12");
            IPNetwork network3 = IPNetwork.Parse("192.168.0.0/16");
            if (!network1.Contains(ipAddress) && !network2.Contains(ipAddress) && !network3.Contains(ipAddress))
            {
                ipRange.IpInvalidMessage = $"Network IP address is not recommended. " +
                        $"<a href=\"https://docs.microsoft.com/en-us/azure/virtual-network/virtual-networks-faq#what-address-ranges-can-i-use-in-my-vnets\" target=\"_blank\">More info</a>";
                ipRange.HolderIpInvalid = true;
                return;
            }

            ipRange.HolderIpInvalid = false;
            ipRange.IpInvalidMessage = string.Empty;
            if (_vnet.Subnets.Count > 0 && ipRange.StartIpHolder != ipRange.StartIP)
            {
                // Too complex to sort out new addresses for all subnets. So reset them. 
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
                    _vnet.SetVnetStartIp(ipRange.Id, ipRange.StartIpHolder);
                    _vnet.ResetSubnets(ipRange.Id);
                }
            }
            else
                _vnet.SetVnetStartIp(ipRange.Id, ipRange.StartIpHolder);

            ipRange.StartIpHolder = ipRange.StartIP;
        }

        private int GetAvailableCount(IPRange ipRange)
        {
            if (ipRange == null || string.IsNullOrEmpty(ipRange.AddressCount))
                return 0;

            int total = int.Parse(ipRange.AddressCount);
            var subnets = _vnet.Subnets.Where(s => s.IPRangeId == ipRange.Id).ToList();
            foreach (var subnet in subnets)
            {
                total -= subnet.UsedCount;
            }

            return total;
        }
    }
}
