using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Net;
using WebCalculator.Models;
using WebCalculatorAPI.Controllers;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace TestWebCalculatorApi
{
    public class SignInControllerTests
    {
        [Fact]
        public void SignIn_ValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var signInController = new SignInController();

            // Mocking ReadAllText method of File class
            
            // Act
            var result = signInController.SignIn(new UserSignInRequest
            {
                Email = "john@example.com",
                Password = "jayNax5n"
            });

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void SignIn_InvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var signInController = new SignInController();

            // Mocking ReadAllText method of File class
           

            // Act
            var result = signInController.SignIn(new UserSignInRequest
            {
                Email = "john@example.com",
                Password = "wrongpassword"
            });

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid email or password", badRequestResult.Value);
        }

        [Fact]
        public void SignIn_ExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange
            var signInController = new SignInController();

            // Mocking ReadAllText method of File class to throw exception
               // var fileMock = new Mock<FileBase>();
              // fileMock.Setup(x => x.ReadAllText("user_data.json")).Throws(new Exception("File not found"));
               // var fileSystemMock = new Mock<System.IO.Abstractions.IFileSystem>> ();
               //fileSystemMock.Setup(x => x.File).Returns(fileMock.Object);
               //signInController.FileSystem = fileSystemMock.Object;

            // Act
           var result = signInController.SignIn(null);

            // Assert
            var statusCodeResult = (ObjectResult)(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}
