using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using static keywordEditor.DTO;

namespace keywordEditor
{
    public static class Editor
    {
        public static void CheckSiteName(string site)
        {
            switch (site)
            {
                case "jobserve":
                    break;
                case "cwjobs":
                    break;
                default:
                    throw new ArgumentException("Invalid site name", "argument");
            }
        }

        public static void AddPositiveKeywords(IMongoDatabase db, string site, string newKeyword)
        {

            var userjobs_collection = db.GetCollection<MyJobs>("userjobs");
            var userdetails_filter = Builders<MyJobs>.Filter.Eq("username", "jzilcov@gmail.com") &
                Builders<MyJobs>.Filter.Eq("site", site);

            // Get user whole document
            var userdetails = userjobs_collection.Find(userdetails_filter);

            // Take user defined keywords
            var positiveKeywords = userdetails.FirstOrDefault().positiveKeywords;

            // Add new keyword
            var positiveKeywords_List = positiveKeywords.ToList<string>();
            positiveKeywords_List.Add(newKeyword.ToUpper());
            positiveKeywords_List = positiveKeywords_List.Distinct().ToList();
            positiveKeywords_List.Sort();

            foreach (var e in positiveKeywords_List)
            {
                Console.WriteLine(e);
            }

            // Update a list of keywords
            var updatePositiveKeywords = Builders<MyJobs>.Update.Set("positiveKeywords", positiveKeywords_List.ToArray());
            userjobs_collection.FindOneAndUpdate<MyJobs>(userdetails_filter, updatePositiveKeywords);

        }

        public static void AddNegativeKeywords(IMongoDatabase db, string site, string newKeyword)
        {


            var userjobs_collection = db.GetCollection<MyJobs>("userjobs");
            var userdetails_filter = Builders<MyJobs>.Filter.Eq("username", "jzilcov@gmail.com") &
                Builders<MyJobs>.Filter.Eq("site", site);

            // Get user whole document
            var userdetails = userjobs_collection.Find(userdetails_filter);

            // Take user defined keywords
            var negativeKeywords = userdetails.FirstOrDefault().negativeKeywords;

            // Add new keyword
            var negativeKeywords_List = negativeKeywords.ToList<string>();
            negativeKeywords_List.Add(newKeyword.ToUpper());
            negativeKeywords_List = negativeKeywords_List.Distinct().ToList();
            negativeKeywords_List.Sort();

            foreach (var e in negativeKeywords_List)
            {
                Console.WriteLine(e);
            }

            // Update a list of keywords
            var updateNegativeKeywords = Builders<MyJobs>.Update.Set("negativeKeywords", negativeKeywords_List.ToArray());
            userjobs_collection.FindOneAndUpdate<MyJobs>(userdetails_filter, updateNegativeKeywords);

        }

        public static void RemovePositiveKeywords(IMongoDatabase db, string site, string newKeyword)
        {

            var userjobs_collection = db.GetCollection<MyJobs>("userjobs");
            var userdetails_filter = Builders<MyJobs>.Filter.Eq("username", "jzilcov@gmail.com") &
                Builders<MyJobs>.Filter.Eq("site", site);

            // Get user whole document
            var userdetails = userjobs_collection.Find(userdetails_filter);

            // Take user defined keywords
            var positiveKeywords = userdetails.FirstOrDefault().positiveKeywords;

            // Add new keyword
            var positiveKeywords_List = positiveKeywords.ToList<string>();
            positiveKeywords_List.Remove(newKeyword.ToUpper());
            //positiveKeywords_List.Sort();

            foreach (var e in positiveKeywords_List)
            {
                Console.WriteLine(e);
            }

            // Update a list of keywords
            var updatePositiveKeywords = Builders<MyJobs>.Update.Set("positiveKeywords", positiveKeywords_List.ToArray());
            userjobs_collection.FindOneAndUpdate<MyJobs>(userdetails_filter, updatePositiveKeywords);

        }

        public static void RemoveNegativeKeywords(IMongoDatabase db, string site, string newKeyword)
        {


            var userjobs_collection = db.GetCollection<MyJobs>("userjobs");
            var userdetails_filter = Builders<MyJobs>.Filter.Eq("username", "jzilcov@gmail.com") &
                Builders<MyJobs>.Filter.Eq("site", site);

            // Get user whole document
            var userdetails = userjobs_collection.Find(userdetails_filter);

            // Take user defined keywords
            var negativeKeywords = userdetails.FirstOrDefault().negativeKeywords;

            // Add new keyword
            var negativeKeywords_List = negativeKeywords.ToList<string>();
            negativeKeywords_List.Remove(newKeyword.ToUpper());
            //negativeKeywords_List.Sort();

            foreach (var e in negativeKeywords_List)
            {
                Console.WriteLine(e);
            }

            // Update a list of keywords
            var updateNegativeKeywords = Builders<MyJobs>.Update.Set("negativeKeywords", negativeKeywords_List.ToArray());
            userjobs_collection.FindOneAndUpdate<MyJobs>(userdetails_filter, updateNegativeKeywords);

        }
    }
}

