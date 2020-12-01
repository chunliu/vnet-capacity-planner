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
    }
}
