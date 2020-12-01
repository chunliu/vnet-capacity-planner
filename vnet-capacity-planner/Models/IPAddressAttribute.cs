using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace vnet_capacity_planner.Models
{
    public class IPAddressAttribute : ValidationAttribute
    {
        public IPAddressAttribute()
        {

        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var ip = (Subnet)validationContext.ObjectInstance;

            if (!IPAddress.TryParse(ip.StartIP, out IPAddress ipAddress))
            {
                return new ValidationResult($"IP address is not valid.");
            }

            return ValidationResult.Success;
        }
    }
}
