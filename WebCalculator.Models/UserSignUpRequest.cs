namespace WebCalculator.Models
{
    public class UserSignUpRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? CardNumber { get; set; }
        public string? Expiration { get; set; }
        public string? Cvc { get; set; }
        public string? Country { get; set; }
    }
}
