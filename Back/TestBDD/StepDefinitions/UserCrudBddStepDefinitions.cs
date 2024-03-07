using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MohamedRemi_Test;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Net;
using System.Text;
using static MohamedRemi_Test.UserCrud;
using Moq;

namespace TestBDD.StepDefinitions
{
    [Binding]
    public sealed class UserCrudBddStepDefinitions
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly Microsoft.Extensions.Logging.ILogger logger = new Logger<UserCrudBddStepDefinitions>(new LoggerFactory());

        private static User _user;
        private static IActionResult _createUserResponse;


        public interface IUserRepository
        {
            Task<User> CreateUserAsync(User user);
            Task<User> FindByIdAsync(string userId);
            Task<User> FindByEmailAsync(string email);
            Task<User> UpdateAsync(string userId, User updatedUser);
            Task<bool> DeleteUserAsync(string userId);
        }

        [Test]
        [Given("an unregistered user")]
        public void GivenAnUnregisteredUser()
        {
            // Implement logic to set up an unregistered user
            _user = new User
            {
                Email = "test.BDD@example.com",
                PasswordHash = "password",
                Role = UserRole.Customer,
                Hotels = new List<string> { "fds7897sdfs", "dsq457f39sd" }
            };
        }

        [Test]
        [When("I add a new user with valid email and password")]
        public async Task WhenIAddANewUserWithValidEmailAndPassword()
        {
            // Implement logic to add a new user with valid email and password

            // Act: Créer l'utilisateur
            var createUserRequest = new DefaultHttpContext().Request;
            createUserRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_user)));
            _createUserResponse = await UserCrud.Post(createUserRequest, logger);
        }

        [Test]
        [Then("the user is successfully created in the system")]
        public void ThenTheUserIsSuccessfullyCreatedInTheSystem()
        {
            Assert.IsInstanceOf<CreatedResult>(_createUserResponse);
        }
    }
}