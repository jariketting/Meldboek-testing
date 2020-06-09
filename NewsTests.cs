using meldboek;
using meldboek.Controllers;
using meldboek.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace XUnitTestMeldboek
{
    public class NewsTests
    {
        Database Db { get; set; }

        /// <summary>
        /// Init test with database and test data
        /// </summary>
        public NewsTests()
        {
            Db = new Database(); // init database

            CleanFeed(); // clean all feeds (empty database)
        }

        /// <summary>
        /// Clean feeds from database
        /// </summary>
        private void CleanFeed()
        {
            _ = Db.ConnectDb("MATCH (n:Post) DELETE n"); // delete all posts
            System.Threading.Thread.Sleep(500); // prevent some issues
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void GetFeed_nofeedfound()
        {
            PersonController controller = new PersonController();

            List<Newspost> feed = controller.GetFeed(); // get news feed

            Assert.Empty(feed);
        }
    }
}
