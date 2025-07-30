using System.Net.Mail;
using Stellantis.ProjectName.Application.Interfaces.Services;

namespace Stellantis.ProjectName.WebApi.Hubs
{
    public class EmailService(Func<ISmtpClient> smtpClientFactory) : IEmailService
    {
        private readonly Func<ISmtpClient> _smtpClientFactory = smtpClientFactory;

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            using var smtp = _smtpClientFactory();

            var htmlBody = $@"
                <div>
                    <br/>
                    {body}
                    <br/>
                    <img src='https://cdn.sanity.io/images/vvbzj3ky/production/ec63367b59c5ea6b13cb20c0e4109b32aeea182a-1344x820.webp?w=1344&h=820&auto=format'
                         alt='Logo'
                         style='width:200px; height:100px;' />
                </div>";

            using var mail = new MailMessage("amsportalproject@gmail.com", recipientEmail)
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            await smtp.SendMailAsync(mail).ConfigureAwait(false);
        }
    }
}