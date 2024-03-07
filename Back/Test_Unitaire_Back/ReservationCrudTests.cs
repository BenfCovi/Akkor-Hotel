using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using MohamedRemi_Test;
using System.Text;
using static MohamedRemi_Test.HotelCrud;
using static MohamedRemi_Test.ReservationCrud;
using Moq;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using static Microsoft.Azure.Amqp.CbsConstants;
using System.Text.RegularExpressions;
using static MohamedRemi_Test.UserCrud;

namespace Test_Unitaire_Back
{
    public class ReservationCrudTests
    {
        private readonly ILogger logger = new Logger<UserCrudTests>(new LoggerFactory());

        public interface IUserRepository
        {
            Task<List<Reservation>> GetAllByHotel(int id);
        }

        [Fact]
        public async Task CrudCycle_GetAll()
        {
            // Arrange: Cr�er un utilisateur de test
            string IdHotel = "65e9c3eba849f06efe6aa579"; 
            // Simuler un token valide
            string validToken = "bWltaWQxNTEx";

            // Act: R�cup�rer l'utilisateur cr�� en incluant le token dans la requ�te
            var getAllRequest = new DefaultHttpContext().Request;
            getAllRequest.Headers["Authorization"] = $"Bearer {validToken}";
            var getAllResponse = await ReservationCrud.GetAllReservationsByHotel(getAllRequest, IdHotel, logger);

            // Assert: V�rifier que l'utilisateur est r�cup�r� avec succ�s
            var retrievedHotelResult = Assert.IsType<OkObjectResult>(getAllResponse);
            Assert.Equal((int)HttpStatusCode.OK, retrievedHotelResult.StatusCode);
        }
    }
}
