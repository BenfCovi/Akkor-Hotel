using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using static MohamedRemi_Test.UserCrud;
using System.Net;

namespace MohamedRemi_Test
{
    public class AuthFunction
    {
        private static IMongoCollection<User> _usersCollection;

        static AuthFunction()
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MongoDBConnection"));
            var database = client.GetDatabase("akkorDB");
            _usersCollection = database.GetCollection<User>("users");
        }

        [FunctionName("AuthFunction")]
        [OpenApiOperation(operationId: "authenticateUser", tags: new[] { "User Authentication" }, Summary = "Allows the authentication of a user and returns a Token")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(AuthRequest), Required = true, Description = "User authentication request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AuthResponse), Description = "The authentication token if authentication is successful")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Description = "Invalid request, missing username or password")]
        public static async Task<IActionResult> AuthenticateUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request for authentication.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JObject.Parse(requestBody);
            string email = data?.email;
            string password = data?.password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return new BadRequestResult();
            }

            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();

            if (user != null)
            {
                var hashedPassword = HashPassword(password);
                if (user.PasswordHash == hashedPassword)
                {
                    var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(email)); // Consider using a more secure token generation strategy
                    return new OkObjectResult(new AuthResponse { Token = token });
                }
            }

            return new BadRequestResult();
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public class AuthRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class AuthResponse
        {
            public string Token { get; set; }
        }
    }
}
