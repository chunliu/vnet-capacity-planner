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

            SubnetSpecs = new SubnetSpec[]
            {
                new()
                {
                    Key = "1",
                    Name = "Subnet1",
                    FixedName = false,
                    StartIP = "10.0.0.0",
                    Cidr = 29,
                    FixedCidr = false,
                    AddressRange = "10.0.0.0 - 10.0.0.7",
                    AddressCount = 8,
                    AvailableCount = 3
                }
            };
        }
    }
}
