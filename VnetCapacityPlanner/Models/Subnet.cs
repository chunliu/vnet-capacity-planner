﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace VnetCapacityPlanner.Models
{
    public class Subnet : IValidatableObject
    {
        private readonly int ReservedIps = 5;

        [Required]
        public string Name { get; set; }
        [Required]
        public string StartIP { get; set; }
        [Required]
        public string ServiceName { get; set; }
        [Required]
        public int ServiceInstances { get; set; }
        public int IpPerInstance { get; set; }
        public int AdditionalIps { get; set; }
        public int IPRangeId { get; set; }
        [DisplayName("Address Range")]
        public string AddressRange 
        { 
            get 
            {
                var network = Network;
                return network == null ? string.Empty : $"{network.Network} - {network.Broadcast}"; 
            }
        }
        [DisplayName("Address Count")]
        public int AddressCount => (int)(Network?.Total ?? 0);
        [DisplayName("Available Count")]
        public int AvailableCount => Service?.FixedSubnetCidr ?? false ? 0 : AddressCount - ((ServiceInstances * IpPerInstance) + AdditionalIps + ReservedIps);
        public int UsedCount => Service?.FixedSubnetCidr ?? false ? AddressCount : (ServiceInstances * IpPerInstance) + AdditionalIps + ReservedIps;
        [DisplayName("Address Space")]
        public string AddressSpace => Network?.ToString() ?? string.Empty;
        public VirtualNetwork VirtualNetwork { get; set; }
        public ServiceSpec Service { get; set; }
        public IPNetwork Network => PopulateNetwork();
        public IPNetwork PopulateNetwork()
        {
            if (Service == null)
                return null;

            int cidr;
            if (Service.FixedSubnetCidr)
            {
                cidr = Service.SubnetCidr;
            }
            else
            {
                var ipCount = ServiceInstances * IpPerInstance + AdditionalIps + ReservedIps;
                cidr = GuessCidr(ipCount, 29);
            }
            
            return IPNetwork.Parse(StartIP, Convert.ToByte(cidr));
        }

        private static int GuessCidr(int addressCount, int start = 29, int end = 8)
        {
            for(int i = start; i >= end; i--)
            {
                if (addressCount <= Math.Pow(2, (32 - i)))
                    return i;
            }

            return 0;
        }

        public Subnet Clone()
        {
            Subnet subnet = (Subnet)this.MemberwiseClone();
            subnet.Service = this.Service.Clone();

            return subnet;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            // Subnet name must be unique.
            if (VirtualNetwork.Subnets.Where(s => s.Name.Equals(Name)).FirstOrDefault() != null)
            {
                results.Add(new ValidationResult("Subnet name must be unique within a virtual network."));
            }
            // IpPerInstance must be > 0 for non fixed cidr.
            if (!Service.FixedSubnetCidr && IpPerInstance <= 0)
            {
                results.Add(new ValidationResult("Address/Instance must be greater than 0."));
            }
            // Number of instances need to be between min and max.
            if (!Service.FixedSubnetCidr && Service.MaxInstances > Service.MinInstances
                && (ServiceInstances > Service.MaxInstances || ServiceInstances < Service.MinInstances))
            {
                results.Add(new ValidationResult($"The instance must be between {Service.MinInstances} and {Service.MaxInstances}."));
            }

            var ipAddress = IPAddress.Parse(StartIP);
            IPNetwork network1 = IPNetwork.Parse("10.0.0.0/8");
            IPNetwork network2 = IPNetwork.Parse("172.16.0.0/12");
            IPNetwork network3 = IPNetwork.Parse("192.168.0.0/16");
            if (!network1.Contains(ipAddress) && !network2.Contains(ipAddress) && !network3.Contains(ipAddress))
            {
                results.Add(new ValidationResult($"The start ip is not in the range of RFC1918."));
                return results;
            }

            var network = Network;
            if (IPRangeId < 0)
            {
                results.Add(new ValidationResult($"The subnet address range {network} is not contained in the virtual network's address spaces."));
                return results;
            }
            // Validate CIDR block.
            if (!Equals(network.Network, ipAddress))
            {
                var current = IPNetwork.ToBigInteger(network.Network);
                var next = current + network.Total;
                var nextIp = IPNetwork.ToIPAddress(next, AddressFamily.InterNetwork);
                results.Add(new ValidationResult($"{StartIP}/{network.Cidr} is not a valid CIDR block. Try {network.Network} or {nextIp} instead."));
                return results;
            }

            // Overlap with other subnets
            foreach (var subnet in VirtualNetwork.Subnets)
            {
                if(network.Overlap(subnet.Network))
                {
                    results.Add(new ValidationResult($"The subnet address range {network} overlaps with {subnet.Name} address range {subnet.Network}."));
                    break;
                }
            }

            return results;
        }
    }
}
