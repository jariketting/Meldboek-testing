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

        public DatabaseTests()
        {
            Db = new Database();
            AddTestPerson();
        }

        private void AddTestPerson()
        {
            _ = Db.ConnectDb("CREATE (p:Person { PersonId: 123456789, FirstName: 'testFirstName', LastName: 'testLastName', Email: 'test@email.com', Password: 'testPassword' })");
            System.Threading.Thread.Sleep(500);
        }

        [Fact]
        public void GetValueDatabase_correctQuery()
        {
            var getNodes = Db.ConnectDb("MATCH (p:Person) WHERE p.PersonId = 123456789 RETURN p LIMIT 1");
            var user = new Person();

            List<INode> nodes = getNodes.Result;
            foreach (var record in nodes)
            {
                var nodeprops = JsonConvert.SerializeObject(record.As<INode>().Properties);
                user = (JsonConvert.DeserializeObject<Person>(nodeprops));
            }

            Assert.Equal("testFirstName", user.FirstName);
            Assert.Equal("testLastName", user.LastName);
            Assert.Equal("test@email.com", user.Email);
            Assert.Equal("testPassword", user.Password);
        }

        [Fact]
        public void GetValueDatabase_wrongQuery()
        {
            var getNodes = Db.ConnectDb("MATCH (p:Persons) WHERE p.PersonId = 123456789 RETURN p LIMIT 1");
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
