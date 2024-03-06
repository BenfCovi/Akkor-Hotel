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
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Net;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using static MohamedRemi_Test.HotelCrud;
using System.Threading;
using System.Collections.Generic;

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
            public List<string> Hotels { get; set; }
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
        [OpenApiOperation(operationId: "createUser", tags: new[] { "User" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(User), Required = true, Description = "User creation request object")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(User), Description = "The created user object, ID will be generated automatically")]
        public static async Task<IActionResult> Post([HttpTrigger(AuthorizationLevel.Function, "post", Route = "User/Create")] HttpRequest req, ILogger log)
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
        [OpenApiOperation(operationId: "getUser", tags: new[] { "User" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The ID of the user to retrieve")]
        [OpenApiSecurity("Bearer",
                         SecuritySchemeType.Http, Name = "authorization",
                         Scheme = OpenApiSecuritySchemeType.Bearer, In = OpenApiSecurityLocationType.Header,
                         BearerFormat = "JWT")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(User), Description = "The requested user object")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Description = "Unauthorized if token is invalid or expired")]
        public static async Task<IActionResult> GetUser(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "User/Get/{id}")] HttpRequest req,
            string id,
            ILogger log)
        {
            log.LogInformation("GetUser function processed a request.");
            Console.WriteLine(id);

            try
            {
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

                // Convert string id to ObjectId
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    // Handle case where id is not a valid ObjectId
                    return new BadRequestObjectResult("Invalid ObjectId format.");
                }

                // Find user by ObjectId
                User user = await _usersCollection.Find(u => u.Id == objectId.ToString()).FirstOrDefaultAsync();

                if (user == null)
                {
                    return new NotFoundResult();
                }

                // Return the user object
                return new OkObjectResult(user);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred in GetUser function.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [FunctionName("GetUserByEmail")]
        [OpenApiOperation(operationId: "GetUserByEmail", tags: new[] { "User" })]
        [OpenApiParameter(name: "email", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The ID of the user to retrieve")]
        [OpenApiSecurity("Bearer",
                    SecuritySchemeType.Http, Name = "authorization",
                    Scheme = OpenApiSecuritySchemeType.Bearer, In = OpenApiSecurityLocationType.Header,
                    BearerFormat = "JWT")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(User), Description = "The requested user object")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Description = "Unauthorized if token is invalid or expired")]
        public static async Task<IActionResult> GetUserByEmail(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = "User/Get/Email/{email}")] HttpRequest req,
       string email,
       ILogger log)
        {
            log.LogInformation("GetUser function processed a request.");
            Console.WriteLine(email);

            try
            {
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


                User user = await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();

                if (user == null)
                {
                    return new NotFoundResult();
                }

                // Return the user object
                return new OkObjectResult(user);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred in GetUser function.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }


        [FunctionName("GetAllUsers")]
        [OpenApiOperation(operationId: "getAllUsers", tags: new[] { "User" })]
        [OpenApiSecurity("Bearer",
                     SecuritySchemeType.Http, Name = "authorization",
                     Scheme = OpenApiSecuritySchemeType.Bearer, In = OpenApiSecurityLocationType.Header,
                     BearerFormat = "JWT")]
        [OpenApiResponseWithBody(statusCode : HttpStatusCode.OK, "List of users", typeof(IEnumerable<User>))]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Description = "Unauthorized if token is invalid or expired")]
        public static async Task<IActionResult> GetAllUsers([HttpTrigger(AuthorizationLevel.Function, "get", Route = "User/GetAll")] HttpRequest req,  ILogger log)
        {
            log.LogInformation("GetAllUsers function processed a request.");

            try
            {
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

                // Retrieve all users from the database
                List<User> users = await _usersCollection.Find(_ => true).ToListAsync();

                if (users == null || users.Count == 0)
                {
                    return new NotFoundResult();
                }

                // Return an HTTP response with the list of users
                return new OkObjectResult(users);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred in GetAllUsers function.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }



        [FunctionName("DeleteUser")]
        [OpenApiOperation(operationId: "deleteUser", tags: new[] { "User" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The ID of the user to retrieve")]
        [OpenApiSecurity("Bearer",
                     SecuritySchemeType.Http, Name = "authorization",
                     Scheme = OpenApiSecuritySchemeType.Bearer, In = OpenApiSecurityLocationType.Header,
                     BearerFormat = "JWT")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "User deleted successfully")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Description = "Unauthorized if token is invalid or expired")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Description = "Bad request if user ID is missing or incorrect")]
        public static async Task<IActionResult> DeleteUser(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "User/Delete/{id}")] HttpRequest req,
        string id,
        ILogger log)
        {
            log.LogInformation("DeleteUser function processed a request.");

            try
            {
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

                // Convert string id to ObjectId
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    // Handle case where id is not a valid ObjectId
                    return new BadRequestObjectResult("Invalid ObjectId format.");
                }

                // Delete user by ObjectId
                var result = await _usersCollection.DeleteOneAsync(u => u.Id == objectId.ToString());

                if (result.DeletedCount == 0)
                {
                    return new NotFoundResult();
                }

                // Return success response
                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred in DeleteUser function.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [FunctionName("UpdateUser")]
        [OpenApiOperation(operationId: "updateUser", tags: new[] { "User" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The ID of the user to retrieve")]
        [OpenApiSecurity("Bearer",
                         SecuritySchemeType.Http, Name = "authorization",
                         Scheme = OpenApiSecuritySchemeType.Bearer, In = OpenApiSecurityLocationType.Header,
                         BearerFormat = "JWT")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(User), Required = true, Description = "User update request object")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(User), Description = "The updated user object")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Description = "Unauthorized if token is invalid or expired")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Description = "Bad request if user data is missing or incorrect")]
        public static async Task<IActionResult> UpdateUser(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "User/Update/{id}")] HttpRequest req,
            string id,
            ILogger log)
        {
            log.LogInformation("UpdateUser function processed a request.");

            try
            {
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

                // Convert string id to ObjectId
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    // Handle case where id is not a valid ObjectId
                    return new BadRequestObjectResult("Invalid ObjectId format.");
                }

                // Retrieve existing user from database
                var existingUser = await _usersCollection.Find(u => u.Id == objectId.ToString()).FirstOrDefaultAsync();
                if (existingUser == null)
                {
                    return new NotFoundResult();
                }

                // Read request body and deserialize into User object
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var updatedUser = JsonConvert.DeserializeObject<User>(requestBody);
                if (updatedUser == null || string.IsNullOrEmpty(updatedUser.Id))
                {
                    return new BadRequestObjectResult("User data is missing or incorrect.");
                }

                // Update user properties
                existingUser.Email = updatedUser.Email;
                existingUser.Role = updatedUser.Role;
                existingUser.Hotels = updatedUser.Hotels;


                // Save updated user to database
                await _usersCollection.ReplaceOneAsync(u => u.Id == objectId.ToString(), existingUser);

                // Return updated user
                return new OkObjectResult(existingUser);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred in UpdateUser function.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [FunctionName("GetUserIdFromToken")]
        [OpenApiOperation(operationId: "getUserIdFromToken", tags: new[] { "User" }, Summary = "Get user ID from authentication token", Description = "Extracts user ID from the provided authentication token.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The user ID extracted from the authentication token.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Description = "Bad request if the authentication header is missing or invalid.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Description = "Internal server error if an unexpected error occurs.")]
        [OpenApiParameter(name: "token", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The Token of the email to retrieve")]
        [OpenApiSecurity("Bearer",
                         SecuritySchemeType.Http, Name = "authorization",
                         Scheme = OpenApiSecuritySchemeType.Bearer, In = OpenApiSecurityLocationType.Header,
                         BearerFormat = "JWT")]
        public static IActionResult GetEmailFromToken([HttpTrigger(AuthorizationLevel.Function, "get", Route = "User/GetEmailFromToken/{token}")] HttpRequest req, string token, ILogger log)
        {
            log.LogInformation("GetEmailFromToken function processed a request.");

            try
            {
                // Récupérer le jeton d'authentification de la requête
                string authHeader = req.Headers["Authorization"];
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return new BadRequestObjectResult("Authorization header is missing or invalid.");
                }

                // Appeler la fonction pour extraire l'ID utilisateur du jeton
                string email = UserCrud.ExtractEmailFromToken(token);

                if (string.IsNullOrEmpty(email))
                {
                    return new BadRequestObjectResult("Failed to extract user ID from token.");
                }

                // Retourner l'ID utilisateur extrait
                return new OkObjectResult(email);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred in GetUserIdFromToken function.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
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
        public static bool ValidateToken(string token)
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
        public static string ExtractEmailFromToken(string token)
        {
            try
            {
                // Décode le jeton à partir de Base64
                var decodedBytes = Convert.FromBase64String(token);
                var decodedEmail = Encoding.UTF8.GetString(decodedBytes);

                return decodedEmail;
            }
            catch (Exception ex)
            {
                // En cas d'erreur, renvoie null ou gère l'erreur selon vos besoins
                Console.WriteLine($"Erreur lors de l'extraction de l'e-mail à partir du jeton : {ex.Message}");
                return null;
            }
        }
        #endregion
    }
}