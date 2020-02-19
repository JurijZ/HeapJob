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
    public static class Matcher    {

        public static void InitialKeywordPopulation(IMongoDatabase db)
        {
            string[] positiveKeywords = { "AD", "ADFS", "ANGULAR", "API", "ASP", "AWS", "AZURE", "BACKEND", "BOOTSTRAP", "C#", "CORE", "CSS", "CSS3", "DIRECTORY", "EF", "ENTITY", "ETL", "EXCEL", "HTML", "HTML5", "IOC", "LDAP", "LINQ", "MONGO", "MONGODB", "MVC", "NET", "POWERSHELL", "RDBMS", "REDIS", "REST", "RESTFULL", "S3", "SCRUM", "SELENIUM", "SERVER", "SQL", "SSAS", "SSIS", "SSL", "SSRS",
                "TLS", "T-SQL", "TYPESCRIPT", "VISUAL", "WEBAPI", "WEBAPIS", "WINDOWS", "XML" };
            Array.Sort(positiveKeywords, StringComparer.InvariantCulture);

            string[] negativeKeywords = { "JAVA", "HL7", "IR35", "STORM", "K2", "OPENTEXT", "WPF", "WCF", "BASIC", "C++", "PYTHON", "ORACLE", "GO", "JAVASCRIPT", "DOCKER", "BANK", "MANAGER", "ADOBE", "SC", "CLEARENCE", "R", "CUBES", "CUBE", "OLAP", "GOOGLE", "POSTGRESQL", "TERADATA", "HIVE", "SCALA", "SOAP", "VB", "VBA", "LINUX", "PL", "JQUERY", "SPRING", "HIBERNATE", "ANALYTICS", "BIZTALK", "ANALYST", "SHAREPOINT", "ADMINISTRATOR", "OFFICE", "365", "NGINX", "APACHE", "TOMCAT", "SHELL", "DEVOPS", "POWERBI", "BACKBONE", "SPECFLOW", "POWERPIVOT", "BI", "TABULAR", "INFOPATH", "MANAGEMENT", "ELASTICSEARCH", "NEO4J", "JEE", "J2E", "REACTJS",
                "REACT", "KNOCKOUT", "DESKTOP", "IMPALA", "RISK", "WINFORMS", "SALESFORCE" };
            Array.Sort(negativeKeywords, StringComparer.InvariantCulture);

            var userjobs_collection = db.GetCollection<MyJobs>("userjobs");
            var update_filter = Builders<MyJobs>.Filter.Eq("username", "jzilcov@gmail.com") & 
                                Builders<MyJobs>.Filter.Eq("site", "cwjobs");
            
            var updatePositiveKeywords = Builders<MyJobs>.Update.Set("positiveKeywords", positiveKeywords);
            userjobs_collection.FindOneAndUpdate<MyJobs>(update_filter, updatePositiveKeywords);

            var updateNegativeKeywords = Builders<MyJobs>.Update.Set("negativeKeywords", negativeKeywords);
            userjobs_collection.FindOneAndUpdate<MyJobs>(update_filter, updateNegativeKeywords);
        }

        public static void PersistNewJobsToMongo(List<JobMatched> jobsMatched, IMongoDatabase db)
        {
            var userjobs_collection = db.GetCollection<MyJobs>("userjobs");

            var update_filter = Builders<MyJobs>.Filter.Eq("username", "jzilcov@gmail.com") &
                Builders<MyJobs>.Filter.Eq("site", "cwjobs");
            var update = Builders<MyJobs>.Update.AddToSetEach("jobs", jobsMatched);

            userjobs_collection.FindOneAndUpdate<MyJobs>(update_filter, update);
        }

        public static List<JobMatched> SelectOnlyMatchingJobs(List<Job> latestjobs, IMongoDatabase db)
        {
            var jobsMatched = new List<JobMatched>();

            var userjobs_collection = db.GetCollection<MyJobs>("userjobs");

            var userdetails_filter = Builders<MyJobs>.Filter.Eq("username", "jzilcov@gmail.com") &
                Builders<MyJobs>.Filter.Eq("site", "cwjobs");
            var userdetails = userjobs_collection.Find(userdetails_filter);

            // Get user defined keywords
            var positiveKeywords = userdetails.FirstOrDefault().positiveKeywords;
            var negativeKeywords = userdetails.FirstOrDefault().negativeKeywords;

            latestjobs.ForEach(j =>
            {
                Logger.Log(j.Title);

                // Parse Description
                var keywordScores = TextParser(j.Description);

                // Calculate overall score
                var score = OverallScoreGenerator(keywordScores, positiveKeywords, negativeKeywords);

                if (score.Positive > score.Negative)
                {
                    jobsMatched.Add(new JobMatched(j.ApplicationURL, j.Title));
                    //Logger.Log("Added - " + j.Title);
                }
            });

            Logger.Log("Number of jobs after filtering - " + jobsMatched.Count);

            return jobsMatched;
        }

        public static List<Job> GetTheNewestJobs(JobMatched lastAppliedJob, IMongoDatabase db)
        {
            var cwjobs_collection = db.GetCollection<Job>("cwjobs");

            string mongodbStartOfTheSearchObjectId;

            if (lastAppliedJob != null)
            {
                //Logger.Log(readyToApplyJobs.ApplicationURL);

                var applicationURL_filter = Builders<Job>.Filter.Eq("ApplicationURL", lastAppliedJob.ApplicationURL);
                var preveousjob = cwjobs_collection.Find(applicationURL_filter).FirstOrDefault();
                Logger.Log("Will be loading from the latest user job - " + (preveousjob?.Id ?? ""));
                mongodbStartOfTheSearchObjectId = preveousjob?.Id;

                if (preveousjob == null)
                {
                    Logger.Log("Will be loading from the start of the day");

                    int timestamp = (int)(DateTime.Now.Date - BsonConstants.UnixEpoch).TotalSeconds;
                    var mongodbStartOfTheDayObjectId = new ObjectId(timestamp, 0, 0, 0);

                    var applicationURL_filter2 = Builders<Job>.Filter.Gt("_id", mongodbStartOfTheDayObjectId);
                    preveousjob = cwjobs_collection.Find(applicationURL_filter2).FirstOrDefault();
                    Logger.Log("Search by ObjectID - " + (preveousjob?.Id ?? ""));
                    mongodbStartOfTheSearchObjectId = preveousjob?.Id;
                }
            }
            else
            {
                // Take jobs from the start of the current day
                Logger.Log("Will be loading from the start of the day");

                int timestamp = (int)(DateTime.Now.Date - BsonConstants.UnixEpoch).TotalSeconds;
                var mongodbObjectId = new ObjectId(timestamp, 0, 0, 0);

                var applicationURL_filter = Builders<Job>.Filter.Gt("_id", mongodbObjectId);
                var preveousjob = cwjobs_collection.Find(applicationURL_filter).FirstOrDefault();
                Logger.Log("Search by ObjectID - " + (preveousjob?.Id ?? ""));
                mongodbStartOfTheSearchObjectId = preveousjob?.Id;
            }

            // Retrieve all latest jobs
            //Logger.Log("ObjectId(\"" + mongodbStartOfTheSearchObjectId + "\")");
            var latestjobs_filter = Builders<Job>.Filter.Gt("_id", ObjectId.Parse(mongodbStartOfTheSearchObjectId));
            var latestjobs = cwjobs_collection.Find(latestjobs_filter);

            // Regex filter
            //var search_filter = Builders<Job>.Filter.Regex("Description", new BsonRegularExpression("C#"));
            //var latestjobs = cwjobs_collection.Find(search_filter);

            // Count
            Logger.Log("Number of jobs before filtering - " + latestjobs.Count());
            //Logger.Log(latestjobs.ToList<Job>()[0].Title);

            return latestjobs.ToList<Job>();
        }

        public static JobMatched GetLastAppliedJob(IMongoDatabase db)
        {
            // Users collection
            var userjobs_collection = db.GetCollection<MyJobs>("userjobs");

            var userdetails_filter = Builders<MyJobs>.Filter.Eq("username", "jzilcov@gmail.com") &
                Builders<MyJobs>.Filter.Eq("site", "cwjobs");
            var userdetails = userjobs_collection.Find(userdetails_filter);

            JobMatched lastAppliedJob = null;
            if (userdetails.Count() != 0)
            {
                // Take the latest applied job
                lastAppliedJob = userdetails.FirstOrDefault().jobs_applied.LastOrDefault();
            }
            else
            {
                // Creates new user if it does not exist
                var newUser = new MyJobs
                {
                    username = "jzilcov@gmail.com",
                    site = "cwjobs",
                    positiveKeywords = new string[] { },
                    negativeKeywords = new string[] { },
                    jobs = new List<JobMatched>(),
                    jobs_applied = new List<JobMatched>(),
                    createdOn = DateTime.Now.ToShortDateString()
                };
                userjobs_collection.InsertOne(newUser);
            }

            return lastAppliedJob;
        }

        public static List<KeywordScore> TextParser(string text)
        {
            // Remove HTML tags
            text = Regex.Replace(text, "<.*?>", " ");
            //Logger.Log(text);

            // Split into words
            var words = Regex.Matches(text.ToUpper(), @""".*?""|[^\s,;/()\-\[\]\{\}\\]+").Cast<Match>()
                .Select(m => m.Value).ToArray();

            Logger.Log("Number of words in the description - " + words.Length);

            List<KeywordScore> keywordScores = new List<KeywordScore>();
            for (var i = 0; i < words.Length; i++)
            {
                //Logger.Log(words[i] + ": weight = " + (words.Length - i).ToString());
                keywordScores.Add(new KeywordScore { Name = words[i], Score = words.Length - i });
            }

            return keywordScores;
        }

        public static FinalScore OverallScoreGenerator(List<KeywordScore> keywordScore, string[] positiveKeywords, string[] negativeKeywords)
        {

            var k = from ks in keywordScore
                    from pk in positiveKeywords
                    where ks.Name == pk
                    group ks by ks.Name
                    into mgroup
                    select mgroup.Key;

            // Caclulate Positive score
            // Join and Group by repeating keywords
            //var k = from p in positiveKeywords
            //        from m in words
            //        where p == m
            //        group m by m
            //        into mgroup                    
            //        select mgroup.Key;

            decimal positiveScore = 0;
            foreach (var w in k)
            {
                
                //Sum is used because there might be multiple same words
                positiveScore = positiveScore + keywordScore.Where(p => p.Name == w).Sum(x => x.Score);

                Logger.Log(w + " " + positiveScore.ToString());
                //Logger.Log(positiveScore);
            }

            // Calculate Negative score
            // Join and Group by repeating keywords
            var n = from ks in keywordScore
                    from pk in negativeKeywords
                    where ks.Name == pk
                    group ks by ks.Name
                    into mgroup
                    select mgroup.Key;

            //var n = from p in negativeKeywords
            //        from m in words
            //        where p == m
            //        group m by m
            //        into mgroup
            //        select mgroup.Key;

            decimal negativeScore = 0;
            foreach (var w in n)
            {                
                //Sum is used because there might be multiple same words
                negativeScore = negativeScore + keywordScore.Where(p => p.Name == w).Sum(x => x.Score); 

                Logger.Log(w + " " + negativeScore.ToString());
                //Logger.Log(negativeScore);
            }

            // Final results
            Logger.Log("Positive score - " + positiveScore);
            Logger.Log("Negative score - " + negativeScore);

            FinalScore finalScore = new FinalScore { Positive = positiveScore, Negative = negativeScore };
            return finalScore;
        }

    }
}
