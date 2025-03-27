// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Not needed for Web API.", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Controllers.{nameof(HealthCheckController)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "WebAPI requires the Controller to be public.", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Controllers.{nameof(HealthCheckController)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.Dto.Filters.{nameof(FilterDto)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.ViewModels.{nameof(ErrorResponseVm)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.ViewModels.{nameof(EntityVmBase)}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = $"~T:Stellantis.ProjectName.WebApi.ViewModels.PagedResultVm`1")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = "~T:Stellantis.ProjectName.WebApi.ViewModels.AreaVm")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = "~T:Stellantis.ProjectName.WebApi.Dto.AreaDto")]
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>", Scope = "type", Target = "~T:Stellantis.ProjectName.WebApi.Controllers.EntityControllerBase`2")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>", Scope = "type", Target = "~T:Stellantis.ProjectName.WebApi.Controllers.EntityControllerBase`2")]
[assembly: SuppressMessage("Maintainability", "CA1515:Considere tornar internos os tipos públicos", Justification = "<Pendente>", Scope = "type", Target = "~T:Stellantis.ProjectName.WebApi.Controllers.AreaControllerBase")]
[assembly: SuppressMessage("Reliability", "CA2007:Considere chamar ConfigureAwait na tarefa esperada", Justification = "<Pendente>", Scope = "member", Target = "~M:Stellantis.ProjectName.WebApi.Controllers.AreaControllerBase.CreateAsync(Stellantis.ProjectName.WebApi.Dto.AreaDto)~System.Threading.Tasks.Task{Microsoft.AspNetCore.Mvc.IActionResult}")]
[assembly: SuppressMessage("Maintainability", "CA1515:Considere tornar internos os tipos públicos", Justification = "<Pendente>", Scope = "type", Target = "~T:Stellantis.ProjectName.WebApi.Dto.Filters.ApplicationDataFilterDto")]
[assembly: SuppressMessage("Maintainability", "CA1515:Considere tornar internos os tipos públicos", Justification = "<Pendente>", Scope = "type", Target = "~T:Stellantis.ProjectName.WebApi.Dto.ApplicationDataDto")]
