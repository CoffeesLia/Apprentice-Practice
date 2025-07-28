using System.Net.Mail;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface ISmtpClient : IDisposable
    {
        Task SendMailAsync(MailMessage message);
    }
}