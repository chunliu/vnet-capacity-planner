using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace vnet_capacity_planner.Models
{
    public class Subnet
    {
        IPNetwork _subnet;

        private readonly int ReservedIps = 5;

        [Required]
        public string Name { get; set; }
        [Required]
        [IPAddress]
        public string StartIP { get; set; }

        [Required]
        public string ServiceName { get; set; }
        [Required]
        public int ServiceInstances { get; set; }
        public int IpPerInstance { get; set; }
        public int AdditionalIps { get; set; }

        [DisplayName("Address Range")]
        public string AddressRange 
        { 
            get { return _subnet == null ? string.Empty : $"{_subnet.Network} - {_subnet.Broadcast}"; }
        }

        [DisplayName("Address Count")]
        public int AddressCount 
        { 
            get { return (int)(_subnet?.Total ?? 0); }
        }

        [DisplayName("Available Count")]
        public int AvailableCount 
        { 
            get { return AddressCount - (ServiceInstances * IpPerInstance + AdditionalIps + ReservedIps); }
        }

        [DisplayName("Address Space")]
        public string AddressSpace
        {
            get { return _subnet?.ToString() ?? string.Empty; }
        }

        public IPNetwork VnetNetwork { get; set; }

        public ServiceSpec Service { get; set; }

        public IPNetwork Network 
        {
            get { return _subnet; } 
        }

        public void PopulateNetwork()
        {
            int cidr = 0;
            if(Service.FixedSubnetCidr)
            {
                cidr = Service.SubnetCidr;
            }
            else
            {
                var ipCount = ServiceInstances * IpPerInstance + AdditionalIps + ReservedIps;
                cidr = GuessCidr(ipCount, 29);
            }

            _subnet = IPNetwork.Parse(StartIP, Convert.ToByte(cidr));

            Console.WriteLine(_subnet.Network);
            Console.WriteLine(_subnet.Broadcast);
            Console.WriteLine(_subnet.Cidr);
        }

        private int GuessCidr(int addressCount, int start = 32)
        {
            for(int i = start; i >= 0; i--)
            {
                if (addressCount <= Math.Pow(2, (32 - i)))
                    return i;
            }

            return -1;
        }

        public Subnet Clone()
        {
            Subnet subnet = (Subnet)this.MemberwiseClone();
            subnet.Service = this.Service.Clone();

            subnet.PopulateNetwork();

            return subnet;
        }
    }
}
