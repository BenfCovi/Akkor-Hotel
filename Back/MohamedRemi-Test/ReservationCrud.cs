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
using static MohamedRemi_Test.RoomCrud;

namespace MohamedRemi_Test
{
    public class ReservationCrud
    {
        #region Constructor
        static ReservationCrud()
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MongoDBConnection"));
            var database = client.GetDatabase("SocialMediaDB");
            _reservationsCollection = database.GetCollection<Reservation>("reservations");
        }
        #endregion

        #region Classe 
        public class Reservation
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }
            public UserCrud.User User { get; set; }
            public HotelCrud.Hotel Hotel { get; set; }
            public int NumberOfPeople { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
        #endregion

        #region Attributs
        private static IMongoCollection<Reservation> _reservationsCollection;
        #endregion

        #region Fonctions

        [FunctionName("CreateReservation")]
        public static async Task<IActionResult> Create([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("Creating a new reservation.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var reservation = JsonConvert.DeserializeObject<Reservation>(requestBody);

            // Vérification des dates
            if (reservation.StartDate >= reservation.EndDate)
            {
                return new BadRequestObjectResult("La date de début doit être antérieure à la date de fin.");
            }

            // Vérifier la capacité de la chambre
            if (reservation.NumberOfPeople > reservation.Hotel.Capacity)
            {
                return new BadRequestObjectResult("Le nombre de personnes dépasse la capacité maximale de la chambre.");
            }

            // Vérification de la disponibilité de la chambre (implémentez cette fonction basée sur la logique fournie précédemment)
            bool available = await IsRoomAvailable(reservation.Hotel.Id, reservation.StartDate, reservation.EndDate);
            if (!available)
            {
                return new BadRequestObjectResult("La chambre n'est pas disponible pour les dates sélectionnées.");
            }

            await _reservationsCollection.InsertOneAsync(reservation);
            return new OkObjectResult(reservation);
        }

        [FunctionName("GetReservation")]
        public static async Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Function, "get", Route = "reservation/{id}")] HttpRequest req, ILogger log, string id)
        {
            log.LogInformation("Getting a reservation by id.");

            var reservation = await _reservationsCollection.Find<Reservation>(r => r.Id == id).FirstOrDefaultAsync();

            if (reservation == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(reservation);
        }

        [FunctionName("UpdateReservation")]
        public static async Task<IActionResult> Put( [HttpTrigger(AuthorizationLevel.Function, "put", Route = "reservation/{id}")] HttpRequest req, ILogger log, string id)
        {
            log.LogInformation("Updating a reservation.");

            var reservation = await _reservationsCollection.Find<Reservation>(r => r.Id == id).FirstOrDefaultAsync();
            if (reservation == null)
            {
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedReservation = JsonConvert.DeserializeObject<Reservation>(requestBody);
            updatedReservation.Id = reservation.Id; // Ensure the ID is not changed

            await _reservationsCollection.ReplaceOneAsync(r => r.Id == id, updatedReservation);

            return new OkObjectResult(updatedReservation);
        }

        [FunctionName("DeleteReservation")]
        public static async Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "reservation/{id}")] HttpRequest req, ILogger log, string id)
        {
            log.LogInformation("Deleting a reservation.");

            var deleteResult = await _reservationsCollection.DeleteOneAsync(r => r.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new NotFoundResult();
            }

            return new OkResult();
        }

        private static async Task<bool> IsRoomAvailable(string roomId, DateTime startDate, DateTime endDate)
        {
            var filterBuilder = Builders<Reservation>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(reservation => reservation.Hotel.Id, roomId),
                filterBuilder.Or(
                    filterBuilder.And(
                        filterBuilder.Lte(reservation => reservation.StartDate, startDate),
                        filterBuilder.Gte(reservation => reservation.EndDate, startDate)
                    ),
                    filterBuilder.And(
                        filterBuilder.Lte(reservation => reservation.StartDate, endDate),
                        filterBuilder.Gte(reservation => reservation.EndDate, endDate)
                    ),
                    filterBuilder.And(
                        filterBuilder.Gte(reservation => reservation.StartDate, startDate),
                        filterBuilder.Lte(reservation => reservation.EndDate, endDate)
                    )
                )
            );

            var existingReservations = await _reservationsCollection.CountDocumentsAsync(filter);
            return existingReservations == 0;
        }
        #endregion
    }
}
