using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Threading.Tasks;
using WebCalculator.Models;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;

namespace WebCalculatorAPI.Controllers
{
    [EnableCors("AllowFrontend")]
    [Route("api/[controller]")]
    [ApiController]
    public class SignUpController : ControllerBase
    {
        private readonly string _stripeSecretKey = "sk_test_51P8zCSRwYPgJLv8zxrJkRCE4aZsqnnrpMalmRAtGAbwd1Q7RWvx4fXOBNHfwn4tw8jEd61w4hYPOHwhR9j5acB6p00RiCl2lKl"; // Replace with your Stripe secret key

        public SignUpController()
        {
            StripeConfiguration.ApiKey = _stripeSecretKey; // Set your API key
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserSignUpRequest request)
        {

            var options = new PaymentIntentCreateOptions
            {
                Amount = 1000,
                Currency = "aud",
                PaymentMethod = "pm_card_visa",
                Confirm = true,
                ReturnUrl = "http://localhost:3000/"
            };

            var service = new PaymentIntentService();


            try
            {
                var paymentIntent = service.Create(options);

                // If the PaymentIntent status is succeeded, the payment was successful
                if (paymentIntent.Status == "succeeded")
                {
                    // Save user data
                    var userData = new
                    {
                        Name = request.Name,
                        Email = request.Email,
                        LastFourDigits = request.CardNumber.Substring(request.CardNumber.Length - 4),
                        ExpiryDate = request.Expiration
                    };

                    // Generate auto-generated password
                    var password = GenerateRandomPassword();

                    // Save user data to JSON file
                    SaveUserDataToJson(userData, password);

                    // Send welcome email
                    SendWelcomeEmail(request.Email, password);

                    return Ok(new { message = "Sign-up successful! Payment of $10 processed." });
                }
                else
                {
                    // Confirm the PaymentIntent to complete the payment
                    var confirmOptions = new PaymentIntentConfirmOptions
                    {
                        PaymentMethod = "pm_card_visa" // Use the provided payment method
                    };

                    var confirmedPaymentIntent = service.Confirm(paymentIntent.Id, confirmOptions);

                    // If the confirmed PaymentIntent status is succeeded, the payment was successful
                    if (confirmedPaymentIntent.Status == "succeeded")
                    {
                        // Save user data and payment details to the database
                        // Implement your database logic here

                        // Return a success response
                        return Ok(new { message = "Sign-up successful! Payment of $10 processed." });
                    }
                    else
                    {
                        // Payment was not successful
                        return BadRequest("Payment failed.");
                    }
                }
            }
            catch (StripeException ex)
            {
                // Handle Stripe errors
                return BadRequest($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private void SendWelcomeEmail(string email, string password)
        {
            // Send welcome email with auto-generated password
            var smtpClient = new SmtpClient("your.smtp.server.com")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential("your-email@example.com", "your-password"),
                EnableSsl = true,
            };

            smtpClient.Send("your-email@example.com", email, "Welcome to Our Service", $"Your auto-generated password is: {password}");
        }

        private void SaveUserDataToJson(dynamic userData, string password)
        {
            // Serialize user data and save to JSON file
            var json = JsonConvert.SerializeObject(new
            {
                Name = userData.Name,
                Email = userData.Email,
                LastFourDigits = userData.LastFourDigits,
                ExpiryDate = userData.ExpiryDate,
                Password = password
            });

            // Save JSON data to file
            var filePath = "user_data.json";
            using (StreamWriter streamWriter = new StreamWriter(filePath))
            {
                streamWriter.Write(json);
            }
        }

        private string GenerateRandomPassword()
        {
            // Generate a random password of length 8
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

}
