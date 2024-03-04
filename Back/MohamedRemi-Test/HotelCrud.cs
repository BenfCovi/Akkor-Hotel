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
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Net;
using Microsoft.OpenApi.Models;

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
            public List<RoomCrud.Room> rooms { get; set; }
            //public int nbRoom { get; set; }

            // Rajouter un Hotel non obligatoire
        }
        #endregion

        #region Attributs
        private static IMongoCollection<Hotel> _hotelsCollection;
        #endregion

        #region Fonctions
        [FunctionName("CreateHotel")]
        [OpenApiOperation(operationId: "createHotel", tags: new[] { "Hotel" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Hotel), Required = true, Description = "Hotel creation request object")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Hotel), Description = "The created hotel object, ID will be generated automatically")]
        public static async Task<IActionResult> Post([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("Creating a new hotel entry.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var hotel = JsonConvert.DeserializeObject<Hotel>(requestBody);
            await _hotelsCollection.InsertOneAsync(hotel);

            return new OkObjectResult(hotel);
        }

        [FunctionName("GetHotel")]
        [OpenApiOperation(operationId: "getHotel", tags: new[] { "Hotel" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The ID of the hotel to retrieve")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Hotel), Description = "The requested hotel object")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Not found if the hotel with the specified ID does not exist")]
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
        //GetBestHotel (4 hotels qui ont le plus de reservations trier par les plus r�cents)


        [FunctionName("UpdateHotel")]
        [OpenApiOperation(operationId: "updateHotel", tags: new[] { "Hotel" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The ID of the hotel to update")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Hotel), Required = true, Description = "Hotel update request object")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Hotel), Description = "The updated hotel object")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Not found if the hotel with the specified ID does not exist")]
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
        [OpenApiOperation(operationId: "deleteHotel", tags: new[] { "Hotel" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The ID of the hotel to delete")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "OK if the hotel is successfully deleted")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Not found if the hotel with the specified ID does not exist")]
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