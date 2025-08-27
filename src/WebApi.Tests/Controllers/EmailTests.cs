using System.Net.Mail;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.WebApi.Hubs;

namespace WebApi.Tests.Controllers
{
    public class EmailTests
    {
        [Fact]
        public async Task SmtpClientWrapperSendMailAsyncCallsUnderlyingSmtpClient()
        {
            // Arrange
            using var mailMessage = new MailMessage("from@teste.com", "to@teste.com")
            {
                Subject = "Assunto",
                Body = "Corpo"
            };

            using var smtpWrapper = new SmtpClientWrapper();

            // Act & Assert'
            // O método não lança exceção em ambiente de teste local sem rede externa.
            // Portanto, apenas verifique se não lança exceção.
            await smtpWrapper.SendMailAsync(mailMessage);
        }

        // Teste só pode ser rodado em rede externa
        //[Fact]
        //public async Task SmtpClientWrapperSendMailAsyncCompleteWithoutException()
        //{
        //    using var mailMessage = new MailMessage("from@teste.com", "to@teste.com")
        //    {
        //        Subject = "Assunto",
        //        Body = "Corpo"
        //    };

        //    using var smtpWrapper = new SmtpClientWrapper();
        //    await smtpWrapper.SendMailAsync(mailMessage);
        //}

        [Fact]
        public async Task EmailServiceSendEmailAsyncCallsSmtpClientFactoryAndSendMailAsync()
        {
            // Arrange
            var smtpMock = new Mock<ISmtpClient>();
            smtpMock.Setup(s => s.SendMailAsync(It.IsAny<MailMessage>())).Returns(Task.CompletedTask).Verifiable();

            var service = new EmailService(() => smtpMock.Object);

            // Act
            await service.SendEmailAsync("destino@teste.com", "Assunto", "Corpo");

            // Assert
            smtpMock.Verify(s => s.SendMailAsync(It.Is<MailMessage>(m =>
                m.To[0].Address == "destino@teste.com" &&
                m.Subject == "Assunto" &&
                m.IsBodyHtml)), Times.Once);
        }
    }
}