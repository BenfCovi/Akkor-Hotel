using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using System.Net;
using static MohamedRemi_Test.HotelCrud;
using static MohamedRemi_Test.UserCrud;
using static MohamedRemi_Test.ReservationCrud;

namespace MohamedRemi_Test
{
    public class ReservationCrud
    {
        #region Constructor
        static ReservationCrud()
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MongoDBConnection"));
            var database = client.GetDatabase("akkorDB");
            _reservationsCollection = database.GetCollection<Reservation>("reservations");
        }
        #endregion

        #region Classe 
        public class Reservation
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }
            public string IdUser { get; set; }
            public string IdHotel { get; set; }
            public int NumberOfRoom { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
        #endregion

        #region Attributs
        private static IMongoCollection<Reservation> _reservationsCollection;
        #endregion

        #region Fonctions

        // CRUD FONCTION CREATE - GET - UPDATE - DELETE (BEARER AUTHORISATION)

        // FONCTION CreateReservation (CRUD)
        [FunctionName("CreateReservation")]
        [OpenApiOperation(operationId: "createReservation", tags: new[] { "Reservation" }, Summary = "Create a reservation")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Reservation), Required = true, Description = "Reservation creation request object")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(Reservation), Description = "The created reservation object")]
        [OpenApiSecurity("Bearer",
                         SecuritySchemeType.Http, Name = "authorization",
                         Scheme = OpenApiSecuritySchemeType.Bearer, In = OpenApiSecurityLocationType.Header,
                         BearerFormat = "JWT")]
        public static async Task<IActionResult> CreateReservation(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Reservation/Create")] HttpRequest req,
            ILogger log)
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

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Reservation reservation = JsonConvert.DeserializeObject<Reservation>(requestBody);

            if (reservation == null || string.IsNullOrEmpty(reservation.IdUser) || string.IsNullOrEmpty(reservation.IdHotel))
            {
                return new BadRequestObjectResult("Hotel data is missing or incorrect.");
            }

            // Vérification des dates
            if (reservation.StartDate >= reservation.EndDate)
            {
                return new BadRequestObjectResult("The start date must be before the end date.");
            }
            //Verification de la disponibilitée des chambres
            var hotelResponse = await HotelCrud.Get(req, reservation.IdHotel, log);
            var hotelResult = hotelResponse as OkObjectResult;

            if (hotelResult != null && hotelResult.Value != null)
            {
                var hotel = hotelResult.Value as HotelCrud.Hotel;
                if (hotel != null)
                {
                    // Vérification de la disponibilité des chambres
                    var maxReservedRoomsResponse = await GetMaxReservedRooms(req, reservation.IdHotel, log);
                    var maxReservedRoomsResult = maxReservedRoomsResponse as OkObjectResult;

                    if (maxReservedRoomsResult != null && maxReservedRoomsResult.Value != null)
                    {
                        dynamic maxReservedRoomsValue = maxReservedRoomsResult.Value;
                        int maxReservedRooms = maxReservedRoomsValue.MaxReservedRooms;

                        if (maxReservedRooms + reservation.NumberOfRoom > hotel.Capacity)
                        {
                            return new BadRequestObjectResult("Not enough available rooms for this reservation.");
                        }
                    }
                    else
                    {
                        return maxReservedRoomsResponse; // Retourne la réponse telle quelle (UnauthorizedResult ou StatusCodeResult)
                    }
                }
                else
                {
                    return new BadRequestObjectResult("Invalid hotel data.");
                }
            }
            else
            {
                return hotelResponse; // Retourne la réponse telle quelle (UnauthorizedResult ou StatusCodeResult)
            }

            if (reservation.Id != "")
            {
                var existingHotel = await _reservationsCollection.Find(h => h.Id == reservation.Id).FirstOrDefaultAsync();
                if (existingHotel != null)
                {
                    return new BadRequestObjectResult("Hotel is already.");
                }
            }
            await _reservationsCollection.InsertOneAsync(reservation);
            return new CreatedResult("Reservation", reservation);

        }

        // FONCTION GetReservationById (CRUD)
        [FunctionName("GetReservationById")]
        [OpenApiOperation(operationId: "getReservationById", tags: new[] { "Reservation" }, Summary = "Get a reservation with your ID")]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The ID of the reservation to retrieve")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Reservation), Description = "The requested reservation object")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Not found if the reservation with the specified ID does not exist")]
        [OpenApiSecurity("Bearer",
                         SecuritySchemeType.Http, Name = "authorization",
                         Scheme = OpenApiSecuritySchemeType.Bearer, In = OpenApiSecurityLocationType.Header,
                         BearerFormat = "JWT")]
        public static async Task<IActionResult> GetReservationById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Reservation/Get/{id}")] HttpRequest req,
            string id,
            ILogger log)
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

            var reservation = await _reservationsCollection.Find<Reservation>(r => r.Id == id).FirstOrDefaultAsync();
            if (reservation == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(reservation);
        }

        // FONCTION UpdateReservation (CRUD)
        [FunctionName("UpdateReservation")]
        [OpenApiOperation(operationId: "updateReservation", tags: new[] { "Reservation" }, Summary = "Update a reservation")]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The ID of the reservation to update")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Reservation), Required = true, Description = "Reservation update request object")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Reservation), Description = "The updated reservation object")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Not found if the reservation with the specified ID does not exist")]
        [OpenApiSecurity("Bearer",
                         SecuritySchemeType.Http, Name = "authorization",
                         Scheme = OpenApiSecuritySchemeType.Bearer, In = OpenApiSecurityLocationType.Header,
                         BearerFormat = "JWT")]
        public static async Task<IActionResult> UpdateReservation(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "Reservation/Update/{id}")] HttpRequest req,
            string id,
            ILogger log)
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

            var reservationToUpdate = await _reservationsCollection.Find<Reservation>(r => r.Id == id).FirstOrDefaultAsync();
            if (reservationToUpdate == null)
            {
                return new NotFoundResult();
            }

            // Vérification des dates
            if (reservationToUpdate.StartDate >= reservationToUpdate.EndDate)
            {
                return new BadRequestObjectResult("The start date must be before the end date.");
            }
            //Verification de la disponibilitée des chambres
            var hotelResponse = await HotelCrud.Get(req, reservationToUpdate.IdHotel, log);
            var hotelResult = hotelResponse as OkObjectResult;

            if (hotelResult != null && hotelResult.Value != null)
            {
                var hotel = hotelResult.Value as HotelCrud.Hotel;
                if (hotel != null)
                {
                    // Vérification de la disponibilité des chambres
                    var maxReservedRoomsResponse = await GetMaxReservedRooms(req, reservationToUpdate.IdHotel, log);
                    var maxReservedRoomsResult = maxReservedRoomsResponse as OkObjectResult;

                    if (maxReservedRoomsResult != null && maxReservedRoomsResult.Value != null)
                    {
                        dynamic maxReservedRoomsValue = maxReservedRoomsResult.Value;
                        int maxReservedRooms = maxReservedRoomsValue.MaxReservedRooms;

                        if (maxReservedRooms + reservationToUpdate.NumberOfRoom > hotel.Capacity)
                        {
                            return new BadRequestObjectResult("Not enough available rooms for this reservation.");
                        }
                    }
                    else
                    {
                        return maxReservedRoomsResponse; // Retourne la réponse telle quelle (UnauthorizedResult ou StatusCodeResult)
                    }
                }
                else
                {
                    return new BadRequestObjectResult("Invalid hotel data.");
                }
            }
            else
            {
                return hotelResponse; // Retourne la réponse telle quelle (UnauthorizedResult ou StatusCodeResult)
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedReservation = JsonConvert.DeserializeObject<Reservation>(requestBody);
            updatedReservation.Id = reservationToUpdate.Id; // Ensure the ID is not changed

            await _reservationsCollection.ReplaceOneAsync(r => r.Id == id, updatedReservation);

            return new OkObjectResult(updatedReservation);
        }

        // FONCTION DeleteReservation (CRUD)
        [FunctionName("DeleteReservation")]
        [OpenApiOperation(operationId: "deleteReservation", tags: new[] { "Reservation" }, Summary = "Delete a reservation with its id")]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The ID of the reservation to delete")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "OK if the reservation is successfully deleted")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Not found if the reservation with the specified ID does not exist")]
        [OpenApiSecurity("Bearer",
                         SecuritySchemeType.Http, Name = "authorization",
                         Scheme = OpenApiSecuritySchemeType.Bearer, In = OpenApiSecurityLocationType.Header,
                         BearerFormat = "JWT")]
        public static async Task<IActionResult> DeleteReservation(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "Reservation/Delete/{id}")] HttpRequest req,
            string id,
            ILogger log)
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

            var deleteResult = await _reservationsCollection.DeleteOneAsync(r => r.Id == id);
            if (deleteResult.DeletedCount == 0)
            {
                return new NotFoundResult();
            }

            return new OkResult();
        }

        // FONCTION GetAllRestervations (qui recupere tout les Reservations) (BEARER AUTHORISATION)
        [FunctionName("GetAllReservations")]
        [OpenApiOperation(operationId: "getAllReservations", tags: new[] { "Reservation" }, Summary = "Get all reservations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Reservation>), Description = "The list of all reservations")]
        [OpenApiSecurity("Bearer",
                         SecuritySchemeType.Http, Name = "authorization",
                         Scheme = OpenApiSecuritySchemeType.Bearer, In = OpenApiSecurityLocationType.Header,
                         BearerFormat = "JWT")]
        public static async Task<IActionResult> GetAllReservations(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Reservation/GetAll")] HttpRequest req,
            ILogger log)
        {

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

                var reservations = await _reservationsCollection.Find(r => true).ToListAsync();

                if (reservations == null || reservations.Count == 0)
                {
                    return new NotFoundResult();
                }

                return new OkObjectResult(reservations);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while getting all reservations.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

        }

        // FONCTION GetAllReservationsByUser (qui recupere tout les Reservations d'un utilisateur) (BEARER AUTHORISATION)
        [FunctionName("GetAllReservationsByUser")]
        [OpenApiOperation(operationId: "getAllReservationsByUser", tags: new[] { "Reservation" }, Summary = "Get all reservations that are taken by a user")]
        [OpenApiParameter(name: "userId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The ID of the user")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Reservation>), Description = "The list of reservations made by the user")]
        [OpenApiSecurity("Bearer",
                         SecuritySchemeType.Http, Name = "authorization",
                         Scheme = OpenApiSecuritySchemeType.Bearer, In = OpenApiSecurityLocationType.Header,
                         BearerFormat = "JWT")]
        public static async Task<IActionResult> GetAllReservationsByUser(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Reservation/GetAllByUser/{userId}")] HttpRequest req,
            string userId,
            ILogger log)
        {
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

                var reservations = await _reservationsCollection.Find(r => r.IdUser == userId).ToListAsync();

                if (reservations == null || reservations.Count == 0)
                {
                    return new NotFoundResult();
                }

                return new OkObjectResult(reservations);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while getting all reservations.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

        }

        // FONCTION GetAllReservationsByHotel (qui recupere tout les Reservations d'un Hotel)  (BEARER AUTHORISATION)
        [FunctionName("GetAllReservationsByHotel")]
        [OpenApiOperation(operationId: "getAllReservationsByHotel", tags: new[] { "Reservation" }, Summary = "Get all hotel reservations")]
        [OpenApiParameter(name: "hotelId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The ID of the hotel")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Reservation>), Description = "The list of reservations made for the hotel")]
        [OpenApiSecurity("Bearer",
                         SecuritySchemeType.Http, Name = "authorization",
                         Scheme = OpenApiSecuritySchemeType.Bearer, In = OpenApiSecurityLocationType.Header,
                         BearerFormat = "JWT")]
        public static async Task<IActionResult> GetAllReservationsByHotel(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Reservation/GetAllByHotel/{hotelId}")] HttpRequest req,
            string hotelId,
            ILogger log)
        {
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

                var reservations = await _reservationsCollection.Find(r => r.IdHotel == hotelId).ToListAsync();

                if (reservations == null)
                {
                    return new NotFoundResult();
                }

                return new OkObjectResult(reservations);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while getting all reservations.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
        #endregion
    }
}
