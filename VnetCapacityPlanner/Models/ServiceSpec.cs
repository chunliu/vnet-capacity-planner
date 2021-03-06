﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace VnetCapacityPlanner.Models
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
        public string RefUrl { get; set; }

        public ServiceSpec Clone()
        {
            return this.MemberwiseClone() as ServiceSpec;
        }
    }
}
