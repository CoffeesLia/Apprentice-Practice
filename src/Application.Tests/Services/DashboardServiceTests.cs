using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using System.Linq.Expressions;
using Xunit;

namespace Application.Tests.Services
{
    public class DashboardServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly DashboardService _dashboardService;

        public DashboardServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _dashboardService = new DashboardService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GetDashboardAsyncReturnsCorrectDashboardData()
        {
            var applications = new List<ApplicationData>
        {
            new(name : "App1") { Name = "App1", AreaId = 1, ResponsibleId = 1 }, 
            new(name : "App2") { Name = "App2", AreaId = 2, ResponsibleId = 2 },
        };

            var incidents = new PagedResult<Incident>
            {
                Result = [],
                Total = 3
            };
            var members = new PagedResult<Member>
            {
                Result = [],
                Total = 5
            };
            var squads = new PagedResult<Squad>
            {
                Result =
            [
                new Squad
                {
                    Name = "Squad A",
                    Members =
                    [
                        new Member { Name = "Alice", Role = "Dev", Cost = 1000m, Email = "alice@example.com" },
                        new Member { Name = "Bob", Role = "QA", Cost = 1200m, Email = "bob@example.com" }
                    ]
                }
            ]
            };

            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository.GetListAsync(It.IsAny<Expression<Func<ApplicationData, bool>>>()))
                .ReturnsAsync(applications);
            _unitOfWorkMock.Setup(u => u.IncidentRepository.GetListAsync(It.IsAny<Expression<Func<Incident, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(incidents);
            _unitOfWorkMock.Setup(u => u.MemberRepository.GetListAsync(It.IsAny<Expression<Func<Member, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(members);
            _unitOfWorkMock.Setup(u => u.SquadRepository.GetListAsync(It.IsAny<Expression<Func<Squad, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(squads);

            var result = await _dashboardService.GetDashboardAsync();

            Assert.Equal(2, result.TotalApplications);
            Assert.Equal(3, result.TotalOpenIncidents);
            Assert.Equal(5, result.TotalMembers);
            Assert.Single(result.Squads);
            Assert.Equal("Squad A", result.Squads.First().SquadName);
            Assert.Equal(2, result.Squads.First().Members.Count);
        }

        [Fact]
        public async Task GetDashboardAsyncIgnoresSquadsWithoutMembers()
        {
            var squads = new PagedResult<Squad>
            {
                Result =
            [
                new Squad { Name = "Empty Squad", Members = null }
            ]
            };

            _unitOfWorkMock.Setup(u => u.SquadRepository.GetListAsync(It.IsAny<Expression<Func<Squad, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(squads);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository.GetListAsync(It.IsAny<Expression<Func<ApplicationData, bool>>>()))
                .ReturnsAsync([]);
            _unitOfWorkMock.Setup(u => u.IncidentRepository.GetListAsync(It.IsAny<Expression<Func<Incident, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Incident> { Result = [] });
            _unitOfWorkMock.Setup(u => u.MemberRepository.GetListAsync(It.IsAny<Expression<Func<Member, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Member> { Result = [] });

            var result = await _dashboardService.GetDashboardAsync();

            Assert.All(result.Squads, s => Assert.Empty(s.Members)); 
        }
    }
}
