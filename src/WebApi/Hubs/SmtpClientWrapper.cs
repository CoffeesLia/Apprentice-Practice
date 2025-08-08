using System.Net;
using System.Net.Mail;
using Stellantis.ProjectName.Application.Interfaces.Services;

namespace Stellantis.ProjectName.WebApi.Hubs
{
    public class SmtpClientWrapper : ISmtpClient
    {
        private readonly string _host = "smtp.gmail.com";
        private readonly int _port = 587;
        private readonly string _username = "amsportalproject@gmail.com";
        private readonly string _password = "cxbz juic zhfs xmoh";
        private readonly bool _enableSsl = true;
        private bool _disposed;

        public async Task SendMailAsync(MailMessage message)
        {
            using var smtpClient = new SmtpClient(_host, _port);
            smtpClient.Credentials = new NetworkCredential(_username, _password);
            smtpClient.EnableSsl = _enableSsl;
            await smtpClient.SendMailAsync(message).ConfigureAwait(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {}
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