using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using static MohamedRemi_Test.UserCrud;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.IO;
using System.Threading.Tasks;

namespace MohamedRemi_Test
{
    public class RoomCrud
    {
        #region Constructor
        static RoomCrud()
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MongoDBConnection"));
            var database = client.GetDatabase("akkorDB");
            _roomsCollection = database.GetCollection<Room>("rooms");
        }
        #endregion

        #region Classe 
        public class Room
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }
            public string name { get; set; }
            public int capacity { get; set; }
            public string description { get; set; }
        }
        #endregion

        #region Attributs
        private static IMongoCollection<Room> _roomsCollection;
        #endregion

        #region Fonctions

        [FunctionName("CreateRoom")]
        public static async Task<IActionResult> Post([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var room = Newtonsoft.Json.JsonConvert.DeserializeObject<Room>(requestBody);

            await _roomsCollection.InsertOneAsync(room);

            return new OkObjectResult(room);
        }

        [FunctionName("GetRoom")]
        public static async Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            string roomId = req.Query["id"];

            var room = await _roomsCollection.Find(Builders<Room>.Filter.Eq("_id", new ObjectId(roomId))).FirstOrDefaultAsync();

            if (room == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(room);
        }

        [FunctionName("UpdateRoom")]
        public static async Task<IActionResult> Put([HttpTrigger(AuthorizationLevel.Function, "put", Route = null)] HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedRoom = Newtonsoft.Json.JsonConvert.DeserializeObject<Room>(requestBody);

            var updateResult = await _roomsCollection.ReplaceOneAsync(
                filter: Builders<Room>.Filter.Eq("_id", new ObjectId(updatedRoom.Id)),
                replacement: updatedRoom);

            if (updateResult.IsAcknowledged && updateResult.ModifiedCount > 0)
            {
                return new OkObjectResult(updatedRoom);
            }

            return new NotFoundResult();
        }

        [FunctionName("DeleteRoom")]
        public static async Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req, ILogger log)
        {
            string roomId = req.Query["id"];

            var deleteResult = await _roomsCollection.DeleteOneAsync(
                filter: Builders<Room>.Filter.Eq("_id", new ObjectId(roomId)));

            if (deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0)
            {
                return new OkResult();
            }

            return new NotFoundResult();
        }


        #endregion
    }
}