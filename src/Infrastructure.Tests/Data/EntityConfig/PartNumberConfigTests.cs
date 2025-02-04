using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;

namespace Infrastructure.Tests.Data.EntityConfig
{
    public class PartNumberConfigTests
    {
        [Fact]
        public void CanInsertPartNumberIntoDatabase()
        {
            var item = new Fixture().Create<PartNumber>();
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: nameof(PartNumberConfigTests))
                .Options;

            using (var context = new Context(options))
            {
                context.PartNumbers.Add(item);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var itemInContext = context.PartNumbers.Single();
                Assert.Equal(item.Id, itemInContext.Id);
                Assert.Equal(item.Code, itemInContext.Code);
                Assert.Equal(item.Description, itemInContext.Description);
            }
        }
    }
}
