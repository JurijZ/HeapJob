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
    public class Engine
    {
        
        public void Execute_Crawler()
        {
            Logger.Log("---CRAWLER STARTED---");

            // Initialize Chrome driver
            ChromeOptions options = new ChromeOptions();
            #if !DEBUG
                options.AddArgument("--headless");  // run Chrome in the background
                options.AddArgument("--disable-translate");
                options.AddArgument("--disable-extensions");
                options.AddArgument("--disable-gpu");
            #endif            
            var driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), options);

            // Initialize MongoDB driver
            var client = new MongoClient("mongodb://localhost:27017");
            IMongoDatabase db = client.GetDatabase("heapjob");
            //var collection = db.GetCollection<Job>("jobserve");


            // Collect the latest jobs from the site
            var listOfElements = Crawler.GetLatestJobs(driver);
            if (listOfElements.Count < 1)
            {
                Logger.Log("No Jobs were found");
                Logger.Log("---CRAWLER FINISHED---");
                return; // Exit method
            }

            // Identify new jobs
            var listOfNewJobs = Crawler.IdentifyNewJobs(listOfElements, db);

            // Retrieve the details of the new jobs
            var newJobsWithDetails = Crawler.RetrieveTheDetailsOfTheJobs(listOfNewJobs, driver);

            // Persist to MongonDB
            Crawler.PersistNewJobsToMongo(newJobsWithDetails, db);            

            // Close Chrome browser and driver
            driver.Close();
            driver.Dispose();

            Logger.Log("---CRAWLER FINISHED---");
        }


        public void Execute_Matcher()
        {
            Logger.Log("---MATCHER STARTED---");

            // Initialize MongoDB driver            
            var client = new MongoClient("mongodb://localhost:27017");
            IMongoDatabase db = client.GetDatabase("heapjob");
                        
            // Get the last applied job or null (if new user)
            JobMatched lastAppliedJob = Matcher.GetLastAppliedJob(db);
            
            // Get a list of the newest jobs
            List<Job> latestJobs = Matcher.GetTheNewestJobs(lastAppliedJob, db);

            // Iterate over filtered results to prepare a list of jobs to be inserted
            var jobsMatched = Matcher.SelectOnlyMatchingJobs(latestJobs, db);

            // Add matched jobs to the user jobs
            Matcher.PersistNewJobsToMongo(jobsMatched, db);

            Logger.Log("---MATCHER FINISHED---");
        }


        public void Execute_Applier()
        {
            Logger.Log("---APLIER STARTED---");

            // Initialize MongoDB driver
            var client = new MongoClient("mongodb://localhost:27017");
            IMongoDatabase db = client.GetDatabase("heapjob");

            // Get my new ready to be applied jobs
            var jobsToBeApplied = Applier.GetReadyToBeAppliedJobs(db);

            // Exit if there is no new jobs
            if (!jobsToBeApplied.Any())
            {
                Logger.Log("---APLIER ENDED---");

                return;
            }
            
            // Initialize Chrome driver
            ChromeOptions options = new ChromeOptions();
            #if !DEBUG
                options.AddArgument("--headless");  // run Chrome in the background
                options.AddArgument("--disable-gpu");
                options.AddArgument("--log-level=3");   //only errors
                options.SetLoggingPreference(LogType.Browser, LogLevel.Off);
                options.SetLoggingPreference(LogType.Driver, LogLevel.Off);
            #endif
            var driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), options);

            // Login to jobserve
            Applier.LoginToSite(driver);

            // Apply to each job
            Random random = new Random();

            foreach (var job in jobsToBeApplied)
            {
                Logger.Log("Applying to URL - " + job.ApplicationURL);

                driver.Navigate().GoToUrl(job.ApplicationURL);                    

                // Check if I previously applied             
                if (driver.PageSource.Contains("divMsg"))
                {
                    Logger.Log("Already Applied, Skipping");

                    // Move job from "jobs" to "jobs_applied"
                    Applier.MoveJobToApplied(job, db);

                    continue;
                }

                // Fill the covering letter box
                if (random.Next(0, 2) == 0)
                {
                    string coveringLetter = $@"Hi,

I'd like to apply for the position of - {job.Title}
I'm a highly experienced developer and available immediately.

Jurij
+447565111967";
                    try
                    {
                        driver.FindElementById("txtCov").Clear();
                        driver.FindElementById("txtCov").SendKeys(coveringLetter);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception is generated - " + ex.Message);
                        Logger.Log("Will Click on the button without filling the covering letter");
                    }
                }
                else
                {
                    Logger.Log("Using web sites default coveringLetter.");
                }

                // Apply
                driver.FindElementById("btn1").Click();
                
                // Move job from "jobs" to "jobs_applied"
                Applier.MoveJobToApplied(job, db);                                       
            }

            // Close Chrome driver
            driver.Close(); // closing chrome browser
            driver.Dispose();   // killing chrome driver process

            Logger.Log("---APLIER ENDED---");
        }
    }
}
