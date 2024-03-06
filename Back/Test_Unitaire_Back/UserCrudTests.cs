using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using MohamedRemi_Test;
using System.Text;
using static MohamedRemi_Test.UserCrud;
using Moq;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using static Microsoft.Azure.Amqp.CbsConstants;
using System.Text.RegularExpressions;

namespace Test_Unitaire_Back
{
    public class UserCrudTests
    {
        private readonly ILogger logger = new Logger<UserCrudTests>(new LoggerFactory());

        public interface IUserRepository
        {
            Task<User> CreateUserAsync(User user);
            Task<User> FindByIdAsync(string userId);
            Task<User> FindByEmailAsync(string email);
            Task<User> UpdateAsync(string userId, User updatedUser);
            Task<bool> DeleteUserAsync(string userId);
        }

        [Fact]
        public async Task CrudCycle_CreateGetUpdateDelete_UserSuccessfullyHandled()
        {
            // Arrange: Créer un utilisateur de test
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "password",
                Role = UserRole.Customer,
                Hotels = new List<string> { "fds7897sdfs", "dsq457f39sd" }
            };

            // Act: Créer l'utilisateur
            var createUserRequest = new DefaultHttpContext().Request;
            createUserRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(user)));
            var createUserResponse = await UserCrud.Post(createUserRequest, logger);

            // Assert: Vérifier que l'utilisateur est créé avec succès
            var createdResult = Assert.IsType<CreatedResult>(createUserResponse);
            Assert.Equal((int)HttpStatusCode.Created, createdResult.StatusCode);
            var createdUser = Assert.IsType<User>(createdResult.Value);

            // Simuler un token valide
            string validToken = "bWltaWQxNTEx";

            // Act: Récupérer l'utilisateur créé en incluant le token dans la requête
            var getUserRequest = new DefaultHttpContext().Request;
            getUserRequest.Headers["Authorization"] = $"Bearer {validToken}";
            var getUserResponse = await UserCrud.GetUser(getUserRequest, createdUser.Id, logger);

            // Assert: Vérifier que l'utilisateur est récupéré avec succès
            var retrievedUserResult = Assert.IsType<OkObjectResult>(getUserResponse);
            Assert.Equal((int)HttpStatusCode.OK, retrievedUserResult.StatusCode);
            var retrievedUser = Assert.IsType<User>(retrievedUserResult.Value);
            Assert.Equal(createdUser.Id, retrievedUser.Id);

            // Update user
            // Modify some properties of the user
            createdUser.Email = "updated@example.com";
            createdUser.Role = UserRole.Admin;
            createdUser.Hotels = new List<string> { "updatedHotelId" };

            // Act: Update user with the authentication token
            var updateUserRequest = new DefaultHttpContext().Request;
            updateUserRequest.Headers["Authorization"] = $"Bearer {validToken}";
            updateUserRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(createdUser)));
            var updateUserResponse = await UserCrud.UpdateUser(updateUserRequest, createdUser.Id, logger);

            // Assert: Vérifier que l'utilisateur est mis à jour avec succès
            var updatedUserResult = Assert.IsType<OkObjectResult>(updateUserResponse);
            Assert.Equal((int)HttpStatusCode.OK, updatedUserResult.StatusCode);
            var updatedUser = Assert.IsType<User>(updatedUserResult.Value);
            Assert.Equal(createdUser.Id, updatedUser.Id);

            // Act: Delete user with the authentication token
            var deleteUserRequest = new DefaultHttpContext().Request;
            deleteUserRequest.Headers["Authorization"] = $"Bearer {validToken}";
            var deleteUserResponse = await UserCrud.DeleteUser(deleteUserRequest, createdUser.Id, logger);

            // Assert: Vérifier que l'utilisateur est supprimé avec succès
            var deletedUserResult1 = Assert.IsType<OkResult>(deleteUserResponse);
            Assert.Equal((int)HttpStatusCode.OK, deletedUserResult1.StatusCode);

            // Act: Get user after deletion
            var deletedUserRequest = new DefaultHttpContext().Request;
            deletedUserRequest.Headers["Authorization"] = $"Bearer {validToken}";
            var deletedUserResponse = await UserCrud.GetUser(deletedUserRequest, createdUser.Id, logger);

            // Assert: Verify that no user is retrieved after deletion
            var deletedUserResult2 = Assert.IsType<NotFoundResult>(deletedUserResponse);
            Assert.Equal((int)HttpStatusCode.NotFound, deletedUserResult2.StatusCode);
        }

        [Theory]
        [InlineData("pass", "d74ff0ee8da3b9806b18c877dbf29bbde50b5bd8e4dad7a3a725000feb82e8f1")] // Test avec un mot de passe simple
        [InlineData("PassWord69008&&", "c82b87eea1d40d488efb79a3c7b6cbf4cbfc8d1a8306c20fb2c63aa3e21dd806")] // Test avec un mot de passe plus complexe
        [InlineData("", "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")] // Test avec un mot de passe vide
        public void HashPassword_ValidInput_ReturnsHashedPassword(string password, string expectedHashedPassword)
        {
            // Act
            string hashedPassword = UserCrud.HashPassword(password);

            // Assert
            Assert.Equal(expectedHashedPassword, hashedPassword);
        }

        [Theory]
        [InlineData("bWltaWQxNTEx", "mimid1511")] // Test avec un token valide
        [InlineData("invalid_token", null)] // Test avec un token invalide
        [InlineData("", "")] // Test avec un token vide
        public void ExtractEmailFromToken_ValidAndInvalidTokens_ReturnsExpectedResult(string token, string expectedEmail)
        {
            // Act
            string extractedEmail = UserCrud.ExtractEmailFromToken(token);
            // Assert
            Assert.Equal(expectedEmail, extractedEmail);
        }

        //TEST TDD
        [Theory]
        [InlineData("test@example.com", true)] // Email valide
        [InlineData("invalid_email", false)] // Email invalide
        [InlineData("another@example.com", true)] // Email valide
        public void IsValidEmail_ValidAndInvalidEmails_ReturnsExpectedResult(string email, bool expectedResult)
        {
            // Act
            bool isValid = IsValidEmail(email);

            // Assert
            Assert.Equal(expectedResult, isValid);
        }
        public static bool IsValidEmail(string email)
        {
            // Utilisation d'une expression régulière pour valider l'email
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(email);
        }

    }
}
