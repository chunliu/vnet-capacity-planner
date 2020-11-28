using System.ComponentModel;

namespace vnet_capacity_planner.Models
{
    public class VnetSpec
    {
        [DisplayName("Key")]
        public string Key { get; set; }

        [DisplayName("Start IP")]
        public string StartIP { get; set; }

        public string AddressSpace
        {
            get { return StartIP + "/" + Cidr.ToString(); }
        }

        [DisplayName("Address Count")]
        public int AddressCount { get; set; }

        public int Cidr { get; set; }
    }

    public class SubnetSpec
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public bool FixedName { get; set; }
        public string StartIP { get; set; }
        public int Cidr { get; set; }
        public bool FixedCidr { get; set; }

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
