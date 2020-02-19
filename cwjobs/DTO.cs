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

namespace cwjobs
{
    public class Job
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string JobId { get; set; }
        public string ApplicationURL { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CreatedAt { get; set; }

        public Job(string JobId, string ApplicationURL, string Title, string Description, string CreatedAt)
        {
            this.JobId = JobId;
            this.ApplicationURL = ApplicationURL;
            this.Title = Title;
            this.Description = Description;
            this.CreatedAt = CreatedAt;
        }
    }

    public class JobMatched
    {
        public string ApplicationURL { get; set; }
        public string Title { get; set; }

        public JobMatched(string ApplicationURL, string Title)
        {
            this.ApplicationURL = ApplicationURL;
            this.Title = Title;
        }
    }

    public class MyJobs
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string username { get; set; }
        public string site { get; set; }
        public string[] positiveKeywords { get; set; }
        public string[] negativeKeywords { get; set; }
        public List<JobMatched> jobs { get; set; }
        public List<JobMatched> jobs_applied { get; set; }
        public string createdOn { get; set; }
    }

    public class KeywordScore
    {
        public string Name { get; set; }
        public decimal Score { get; set; }
    }

    public class FinalScore
    {
        public decimal Positive { get; set; }
        public decimal Negative { get; set; }
    }

}
