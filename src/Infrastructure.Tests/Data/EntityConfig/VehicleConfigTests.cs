using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;

namespace Infrastructure.Tests.Data.EntityConfig
{
    public class VehicleConfigTests
    {

        [Fact]
        public void CanInsertVehicleIntoDatabase()
        {
            var item = new Fixture().Create<Vehicle>();
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: nameof(VehicleConfigTests))
                .Options;

            using (var context = new Context(options))
            {
                context.Vehicles.Add(item);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var itemInContext = context.Vehicles.Single();
                Assert.Equal(item.Chassi, itemInContext.Chassi);
                Assert.Equal(item.Id, itemInContext.Id);
            }
        }
    }
}
