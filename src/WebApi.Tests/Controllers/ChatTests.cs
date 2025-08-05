using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Hubs;

namespace WebApi.Tests.Controllers
{
    public class ChatTests
    {
        [Fact]
        public async Task GetMessagesReturnsOrderedMessages()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var messages = new List<Chat>
            {
                new() { Id = Guid.NewGuid(), User = "user1", Message = "msg1", SentAt = DateTime.UtcNow.AddMinutes(-2) },
                new() { Id = Guid.NewGuid(), User = "user2", Message = "msg2", SentAt = DateTime.UtcNow.AddMinutes(-1) },
                new() { Id = Guid.NewGuid(), User = "user3", Message = "msg3", SentAt = DateTime.UtcNow }
            };

            using (var context = new Context(options))
            {
                context.ChatMessages.AddRange(messages);
                await context.SaveChangesAsync();
            }

            using (var context = new Context(options))
            {
                var controller = new ChatController(context);

                // Act
                var result = await controller.GetMessages();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var returnMessages = Assert.IsType<IEnumerable<Chat>>(okResult.Value, exactMatch: false);

                var ordered = returnMessages.OrderBy(m => m.SentAt).ToList();
                Assert.Equal(ordered, [.. returnMessages]);
                Assert.Equal(3, ordered.Count);
                Assert.Equal("user1", ordered[0].User);
                Assert.Equal("user2", ordered[1].User);
                Assert.Equal("user3", ordered[2].User);
            }
        }

        [Fact]
        public async Task SendMessageShouldAddChatMessageAndSendToAllClients()
        {
            // Arrange
            var mockClients = new Mock<IHubCallerClients>();
            var mockAll = new Mock<IClientProxy>();
            mockClients.Setup(c => c.All).Returns(mockAll.Object);

            var user = "testUser";
            var message = "Hello, world!";

            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            using var context = new Context(options);
            using var hub = new ChatHub(context)
            {
                Clients = mockClients.Object
            };

            // Act
            await hub.SendMessage(user, message);

            // Assert
            var chatMessages = context.ChatMessages.ToList();
            Assert.Single(chatMessages);
            var chatMessage = chatMessages[0];
            Assert.Equal(user, chatMessage.User);
            Assert.Equal(message, chatMessage.Message);
            Assert.True(chatMessage.Id != Guid.Empty);
            Assert.True((DateTime.UtcNow - chatMessage.SentAt).TotalSeconds < 5);

            mockAll.Verify(
                c => c.SendCoreAsync(
                    "ReceiveMessage",
                    It.Is<object[]>(o =>
                        ValidateReceiveMessageArgs(o, user, message)
                    ),
                    default
                ),
                Times.Once
            );
        }

        [Fact]
        public void DisposeShouldDisposeContext()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase("TestDb_Dispose")
                .Options;

            using var context = new Context(options);
            var hub = new ChatHub(context);

            // Act
            hub.Dispose();

            // Assert
            Assert.True(true);
        }

        private static bool ValidateReceiveMessageArgs(object[] o, string expectedUser, string expectedMessage)
        {
            if (o.Length != 3) return false;
            if (!Guid.TryParse(o[0]?.ToString(), out _)) return false;
            if ((string)o[1] != expectedUser) return false;
            if ((string)o[2] != expectedMessage) return false;
            return true;
        }
    }
}