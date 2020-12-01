using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace vnet_capacity_planner.Models
{
    public class VnetSpec
    {
        private IPNetwork _ipNetwork = null;
        private int _cidr = 0;
        public string Key { get; set; }

        public string StartIP { get; set; }

        public string AddressSpace
        {
            get { return StartIP + "/" + Cidr.ToString(); }
        }

        public string AddressCount 
        { 
            get
            {
                if (string.IsNullOrEmpty(StartIP))
                    return "0";

                if (_ipNetwork == null)
                    _ipNetwork = IPNetwork.Parse(AddressSpace);

                return _ipNetwork.Total.ToString();
            }
        }

        public int Cidr 
        { 
            get
            {
                return _cidr;
            }
            set
            {
                if(_cidr != value)
                {
                    _cidr = value;
                    if (!string.IsNullOrEmpty(StartIP))
                        _ipNetwork = IPNetwork.Parse(AddressSpace);
                }
            }
        }

        public string AddressRange
        {
            get
            {
                if (string.IsNullOrEmpty(StartIP))
                    return "";

                if (_ipNetwork == null)
                    _ipNetwork = IPNetwork.Parse(AddressSpace);

                return $"{_ipNetwork.Network} - {_ipNetwork.Broadcast}";
            }
        }
    }

    public class SubnetSpec
    {
        public string Key { get; set; }
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
