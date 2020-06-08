using System;
using System.Collections.Generic;
using meldboek;
using meldboek.Controllers;
using meldboek.Models;
using Neo4j.Driver;
using Newtonsoft.Json;
using Xunit;
using FakeHttpContext;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace XUnitTestMeldboek
{
    public class ForumControllerTest
    {
        Database Db { get; set; }

        private List<Person> PersonList; // personlist
        private ForumController FC = new ForumController();
        private Person LoggedInPerson;
        private Person FriendPerson;
        private List<Forum> ForumList = new List<Forum>();

        /// <summary>
        /// Init test with database connection and generate random names
        /// </summary>

        public ForumControllerTest()
        {



            Db = new Database(); // init database
            //Delete EVERYTHING first
            _ = Db.ConnectDb("MATCH (n) DETACH DELETE n");
            System.Threading.Thread.Sleep(500); // prevent some issues


            //make 50 Persons
            PersonList = new List<Person>();
            int count = 50;
            while(count != 0)
            {

                int randomId = Faker.NumberFaker.Number(); // create random id for user
                string firstName = Faker.NameFaker.FirstName(); // create random fist name
                string lastName = Faker.NameFaker.LastName(); // create random last name
                string email = firstName + lastName + "@test.com"; // build email with first and last name
                string passwd = firstName + lastName + "!"; // build password with first and last name
                
                PersonList.Add(new Person(randomId, firstName, lastName, email, passwd));
                AddTestPerson(new Person(randomId, firstName, lastName, email, passwd)); // add test person
                count -= 1;
            }


            //ingelogde persoon pakken
            LoggedInPerson = PersonList[1];
            PersonList.Remove(LoggedInPerson);
            FC.Testing = true;
            FC.TestPerson = LoggedInPerson;
            AddTestForums();




        }

        /// <summary>
        /// Add test person to databae with random generated naming
        /// </summary>
        private void AddTestPerson(Person p)
        {


            // run query
            var r = Db.ConnectDb("CREATE (p:Person { PersonId: " + p.PersonId + ", FirstName: '" + p.FirstName + "', LastName: '" + p.LastName + "', Email: '" + p.Email + "', Password: '" + p.Password + "' })");
            r.Wait();
        }
        private void AddTestForums()
        {
            int count = 10;
            while (count != 0)
            {
                Forum f = new Forum(FC.GetNewForumId(), LoggedInPerson, Faker.TextFaker.Sentence(), Faker.TextFaker.Sentences(2), Faker.DateTimeFaker.DateTime(DateTime.Now.AddDays(-200), DateTime.Now));



                ForumList.Add(f);
                FC.SaveForum(f);
                count -= 1;
            }
        }

        [Fact]
        public void SaveForumTest()
        {
            

            var r1 = Db.ConnectDb("MATCH (n:Forum) RETURN n");
            r1.Wait();
            Assert.True(r1.Result.Count == 10);

        }

        [Fact]
        public void LoadForumTest()
        {
            Forum f = FC.LoadForum(ForumList[1].ForumId);
            Assert.Equal(f.ForumId, ForumList[1].ForumId);

        }
        [Fact]
        public void GetAllForumsTest()
        {
            var r = FC.GetAllForums();
            Assert.True(r.Count == 10);

        }
        [Fact]
        public void DeleteForumTest()
        {
            FC.DeleteForum(ForumList[8]);
            var r = Db.ConnectDb("MATCH(n: Forum) where n.ForumId = " + ForumList[8].ForumId + " return n");
            r.Wait();
            Assert.True(r.Result.Count == 0);
        }
    }
}
