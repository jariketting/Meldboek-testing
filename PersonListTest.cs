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
    public class PersonListTest
    {
        Database Db { get; set; }

        private List<Person> PersonList; // personlist
        private PersonController PC = new PersonController();
        private LoginController LC = new LoginController();
        private Person LoggedInPerson;
        private Person FriendPerson;

        /// <summary>
        /// Init test with database connection and generate random names
        /// </summary>

        public PersonListTest()
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
            PC.Testing = true;
            PC.TestPerson = LoggedInPerson;



            //Make two groups
            int GroupId = 1;
            Group g = new Group(GroupId, "Testgroep");

            var r = Db.ConnectDb("CREATE (g:Group {GroupId: " + g.GroupId + ", GroupName: '" + g.GroupName + "' }) RETURN g");

            r.Wait();


            //Attach logged in person to group as manager

            var r2 = Db.ConnectDb("MATCH (p:Person),(g:Group) WHERE p.PersonId = " + LoggedInPerson.PersonId + " AND g.GroupId = " + GroupId + " CREATE(p) -[r: IsOwner]->(g) RETURN p, g");
            r2.Wait();

            //Attach logged in person to group as member
            var r3 = Db.ConnectDb("MATCH (a:Person), (b:Group) WHERE a.PersonId = " + LoggedInPerson.PersonId + " AND b.GroupId = " + GroupId + " CREATE (a)-[r:IsInGroup]->(b)" + " RETURN a");
            r3.Wait();

            //give logged in person a friend
            FriendPerson = PersonList[2];
            var r4 = Db.ConnectDb("MATCH (a:Person), (b:Person) WHERE a.PersonId = " + LoggedInPerson.PersonId + " AND b.PersonId = " + FriendPerson.PersonId + " CREATE (a)-[r:IsFriendsWith]->(b)" + " RETURN a");

            r4.Wait();
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



        //Test for GetPersonlist and CheckFriendStatus
        [Fact]
        public void GetPersonlistTest()
        {

            List<PersonInfo> ResultList = new List<PersonInfo>();
            List<PersonInfo> Checklist = PC.GetPersonlist();

            foreach (Person p in PersonList)
            {
                PersonInfo PI = new PersonInfo();
                PI.Person = p;
                PI.Status = PC.CheckFriendStatus(p.PersonId);
                ResultList.Add(PI);
            }

            foreach(PersonInfo PI in ResultList)
            {



                Boolean found = false;
                foreach(PersonInfo PI2 in Checklist)
                {
                    if (PI.Person.Email.Equals(PI2.Person.Email))
                    {
                        found = true;
                    }
                }
                Assert.True(found);

                
                    
                
                
            }
            foreach (PersonInfo PI in Checklist)
            {
                Boolean found = false;
                foreach (PersonInfo PI2 in ResultList)
                {
                    if (PI.Person.Email.Equals(PI2.Person.Email))
                    {
                        found = true;
                    }
                }
                Assert.True(found);
            }


        }

        [Fact]
        public void GetMaxPersonIdTest()
        {
            List<int> NumberList = new List<int>();
            int count = 100;
            while (count != 0)
            {
                int rNumber = PC.GetMaxPersonId();
                if (NumberList.Contains(rNumber))
                {
                    Assert.True(false);
                }
                else{
                    Assert.True(true);
                }

                NumberList.Add(rNumber);
                count -= 1;
            }
            

        }

        [Fact]
        public void GetPersonTest()
        {
            Person Testperson = PC.GetPerson(LoggedInPerson.PersonId);
            Assert.Equal(Testperson.Email, LoggedInPerson.Email);
        }

        [Fact]
        public void DeleteFriendProfileTest()
        {
             var r = PC.DeleteFriendProfile(FriendPerson.PersonId);
            r.Wait();

            var r2 = Db.ConnectDb("MATCH (a:Person {PersonId: " + LoggedInPerson.PersonId + "})-[r:IsFriendsWith]->(b:Person {PersonId: " + FriendPerson.PersonId + "}) return a");
            var result = r2.Result;
            Assert.True(result.Count == 0);



        }

        [Fact]
        public void RequestFriendTest()
        {
            PC.AddFriend(LoggedInPerson.PersonId, FriendPerson.PersonId);
            string s = "MATCH (a:Person {PersonId: " + LoggedInPerson.PersonId + "})-[r:FriendPending]->(b:Person {PersonId: " + FriendPerson.PersonId + "}) return a";
            var r2 = Db.ConnectDb("MATCH (a:Person {PersonId: " + LoggedInPerson.PersonId + "})-[r:FriendPending]->(b:Person {PersonId: " + FriendPerson.PersonId + "}) return a");;
            r2.Wait();
            var result = r2.Result;
            Assert.True(result.Count == 1);
        }

        [Fact]
        public void RefuseFriendReqTest()
        {

           var r = PC.RefuseFriendReq(LoggedInPerson.PersonId,null);
            r.Wait();
            var r2 = Db.ConnectDb("MATCH (a:Person {PersonId: " + LoggedInPerson.PersonId + "})-[r:FriendPending]->(b:Person {PersonId: " + FriendPerson.PersonId + "}) return a"); ;
            r2.Wait();
            var result = r2.Result;
            Assert.True(result.Count == 0);


        }

        [Fact]
        public void GetFriendsTest()
        {



            var r1 = Db.ConnectDb("MATCH (a:Person), (b:Person) WHERE a.PersonId = " + LoggedInPerson.PersonId + " AND b.PersonId = " + PersonList[30].PersonId + " CREATE (a)-[r:IsFriendsWith]->(b)" + " RETURN a");

            r1.Wait();

            var r2 = Db.ConnectDb("MATCH (a:Person), (b:Person) WHERE a.PersonId = " + LoggedInPerson.PersonId + " AND b.PersonId = " + PersonList[31].PersonId + " CREATE (a)-[r:IsFriendsWith]->(b)" + " RETURN a");

            r2.Wait();

            var r3 = Db.ConnectDb("MATCH (a:Person), (b:Person) WHERE a.PersonId = " + LoggedInPerson.PersonId + " AND b.PersonId = " + PersonList[32].PersonId + " CREATE (a)-[r:IsFriendsWith]->(b)" + " RETURN a");

            r3.Wait();

            var result = PC.GetFriends();
            Assert.True(result.Count == 3);
        }
    }
}
