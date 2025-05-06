using Stellantis.ProjectName.WebApi.Dto;

namespace WebApi.Tests.Dto
{
    public class ErrorResponseTests
    {
        [Fact]
        public void Constructor_InitializeProperties()
        {
            // Arrange
            var error = "Test Error";
            var message = "Test Message";

            // Act
            var errorResponse = new ErrorResponse(error, message);

            // Assert
            Assert.Equal(error, errorResponse.Error);
            Assert.Equal(message, errorResponse.Message);
        }

        [Fact]
        public void BadRequest_ReturnErrorResponseWithBadRequestError()
        {
            // Arrange
            var message = "Test Message";

            // Act
            var errorResponse = ErrorResponse.BadRequest(message);

            // Assert
            Assert.Equal("Bad Request", errorResponse.Error);
            Assert.Equal(message, errorResponse.Message);
        }

        [Fact]
        public void Equals_ReturnTrueForEqualObjects()
        {
            // Arrange
            var error = "Test Error";
            var message = "Test Message";
            var errorResponse1 = new ErrorResponse(error, message);
            var errorResponse2 = new ErrorResponse(error, message);

            // Act
            var result = errorResponse1.Equals(errorResponse2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Equals_ReturnFalseForDifferentValues()
        {
            // Arrange
            var errorResponse1 = new ErrorResponse("Error 1", "Message 1");
            var errorResponse2 = new ErrorResponse("Error 2", "Message 2");

            // Act
            var result = errorResponse1.Equals(errorResponse2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equals_ReturnFalseForDifferentObjects()
        {
            // Arrange
            var item = new { Error = "Error", Message = "Message 2" };
            var errorResponse = new ErrorResponse("Error", "Message");

            // Act
            var result = errorResponse.Equals(item);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetHashCode_ReturnSameHashCodeForEqualObjects()
        {
            // Arrange
            var error = "Test Error";
            var message = "Test Message";
            var errorResponse1 = new ErrorResponse(error, message);
            var errorResponse2 = new ErrorResponse(error, message);

            // Act
            var hashCode1 = errorResponse1.GetHashCode();
            var hashCode2 = errorResponse2.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        [Fact]
        public void GetHashCode_ReturnDifferentHashCodeForDifferentObjects()
        {
            // Arrange
            var errorResponse1 = new ErrorResponse("Error 1", "Message 1");
            var errorResponse2 = new ErrorResponse("Error 2", "Message 2");

            // Act
            var hashCode1 = errorResponse1.GetHashCode();
            var hashCode2 = errorResponse2.GetHashCode();

            // Assert
            Assert.NotEqual(hashCode1, hashCode2);
        }
    }
}

