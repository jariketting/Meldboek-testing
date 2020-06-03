using System;
using System.Collections.Generic;
using System.Text;
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

        private int randomId;
        private string firstName;
        private string lastName;
        private string email;
        private string passwd;

        public DatabaseTests()
        {
            randomId = Faker.NumberFaker.Number();
            firstName = Faker.NameFaker.FirstName();
            lastName = Faker.NameFaker.LastName();
            email = firstName + lastName + "@test.com";
            passwd = firstName + lastName + "!";

            Db = new Database();
            AddTestPerson();
        }

        private void AddTestPerson()
        {
            _ = Db.ConnectDb("CREATE (p:Person { PersonId: " + randomId + ", FirstName: '" + firstName + "', LastName: '" + lastName+ "', Email: '" + email + "', Password: '" + passwd + "' })");
            System.Threading.Thread.Sleep(500);
        }

        [Fact]
        public void GetValueDatabase_correctQuery()
        {
            var getNodes = Db.ConnectDb("MATCH (p:Person) WHERE p.PersonId = " + randomId + " RETURN p LIMIT 1");
            var user = new Person();

            List<INode> nodes = getNodes.Result;
            foreach (var record in nodes)
            {
                var nodeprops = JsonConvert.SerializeObject(record.As<INode>().Properties);
                user = (JsonConvert.DeserializeObject<Person>(nodeprops));
            }

            Assert.Equal(firstName, user.FirstName);
            Assert.Equal(lastName, user.LastName);
            Assert.Equal(email, user.Email);
            Assert.Equal(passwd, user.Password);
        }

        [Fact]
        public void GetValueDatabase_wrongQuery()
        {
            var getNodes = Db.ConnectDb("MATCH (p:Persons) WHERE p.PersonId = " + randomId + " RETURN p LIMIT 1");
            var user = new Person();

            List<INode> nodes = getNodes.Result;
            foreach (var record in nodes)
            {
                var nodeprops = JsonConvert.SerializeObject(record.As<INode>().Properties);
                user = (JsonConvert.DeserializeObject<Person>(nodeprops));
            }

            Assert.Null(user.FirstName);
            Assert.Null(user.LastName);
            Assert.Null(user.Email);
            Assert.Null(user.Password);
        }
    }
}
