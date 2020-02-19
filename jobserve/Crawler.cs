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

namespace jobserve
{
    public static class Crawler    {


        public static void PersistNewJobsToMongo(List<Job> newJobs, IMongoDatabase db)
        {
            var collection = db.GetCollection<Job>("jobserve");

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

                IWebElement Desc = (new WebDriverWait(driver, TimeSpan.FromSeconds(3)))
                    .Until(ExpectedConditions.ElementExists(By.Id("skls")));
                j.Title = driver.FindElementById("h3Pos").Text;
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

            var collection = db.GetCollection<Job>("jobserve");

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
                //Logger.Log("Checking jobserver ID - " + JobId);
                if (JobId == latestJobId)
                {
                    Logger.Log("Match is found, breaking on - " + JobId);
                    break;
                }

                // Copile a URL
                string ApplicationURL = "https://www.jobserve.com/gb/en/FastTrackJobApplication.aspx?jobid=" + JobId;
                
                // Add new object to the list
                listOfJobs.Add(new Job(JobId, ApplicationURL, "", "", DateTime.Now.ToString("yyyy-MM-dd HHmmss")));
            }

            return listOfJobs;
        }

        public static ReadOnlyCollection<IWebElement> GetLatestJobs(ChromeDriver driver)
        {
            Logger.Log("New logger");

            // Login or just go to the search page)
            //LoginToSite(driver);
            driver.Navigate().GoToUrl("https://www.jobserve.com/gb/en/Job-Search/");

            // Extended search tab
            driver.FindElementByXPath("//a[@id='tab_pqs']//span[@id='lab2']").Click();

            // Enter the search Keyword
            var fldKeywords = driver.FindElementById("txtKey");
            fldKeywords.Clear();
            fldKeywords.SendKeys("C# SQL");
            //output.WriteLine("Button Title: - " + fldKeywords.GetAttribute("value"));
            //Assert.True(btn2.GetAttribute("value") == "Job Seeker", "Button was not found");

            // Enter Location
            var fldLocation = driver.FindElementById("txtLoc");
            fldLocation.Clear();
            fldLocation.SendKeys("London");
            //btn2.Click();

            // Select Distance
            (new SelectElement(driver.FindElementById("selRad"))).SelectByValue("25");  //5, 10, 15, 25

            // Select Duration
            (new SelectElement(driver.FindElementById("selAge"))).SelectByValue("0"); //Today
            //(new SelectElement(driver.FindElementById("selAge"))).SelectByValue("1");   //Within 1 Day

            // Select Contracts only
            (new SelectElement(driver.FindElementById("selJType"))).SelectByValue("2");

            // Select the Rate
            driver.FindElementById("ddcl-selRate").Click();
            driver.FindElementById("ddcl-selRate-i3").Click(); //30-40 per hour
            driver.FindElementById("ddcl-selRate-i4").Click();
            driver.FindElementById("ddcl-selRate-i5").Click();
            driver.FindElementById("ddcl-selRate-i6").Click(); //65-80 per hour

            // Click Search button
            driver.FindElementById("btnSearch").Click();

            // Order jobs by the Latest
            var sort = driver.FindElement(By.Id("sortSelect"));
            Actions builder = new Actions(driver);  // Create interaction              
            builder.MoveToElement(sort).Perform();  // Move cursor to the Main Menu Element
            Thread.Sleep(500);  // wait half a second for submenu to be displayed  
            driver.FindElementById("lkLt").Click(); // Clicking on the submenu - Latest

            // Enumerate jobs shown on the page
            Thread.Sleep(1000);
            var lstElements = driver.FindElementById("jsJobResContent").FindElements(By.ClassName("jobItem"));
            Logger.Log("Number of jobs to analyse: " + lstElements.Count);
            
            return lstElements;
        }
    }
}
