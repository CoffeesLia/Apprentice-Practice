using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Services
{
    public class EmployeeService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Employee> validator)
        : EntityServiceBase<Employee, IEmployeeRepository>(unitOfWork, localizerFactory, validator), IEmployeeService
    {
        protected override IEmployeeRepository Repository => base.UnitOfWork.EmployeeRepository;
    }
}
