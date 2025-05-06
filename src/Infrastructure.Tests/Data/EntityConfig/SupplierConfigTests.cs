using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;

namespace Infrastructure.Tests.Data.EntityConfig
{
    public class SupplierConfigTests
    {
        [Fact]
        public void CanInsertSupplierIntoDatabase()
        {
            var item = new Fixture().Create<Supplier>();
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: nameof(SupplierConfigTests))
                .Options;

            using (var context = new Context(options))
            {
                context.Suppliers.Add(item);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var itemInContext = context.Suppliers.Single();
                Assert.Equal(item.Id, itemInContext.Id);
                Assert.Equal(item.Code, itemInContext.Code);
                Assert.Equal(item.Address, itemInContext.Address);
                Assert.Equal(item.CompanyName, itemInContext.CompanyName);
                Assert.Equal(item.Phone, itemInContext.Phone);
            }
        }
    }
}
