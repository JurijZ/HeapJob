using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace keywordEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter site name (jobserve/cwjobs): ");
            var site = Console.ReadLine();
            Editor.CheckSiteName(site);

            Console.Write("Add or Remove the keyword? (a/r): ");
            var action = Console.ReadLine();

            Console.Write("Enter a keyword: ");
            var newKeyword = Console.ReadLine();

            Console.Write("Is it a Positive or a Negative keyword (p/n): ");
            var keywordType = Console.ReadLine();

            // Initialize MongoDB driver
            var client = new MongoClient("mongodb://localhost:27017");
            IMongoDatabase db = client.GetDatabase("heapjob");


            if (action.ToString() == "a")
            {
                if (keywordType.ToString() == "p")
                {
                    Editor.AddPositiveKeywords(db, site, newKeyword);
                }
                else if (keywordType.ToString() == "n")
                {
                    Editor.AddNegativeKeywords(db, site, newKeyword);
                }
                else
                {
                    Console.WriteLine("Nothing will be changed");
                }
            }
            else if (action.ToString() == "r")
            {
                if (keywordType.ToString() == "p")
                {
                    Editor.RemovePositiveKeywords(db, site, newKeyword);
                }
                else if (keywordType.ToString() == "n")
                {
                    Editor.RemoveNegativeKeywords(db, site, newKeyword);
                }
                else
                {
                    Console.WriteLine("Nothing will be changed");
                }
            }
            else
            {
                Console.WriteLine("Cannot recognize the comand");
            }

            Console.ReadKey();
        }
    }
}
