using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;

namespace Infrastructure.Tests.Data.EntityConfig
{
    public class PartNumberVehicleConfigTests
    {
        [Fact]
        public void CanInsertPartNumberVehicleIntoDatabase()
        {
            var item = new Fixture().Create<VehiclePartNumber>();
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: nameof(PartNumberVehicleConfigTests))
                .Options;

            using (var context = new Context(options))
            {
                context.VehiclePartNumbers.Add(item);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var itemInContext = context.VehiclePartNumbers.Single();
                Assert.Equal(item.PartNumberId, itemInContext.PartNumberId);
                Assert.Equal(item.VehicleId, itemInContext.VehicleId);
                Assert.Equal(item.Amount, itemInContext.Amount);
            }
        }
    }
}
