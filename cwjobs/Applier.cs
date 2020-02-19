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
    public static class Applier
    {

        public static void MoveJobToApplied(JobMatched job, IMongoDatabase db)
        {
            var userjobs_collection = db.GetCollection<MyJobs>("userjobs");

            // Move job record to the "jobs_applied" array and delete from the "jobs" array
            var filter = Builders<MyJobs>.Filter.Eq("username", "jzilcov@gmail.com") &
                Builders<MyJobs>.Filter.Eq("site", "cwjobs");
            var insert = Builders<MyJobs>.Update.AddToSet("jobs_applied", job);
            userjobs_collection.FindOneAndUpdate<MyJobs>(filter, insert);

            //var delete_filter = new BsonDocument("username", "bodrum");
            var jobToDelete = Builders<JobMatched>.Filter.Eq("ApplicationURL", job.ApplicationURL);
            var delete = Builders<MyJobs>.Update.PullFilter("jobs", jobToDelete);
            var result = userjobs_collection.FindOneAndUpdateAsync(filter, delete).Result;
        }


        public static List<JobMatched> GetReadyToBeAppliedJobs(IMongoDatabase db)
        {
            var userjobs = db.GetCollection<MyJobs>("userjobs");

            var filter = Builders<MyJobs>.Filter.Eq("username", "jzilcov@gmail.com") &
                Builders<MyJobs>.Filter.Eq("site", "cwjobs");
            var myjobs = userjobs.Find<MyJobs>(filter);
            var jobsToBeApplied = myjobs.FirstOrDefault().jobs;

            return jobsToBeApplied;
        }


        public static void LoginToSite(ChromeDriver driver)
        {
            // Front page (fill the form)
            driver.Navigate().GoToUrl("https://www.cwjobs.co.uk/account/signin?ReturnUrl=/");
            
            // Login
            IWebElement txtEmail = (new WebDriverWait(driver, TimeSpan.FromSeconds(3)))
                .Until(ExpectedConditions.ElementExists(By.Id("Form_Email")));
            txtEmail.SendKeys("jzilcov@gmail.com");

            IWebElement txbPassword = (new WebDriverWait(driver, TimeSpan.FromSeconds(3)))
                .Until(ExpectedConditions.ElementExists(By.Id("Form_Password")));
            txbPassword.SendKeys("8tynokar");

            //driver.FindElementByXPath("//input[@id='txbEmail']").SendKeys("jzilcov@gmail.com");
            //driver.FindElementByXPath("//input[@id='txbPassword']").SendKeys("Csharp.job1");
            //driver.FindElementByXPath("//input[@id='btnlogin']").Click();
            driver.FindElementById("btnLogin").Click();
        }
    }
}
