using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using System.Security.Cryptography;
using System.Text;

namespace MohamedRemi_Test
{
    public static class UserCrud
    {
        #region Constructor
        static UserCrud()
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MongoDBConnection"));
            var database = client.GetDatabase("SocialMediaDB");
            _usersCollection = database.GetCollection<User>("users");
        }
        #endregion

        #region Classe 
        public class User
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }
            public string PasswordHash { get; set; }
            public string Email { get; set; }
            public UserRole Role { get; set; }
        }

        // Définition de l'énumération UserRole
        public enum UserRole
        {
            Admin,
            Employee,
            Customer
        }
        #endregion

        #region Attributs
        private static IMongoCollection<User> _usersCollection;
        #endregion

        #region Fonctions

        [FunctionName("CreateUser")]
        public static async Task<IActionResult> Post([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("CreateUser function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var user = JsonConvert.DeserializeObject<User>(requestBody);

            if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.PasswordHash))
            {
                return new BadRequestObjectResult("User data is missing or incorrect.");
            }

            var existingUser = await _usersCollection.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return new BadRequestObjectResult("Email is already in use.");
            }

            // Hash the password before saving the user to the database
            user.PasswordHash = UserCrud.HashPassword(user.PasswordHash);

            await _usersCollection.InsertOneAsync(user);
            return new CreatedResult("User", user);
        }

        [FunctionName("GetUser")]
        public static async Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("GetUser function processed a request.");

            // Token validation
            string authHeader = req.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return new UnauthorizedResult();
            }

            string token = authHeader.Substring("Bearer ".Length).Trim();
            if (!ValidateToken(token))
            {
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var requestUser = JsonConvert.DeserializeObject<User>(requestBody);

            if (requestUser == null || string.IsNullOrEmpty(requestUser.Id))
            {
                return new BadRequestObjectResult("User ID is missing or incorrect.");
            }

            var filter = Builders<User>.Filter.Eq(u => u.Id, requestUser.Id);
            var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();

            if (user == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(user);
        }

        [FunctionName("DeleteUser")]
        public static async Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("DeleteUser function processed a request.");

            // Token validation
            string authHeader = req.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return new UnauthorizedResult();
            }

            string token = authHeader.Substring("Bearer ".Length).Trim();
            if (!ValidateToken(token))
            {
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var requestUser = JsonConvert.DeserializeObject<User>(requestBody);

            if (requestUser == null || string.IsNullOrEmpty(requestUser.Id))
            {
                return new BadRequestObjectResult("User ID is missing or incorrect.");
            }

            var filter = Builders<User>.Filter.Eq(u => u.Id, requestUser.Id);
            var result = await _usersCollection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                return new NotFoundResult();
            }

            return new OkResult();
        }
        [FunctionName("UpdateUser")]
        public static async Task<IActionResult> Put([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("UpdateUser function processed a request.");

            string authHeader = req.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return new UnauthorizedResult();
            }

            string token = authHeader.Substring("Bearer ".Length).Trim();
            if (!ValidateToken(token))
            {
                return new UnauthorizedResult();
            }

            // Récupérer le nom d'utilisateur ou l'ID à partir du token
            // La manière exacte de faire cela dépend de votre implémentation du token
            string userId = ExtractUserIdFromToken(token); // Cette fonction doit être implémentée par vos soins

            var userFromDb = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (userFromDb == null)
            {
                return new UnauthorizedResult(); // Ou NotFound, selon la logique d'application
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updateUser = JsonConvert.DeserializeObject<User>(requestBody);
            if (updateUser == null || string.IsNullOrEmpty(updateUser.Id))
            {
                return new BadRequestObjectResult("User data is missing or incorrect.");
            }

            // Vérifier si l'utilisateur demandeur a le droit de mettre à jour les informations
            if (userFromDb.Id != updateUser.Id && userFromDb.Role != UserRole.Admin)
            {
                return new UnauthorizedResult();
            }

            // Mise à jour de l'utilisateur
            // ...

            return new OkObjectResult(updateUser);
        }

        // Fonctions stockées -----------------------------------------------------------------
        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
        private static bool ValidateToken(string token)
        {
            try
            {
                // Décoder le token de Base64
                var decodedBytes = Convert.FromBase64String(token);
                var decodedUsername = Encoding.UTF8.GetString(decodedBytes);

                // Vérifier si un utilisateur avec ce username existe dans la base de données
                var filter = Builders<User>.Filter.Eq(u => u.Email, decodedUsername);
                var user = _usersCollection.Find(filter).FirstOrDefault();

                // Si un utilisateur existe avec ce username, le token est considéré comme valide
                return user != null;
            }
            catch
            {
                // En cas d'erreur lors du décodage ou de la recherche, le token est invalide
                return false;
            }
        }
        private static string ExtractUserIdFromToken(string token)
        {
            // Extrait l'identifiant de l'utilisateur à partir du token
            // La logique exacte dépend de la structure de votre token
            return ""; // Retourner l'ID utilisateur extrait du token
        }
        #endregion
    }
}