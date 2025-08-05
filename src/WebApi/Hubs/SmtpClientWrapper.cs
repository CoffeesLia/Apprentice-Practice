using System.Net;
using System.Net.Mail;
using Stellantis.ProjectName.Application.Interfaces.Services;

namespace Stellantis.ProjectName.WebApi.Hubs
{
    public class SmtpClientWrapper : ISmtpClient
    {
        private readonly SmtpClient _client;
        private bool _disposed;

        public SmtpClientWrapper() =>
            _client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("amsportalproject@gmail.com", "cxbz juic zhfs xmoh"),
                EnableSsl = true
            };

        public Task SendMailAsync(MailMessage message) => _client.SendMailAsync(message);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _client.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}