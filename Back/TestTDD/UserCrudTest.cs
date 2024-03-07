using Microsoft.AspNetCore.Http;
using MohamedRemi_Test;

namespace TestTDD
{
    [TestClass]
    public class UserCrudTest
    {
        [TestMethod]
        public void ExtractEmailFromToken_ValidToken_ReturnsDecodedEmail()
        {
            // Arrange
            var token = "bWltaWQxNTEx"; // Base64 encoded string "This is a token of the token decoded email."

            // Act
            var result = UserCrud.ExtractEmailFromToken(token) ;

            // Assert
            Assert.AreEqual(result, "mimid1511"); // Remplacer "decoded email." par l'e-mail attendu après le décodage du token.
        }
    }
}