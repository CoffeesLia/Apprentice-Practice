using AutoFixture;
using Stellantis.ProjectName.Infrastructure.Data.Extensions;
using System.ComponentModel;

namespace Infrastructure.Tests.Data.Extensions
{
    public class QueryableExtensionsTests
    {
        private class TestEntity
        {
            [DefaultValue(0)]
            public int Id { get; set; }
        }

        [Theory]
        [InlineData(null)]
        [InlineData(OrderDirection.Ascending)]
        [InlineData(OrderDirection.Descending)]
        public void OrderByShouldOrderCorrectly(string? orderDirection)
        {
            // Arrange
            Fixture fixture = new();
            IQueryable<TestEntity> data = fixture.CreateMany<TestEntity>(10).AsQueryable();

            // Act
            IQueryable<TestEntity> result = data.OrderBy(nameof(TestEntity.Id), orderDirection);

            // Assert
            IOrderedQueryable<TestEntity> expected = orderDirection switch
            {
                null or OrderDirection.Ascending => data.OrderBy(e => e.Id),
                OrderDirection.Descending => data.OrderByDescending(e => e.Id),
                _ => throw new ArgumentOutOfRangeException(nameof(orderDirection), "Invalid direction.")
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public void OrderByShouldThrowArgumentExceptionWhenInvalidOrderDirection()
        {
            // Arrange
            Fixture fixture = new();
            IQueryable<TestEntity> data = fixture.CreateMany<TestEntity>(10).AsQueryable();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => data.OrderBy(nameof(TestEntity.Id), "InvalidDirection"));
        }
    }
}
