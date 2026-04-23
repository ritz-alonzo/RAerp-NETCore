using Microsoft.Playwright;
using NUnit.Framework;

namespace RAerp_NETCore_Tests.E2ETests
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class RaerpPlaywrightTest : PageTest
    {
        //private IBrowser _browser;
        //private IPage _page;

        //[SetUp]
        //public async Task Setup()
        //{
        //    _browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        //    {
        //        Headless = false  // Show browser
        //    });

        //    _page = await _browser.NewPageAsync();
        //}

        [Test]
        public async Task HomepageHasPlaywrightInTitleAndGetStartedLinkLinkingtoTheIntroPage()
        {
            // Local Site: https://localhost:44358/
            // Test Site: http://raerpv1.somee.com/

            // Browser: Chromium
            var browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false  // Show browser
            });

            var page = await browser.NewPageAsync();

            await page.GotoAsync("https://localhost:44358/");

            // Expect a title "to contain" a substring.
            await Expect(page).ToHaveTitleAsync(new Regex("Login"));

            // create a locator
            //var getStarted = page.Locator("text=Get Started");

            //// Expect an attribute "to be strictly equal" to the value.
            //await Expect(getStarted).ToHaveAttributeAsync("href", "/docs/intro");

            //// Click the get started link.
            //await getStarted.ClickAsync();

            //// Expects the URL to contain intro.
            //await Expect(page).ToHaveURLAsync(new Regex(".*intro"));

            await page.GetByLabel("Username").ClickAsync();
            await page.GetByLabel("Username").FillAsync("raerpSAdmin");
            await page.GetByLabel("Username").PressAsync("Tab");
            await page.GetByLabel("Password").FillAsync("@dm!n123");
            await page.GetByRole(AriaRole.Button, new() { NameString = "Sign in" }).ClickAsync();
            await page.WaitForURLAsync("https://localhost:44358/Home/Index");
            await page.GetByRole(AriaRole.Link, new() { NameString = "Business Entities" }).ClickAsync();
            await page.GetByRole(AriaRole.Link, new() { NameString = "butest" }).ClickAsync();
            await page.WaitForURLAsync("https://localhost:44358/BusinessEntities/List/?entityTypeId=fe791d74-870f-4740-17ed-08dc261d4c81");
            await page.GetByRole(AriaRole.Link, new() { NameString = "BU-0002" }).ClickAsync();
            await page.WaitForURLAsync("https://localhost:44358/BusinessEntities/Index/0dab2176-a9de-4cfb-2165-08dc2ca37f4f");
            await page.GetByRole(AriaRole.Tab, new() { NameString = "Address" }).ClickAsync();
            await page.GetByLabel("AddressLine1").ClickAsync();
            await page.GetByLabel("AddressLine1").FillAsync("test321");
            await page.GetByLabel("AddressLine2").ClickAsync();
            await page.GetByRole(AriaRole.Button, new() { NameString = "Save" }).ClickAsync();
            await page.WaitForURLAsync("https://localhost:44358/BusinessEntities/Index/0dab2176-a9de-4cfb-2165-08dc2ca37f4f");
            await Expect(page.GetByText("Successfully updated Business Entity")).ToBeVisibleAsync();
        }
    }
}
