using BlazorApplicationInsights;
using BlazorDownloadFile;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VnetCapacityPlanner.Models;

namespace VnetCapacityPlanner
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            // Add antd
            builder.Services.AddAntDesign();
            builder.Services.AddSingleton<VirtualNetwork>();
            builder.Services.AddBlazorApplicationInsights(async applicationInsights =>
            {
                var telemetryItem = new TelemetryItem()
                {
                    Tags = new Dictionary<string, object>()
                    {
                        { "ai.cloud.role", "SPA" },
                        { "ai.cloud.roleInstance", "vnet-planner" },
                    }
                };

                await applicationInsights.AddTelemetryInitializer(telemetryItem);
            });
            builder.Services.AddBlazorDownloadFile();

            var host = builder.Build();
            var vnetState = host.Services.GetRequiredService<VirtualNetwork>();
            await vnetState.Initialize();

            await host.RunAsync();
        }
    }
}
