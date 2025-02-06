// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Not needed for Web API.", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Controllers.{nameof(HealthCheckController)}")]
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Not needed for Web API.", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Controllers.{nameof(PartNumberController)}")]
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Not needed for Web API.", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Controllers.{nameof(SupplierController)}")]
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Not needed for Web API.", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Controllers.{nameof(VehicleController)}")]
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Not needed for Web API.", Scope = "type", Target = "~T:Stellantis.ProjectName.WebApi.Controllers.EntityControllerBase`2")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "WebAPI requires the Controller to be public.", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Controllers.{nameof(HealthCheckController)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "WebAPI requires the Controller to be public.", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Controllers.{nameof(PartNumberController)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "WebAPI requires the Controller to be public.", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Controllers.{nameof(SupplierController)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "WebAPI requires the Controller to be public.", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Controllers.{nameof(VehicleController)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "WebAPI requires the Controller to be public.", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Controllers.EntityControllerBase`2")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Dto.{nameof(BaseEntityDto)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Dto.{nameof(PartNumberDto)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Dto.{nameof(PartNumberSupplierDto)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Dto.{nameof(VehiclePartNumberDto)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Dto.{nameof(SupplierDto)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Dto.{nameof(VehicleDto)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Dto.Filters.{nameof(BaseFilterDto)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Dto.Filters.{nameof(PartNumberFilterDto)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Dto.Filters.{nameof(SupplierFilterDto)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Dto.Filters.{nameof(VehicleFilterDto)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Dto.Validators.BaseFilterDtoValidator`1")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.ViewModels.{nameof(ErrorResponseVm)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = "~T:Stellantis.ProjectName.WebApi.Dto.PagedResultDto`1")]
