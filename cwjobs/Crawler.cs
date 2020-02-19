using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using OpenQA.Selenium.Interactions;
using System.Threading;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.ObjectModel;

namespace cwjobs
{
    public static class Crawler    {


        public static void PersistNewJobsToMongo(List<Job> newJobs, IMongoDatabase db)
        {
            var collection = db.GetCollection<Job>("cwjobs");

            if (newJobs.Count > 0)
            {
                // Reverse the list of jobs - Ascending
                newJobs.Reverse();

                // Insert
                collection.InsertMany(newJobs);
            }
        }

        public static List<Job> RetrieveTheDetailsOfTheJobs(List<Job> listOfJobs, ChromeDriver driver)
        {
            var Jobs = new List<Job>();

            Logger.Log("Number of new jobs - " + listOfJobs.Count);
            foreach (var j in listOfJobs)
            {
                // Open advert details
                driver.Navigate().GoToUrl(j.ApplicationURL);

                //new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(ExpectedConditions.ElementExists(By.Id("md_skills")));
                //new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(ExpectedConditions.StalenessOf(driver.FindElementById("md_skills")));
                //j.Description = driver.FindElementByXPath("//div[@class='job-description']//p").Text;

                IWebElement Desc = (new WebDriverWait(driver, TimeSpan.FromSeconds(3)))
                    .Until(ExpectedConditions.ElementExists(By.XPath("//div[@class='job-description']")));

                j.Title = driver.FindElementByXPath("//div[@class='col-xs-12 col-md-10 col-md-offset-1 col-page-header']//h1").Text;
                j.Description = j.Title + " " + Desc.GetAttribute("innerHTML");

                Jobs.Add(j);

                //Logger.Log(j.JobId);
                Logger.Log(j.Title);
                //Logger.Log(j.ApplicationURL);
            }

            return Jobs;
        }

        public static List<Job> IdentifyNewJobs(ReadOnlyCollection<IWebElement> lstElements, IMongoDatabase db)
        {
            var listOfJobs = new List<Job>();

            var collection = db.GetCollection<Job>("cwjobs");

            // Find the latest job from the preveous import
            var currentTime = DateTime.Now.ToString("yyyy-MM-dd");
            var latestJob = collection.Find<Job>(FilterDefinition<Job>.Empty)
                .Limit(1)
                .SortByDescending(j => j.Id);

            Logger.Log("Latest job exist - " + latestJob.Count());
            var ljob = latestJob.FirstOrDefault();
            string latestJobId = ljob?.JobId ?? "";
            //Logger.Log("Latest job Id: " + latestJobId);

            foreach (var e in lstElements)
            {
                var JobId = e.GetAttribute("id");

                // Check if the job has already been saved before
                // If it is exist then break the loop
                //Logger.Log("Checking job ID - " + JobId);
                if (JobId == latestJobId)
                {
                    Logger.Log("Match is found, breaking on - " + JobId);
                    break;
                }

                // Copile a URL
                string ApplicationURL = "https://www.cwjobs.co.uk/job/" + JobId;
                
                // Add new object to the list
                listOfJobs.Add(new Job(JobId, ApplicationURL, "", "", DateTime.Now.ToString("yyyy-MM-dd HHmmss")));
            }

            return listOfJobs;
        }

        public static ReadOnlyCollection<IWebElement> GetLatestJobs(ChromeDriver driver)
        {
            //LoginToSite(driver);

            // Go straight to the search results page
            driver.Navigate().GoToUrl("https://www.cwjobs.co.uk/jobs/contract/sql-or-c%23/in-london?salary=300&salarytypeid=4&q=Sql+Or+C%23&radius=20&sort=2");
                        
            // Enumerate jobs shown on the page
            Thread.Sleep(1000);
            var lstElements = driver.FindElements(By.ClassName("job"));
            Logger.Log("Number of jobs to analyse: " + lstElements.Count);
            
            return lstElements;
        }
    }
}
