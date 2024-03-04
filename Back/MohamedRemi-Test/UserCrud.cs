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

        // D�finition de l'�num�ration UserRole
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

            // R�cup�rer le nom d'utilisateur ou l'ID � partir du token
            // La mani�re exacte de faire cela d�pend de votre impl�mentation du token
            string userId = ExtractUserIdFromToken(token); // Cette fonction doit �tre impl�ment�e par vos soins

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

            // V�rifier si l'utilisateur demandeur a le droit de mettre � jour les informations
            if (userFromDb.Id != updateUser.Id && userFromDb.Role != UserRole.Admin)
            {
                return new UnauthorizedResult();
            }

            // Mise � jour de l'utilisateur
            // ...

            return new OkObjectResult(updateUser);
        }

        // Fonctions stock�es -----------------------------------------------------------------
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
                // D�coder le token de Base64
                var decodedBytes = Convert.FromBase64String(token);
                var decodedUsername = Encoding.UTF8.GetString(decodedBytes);

                // V�rifier si un utilisateur avec ce username existe dans la base de donn�es
                var filter = Builders<User>.Filter.Eq(u => u.Email, decodedUsername);
                var user = _usersCollection.Find(filter).FirstOrDefault();

                // Si un utilisateur existe avec ce username, le token est consid�r� comme valide
                return user != null;
            }
            catch
            {
                // En cas d'erreur lors du d�codage ou de la recherche, le token est invalide
                return false;
            }
        }
        private static string ExtractUserIdFromToken(string token)
        {
            // Extrait l'identifiant de l'utilisateur � partir du token
            // La logique exacte d�pend de la structure de votre token
            return ""; // Retourner l'ID utilisateur extrait du token
        }
        #endregion
    }
}