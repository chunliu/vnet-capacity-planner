using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace vnet_capacity_planner.Models
{
    public class Subnet
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [IPAddress]
        public string StartIP { get; set; }
        public int Cidr { get; set; }
        [Required]
        public string ServiceName { get; set; }

        public int ServiceInstances { get; set; }
        public int IpPerInstance { get; set; }
        public int AdditionalIps { get; set; }

        [DisplayName("Address Range")]
        public string AddressRange { get; set; }

        [DisplayName("Address Count")]
        public int AddressCount { get; set; }

        [DisplayName("Available Count")]
        public int AvailableCount { get; set; }

        [DisplayName("Address Space")]
        public string AddressSpace
        {
            get { return StartIP + "/" + Cidr.ToString(); }
        }
    }
}
