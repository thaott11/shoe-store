using Shoe_Store_HandleAPI.Models;
using System.Net.Mail;
using System.Net;

namespace Shoe_Store_HandleAPI.Service
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ModelDbContext _db;
        public EmailService(IConfiguration config, ModelDbContext db)
        {
            _config = config;
            _db = db;
        }

        public async Task SendEmail(string email, int clientId)
        {
            var smtpHost = _config["SmtpSettings:Host"];
            var smtpPort = int.Parse(_config["SmtpSettings:Port"]);
            var smtpEnableSsl = bool.Parse(_config["SmtpSettings:EnableSsl"]);

            var fromAddress = new MailAddress("admin@example.com");
            var toAddress = new MailAddress(email);
            var mailMessage = new MailMessage
            {
                From = fromAddress,
                Subject = "Account Verification",
                Body = $@"
        <div style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; text-align: center;'>
            <div style='background-color: white; padding: 30px; border-radius: 10px; max-width: 600px; margin: 0 auto; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);'>
                <h2 style='color: #2c3e50;'>Welcome to Shoe Store!</h2>
                <p>Thank you for joining our community. We are delighted to have you as part of our team.</p>
                <p>Your account has been created with the following details:</p>
                <ul style='list-style: none; padding: 0;'>
                    <li><strong>Username:</strong> {email}</li>
                </ul>
                <p>Please follow the link below to verify your account and proceed with your next tasks:</p>
                <a href='https://localhost:7279/{clientId}' style='display: inline-block; padding: 10px 20px; color: white; background-color: #3498db; text-decoration: none; border-radius: 5px;'>Verify Email</a>
                <p style='margin-top: 20px;'>This link will expire in 5 minutes for your security.</p>
                <p>If you have any questions, please contact us at thaott@gmail.com.</p>
                <p>Thank you and have a great day!</p>
            </div>
        </div>",
                IsBodyHtml = true
            };

            mailMessage.To.Add(toAddress);

            using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
            {
                smtpClient.EnableSsl = smtpEnableSsl;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(
                    _config["SmtpSettings:UserName"],
                    _config["SmtpSettings:Password"]);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }


    }
}
