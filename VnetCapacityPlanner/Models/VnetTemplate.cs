using AzureDesignStudio.AzureResources.Base;
using AzNetwork = AzureDesignStudio.AzureResources.Network;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using VnetCapacityPlanner.Utils;

namespace VnetCapacityPlanner.Models
{
    public class VnetTemplate
    {
        private readonly DeploymentTemplate deploymentTemplate = new()
        {
            Schema = "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
            ContentVersion = "1.0.0.0",
            Parameters = new Dictionary<string, Parameter>
            {
                { "vnetPlanner_vnetName", new Parameter { Type = "string", DefaultValue = "vnetplanner-vnet" } },
                { "vnetPlanner_location", new Parameter { Type = "string", DefaultValue = "eastus" } }
            },
            Resources = new List<ResourceBase>()
        };

        private readonly AzNetwork.VirtualNetworks virtualNetworks = new()
        {
            Name = "[parameters('vnetPlanner_vnetName')]",
            Location = "[parameters('vnetPlanner_location')]",
            Properties = new AzNetwork.VirtualNetworkPropertiesFormat()
        };

        public VnetTemplate()
        {
            deploymentTemplate.Resources.Add(virtualNetworks);
        }

        public void AddAddressPrefix(string addressPrefix)
        {
            if (virtualNetworks.Properties.AddressSpace == null)
            {
                virtualNetworks.Properties.AddressSpace = new AzNetwork.AddressSpace
                {
                    AddressPrefixes = new List<string>
                    {
                        addressPrefix
                    }
                };
            }
            else
            {
                virtualNetworks.Properties.AddressSpace.AddressPrefixes.Add(addressPrefix);
            }
        }

        public void AddSubnet(string name, string addressPrefix)
        {
            AzNetwork.Subnet subnet = new()
            {
                Name = name,
                Properties = new()
                {
                    AddressPrefix = addressPrefix,
                    PrivateEndpointNetworkPolicies = "Enabled",
                    PrivateLinkServiceNetworkPolicies = "Enabled"
                },
            };

            if (virtualNetworks.Properties.Subnets == null)
            {
                virtualNetworks.Properties.Subnets = new List<AzNetwork.Subnet>
                {
                    subnet
                };
            }
            else
            {
                virtualNetworks.Properties.Subnets.Add(subnet);
            }
        }

        public string GenerateArmTemplate()
        {
            return JsonSerializer.Serialize(deploymentTemplate,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                    Converters = { new ResourceBaseJsonConverter() }
                });
        }
    }
}
