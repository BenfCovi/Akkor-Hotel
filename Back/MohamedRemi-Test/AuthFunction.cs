using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using static MohamedRemi_Test.UserCrud;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace MohamedRemi_Test
{
    public class AuthFunction
    {
        #region Constructor
        static AuthFunction()
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MongoDBConnection"));
            var database = client.GetDatabase("SocialMediaDB");
            _usersCollection = database.GetCollection<User>("users");
        }
        #endregion

        #region Attributs
        private static IMongoCollection<User> _usersCollection;
        #endregion

        #region Fonctions

        [FunctionName("AuthFunction")]
        public static async Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request for authentication.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JObject.Parse(requestBody);
            string username = data?.username;
            string password = data?.password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return new BadRequestResult();
            }

            var filter = Builders<User>.Filter.Eq(u => u.Email, username);
            var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();

            if (user != null)
            {
                var hashedPassword = HashPassword(password);
                if (user.PasswordHash == hashedPassword)
                {
                    // Generate a token
                    var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(username));
                    return new OkObjectResult(new { token = token });
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

        #endregion
    }
}