using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace keywordEditor
{
    class DTO
    {
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
    }
}
