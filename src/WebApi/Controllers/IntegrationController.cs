using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/Integration")]
    internal sealed class IntegrationController : EntityControllerBase<Integration, Context>
    {
        private readonly IStringLocalizer _localizer;
        private readonly IAreaService _service;
        private readonly IMapper _mapper;

        public IntegrationController(IAreaService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
        {
            _service = service;
            _mapper = mapper;
            _localizer = localizerFactory.Create(typeof(AreaResources));
        }

        // GET: IntegrationController
        public ActionResult Index()
        {
            return View();
        }

        // GET: IntegrationController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: IntegrationController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: IntegrationController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: IntegrationController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: IntegrationController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: IntegrationController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: IntegrationController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
