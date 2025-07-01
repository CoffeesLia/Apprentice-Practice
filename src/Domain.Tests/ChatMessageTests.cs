using Stellantis.ProjectName.Domain.Entities;

namespace Domain.Tests
{
    public class ChatMessageTests
    {
        // Verifica se a propriedade Id é atribuída e lida corretamente.
        [Fact]
        public void IdShouldAllowAssignmentAndRead()
        {
            // Arrange
            var chatMessage = new ChatMessage();
            var id = Guid.NewGuid();

            // Act
            chatMessage.Id = id;

            // Assert
            Assert.Equal(id, chatMessage.Id);
        }

        // Verifica se a propriedade User é atribuída e lida corretamente.
        [Fact]
        public void UserShouldAllowAssignmentAndRead()
        {
            // Arrange
            var chatMessage = new ChatMessage();
            var user = "usuario_teste";

            // Act
            chatMessage.User = user;

            // Assert
            Assert.Equal(user, chatMessage.User);
        }

        // Verifica se a propriedade Message é atribuída e lida corretamente.
        [Fact]
        public void MessageShouldAllowAssignmentAndRead()
        {
            // Arrange
            var chatMessage = new ChatMessage();
            var message = "Mensagem de teste";

            // Act
            chatMessage.Message = message;

            // Assert
            Assert.Equal(message, chatMessage.Message);
        }

        // Verifica se a propriedade SentAt é atribuída e lida corretamente.
        [Fact]
        public void SentAtShouldAllowAssignmentAndRead()
        {
            // Arrange
            var chatMessage = new ChatMessage();
            var sentAt = DateTime.UtcNow;

            // Act
            chatMessage.SentAt = sentAt;

            // Assert
            Assert.Equal(sentAt, chatMessage.SentAt);
        }
    }
}