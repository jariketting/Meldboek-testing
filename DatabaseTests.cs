using System;
using System.Collections.Generic;
using meldboek;
using meldboek.Models;
using Neo4j.Driver;
using Newtonsoft.Json;
using Xunit;

/// <summary>
/// 2.1.5 Als user wil ik dat mijn data wordt opgeslagen
/// 
/// D2 Connectie maken met de database en een waarde returnen(ConnectDb2())
/// </summary>
namespace XUnitTestMeldboek
{
    public class DatabaseTests
    {
        Database Db { get; set; }

        private int randomId; // random id of user
        private string firstName; // first name
        private string lastName;  // last name
        private string email; // email
        private string passwd; // password

        /// <summary>
        /// Init test with database connection and generate random names
        /// </summary>
        public DatabaseTests()
        {
            randomId = Faker.NumberFaker.Number(); // create random id for user
            firstName = Faker.NameFaker.FirstName(); // create random fist name
            lastName = Faker.NameFaker.LastName(); // create random last name
            email = firstName + lastName + "@test.com"; // build email with first and last name
            passwd = firstName + lastName + "!"; // build password with first and last name

            Db = new Database(); // init database
            AddTestPerson(); // add test person
        }

        /// <summary>
        /// Add test person to databae with random generated naming
        /// </summary>
        private void AddTestPerson()
        {
            // run query
            _ = Db.ConnectDb("CREATE (p:Person { PersonId: " + randomId + ", FirstName: '" + firstName + "', LastName: '" + lastName+ "', Email: '" + email + "', Password: '" + passwd + "' })");
            System.Threading.Thread.Sleep(500); // prevent some issues
        }

        /// <summary>
        /// Get person from database and check values
        /// </summary>
        [Fact]
        public void GetValueDatabase_correctQuery()
        {
            var getNodes = Db.ConnectDb("MATCH (p:Person) WHERE p.PersonId = " + randomId + " RETURN p LIMIT 1"); // get person node
            var user = new Person(); // create new user

            List<INode> nodes = getNodes.Result; // list with nodes
            foreach (var record in nodes)
            {
                // extract user from node
                var nodeprops = JsonConvert.SerializeObject(record.As<INode>().Properties); 
                user = (JsonConvert.DeserializeObject<Person>(nodeprops));
            }

            Assert.Equal(firstName, user.FirstName); // check if fist name is correct
            Assert.Equal(lastName, user.LastName); // check if last name is correct
            Assert.Equal(email, user.Email); // check if email is correct
            Assert.Equal(passwd, user.Password); // check if password is correct
        }

        /// <summary>
        /// Wrong database query, should return null values
        /// </summary>
        [Fact]
        public void GetValueDatabase_wrongQuery()
        {
            var getNodes = Db.ConnectDb("MATCH (p:Persons) WHERE p.PersonId = " + randomId + " RETURN p LIMIT 1"); // wrong query
            var user = new Person();

            List<INode> nodes = getNodes.Result; // list with nodes
            foreach (var record in nodes)
            {
                // extact potential user
                var nodeprops = JsonConvert.SerializeObject(record.As<INode>().Properties);
                user = (JsonConvert.DeserializeObject<Person>(nodeprops));
            }

            // check if all values are null (as they should)
            Assert.Null(user.FirstName);
            Assert.Null(user.LastName);
            Assert.Null(user.Email);
            Assert.Null(user.Password);
        }
    }
}
