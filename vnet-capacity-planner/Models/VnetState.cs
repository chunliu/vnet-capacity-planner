using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vnet_capacity_planner.Models
{
    public class VnetState
    {
        public VnetSpec[] VnetSpecs { get; set; }
        public SubnetSpec[] SubnetSpecs { get; set; }

        public VnetState()
        {
            VnetSpecs = new VnetSpec[]
            {
                    new()
                    {
                        Key = "1",
                        StartIP = "10.0.0.0",
                        AddressCount = 8,
                        Cidr = 29
                    }
            };

            SubnetSpecs = Array.Empty<SubnetSpec>();
        }

        public event Action OnVnetStartIpChange;
        private void NotifyVnetStartIpChange() => OnVnetStartIpChange?.Invoke();
        
        public void SetVnetStartIp(int index, string ip)
        {
            VnetSpecs[index].StartIP = ip;
            NotifyVnetStartIpChange();
        }
    }
}
