using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;
using BlazorDownloadFile;
using Microsoft.AspNetCore.Components;
using vnet_capacity_planner.Models;
using AntDesign;
using System.Linq;

namespace vnet_capacity_planner.Pages
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

        private async Task ClickExportCSV(MouseEventArgs e)
        {
            _logger.LogInformation("Export CSV");
            // Create CSV
            StringBuilder csvBuilder = new StringBuilder("Type,Name,Resource,Address Space,Address Range,Address Count");
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

            // Download the file
            var csvBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            var csvB64 = Convert.ToBase64String(csvBytes);

            var result = await _dldFile.DownloadFile("vnet.csv", csvB64, "application/octet-stream");
            if (!result.Succeeded)
            {
                _logger.LogError("Download file error", result.ErrorName, result.ErrorMessage);
                Console.WriteLine($"{result.ErrorName}, {result.ErrorMessage}");
            }
        }
    }
}
