using meldboek;
using meldboek.Controllers;
using meldboek.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace XUnitTestMeldboek
{
    public class NewsTests
    {
        Database Db { get; set; }
        PersonController personController { get; set; }

        private string title; // store test title
        private string description; // store test description
        private string group = "Algemeen"; // group type

        /// <summary>
        /// Init test with database and test data
        /// </summary>
        public NewsTests()
        {
            Db = new Database(); // init database
            personController = new PersonController();

            title = Faker.StringFaker.Alpha(6);
            description = Faker.TextFaker.Sentence();
        }

        /// <summary>
        /// Clean feeds from database
        /// </summary>
        private void CleanFeed()
        {
            _ = Db.ConnectDb("MATCH (n:Post) DELETE n"); // delete all posts
            System.Threading.Thread.Sleep(500); // prevent some issues
        }

        private void AddTestPost()
        {
            _ = personController.AddPost(title, description, group, null);
            System.Threading.Thread.Sleep(500); // prevent some issues
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void GetFeed_nofeedfound()
        {
            CleanFeed(); // clean all feeds (empty database)

            List<Newspost> feed = personController.GetFeed(); // get news feed

            Assert.Empty(feed);
        }

        [Fact]
        public void GetFeed_feedFound()
        {
            AddTestPost(); // add test post to database

            List<Newspost> feed = personController.GetFeed(); // get news feed

            Newspost testPost = feed.First();

            Assert.Equal(title, testPost.Title); // test if title is correct
            Assert.Equal(description, testPost.Description); // test if post is correct
        }
    }
}
