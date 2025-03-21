using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using Stellantis.ProjectName.Infrastructure.Data;
using Xunit;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Tests.Data.Repositories
{
    public class DataServiceRepositoryTests
    {
        private readonly Mock<Context> _contextMock;
        private readonly Mock<IStringLocalizer<DataServiceRepository>> _localizerMock;
        private readonly DataServiceRepository _repository;

        public DataServiceRepositoryTests()
        {
            _contextMock = new Mock<Context>(new DbContextOptions<Context>());
            _localizerMock = new Mock<IStringLocalizer<DataServiceRepository>>();
            _repository = new DataServiceRepository(_contextMock.Object, _localizerMock.Object);
        }

        [Fact]
        public async Task GetServiceByIdAsyncShouldReturnServiceWhenServiceExists()
        {
            // Arrange
            var serviceId = 1;
            var service = new EDataService { Id = serviceId, Name = "Test Service" };

            Mock<DbSet<EDataService>> dbSetMock = MockDbSet([service]);
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);

            // Act
            var result = await _repository.GetServiceByIdAsync(serviceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(serviceId, result.Id);
            Assert.Equal("Test Service", result.Name);
        }

        [Fact]
        public async Task GetServiceByIdAsyncShouldThrowExceptionWhenServiceNotFound()
        {
            // Arrange
            var serviceId = 1;
            var dbSetMock = MockDbSet(new List<EDataService>());
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);

            var localizedString = new LocalizedString("GetServiceById_ServiceNotFound", $"Service with ID {serviceId} not found.");
            _localizerMock.Setup(l => l["GetServiceById_ServiceNotFound", serviceId]).Returns(localizedString);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.GetServiceByIdAsync(serviceId));
            Assert.Equal(localizedString.Value, exception.Message);
        }

        [Fact]
        public async Task GetAllServicesAsyncShouldReturnServicesWhenServicesExist()
        {
            // Arrange
            var services = new List<EDataService>
            {
                new() { Id = 1, Name = "Service 1" },
                new() { Id = 2, Name = "Service 2" }
            };

            var dbSetMock = MockDbSet(services);
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);

            // Act
            var result = await _repository.GetAllServicesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllServicesAsyncShouldThrowExceptionWhenNoServicesExist()
        {
            // Arrange
            var dbSetMock = MockDbSet(new List<EDataService>());
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);

            var localizedString = new LocalizedString("GetAllServices_NoServicesFound", "No services found.");
            _localizerMock.Setup(l => l["GetAllServices_NoServicesFound"]).Returns(localizedString);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.GetAllServicesAsync());
            Assert.Equal(localizedString.Value, exception.Message);
        }

        [Fact]
        public async Task AddServiceAsyncShouldAddServiceWhenServiceDoesNotExist()
        {
            // Arrange
            var service = new EDataService { Id = 1, Name = "New Service" };

            var dbSetMock = MockDbSet(new List<EDataService>());
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);
            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            await _repository.AddServiceAsync(service);

            // Assert
            dbSetMock.Verify(m => m.AddAsync(service, It.IsAny<CancellationToken>()), Times.Once);
            _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddServiceAsyncShouldThrowExceptionWhenServiceNameAlreadyExists()
        {
            // Arrange
            var service = new EDataService { Id = 1, Name = "Existing Service" };
            var existingService = new EDataService { Id = 2, Name = "Existing Service" };

            var dbSetMock = MockDbSet([existingService]);
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);

            var localizedString = new LocalizedString("ServiceNameAlreadyExists", $"Service with name {service.Name} already exists.");
            _localizerMock.Setup(l => l["ServiceNameAlreadyExists", service.Name]).Returns(localizedString);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.AddServiceAsync(service));
            Assert.Equal(localizedString.Value, exception.Message);
        }

        [Fact]
        public async Task UpdateServiceAsyncShouldUpdateServiceWhenServiceExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new Context(options))
            {
                var existingService = new EDataService { Id = 1, Name = "Old Service" };
                context.Set<EDataService>().Add(existingService);
                await context.SaveChangesAsync();
            }

            using (var context = new Context(options))
            {
                var repository = new DataServiceRepository(context, _localizerMock.Object);

                var service = new EDataService { Id = 1, Name = "Updated Service" };

                // Act
                await repository.UpdateServiceAsync(service);

                // Assert
                var updatedService = await context.Set<EDataService>().FindAsync(1);
                Assert.NotNull(updatedService); 
                Assert.Equal("Updated Service", updatedService!.Name);
            }
        }

        [Fact]
        public async Task UpdateServiceAsyncShouldThrowExceptionWhenServiceNotFound()
        {
            // Arrange
            var service = new EDataService { Id = 1, Name = "Non-Existent Service" };

            var dbSetMock = MockDbSet(new List<EDataService>());
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);

            var localizedString = new LocalizedString("ServiceNotFound", $"Service with ID {service.Id} not found.");
            _localizerMock.Setup(l => l["ServiceNotFound", service.Id]).Returns(localizedString);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.UpdateServiceAsync(service));
            Assert.Equal(localizedString.Value, exception.Message);
        }

        [Fact]
        public async Task DeleteServiceAsyncShouldDeleteServiceWhenServiceExists()
        {
            // Arrange
            var serviceId = 1;
            var service = new EDataService { Id = serviceId, Name = "Service to Delete" };

            var dbSetMock = MockDbSet(new List<EDataService> { service });
            dbSetMock.Setup(m => m.FindAsync(serviceId)).ReturnsAsync(service);
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);
            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            await _repository.DeleteServiceAsync(serviceId);

            // Assert
            dbSetMock.Verify(m => m.Remove(service), Times.Once);
            _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteServiceAsyncShouldThrowExceptionWhenServiceNotFound()
        {
            // Arrange
            var serviceId = 1;

            var dbSetMock = MockDbSet(new List<EDataService>());
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);

            var localizedString = new LocalizedString("ServiceNotFound", $"Service with ID {serviceId} not found.");
            _localizerMock.Setup(l => l["ServiceNotFound", serviceId]).Returns(localizedString);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.DeleteServiceAsync(serviceId));
            Assert.Equal(localizedString.Value, exception.Message);
        }

        private static Mock<DbSet<T>> MockDbSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();

            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            dbSetMock.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken cancellationToken) => new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

            return dbSetMock;
        }
    }

    // Helper classes for async mocking
    internal class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner = inner;

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return ValueTask.FromResult(_inner.MoveNext());
        }
    }

    internal class TestAsyncQueryProvider<T>(IQueryProvider inner) : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner = inner;

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<T>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object? Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var result = Execute(expression);
            var genericTypeArguments = typeof(TResult).GenericTypeArguments;
            if (genericTypeArguments.Length == 0)
            {
                throw new InvalidOperationException("TResult does not have generic type arguments.");
            }
            var fromResultMethod = typeof(Task).GetMethod("FromResult")?.MakeGenericMethod(genericTypeArguments[0])
            ?? throw new InvalidOperationException("Could not find FromResult method.");
            return (TResult)fromResultMethod.Invoke(null, [result])!;
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }
    }
}