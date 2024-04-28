using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Stripe;
using System;
using System.Threading.Tasks;
using System.Web.Http.Results;
using WebCalculator.Models;
using WebCalculatorAPI.Controllers;
using Xunit;




namespace TestWebCalculatorApi
{
    public class SignUpControllerTests
    {
        private readonly IConfiguration _configuration;
        public SignUpControllerTests()
        {
            _configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile(@"appsettings.json", false, false)
               .AddEnvironmentVariables()
               .Build();
        }


        [Fact]
        public async Task SignUp_ValidRequest_ReturnsOkResult()
        {
                
            // Arrange
            //var configurationMock = new Mock<IConfiguration>();
            //configurationMock.SetupGet(x => x["StripeSettings:SecretKey"]).Returns("your_stripe_secret_key");
           // configurationMock.SetupGet(x => x["StripeSettings:PriceId"]).Returns("your_stripe_price_id");

            var paymentMethodServiceMock = new Mock<PaymentMethodService>();
            paymentMethodServiceMock.Setup(x => x.Create(It.IsAny<PaymentMethodCreateOptions>(),null)).Returns(new PaymentMethod { Id = "payment_method_id" });

            var customerServiceMock = new Mock<CustomerService>();
            var customer = new Customer { Id = "customer_id" };
            customerServiceMock.Setup(x => x.Create(It.IsAny<CustomerCreateOptions>(),null)).Returns(customer);

            var subscriptionServiceMock = new Mock<SubscriptionService>();
            subscriptionServiceMock.Setup(x => x.Create(It.IsAny<SubscriptionCreateOptions>(),null)).Returns(new Subscription { Id = "subscription_id" });

            var signUpController = new SignUpController(_configuration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await signUpController.SignUp(new UserSignUpRequest
            {
                Name = "John Doe",
                Email = "john@example.com",
                CardNumber = "4242424242424242", // Valid test card number for Stripe
                Expiration = "12/25"
                // Add other required properties for the request
            });

            // Assert
            // Check if the result is of type OkObjectResult
            Assert.IsType<OkObjectResult>(result);

            // Check the message in the response
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            dynamic responseData = okResult.Value;
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task SignUp_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            var signUpController = new SignUpController(configurationMock.Object);
            signUpController.ModelState.AddModelError("Email", "Email is required."); // Simulate ModelState error

            // Act
            var result = await signUpController.SignUp(new UserSignUpRequest());

            Assert.IsType<BadRequestObjectResult>(result);
        }

    }
}

        
