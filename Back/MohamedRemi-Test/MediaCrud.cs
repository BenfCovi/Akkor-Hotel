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
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using static MohamedRemi_Test.ReservationCrud;
using static MohamedRemi_Test.UserCrud;
using Azure.Storage.Blobs;

namespace MohamedRemi_Test
{
    public class MediaCrud
    {
        #region Constructor
        static MediaCrud()
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MongoDBConnection"));
            var database = client.GetDatabase("akkorDB");
            _mediasCollection = database.GetCollection<Media>("medias");
            _usersCollection = database.GetCollection<User>("users");
        }
        #endregion

        #region Classe
            public class Media
            {
                [BsonId]
                [BsonRepresentation(BsonType.ObjectId)]
                public string Id { get; set; }
                public string Type { get; set; }
                public string Url { get; set; }
                public DateTime Timestamp { get; set; }
            }
            public class MediaRequest
            {
                public string Id { get; set; }
            }
        #endregion

        #region Attributs
        private static IMongoCollection<Media> _mediasCollection;
            private static IMongoCollection<User> _usersCollection;
        #endregion

        #region Fonctions

        [FunctionName("CreateMedia")]
        public static async Task<IActionResult> Post([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("CreateMedia function processed a request.");

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

            var formdata = await req.ReadFormAsync();
            var file = req.Form.Files["file"];
            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("No file was uploaded.");
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var blobContainerClient = blobServiceClient.GetBlobContainerClient("media");
            await blobContainerClient.CreateIfNotExistsAsync();

            var blobClient = new BlobClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "media", fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            var mediaUrl = blobClient.Uri.ToString();

            var media = new Media
            {
                Type = file.ContentType.StartsWith("image/") ? "image" : "video",
                Url = mediaUrl,
                Timestamp = DateTime.UtcNow
            };

            await _mediasCollection.InsertOneAsync(media);

            return new OkObjectResult(new { mediaUrl, mediaType = media.Type });
        }

        [FunctionName("GetMedia")]
        public static async Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("GetMedia function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var requestData = JsonConvert.DeserializeObject<MediaRequest>(requestBody);
            if (requestData == null || string.IsNullOrEmpty(requestData.Id))
            {
                return new BadRequestObjectResult("Media ID is missing or incorrect.");
            }

            var media = await _mediasCollection.Find(m => m.Id == requestData.Id).FirstOrDefaultAsync();
            if (media == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(media);
        }

        [FunctionName("UpdateMedia")]
        public static async Task<IActionResult> Put([HttpTrigger(AuthorizationLevel.Function, "put", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("UpdateMedia function processed a request.");

            var formData = await req.ReadFormAsync();
            var file = req.Form.Files["file"];
            string mediaId = req.Form["id"];

            if (string.IsNullOrEmpty(mediaId) || file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("Media ID or file is missing or incorrect.");
            }

            var mediaToUpdate = await _mediasCollection.Find(m => m.Id == mediaId).FirstOrDefaultAsync();
            var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));

            if (mediaToUpdate != null && !string.IsNullOrEmpty(mediaToUpdate.Url))
            {
                var blobUriBuilder = new BlobUriBuilder(new Uri(mediaToUpdate.Url));
                var oldBlobClient = blobServiceClient.GetBlobContainerClient(blobUriBuilder.BlobContainerName).GetBlobClient(blobUriBuilder.BlobName);
                await oldBlobClient.DeleteIfExistsAsync();
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var containerClient = blobServiceClient.GetBlobContainerClient("media");
            var newBlobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await newBlobClient.UploadAsync(stream, overwrite: true);
            }

            var mediaUrl = newBlobClient.Uri.ToString();

            var updateDefinition = Builders<Media>.Update
                .Set(m => m.Url, mediaUrl)
                .Set(m => m.Timestamp, DateTime.UtcNow);

            var filter = Builders<Media>.Filter.Eq(m => m.Id, mediaId);
            var updateResult = await _mediasCollection.UpdateOneAsync(filter, updateDefinition);

            if (updateResult.MatchedCount == 0)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(new { Id = mediaId, Url = mediaUrl });
        }

        [FunctionName("DeleteMedia")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("DeleteMedia function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var mediaData = JsonConvert.DeserializeObject<MediaRequest>(requestBody);
            string mediaId = mediaData?.Id;

            if (string.IsNullOrEmpty(mediaId))
            {
                return new BadRequestObjectResult("Media ID is missing or incorrect.");
            }

            var mediaToDelete = await _mediasCollection.Find(m => m.Id == mediaId).FirstOrDefaultAsync();
            if (mediaToDelete != null)
            {
                if (!string.IsNullOrEmpty(mediaToDelete.Url))
                {
                    var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
                    var blobUriBuilder = new BlobUriBuilder(new Uri(mediaToDelete.Url));
                    var blobContainerClient = blobServiceClient.GetBlobContainerClient(blobUriBuilder.BlobContainerName);
                    var blobClient = blobContainerClient.GetBlobClient(blobUriBuilder.BlobName);

                    await blobClient.DeleteIfExistsAsync();
                }

                var deleteFilter = Builders<Media>.Filter.Eq(m => m.Id, mediaId);
                await _mediasCollection.DeleteOneAsync(deleteFilter);
            }
            else
            {
                return new NotFoundResult();
            }

            return new OkResult();
        }

        #endregion    
    }
}
