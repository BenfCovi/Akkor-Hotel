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
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MohamedRemi_Test
{
    public class HotelCrud
    {
        #region Constructor
        static HotelCrud()
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MongoDBConnection"));
            var database = client.GetDatabase("SocialMediaDB");
            _hotelsCollection = database.GetCollection<Hotel>("hotels");
        }
        #endregion

        #region Classe 
        public class Hotel
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }
            public string name { get; set; }
            public string location { get; set; }
            public string description { get; set; }
            public Dictionary<int, string> picture_list { get; set; }
            //public List<RoomCrud.Room> rooms { get; set; }
            public int nbRoom { get; set; }

            // Rajouter un Hotel non obligatoire
        }
        #endregion

        #region Attributs
        private static IMongoCollection<Hotel> _hotelsCollection;
        #endregion

        #region Fonctions
        [FunctionName("CreateHotel")]
        public static async Task<IActionResult> Post([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("Creating a new hotel entry.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var hotel = JsonConvert.DeserializeObject<Hotel>(requestBody);
            await _hotelsCollection.InsertOneAsync(hotel);

            return new OkObjectResult(hotel);
        }
        [FunctionName("GetHotel")]
        public static async Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Function, "get", Route = "Hotel/{id}")] HttpRequest req, string id, ILogger log)
        {
            log.LogInformation("Getting a hotel by id.");

            var hotel = await _hotelsCollection.Find<Hotel>(hotel => hotel.Id == id).FirstOrDefaultAsync();
            if (hotel == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(hotel);
        }
        //GetLastHotel (8 dernieres reservations)
        //GetBestHotel (4 hotels qui ont le plus de reservations trier par les plus récents)
        [FunctionName("UpdateHotel")]
        public static async Task<IActionResult> Put([HttpTrigger(AuthorizationLevel.Function, "put", Route = "Hotel/{id}")] HttpRequest req, string id, ILogger log)
        {
            log.LogInformation("Updating a hotel.");

            var hotelToUpdate = await _hotelsCollection.Find<Hotel>(h => h.Id == id).FirstOrDefaultAsync();
            if (hotelToUpdate == null)
            {
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedHotel = JsonConvert.DeserializeObject<Hotel>(requestBody);
            updatedHotel.Id = hotelToUpdate.Id; // Ensure the ID is not changed

            await _hotelsCollection.ReplaceOneAsync(h => h.Id == id, updatedHotel);

            return new OkObjectResult(updatedHotel);
        }
        [FunctionName("DeleteHotel")]
        public static async Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "Hotel/{id}")] HttpRequest req, string id, ILogger log)
        {
            log.LogInformation("Deleting a hotel.");

            var deleteResult = await _hotelsCollection.DeleteOneAsync(hotel => hotel.Id == id);
            if (deleteResult.DeletedCount == 0)
            {
                return new NotFoundResult();
            }

            return new OkResult();
        }


        #endregion
    }
}