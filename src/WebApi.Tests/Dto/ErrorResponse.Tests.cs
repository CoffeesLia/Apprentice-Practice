using Stellantis.ProjectName.WebApi.Dto;

namespace WebApi.Tests.Dto
{
    public class ErrorResponseTests
    {
        [Fact]
        public void ConstructorInitializeProperties()
        {
            // Arrange
            string error = "Test Error";
            string message = "Test Message";

            // Act
            ErrorResponse errorResponse = new(error, message);

            // Assert
            Assert.Equal(error, errorResponse.Error);
            Assert.Equal(message, errorResponse.Message);
        }

        [Fact]
        public void BadRequestReturnErrorResponseWithBadRequestError()
        {
            // Arrange
            string message = "Test Message";

            // Act
            ErrorResponse errorResponse = ErrorResponse.BadRequest(message);

            // Assert
            Assert.Equal("Bad Request", errorResponse.Error);
            Assert.Equal(message, errorResponse.Message);
        }

        [Fact]
        public void EqualsReturnTrueForEqualObjects()
        {
            // Arrange
            string error = "Test Error";
            string message = "Test Message";
            ErrorResponse errorResponse1 = new(error, message);
            ErrorResponse errorResponse2 = new(error, message);

            // Act
            bool result = errorResponse1.Equals(errorResponse2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EqualsReturnFalseForDifferentValues()
        {
            // Arrange
            ErrorResponse errorResponse1 = new("Error 1", "Message 1");
            ErrorResponse errorResponse2 = new("Error 2", "Message 2");

            // Act
            bool result = errorResponse1.Equals(errorResponse2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EqualsReturnFalseForDifferentObjects()
        {
            // Arrange
            var item = new { Error = "Error", Message = "Message 2" };
            ErrorResponse errorResponse = new("Error", "Message");

            // Act
            bool result = errorResponse.Equals(item);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetHashCodeReturnSameHashCodeForEqualObjects()
        {
            // Arrange
            string error = "Test Error";
            string message = "Test Message";
            ErrorResponse errorResponse1 = new(error, message);
            ErrorResponse errorResponse2 = new(error, message);

            // Act
            int hashCode1 = errorResponse1.GetHashCode();
            int hashCode2 = errorResponse2.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        [Fact]
        public void GetHashCodeReturnDifferentHashCodeForDifferentObjects()
        {
            // Arrange
            ErrorResponse errorResponse1 = new("Error 1", "Message 1");
            ErrorResponse errorResponse2 = new("Error 2", "Message 2");

            // Act
            int hashCode1 = errorResponse1.GetHashCode();
            int hashCode2 = errorResponse2.GetHashCode();

            // Assert
            Assert.NotEqual(hashCode1, hashCode2);
        }
    }
}

