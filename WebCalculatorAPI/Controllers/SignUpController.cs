using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Stripe;
using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using WebCalculator.Models;

namespace WebCalculatorAPI.Controllers
{
    [EnableCors("AllowFrontend")]
    [Route("api/[controller]")]
    [ApiController]
    public class SignUpController : ControllerBase
    {
        private readonly string _stripeSecretKey = "sk_test_51P8zCSRwYPgJLv8zxrJkRCE4aZsqnnrpMalmRAtGAbwd1Q7RWvx4fXOBNHfwn4tw8jEd61w4hYPOHwhR9j5acB6p00RiCl2lKl"; // Replace with your Stripe secret key
        private readonly string _stripePriceId = "price_1P9wbtRwYPgJLv8zcfowMqnx"; // Replace with your Stripe price ID for the monthly subscription

        public SignUpController()
        {
            StripeConfiguration.ApiKey = _stripeSecretKey;
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserSignUpRequest request)
        {
            try
            {

                // Create payment method
                var paymentMethodOptions = new PaymentMethodCreateOptions
                {
                    Type = "card",
                    Card = new PaymentMethodCardOptions
                    {
                        Token = "tok_visa"
                    },
                };

                var paymentMethodService = new PaymentMethodService();
                var paymentMethod = paymentMethodService.Create(paymentMethodOptions);


                // Create customer on Stripe
                var customerOptions = new CustomerCreateOptions
                {
                    Email = request.Email,
                    Name = request.Name,
                    Balance = 5,
                    PaymentMethod = paymentMethod.Id,
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = paymentMethod.Id // Also set the default payment method for invoices, if needed
                    }

                    // Add more customer options as needed
                };

                var customerService = new CustomerService();
                var customer = customerService.Create(customerOptions);


                

               

                // Attach payment method to the customer
                var attachOptions = new PaymentMethodAttachOptions
                {
                    Customer = customer.Id,
                };

                var paymentMethodAttached = paymentMethodService.Attach(paymentMethod.Id, attachOptions);

                StripeConfiguration.ApiKey = _stripeSecretKey;



                var options = new SubscriptionCreateOptions
                {
                    Customer = customer.Id,
                    Items = new List<SubscriptionItemOptions>
                    {
                     new SubscriptionItemOptions { Price = _stripePriceId },
                    },
                };
                var service = new SubscriptionService();
                var subscription = service.Create(options);

                var suoptions = new SubscriptionUpdateOptions
                {
                    PaymentSettings = new SubscriptionPaymentSettingsOptions
                    {
                        PaymentMethodTypes = new List<string> { "card", "au_becs_debit" },
                    },
                };
                var service1 = new SubscriptionService();
                service1.Update(subscription.Id, suoptions);



                // Save user data and subscription details
                var userData = new
                {
                    Name = request.Name,
                    Email = request.Email,
                    LastFourDigits = request.CardNumber.Substring(request.CardNumber.Length - 4),
                    ExpiryDate = request.Expiration
                };

                SaveUserDataToJson(userData, customer.Id);

                // Send welcome email
               // SendWelcomeEmail(request.Email);

                return Ok(new { message = "Sign-up successful! Subscription created." });
            }
            catch (StripeException ex)
            {
                return BadRequest($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private void SendWelcomeEmail(string email)
        {
            var smtpClient = new SmtpClient("your.smtp.server.com")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential("your-email@example.com", "your-password"),
                EnableSsl = true,
            };

            smtpClient.Send("your-email@example.com", email, "Welcome to Our Service", "Thank you for signing up!");
        }

        private void SaveUserDataToJson(dynamic userData, string subscriptionId)
        {
            var json = JsonConvert.SerializeObject(new
            {
                Name = userData.Name,
                Email = userData.Email,
                LastFourDigits = userData.LastFourDigits,
                ExpiryDate = userData.ExpiryDate,
                SubscriptionId = subscriptionId
            });

            var filePath = "user_data.json";
            using (StreamWriter streamWriter = new StreamWriter(filePath))
            {
                streamWriter.Write(json);
            }
        }
    }
}
