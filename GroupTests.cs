using meldboek;
using meldboek.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace XUnitTestMeldboek
{
    public class GroupTests
    {
        Database Db { get; set; }

        // person
        private int personId; // random id of user
        private string firstName; // first name
        private string lastName;  // last name
        private string email; // email
        private string passwd; // password

        // group
        private int groupId; // random id of group
        private string groupName; // name if group

        public GroupTests()
        {
            personId = Faker.NumberFaker.Number(); // create random id for user
            firstName = Faker.NameFaker.FirstName(); // create random fist name
            lastName = Faker.NameFaker.LastName(); // create random last name
            email = firstName + lastName + "@test.com"; // build email with first and last name
            passwd = firstName + lastName + "!"; // build password with first and last name

            groupId = Faker.NumberFaker.Number(); // create random id of group
            groupName = Faker.StringFaker.Alpha(6); // create random name for group

            Db = new Database(); // init database
            AddTestPerson(); // add test person
            AddTestGroup(); // add test group
        }

        /// <summary>
        /// Add test person to databae with random generated naming
        /// </summary>
        private void AddTestPerson()
        {
            // run query
            _ = Db.ConnectDb("CREATE (p:Person { PersonId: " + personId + ", FirstName: '" + firstName + "', LastName: '" + lastName + "', Email: '" + email + "', Password: '" + passwd + "' })");
            System.Threading.Thread.Sleep(500); // prevent some issues
        }

        /// <summary>
        /// Add test group with random name and id
        /// </summary>
        private void AddTestGroup()
        {
            _ = Db.ConnectDb("CREATE (g:Group {GroupId: " + groupId + ", GroupName: '" + groupName + "' }) RETURN g");
        }

        /// <summary>
        /// Add person to group with correct values
        /// </summary>
        [Fact]
        public void AddPersonToGroup_correctValues()
        {
            PersonController controller = new PersonController();

            var actionResult = controller.AddPersonToGroup(personId, groupId) as RedirectToActionResult;

            Assert.NotNull(actionResult);
            Assert.Equal("ManageGroup", actionResult.ActionName);
        }
    }
}
