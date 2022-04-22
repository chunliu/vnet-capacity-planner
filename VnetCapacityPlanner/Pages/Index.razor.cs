using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;
using BlazorDownloadFile;
using Microsoft.AspNetCore.Components;
using VnetCapacityPlanner.Models;
using AntDesign;
using System.Linq;

namespace VnetCapacityPlanner.Pages
{
    public partial class Index
    {
        [Inject]
        VirtualNetwork _vnet { get; set; }
        [Inject]
        MessageService _message { get; set; }
        [Inject]
        ILogger<Index> _logger { get; set; }
        [Inject]
        IBlazorDownloadFileService _dldFile { get; set; }

        private async Task ClickExportArm(MouseEventArgs e)
        {
            VnetTemplate template = new VnetTemplate();
            foreach(var ir in _vnet.IPRanges)
            {
                template.AddAddressPrefix(ir.AddressSpace);
            }
            foreach(var subnet in _vnet.Subnets)
            {
                template.AddSubnet(subnet.Name, subnet.AddressSpace);
            }

            var armString = template.GenerateArmTemplate();
            await GenerateDownload("vnet-planner.json", armString);
        }

        private async Task ClickExportCSV(MouseEventArgs e)
        {
            _logger.LogInformation("Export CSV");
            // Create CSV
            StringBuilder csvBuilder = new("Type,Name,Resource,Address Space,Address Range,Address Count");
            csvBuilder.AppendLine();
            foreach (var ipRange in _vnet.IPRanges)
            {
                csvBuilder.Append($"Virtual Network,,,{ipRange.AddressSpace},{ipRange.AddressRange},{ipRange.AddressCount}");
                csvBuilder.AppendLine();
                var subnets = _vnet.Subnets.Where(s => s.IPRangeId == ipRange.Id).ToList();
                foreach(var subnet in subnets)
                {
                    csvBuilder.Append($"Subnet,{subnet.Name},{subnet.ServiceName},{subnet.AddressSpace},{subnet.AddressRange},{subnet.AddressCount}");
                    csvBuilder.AppendLine();
                }
            }

            await GenerateDownload("vnet-planner.csv", csvBuilder.ToString());
        }

        private async Task GenerateDownload(string filename, string content)
        {
            // Download the file
            var dlBytes = Encoding.UTF8.GetBytes(content);
            var dlB64 = Convert.ToBase64String(dlBytes);

            var result = await _dldFile.DownloadFile(filename, dlB64, "application/octet-stream");
            if (!result.Succeeded)
            {
                _logger.LogError("Download file error: {ErrorName}, {ErrorMessage}", result.ErrorName, result.ErrorMessage);
                Console.WriteLine($"{result.ErrorName}, {result.ErrorMessage}");
            }
        }
    }
}
