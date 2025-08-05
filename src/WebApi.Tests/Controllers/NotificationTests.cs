using System.Globalization;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Hubs;

namespace WebApi.Tests.Controllers
{
    public class NotificationTests
    {
        private static NotificationService CreateService(Context context,
            Mock<IHubContext<NotificationHub>> hubContextMock,
            Mock<IStringLocalizer<NotificationResources>> localizerMock,
            Mock<IEmailService> emailServiceMock)
        {
            return new NotificationService(hubContextMock.Object, context, localizerMock.Object, emailServiceMock.Object);
        }

        private static async Task<NotificationController> CreateControllerWithDataAsync(List<Notification> notifications)
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new Context(options);
            context.Notifications.AddRange(notifications);
            await context.SaveChangesAsync().ConfigureAwait(false);

            var controller = new NotificationController(context);
            return controller;
        }

        [Fact]
        public async Task NotifyMembersAsyncHandlesSmtpExceptionAndContinues()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "NotifyMembersAsyncSmtpExceptionTest")
                .Options;
            using var context = new Context(options);

            var member = new Member { Id = 2, Name = "Carlos", Email = "carlos@teste.com", Role = "PO", Cost = 1000 };
            var message = "Mensagem de teste";

            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            clientsMock.Setup(c => c.User(member.Email)).Returns(clientProxyMock.Object);
            clientProxyMock
                .Setup(c => c.SendCoreAsync("ReceiveNotification", It.Is<object[]>(args => args.Length == 1 && (string)args[0] == message), default))
                .Returns(Task.CompletedTask);

            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

            var localizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock
                .Setup(e => e.SendEmailAsync(member.Email, It.IsAny<string>(), message))
                .ThrowsAsync(new SmtpException("Falha SMTP"));

            var service = CreateService(context, hubContextMock, localizerMock, emailServiceMock);

            // Act
            await service.NotifyMembersAsync([member], message);

            // Assert
            var notification = context.Notifications.FirstOrDefault();
            Assert.NotNull(notification);
            Assert.Equal(member.Email, notification.UserEmail);
            Assert.Equal(message, notification.Message);
        }

        [Fact]
        public async Task NotifyMembersAsyncThrowsOnInvalidOperationException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "NotifyMembersAsyncInvalidOperationExceptionTest")
                .Options;
            using var context = new Context(options);

            var member = new Member { Id = 3, Name = "Ana", Email = "ana@teste.com", Role = "PO", Cost = 1000 };
            var message = "Mensagem de teste";

            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            clientsMock.Setup(c => c.User(member.Email)).Returns(clientProxyMock.Object);
            clientProxyMock
                .Setup(c => c.SendCoreAsync("ReceiveNotification", It.Is<object[]>(args => args.Length == 1 && (string)args[0] == message), default))
                .Returns(Task.CompletedTask);

            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

            var localizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock
                .Setup(e => e.SendEmailAsync(member.Email, It.IsAny<string>(), message))
                .ThrowsAsync(new InvalidOperationException("Erro de operação"));

            var service = CreateService(context, hubContextMock, localizerMock, emailServiceMock);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await service.NotifyMembersAsync([member], message).ConfigureAwait(false);
            });
        }

        [Fact]
        public async Task NotifyIncidentCreatedAsyncSendsNotificationToAllMembers()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "NotifyIncidentCreatedAsyncTest")
                .Options;
            using var context = new Context(options);

            var incident = new Incident
            {
                Id = 2,
                Status = default,
                CreatedAt = DateTime.UtcNow,
                Title = "Incidente Teste",
                Description = "Descrição do incidente"
            };
            
            var member = new Member { Id = 1, Name = "João", Email = "joao@teste.com", Role = "PO", Cost = 1000 };
            context.Incidents.Add(incident);
            context.Members.Add(member);
            incident.Members = [member];
            await context.SaveChangesAsync();

            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            clientsMock.Setup(c => c.User(member.Email)).Returns(clientProxyMock.Object);
            clientProxyMock
                .Setup(c => c.SendCoreAsync("ReceiveNotification", It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);

            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

            var localizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            localizerMock.Setup(l => l["IncidentCreatedMessage", member.Name, incident.Title])
                .Returns(new LocalizedString("IncidentCreatedMessage", "Mensagem de incidente criado"));

            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(e => e.SendEmailAsync(member.Email, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var service = new NotificationService(hubContextMock.Object, context, localizerMock.Object, emailServiceMock.Object);

            // Act
            await service.NotifyIncidentCreatedAsync(incident.Id);

            // Assert
            var notification = context.Notifications.FirstOrDefault();
            Assert.NotNull(notification);
            Assert.Equal(member.Email, notification.UserEmail);
            Assert.Equal("Mensagem de incidente criado", notification.Message);
        }

        [Fact]
        public async Task NotifyIncidentStatusChangeAsyncSendsNotificationToAllMembers()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "NotifyIncidentStatusChangeAsyncTest")
                .Options;
            using var context = new Context(options);

            var incident = new Incident
            {
                Id = 2,
                Status = default,
                CreatedAt = DateTime.UtcNow,
                Title = "Incidente Teste",
                Description = "Descrição do incidente"
            };
            
            var member = new Member { Id = 2, Name = "Maria", Email = "maria@teste.com", Role = "PO", Cost = 1000 };
            context.Incidents.Add(incident);
            context.Members.Add(member);
            incident.Members = [member];
            await context.SaveChangesAsync();

            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            clientsMock.Setup(c => c.User(member.Email)).Returns(clientProxyMock.Object);
            clientProxyMock
                .Setup(c => c.SendCoreAsync("ReceiveNotification", It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);

            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

            var localizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            localizerMock.Setup(l => l["IncidentStatusChangedMessage", member.Name, incident.Title, incident.Status])
                .Returns(new LocalizedString("IncidentStatusChangedMessage", "Mensagem de status alterado"));

            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(e => e.SendEmailAsync(member.Email, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var service = new NotificationService(hubContextMock.Object, context, localizerMock.Object, emailServiceMock.Object);

            // Act
            await service.NotifyIncidentStatusChangeAsync(incident.Id);

            // Assert
            var notification = context.Notifications.FirstOrDefault();
            Assert.NotNull(notification);
            Assert.Equal(member.Email, notification.UserEmail);
            Assert.Equal("Mensagem de status alterado", notification.Message);
        }

        [Fact]
        public async Task NotifyFeedbackCreatedAsyncSendsNotificationToAllMembers()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "NotifyFeedbackCreatedAsyncTest")
                .Options;
            using var context = new Context(options);

            var feedback = new Feedback
            {
                Id = 4,
                Title = "Feedback Y",
                Status = default,
                CreatedAt = DateTime.UtcNow,
                Description = "Descrição do feedback"
            };
            
            var member = new Member { Id = 3, Name = "Carlos", Email = "carlos@teste.com", Role = "PO", Cost = 1000 };
            context.Feedbacks.Add(feedback);
            context.Members.Add(member);
            feedback.Members = [member];
            await context.SaveChangesAsync();

            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            clientsMock.Setup(c => c.User(member.Email)).Returns(clientProxyMock.Object);
            clientProxyMock
                .Setup(c => c.SendCoreAsync("ReceiveNotification", It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);

            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

            var localizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            localizerMock.Setup(l => l["FeedbackCreatedMessage", member.Name, feedback.Title])
                .Returns(new LocalizedString("FeedbackCreatedMessage", "Mensagem de feedback criado"));

            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(e => e.SendEmailAsync(member.Email, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var service = new NotificationService(hubContextMock.Object, context, localizerMock.Object, emailServiceMock.Object);

            // Act
            await service.NotifyFeedbackCreatedAsync(feedback.Id);

            // Assert
            var notification = context.Notifications.FirstOrDefault();
            Assert.NotNull(notification);
            Assert.Equal(member.Email, notification.UserEmail);
            Assert.Equal("Mensagem de feedback criado", notification.Message);
        }

        [Fact]
        public async Task NotifyFeedbackStatusChangeAsyncSendsNotificationToAllMembers()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "NotifyFeedbackStatusChangeAsyncTest")
                .Options;
            using var context = new Context(options);

            var feedback = new Feedback
            {
                Id = 4,
                Title = "Feedback Y",
                Status = default,
                CreatedAt = DateTime.UtcNow,
                Description = "Descrição do feedback"
            };
            
            var member = new Member { Id = 4, Name = "Ana", Email = "ana@teste.com", Role = "PO", Cost = 1000 };
            context.Feedbacks.Add(feedback);
            context.Members.Add(member);
            feedback.Members = [member];
            await context.SaveChangesAsync();

            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            clientsMock.Setup(c => c.User(member.Email)).Returns(clientProxyMock.Object);
            clientProxyMock
                .Setup(c => c.SendCoreAsync("ReceiveNotification", It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);

            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

            var localizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            localizerMock.Setup(l => l["FeedbackStatusChangedMessage", member.Name, feedback.Title, feedback.Status])
                .Returns(new LocalizedString("FeedbackStatusChangedMessage", "Mensagem de status de feedback alterado"));

            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(e => e.SendEmailAsync(member.Email, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var service = new NotificationService(hubContextMock.Object, context, localizerMock.Object, emailServiceMock.Object);

            // Act
            await service.NotifyFeedbackStatusChangeAsync(feedback.Id);

            // Assert
            var notification = context.Notifications.FirstOrDefault();
            Assert.NotNull(notification);
            Assert.Equal(member.Email, notification.UserEmail);
            Assert.Equal("Mensagem de status de feedback alterado", notification.Message);
        }

        [Fact]
        public async Task NotifyMembersAsyncAddsNotificationAndSendsSignalRAndEmail()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "NotifyMembersAsyncTest")
                .Options;
            using var context = new Context(options);

            var member = new Member { Id = 1, Name = "João", Email = "joao@teste.com", Role = "PO", Cost = 1000 };
            var message = "Mensagem de teste";

            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            clientsMock.Setup(c => c.User(member.Email)).Returns(clientProxyMock.Object);
            clientProxyMock
                .Setup(c => c.SendCoreAsync("ReceiveNotification", It.Is<object[]>(args => args.Length == 1 && (string)args[0] == message), default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

            var localizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(e => e.SendEmailAsync(member.Email, It.IsAny<string>(), message)).Returns(Task.CompletedTask);

            var service = CreateService(context, hubContextMock, localizerMock, emailServiceMock);

            // Act
            await service.NotifyMembersAsync([member], message);

            // Assert
            var notification = context.Notifications.FirstOrDefault();
            Assert.NotNull(notification);
            Assert.Equal(member.Email, notification.UserEmail);
            Assert.Equal(message, notification.Message);
            clientProxyMock.Verify(c => c.SendCoreAsync("ReceiveNotification", It.Is<object[]>(args => args.Length == 1 && (string)args[0] == message), default), Times.Once);
            emailServiceMock.Verify(e => e.SendEmailAsync(member.Email, "Portal AMS - Nova notificação", message), Times.Once);
        }

        [Fact]
        public async Task ListNotificationsAsyncReturnsNotificationsForUser()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "ListNotificationsAsyncTest")
                .Options;
            using var context = new Context(options);
            context.Notifications.Add(new Notification { UserEmail = "a@b.com", Message = "Msg1" });
            context.Notifications.Add(new Notification { UserEmail = "b@c.com", Message = "Msg2" });
            await context.SaveChangesAsync();

            var service = CreateService(context,
                new Mock<IHubContext<NotificationHub>>(),
                new Mock<IStringLocalizer<NotificationResources>>(),
                new Mock<IEmailService>());

            // Act
            var result = await service.ListNotificationsAsync("a@b.com");

            // Assert
            Assert.Single(result);
            Assert.Equal("Msg1", result.First().Message);
        }

        [Fact]
        public async Task ListAllNotificationsAsyncReturnsAllNotifications()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "ListAllNotificationsAsyncTest")
                .Options;
            using var context = new Context(options);
            context.Notifications.Add(new Notification { UserEmail = "a@b.com", Message = "Msg1" });
            context.Notifications.Add(new Notification { UserEmail = "b@c.com", Message = "Msg2" });
            await context.SaveChangesAsync();

            var service = CreateService(context,
                new Mock<IHubContext<NotificationHub>>(),
                new Mock<IStringLocalizer<NotificationResources>>(),
                new Mock<IEmailService>());

            // Act
            var result = await service.ListAllNotificationsAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetNotificationsReturnsOrderedNotifications()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var notifications = new List<Notification>
            {
                new() { Id = 1, Message = "Msg1", SentAt = now.AddMinutes(2) },
                new() { Id = 2, Message = "Msg2", SentAt = now.AddMinutes(1) },
                new() { Id = 3, Message = "Msg3", SentAt = now.AddMinutes(3) }
            };
            var controller = await CreateControllerWithDataAsync(notifications);

            // Act
            var result = await controller.GetNotifications();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedNotifications = Assert.IsType<List<Notification>>(okResult.Value, exactMatch: false);
            Assert.Equal(3, returnedNotifications.Count);
            Assert.Equal("Msg2", returnedNotifications[0].Message);
            Assert.Equal("Msg1", returnedNotifications[1].Message);
            Assert.Equal("Msg3", returnedNotifications[2].Message);
        }

        [Fact]
        public async Task GetNotificationsReturnsEmptyListWhenNoNotifications()
        {
            // Arrange
            var controller = await CreateControllerWithDataAsync([]);

            // Act
            var result = await controller.GetNotifications();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedNotifications = Assert.IsType<List<Notification>>(okResult.Value, exactMatch: false);
            Assert.Empty(returnedNotifications);
        }

        [Fact]
        public void NotificationPropertiesShouldBeSetCorrectly()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var notification = new Notification
            {
                Id = 1,
                UserEmail = "usuario@teste.com",
                Message = "Mensagem de teste",
                SentAt = now
            };

            // Assert
            Assert.Equal(1, notification.Id);
            Assert.Equal("usuario@teste.com", notification.UserEmail);
            Assert.Equal("Mensagem de teste", notification.Message);
            Assert.Equal(now, notification.SentAt);
        }

        [Fact]
        public void NotificationDefaultValuesShouldBeEmpty()
        {
            // Arrange
            var notification = new Notification();

            // Assert
            Assert.Equal(0, notification.Id);
            Assert.Equal(string.Empty, notification.UserEmail);
            Assert.Equal(string.Empty, notification.Message);
            Assert.Equal(default, notification.SentAt);
        }

        [Fact]
        public void FeedbackCreatedMessageShouldReturnFormattedString()
        {
            // Arrange
            var nome = "João";
            var titulo = "Feedback Importante";
            var template = NotificationResources.FeedbackCreatedMessage;

            // Act
            var mensagem = string.Format(CultureInfo.InvariantCulture, template, nome, titulo);

            // Assert
            Assert.Contains(nome, mensagem, System.StringComparison.Ordinal);
            Assert.Contains(titulo, mensagem, System.StringComparison.Ordinal);
            Assert.StartsWith("Olá", mensagem, System.StringComparison.Ordinal);
        }

        [Fact]
        public async Task NotifyIncidentCreatedAsyncDoesNothingWhenIncidentNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "NotifyIncidentCreatedAsync_DoesNothing_WhenIncidentNotFound")
                .Options;
            using var context = new Context(options);

            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            var localizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            var emailServiceMock = new Mock<IEmailService>();

            var service = new NotificationService(hubContextMock.Object, context, localizerMock.Object, emailServiceMock.Object);

            // Act
            await service.NotifyIncidentCreatedAsync(999); 

            // Assert
            Assert.Empty(context.Notifications);
            hubContextMock.VerifyNoOtherCalls();
            emailServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task NotifyIncidentCreatedAsyncDoesNothingWhenNoMembers()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "NotifyIncidentCreatedAsync_DoesNothing_WhenNoMembers")
                .Options;
            using var context = new Context(options);

            var incident = new Incident
            {
                Id = 10,
                Title = "Incidente sem membros",
                Description = "Desc",
                Status = default,
                CreatedAt = DateTime.UtcNow,
                Members = []
            };
            context.Incidents.Add(incident);
            await context.SaveChangesAsync();

            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            var localizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            var emailServiceMock = new Mock<IEmailService>();

            var service = new NotificationService(hubContextMock.Object, context, localizerMock.Object, emailServiceMock.Object);

            // Act
            await service.NotifyIncidentCreatedAsync(incident.Id);

            // Assert
            Assert.Empty(context.Notifications);
            hubContextMock.VerifyNoOtherCalls();
            emailServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task NotifyFeedbackCreatedAsyncDoesNothingWhenFeedbackNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "NotifyFeedbackCreatedAsync_DoesNothing_WhenFeedbackNotFound")
                .Options;
            using var context = new Context(options);

            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            var localizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            var emailServiceMock = new Mock<IEmailService>();

            var service = new NotificationService(hubContextMock.Object, context, localizerMock.Object, emailServiceMock.Object);

            // Act
            await service.NotifyFeedbackCreatedAsync(999);

            // Assert
            Assert.Empty(context.Notifications);
            hubContextMock.VerifyNoOtherCalls();
            emailServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task NotifyIncidentStatusChangeAsyncDoesNothingWhenIncidentNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "NotifyIncidentStatusChangeAsync_DoesNothing_WhenIncidentNotFound")
                .Options;
            using var context = new Context(options);

            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            var localizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            var emailServiceMock = new Mock<IEmailService>();

            var service = new NotificationService(hubContextMock.Object, context, localizerMock.Object, emailServiceMock.Object);

            // Act
            await service.NotifyIncidentStatusChangeAsync(999); // ID inexistente

            // Assert
            Assert.Empty(context.Notifications);
            hubContextMock.VerifyNoOtherCalls();
            emailServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task NotifyIncidentStatusChangeAsyncDoesNothingWhenNoMembers()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "NotifyIncidentStatusChangeAsync_DoesNothing_WhenNoMembers")
                .Options;
            using var context = new Context(options);

            var incident = new Incident
            {
                Id = 30,
                Title = "Incidente sem membros",
                Description = "Desc",
                Status = default,
                CreatedAt = DateTime.UtcNow,
                Members = []
            };
            context.Incidents.Add(incident);
            await context.SaveChangesAsync();

            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            var localizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            var emailServiceMock = new Mock<IEmailService>();

            var service = new NotificationService(hubContextMock.Object, context, localizerMock.Object, emailServiceMock.Object);

            // Act
            await service.NotifyIncidentStatusChangeAsync(incident.Id);

            // Assert
            Assert.Empty(context.Notifications);
            hubContextMock.VerifyNoOtherCalls();
            emailServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task NotifyFeedbackStatusChangeAsyncDoesNothingWhenFeedbackNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "NotifyFeedbackStatusChangeAsync_DoesNothing_WhenFeedbackNotFound")
                .Options;
            using var context = new Context(options);

            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            var localizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            var emailServiceMock = new Mock<IEmailService>();

            var service = new NotificationService(hubContextMock.Object, context, localizerMock.Object, emailServiceMock.Object);

            // Act
            await service.NotifyFeedbackStatusChangeAsync(999); 

            // Assert
            Assert.Empty(context.Notifications);
            hubContextMock.VerifyNoOtherCalls();
            emailServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task NotifyFeedbackStatusChangeAsyncDoesNothingWhenNoMembers()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "NotifyFeedbackStatusChangeAsync_DoesNothing_WhenNoMembers")
                .Options;
            using var context = new Context(options);

            var feedback = new Feedback
            {
                Id = 40,
                Title = "Feedback sem membros",
                Description = "Desc",
                Status = default,
                CreatedAt = DateTime.UtcNow,
                Members = []
            };
            context.Feedbacks.Add(feedback);
            await context.SaveChangesAsync();

            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            var localizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            var emailServiceMock = new Mock<IEmailService>();

            var service = new NotificationService(hubContextMock.Object, context, localizerMock.Object, emailServiceMock.Object);

            // Act
            await service.NotifyFeedbackStatusChangeAsync(feedback.Id);

            // Assert
            Assert.Empty(context.Notifications);
            hubContextMock.VerifyNoOtherCalls();
            emailServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task NotifyFeedbackCreatedAsyncDoesNothingWhenNoMembers()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "NotifyFeedbackCreatedAsync_DoesNothing_WhenNoMembers")
                .Options;
            using var context = new Context(options);

            var feedback = new Feedback
            {
                Id = 20,
                Title = "Feedback sem membros",
                Description = "Desc",
                Status = default,
                CreatedAt = DateTime.UtcNow,
                Members = []
            };
            context.Feedbacks.Add(feedback);
            await context.SaveChangesAsync();

            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            var localizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            var emailServiceMock = new Mock<IEmailService>();

            var service = new NotificationService(hubContextMock.Object, context, localizerMock.Object, emailServiceMock.Object);

            // Act
            await service.NotifyFeedbackCreatedAsync(feedback.Id);

            // Assert
            Assert.Empty(context.Notifications);
            hubContextMock.VerifyNoOtherCalls();
            emailServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void IncidentStatusChangedMessageShouldReturnFormattedString()
        {
            // Arrange
            var nome = "Maria";
            var titulo = "Incidente X";
            var status = "Resolvido";
            var template = NotificationResources.IncidentStatusChangedMessage;

            // Act
            var mensagem = string.Format(CultureInfo.InvariantCulture, template, nome, titulo, status);

            // Assert
            Assert.Contains(nome, mensagem, System.StringComparison.Ordinal);
            Assert.Contains(titulo, mensagem, System.StringComparison.Ordinal);
            Assert.Contains(status, mensagem, System.StringComparison.Ordinal);
            Assert.StartsWith("Olá", mensagem, System.StringComparison.Ordinal);
        }

        [Fact]
        public void FeedbackAddMemberShouldReturnResourceString()
        {
            // Act
            var resource = NotificationResources.FeedbackAddMember;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(resource));
            Assert.StartsWith("Olá", resource, System.StringComparison.Ordinal);
        }

        [Fact]
        public void FeedbackRemoveMemberShouldReturnFormattedString()
        {
            // Arrange
            var nome = "Carlos";
            var titulo = "Feedback A";
            var template = NotificationResources.FeedbackRemoveMember;

            // Act
            var mensagem = string.Format(CultureInfo.InvariantCulture, template, nome, titulo);

            // Assert
            Assert.Contains(nome, mensagem, System.StringComparison.Ordinal);
            Assert.Contains(titulo, mensagem, System.StringComparison.Ordinal);
            Assert.StartsWith("Olá", mensagem, System.StringComparison.Ordinal);
        }

        [Fact]
        public void FeedbackStatusChangedMessageShouldReturnFormattedString()
        {
            // Arrange
            var nome = "Ana";
            var titulo = "Feedback B";
            var status = "Aprovado";
            var template = NotificationResources.FeedbackStatusChangedMessage;

            // Act
            var mensagem = string.Format(CultureInfo.InvariantCulture, template, nome, titulo, status);

            // Assert
            Assert.Contains(nome, mensagem, System.StringComparison.Ordinal);
            Assert.Contains(titulo, mensagem, System.StringComparison.Ordinal);
            Assert.Contains(status, mensagem, System.StringComparison.Ordinal);
            Assert.StartsWith("Olá", mensagem, System.StringComparison.Ordinal);
        }

        [Fact]
        public void IncidentAddMemberShouldReturnFormattedString()
        {
            // Arrange
            var nome = "Pedro";
            var titulo = "Incidente Y";
            var template = NotificationResources.IncidentAddMember;

            // Act
            var mensagem = string.Format(CultureInfo.InvariantCulture, template, nome, titulo);

            // Assert
            Assert.Contains(nome, mensagem, System.StringComparison.Ordinal);
            Assert.Contains(titulo, mensagem, System.StringComparison.Ordinal);
            Assert.StartsWith("Olá", mensagem, System.StringComparison.Ordinal);
        }

        [Fact]
        public void IncidentCreatedMessageShouldReturnFormattedString()
        {
            // Arrange
            var nome = "Julia";
            var titulo = "Incidente Z";
            var template = NotificationResources.IncidentCreatedMessage;

            // Act
            var mensagem = string.Format(CultureInfo.InvariantCulture, template, nome, titulo);

            // Assert
            Assert.Contains(nome, mensagem, System.StringComparison.Ordinal);
            Assert.Contains(titulo, mensagem, System.StringComparison.Ordinal);
            Assert.StartsWith("Olá", mensagem, System.StringComparison.Ordinal);
        }

        [Fact]
        public void IncidentRemoveMemberShouldReturnFormattedString()
        {
            // Arrange
            var nome = "Rafael";
            var titulo = "Incidente W";
            var template = NotificationResources.IncidentRemoveMember;

            // Act
            var mensagem = string.Format(CultureInfo.InvariantCulture, template, nome, titulo);

            // Assert
            Assert.Contains(nome, mensagem, System.StringComparison.Ordinal);
            Assert.Contains(titulo, mensagem, System.StringComparison.Ordinal);
            Assert.StartsWith("Olá", mensagem, System.StringComparison.Ordinal);
        }

        [Fact]
        public async Task JoinGroupCallsAddToGroupAsyncWithCorrectParameters()
        {
            // Arrange
            var groupName = "TestGroup";
            var connectionId = "12345";

            var groupsMock = new Mock<IGroupManager>();
            groupsMock
                .Setup(g => g.AddToGroupAsync(connectionId, groupName, default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var contextMock = new Mock<HubCallerContext>();
            contextMock.Setup(c => c.ConnectionId).Returns(connectionId);

            using var hub = new NotificationHub
            {
                Context = contextMock.Object,
                Groups = groupsMock.Object
            };
            // Act
            await hub.JoinGroup(groupName);

            // Assert
            groupsMock.Verify(g => g.AddToGroupAsync(connectionId, groupName, default), Times.Once);
        }

        [Fact]
        public async Task LeaveGroupCallsRemoveFromGroupAsyncWithCorrectParameters()
        {
            // Arrange
            var groupName = "TestGroup";
            var connectionId = "54321";

            var groupsMock = new Mock<IGroupManager>();
            groupsMock
                .Setup(g => g.RemoveFromGroupAsync(connectionId, groupName, default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var contextMock = new Mock<HubCallerContext>();
            contextMock.Setup(c => c.ConnectionId).Returns(connectionId);

            using var hub = new NotificationHub
            {
                Context = contextMock.Object,
                Groups = groupsMock.Object
            };
            // Act
            await hub.LeaveGroup(groupName);

            // Assert
            groupsMock.Verify(g => g.RemoveFromGroupAsync(connectionId, groupName, default), Times.Once);
        }
    }
}