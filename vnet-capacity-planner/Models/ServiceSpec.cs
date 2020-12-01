using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace vnet_capacity_planner.Models
{
    public class ServiceSpec
    {
        public string Name { get; set; }
        public bool FixedSubnetName { get; set; }
        public string SubnetName { get; set; }
        public bool FixedSubnetCidr { get; set; }
        public int SubnetCidr { get; set; }
        public int MinInstances { get; set; }
        public int MaxInstances { get; set; }
        public int IpPerInstance { get; set; }
        public int AdditionalIps { get; set; }
    }
}
