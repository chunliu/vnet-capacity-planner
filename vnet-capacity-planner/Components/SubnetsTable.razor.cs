using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using vnet_capacity_planner.Models;

namespace vnet_capacity_planner.Components
{
    public partial class SubnetsTable
    {
        bool modalVisible = false;
        Form<Subnet> subnetForm;
        Subnet subnet = null;
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

        private void SubnetHasChanged() => StateHasChanged();

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

            // RFC1918
            IPNetwork network1 = IPNetwork.Parse("10.0.0.0/8");
            IPNetwork network2 = IPNetwork.Parse("172.16.0.0/12");
            IPNetwork network3 = IPNetwork.Parse("192.168.0.0/16");
            if (!network1.Contains(ipAddress) && !network2.Contains(ipAddress) && !network3.Contains(ipAddress))
            {
                startIpError = true;
                errorMessage = "The start ip is not in the range of RFC1918.";
                return;
            }

            subnet.IPRangeId = GetIpRangeId(IPNetwork.Parse($"{subnet.StartIP}/32"));
            if (subnet.IPRangeId < 0)
            {
                startIpError = true;
                errorMessage = "The start ip is not contained in the virtual network's address spaces.";
                return;
            }

            startIpError = false;
            errorMessage = string.Empty;
        }

        private int GetIpRangeId(IPNetwork network)
        {
            // The subnet should belong to the ip range with which it has the smallest gap.
            int ipRangeId = -1;
            BigInteger smallestGap = new BigInteger(16777216); // The largest number of addresses (10.0.0.0/8).
            BigInteger networkIP = IPNetwork.ToBigInteger(network.Network);
            foreach (var ipRange in _vnet.IPRanges)
            {
                if (ipRange.IPNetwork != null)
                {
                    var irIP = IPNetwork.ToBigInteger(ipRange.IPNetwork.Network);
                    var gap = BigInteger.Abs(networkIP - irIP);
                    if (smallestGap > gap)
                    {
                        smallestGap = gap;
                        ipRangeId = ipRange.Id;
                        if (smallestGap == 0)
                        {
                            break;
                        }
                    }
                }
            }

            return ipRangeId;
        }

        private void AddSubnetClick()
        {
            startIpError = false;
            errorMessage = string.Empty;
            subnet = new Subnet
            {
                ServiceName = string.Empty,
                VirtualNetwork = _vnet
            };

            modalVisible = true;
        }

        private async Task HandleOk(MouseEventArgs e)
        {
            if (!subnetForm.Validate())
                return;

            _vnet.AddSubnet(subnet.Clone());

            await CloseModal();
        }

        private async Task HandleCancel(MouseEventArgs e) => await CloseModal();

        private async Task CloseModal()
        {
            modalVisible = false;
            await Task.Delay(50); // Handle scrolling issue.
            subnet = null;
        }
    }
}
