using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;

namespace Infrastructure.Tests.Data.EntityConfig
{
    public class PartNumberSupplierConfigTests
    {
        [Fact]
        public void CanInsertPartNumberSupplierIntoDatabase()
        {
            var item = new Fixture().Create<PartNumberSupplier>();
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: nameof(PartNumberSupplierConfigTests))
                .Options;

            using (var context = new Context(options))
            {
                context.PartNumberSuppliers.Add(item);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var itemInContext = context.PartNumberSuppliers.Single();
                Assert.Equal(item.UnitPrice, itemInContext.UnitPrice);
                Assert.Equal(item.PartNumberId, itemInContext.PartNumberId);
                Assert.Equal(item.SupplierId, itemInContext.SupplierId);
            }
        }
    }
}
