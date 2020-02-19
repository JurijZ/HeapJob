using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using cwjobs;
using System.Collections.ObjectModel;

namespace cwjobs_Tests
{
    [TestClass]
    public class Tests
    {


        [TestMethod]
        public void Crawler_GetLatestJobs_AListOfJobs()
        {
            // Arrange
            ChromeOptions options = new ChromeOptions();
            ReadOnlyCollection<IWebElement> lstElements;
            
            using (var driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), options))
            {
                // Act
                lstElements = Crawler.GetLatestJobs(driver);

                //Assert
                Logger.Log(lstElements.Count);
                Assert.IsTrue(lstElements.Count > 0);

                foreach (var e in lstElements)
                {
                    string id = e.GetAttribute("id");

                    // Compile a URL
                    string ApplicationURL = "https://www.cwjobs.co.uk/job/" + id;

                    Console.WriteLine(ApplicationURL);
                }
            }
            
            //Console.WriteLine(lstElements.First().Text); //FirstOrDefault().ToBsonDocument());
            
        }

        [TestMethod]
        public void Crawler_EndToEnd()
        {
            // Arrange
            var engine = new Engine();

            // Act
            engine.Execute_Crawler();

            // Assert

        }

        [TestMethod]
        public void Matcher_EndToEnd()
        {
            // Arrange
            var engine = new Engine();

            // Act
            engine.Execute_Matcher();

            // Assert
        }

        [TestMethod]
        public void Matcher_OverwriteKeywords()
        {
            // Arrange
            var client = new MongoClient("mongodb://localhost:27017");
            IMongoDatabase db = client.GetDatabase("heapjob");

            // Act
            Matcher.InitialKeywordPopulation(db);

            // Assert

        }

        [TestMethod]
        public void Applier_EndToEnd()
        {
            // Arrange
            var engine = new Engine();

            // Act
            engine.Execute_Applier();

            // Assert
        }

        [TestMethod]
        public void Applier_LoginToSite_UserName()
        {
            // Arrange
            ChromeOptions options = new ChromeOptions();

            using (var driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), options))
            {
                // Act
                Applier.LoginToSite(driver);
                System.Threading.Thread.Sleep(2000);

                // Assert
                // Watch behaviour in the browser
            }
        }
    }
}
