using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Threading;
using OpenQA.Selenium.Safari;
using System.Collections.Generic;
using System.Web;
using CsvHelper;
using System.Globalization;

namespace WebScraping
{
    class Program
    {
        static void Main(String[] args)
        {
            ScrapingTest scrapingTest = new ScrapingTest();

            try
            {
                scrapingTest.start_Browser();

                while (true)
                {
                    Console.WriteLine("Press Y to scrape YouTube, I to scrape ICT jobs, W to scrape Wikipedia, or Q to quit: ");
                    string choice = Console.ReadLine();

                    if (choice.ToLower() == "q")
                    {
                        break;
                    }

                    Console.WriteLine("Enter a search term: ");
                    string query = System.Net.WebUtility.UrlEncode(Console.ReadLine());

                    if (choice.ToLower() == "y")
                    {
                        scrapingTest.YouTubeScraping(query);
                    }
                    else if (choice.ToLower() == "i")
                    {
                        scrapingTest.ICTJobScraper(query);
                    }
                    else if (choice.ToLower() == "w")
                    {
                        scrapingTest.WikipediaScraper(query);
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice. Please try again.");
                    }
                }
            }       
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            finally
            {
                scrapingTest.close_Browser();
            }
        }

    }

    [TestFixture]
    public class ScrapingTest
    {
        static Int32 vcount = 1;
        public IWebDriver driver;

        /* LambdaTest Credentials and Grid URL */
        String username = "";
        String accesskey = "";
        String gridURL = "@hub.lambdatest.com/wd/hub";

        [SetUp]
        public void start_Browser()
        {
            ChromeOptions options = new ChromeOptions();
            options.BrowserVersion = "120.0";

            // Set LambdaTest credentials
            Dictionary<string, object> ltOptions = new Dictionary<string, object>();
            ltOptions.Add("username", username);
            ltOptions.Add("accessKey", accesskey);
            ltOptions.Add("platformName", "Windows 10");
            ltOptions.Add("project", "webscraper");
            options.AddAdditionalOption("LT:Options", ltOptions);

            // Initialize the driver
            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
        }

        [Test(Description = "Web Scraping search query"), Order(1)]
        public void YouTubeScraping(String query)
        {
            String url = "https://www.youtube.com/results?search_query=" + query;
            driver.Url = url;
            /* Explicit Wait to ensure that the page is loaded completely by reading the DOM state */
            var timeout = 10000; /* Maximum wait time of 10 seconds */
            var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            try
            {
                // Find and click the reject button
                By buttonLocatorReject = By.XPath("//*[@id='content']/div[2]/div[6]/div[1]/ytd-button-renderer[1]/yt-button-shape/button/yt-touch-feedback-shape/div/div[2]");
                IWebElement rejectbtn = driver.FindElement(buttonLocatorReject);
                rejectbtn.Click();
            }
            catch (NoSuchElementException)
            {  
            }


            // Find the recently uploaded button by its text content
            By buttonLocatorRecent = By.XPath("//yt-chip-cloud-chip-renderer[contains(., 'Recently uploaded')]");
            IWebElement recentbtn = driver.FindElement(buttonLocatorRecent);
            recentbtn.Click();


            Thread.Sleep(5000);

            By elem_video_link = By.XPath("//ytd-video-renderer");
            ReadOnlyCollection<IWebElement> videos = driver.FindElements(elem_video_link);
            Console.WriteLine("******* Here are the 5 most recent video uploads for " + query + "*******");

            List<YouTubeScrapedData> scrapedDataList = new List<YouTubeScrapedData>();

            /* Go through the Videos List and scrap the same to get the attributes of the videos in the channel */
            foreach (IWebElement video in videos)
            {
                if (vcount == 6)
                {
                    break;
                }

                YouTubeScrapedData scrapedData = new YouTubeScrapedData();

                string str_title, str_views, str_rel, str_link, str_channel;
                IWebElement elem_video_title = video.FindElement(By.CssSelector("#video-title"));
                str_title = elem_video_title.Text;

                IWebElement elem_video_channel = video.FindElement(By.XPath(".//*[@id='channel-name']//a"));
                str_channel = elem_video_channel.GetAttribute("textContent").Trim();

                IWebElement elem_video_views = video.FindElement(By.XPath(".//*[@id='metadata-line']/span[1]"));
                str_views = elem_video_views.Text;

                IWebElement elem_video_reldate = video.FindElement(By.XPath(".//*[@id='metadata-line']/span[2]"));
                str_rel = elem_video_reldate.Text;

                IWebElement elem_video_lnk = video.FindElement(By.XPath(".//a[@id=\"video-title\"]"));
                str_link = elem_video_lnk.GetAttribute("href");

                scrapedData.Title = elem_video_title.Text;
                scrapedData.Channel = elem_video_channel.GetAttribute("textContent").Trim();
                scrapedData.Views = elem_video_views.Text;
                scrapedData.ReleaseDate = elem_video_reldate.Text;
                scrapedData.Link = elem_video_lnk.GetAttribute("href");
                scrapedDataList.Add(scrapedData);

                Console.WriteLine("******* Video " + vcount + " *******");
                Console.WriteLine("Video Title: " + str_title);
                Console.WriteLine("Channel: " + str_channel);
                Console.WriteLine("Video Views: " + str_views);
                Console.WriteLine("Video Release Date: " + str_rel);
                Console.WriteLine("Video Link: " + str_link);
                Console.WriteLine("\n");
                vcount++;
            }
            vcount == 1;
            try
            {
                // Write scraped data to CSV file directly
                string outputPath = @"";
                using (var writer = new StreamWriter(outputPath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(scrapedDataList);
                }
                Console.WriteLine("CSV file successfully written.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while writing to the CSV file: " + ex.Message);
            }

        }

        public void ICTJobScraper(String query)
        {
            String url = "https://www.ictjob.be/en/search-it-jobs?query=keyword%3A%3A" + query;
            driver.Url = url;
            /* Explicit Wait to ensure that the page is loaded completely by reading the DOM state */
            var timeout = 10000;
            var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            /* Order by recent */
            By buttonLocatorRecent = By.CssSelector("#sort-by-date");
            IWebElement recentbtn = driver.FindElement(buttonLocatorRecent);
            recentbtn.Click();
            Thread.Sleep(3000);

            By elem_job_link = By.CssSelector("ul.search-result-list.clearfix li.search-item.clearfix");
            ReadOnlyCollection<IWebElement> jobs = driver.FindElements(elem_job_link);           
            List<ICTJobScrapedData> scrapedDataList = new List<ICTJobScrapedData>();

            Console.WriteLine("******* Here are the 5 most recent job offers for " + query + "*******");
            /* Go through the Videos List and scrap the same to get the attributes of the videos in the channel */
            foreach (IWebElement job in jobs)
            {
                if (vcount == 6)
                {
                    break;
                }

                ICTJobScrapedData scrapedData = new ICTJobScrapedData();

                string str_title, str_company, str_location, str_keyword, str_link;
                IWebElement elem_job_title = job.FindElement(By.CssSelector("h2.job-title"));
                str_title = elem_job_title.Text;

                IWebElement elem_job_company = job.FindElement(By.CssSelector("span.job-company"));
                str_company = elem_job_company.Text;

                IWebElement elem_job_location = job.FindElement(By.CssSelector("span.job-location span span"));
                str_location = elem_job_location.Text;

                IWebElement elem_job_keyword = job.FindElement(By.CssSelector("span.job-keywords"));
                str_keyword = elem_job_keyword.Text;

                IWebElement elem_job_lnk = job.FindElement(By.CssSelector("a.job-title.search-item-link"));
                str_link = elem_job_lnk.GetAttribute("href");

                scrapedData.JobTitle = elem_job_title.Text;
                scrapedData.Company = elem_job_company.Text;
                scrapedData.Location = elem_job_location.Text;
                scrapedData.Keywords = elem_job_keyword.Text;
                scrapedData.Link = elem_job_lnk.GetAttribute("href");
                scrapedDataList.Add(scrapedData);

                Console.WriteLine("******* Job " + vcount + " *******");
                Console.WriteLine("Job Title: " + str_title);
                Console.WriteLine("Company: " + str_company);
                Console.WriteLine("Job Location: " + str_location);
                Console.WriteLine("Keywords: " + str_keyword);
                Console.WriteLine("Details Link: " + str_link);
                Console.WriteLine("\n");
                vcount++;
            }
            vcount == 1;
            try
            {
                // Write scraped data to CSV file directly
                string outputPath = @"";
                using (var writer = new StreamWriter(outputPath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(scrapedDataList);
                }
                Console.WriteLine("CSV file successfully written.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while writing to the CSV file: " + ex.Message);
            }
        }

        public void WikipediaScraper(String query)
        {
            String url = "https://en.wikipedia.org/wiki/" + query;
            driver.Url = url;

            // Wait for the page to load
            var timeout = 10000;
            var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            IWebElement elem_wiki_title = driver.FindElement(By.CssSelector("h1 i"));
            string str_title = elem_wiki_title.Text;
            Console.WriteLine("Title: " + str_title);

            Console.WriteLine("******* Table Of Contents *******");
            ReadOnlyCollection<IWebElement> titles = driver.FindElements(By.CssSelector("#mw-content-text h2"));
            foreach (IWebElement title in titles)
            {
                string titleText = title.Text.Trim();

                if (!string.IsNullOrEmpty(titleText) &&
                    !titleText.Equals("See also", StringComparison.OrdinalIgnoreCase) &&
                    !titleText.Equals("References", StringComparison.OrdinalIgnoreCase) &&
                    !titleText.Equals("External links", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(titleText);
                }
            }

            Console.WriteLine("******* References *******");
            ReadOnlyCollection<IWebElement> referenceItems = driver.FindElements(By.CssSelector("ol.references li"));
            foreach (IWebElement referenceItem in referenceItems)
            {
                IWebElement referenceLink = referenceItem.FindElement(By.CssSelector("a[href]"));

                string referenceHref = referenceLink.GetAttribute("href");

                Console.WriteLine($"Reference Link: {referenceHref}");
            }
        }


        [TearDown]
        public void close_Browser()
        {
            driver.Quit();
        }
    }

    public class YouTubeScrapedData
    {
        public string Title { get; set; }
        public string Channel { get; set; }
        public string Views { get; set; }
        public string ReleaseDate { get; set; }
        public string Link { get; set; }
    }

    public class ICTJobScrapedData
    {
        public string JobTitle { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public string Keywords { get; set; }
        public string Link { get; set; }
    }
}
