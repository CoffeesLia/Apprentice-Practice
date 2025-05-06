using AutoFixture;
using Stellantis.ProjectName.Infrastructure.Data.Extensions;

namespace Infrastructure.Tests.Data.Extensions
{
    public class QueryableExtensionsTests
    {
        private class TestEntity
        {
            public int Id { get; set; } = 0;
        }

        [Theory]
        [InlineData(null)]
        [InlineData(OrderDirection.Ascending)]
        [InlineData(OrderDirection.Descending)]
        public void OrderBy_ShouldOrderCorrectly(string? orderDirection)
        {
            // Arrange
            var fixture = new Fixture();
            var data = fixture.CreateMany<TestEntity>(10).AsQueryable();

            // Act
            var result = data.OrderBy(nameof(TestEntity.Id), orderDirection);

            // Assert
            var expected = orderDirection switch
            {
                null or OrderDirection.Ascending => data.OrderBy(e => e.Id),
                OrderDirection.Descending => data.OrderByDescending(e => e.Id),
                _ => throw new ArgumentOutOfRangeException(nameof(orderDirection), "Invalid direction.")
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public void OrderBy_ShouldThrowArgumentException_WhenInvalidOrderDirection()
        {
            // Arrange
            var fixture = new Fixture();
            var data = fixture.CreateMany<TestEntity>(10).AsQueryable();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => data.OrderBy(nameof(TestEntity.Id), "InvalidDirection"));
        }
    }
}
