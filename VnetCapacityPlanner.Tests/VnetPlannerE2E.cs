using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace VnetCapacityPlanner.Tests
{
    public class VnetPlannerE2E : IClassFixture<DevHostServerFixture>
    {
        private readonly DevHostServerFixture _server;

        public VnetPlannerE2E(DevHostServerFixture server)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("testsettings.json")
                .AddJsonFile("testsettings.local.json", true)
                .Build();

            var root = Path.Combine(AppContext.BaseDirectory, "../../../../");
            var location = Path.GetFullPath(Path.Combine(root, config["contentRoot"]));

            _server = server;
            _server.ContentRoot = location;
        }

        [Fact]
        public async Task SiteIsLoaded()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync();
            var page = await browser.NewPageAsync();
            await page.GotoAsync(_server.RootUri.AbsoluteUri);
            var header = await page.WaitForSelectorAsync(".site-title");
            Assert.Equal("Azure Virtual Network Capacity Planner", await header.TextContentAsync());
        }

        [Fact]
        public async Task AddSubnet()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync();
            //await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            //{
            //    Headless = false,
            //    SlowMo = 250,
            //});
            var page = await browser.NewPageAsync();
            await page.GotoAsync(_server.RootUri.AbsoluteUri);
            // Click text="Add Subnet"
            await page.ClickAsync("text=\"Add Subnet\"");

            // Click input[role="combobox"]
            await page.ClickAsync("input[role=\"combobox\"]");

            // Click text="Azure Bastion"
            await page.ClickAsync("text=\"Azure Bastion\"");

            // Click //div[normalize-space(.)='Start Address']/div[2]/div/div/input[normalize-space(@type)='text']
            await page.ClickAsync("//div[normalize-space(.)='Start Address']/div[2]/div/div/input[normalize-space(@type)='text']");

            // Fill //div[normalize-space(.)='Start Address']/div[2]/div/div/input[normalize-space(@type)='text']
            await page.FillAsync("//div[normalize-space(.)='Start Address']/div[2]/div/div/input[normalize-space(@type)='text']", "10.0.0.0");

            // Click text="OK"
            await page.ClickAsync("text=\"OK\"");

            // Search text="10.0.0.0/27"
            var addressSpace = await page.TextContentAsync("//table[1]/tbody/tr[@data-row-key='0']/td[2]");
            Assert.Equal("10.0.0.0/27", addressSpace);
        }
    }
}
