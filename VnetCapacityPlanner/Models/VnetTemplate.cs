using System.Collections.Generic;

namespace VnetCapacityPlanner.Models
{
    public class VnetTemplate
    {
        private List<string> vnetAddressPrefixes = new List<string>();
        private List<Dictionary<string, dynamic>> subnets = new List<Dictionary<string, dynamic>>();

        private Dictionary<string, dynamic> template = new Dictionary<string, dynamic>();

        public VnetTemplate()
        {
            template.Add("$schema", "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#");
            template.Add("contentVersion", "1.0.0.0");

            var param1 = new Dictionary<string, string>
            {
                { "type", "String" },
                { "defaultValue", "vnetplanner-vnet" }
            };
            var param2 = new Dictionary<string, string>
            {
                { "type", "String" },
                { "defaultValue", "eastus" }
            };
            template.Add("parameters", new Dictionary<string, dynamic>
            {
                { "vnetPlanner_vnetName", param1 },
                { "vnetPlanner_location", param2 }
            });

            template.Add("variables", new Dictionary<string, dynamic>());

            Dictionary<string, List<string>> addressSpace = new Dictionary<string, List<string>>
            {
                { "addressPrefixes", vnetAddressPrefixes }
            };
            Dictionary<string, dynamic> properties = new Dictionary<string, dynamic>
            {
                { "addressSpace", addressSpace },
                { "subnets", subnets }
            };

            var vnetResource = new Dictionary<string, dynamic>
            {
                { "type", "Microsoft.Network/virtualNetworks" },
                { "apiVersion", "2020-07-01" },
                { "name", "[parameters('vnetPlanner_vnetName')]" },
                { "location", "[parameters('vnetPlanner_location')]" },
                { "properties", properties }
            };
            var resources = new List<Dictionary<string, dynamic>>
            {
                vnetResource
            };
            template.Add("resources", resources);
        }

        public Dictionary<string, dynamic> ArmTemplate
        {
            get => template;
        }

        public void AddAddressPrefix(string addressPrefix)
        {
            vnetAddressPrefixes.Add(addressPrefix);
        }

        public void AddSubnet(string name, string addressPrefix)
        {
            var properties = new Dictionary<string, dynamic>
            {
                { "addressPrefix", addressPrefix },
                { "delegations", new List<string>() },
                { "privateEndpointNetworkPolicies", "Enabled" },
                { "privateLinkServiceNetworkPolicies", "Enabled" }
            };
            var subnet = new Dictionary<string, dynamic>
            {
                { "name", name },
                { "properties", properties }
            };

            subnets.Add(subnet);
        }
    }
}
