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
using jobserve;
using System.Collections.ObjectModel;

namespace jobserve_Tests
{
    [TestClass]
    public class Tests
    {


        [TestMethod]
        public void Crawler_BrowseWebSite_GetAListOfJobs()
        {
            // Arrange
            ChromeOptions options = new ChromeOptions();
            ReadOnlyCollection<IWebElement> lstElements;

            // Act
            using (var driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), options))
            {
                lstElements = Crawler.GetLatestJobs(driver);
            }

            // Assert
            Logger.Log(lstElements.Count);
            Assert.IsTrue(lstElements.Count > 0);
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

    }
}
