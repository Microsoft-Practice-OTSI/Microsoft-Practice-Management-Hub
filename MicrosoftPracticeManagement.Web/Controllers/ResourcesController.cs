using Microsoft.AspNetCore.Mvc;
using MicrosoftPracticeManagement.Services.DTOs;
using MicrosoftPracticeManagement.Services.Interfaces;
using MicrosoftPracticeManagement.Web.ViewModels;

namespace MicrosoftPracticeManagement.Web.Controllers
{
    public class ResourcesController : Controller
    {
        private readonly IResourceService _resourceService;
        private readonly ILogger<ResourcesController> _logger;

        public ResourcesController(IResourceService resourceService, ILogger<ResourcesController> logger)
        {
            _resourceService = resourceService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search, string? account, string? project, string? technology,
            string? location, string? designation, string? status, string? availability,
            string? sortBy = "name", bool sortDesc = false, int page = 1, int pageSize = 15)
        {
            try
            {
                var (items, total) = await _resourceService.GetFilteredResourcesAsync(
                    search, account, project, technology, location, designation, status, availability,
                    sortBy, sortDesc, page, pageSize);

                var allResources = await _resourceService.GetAllResourcesAsync();

                var vm = new ResourceListViewModel
                {
                    Resources = items,
                    TotalCount = total,
                    PageIndex = page,
                    PageSize = pageSize,
                    Search = search,
                    Account = account,
                    Project = project,
                    Technology = technology,
                    Location = location,
                    Designation = designation,
                    Status = status,
                    Availability = availability,
                    SortBy = sortBy,
                    SortDesc = sortDesc,
                    AvailableLocations = allResources.Select(r => r.Location).Where(l => !string.IsNullOrEmpty(l)).Distinct().OrderBy(l => l).ToList(),
                    AvailableTechnologies = allResources.Select(r => r.PrimarySkill).Where(s => !string.IsNullOrEmpty(s)).Distinct().OrderBy(s => s).ToList(),
                    AvailableDesignations = allResources.Select(r => r.Designation).Where(d => !string.IsNullOrEmpty(d)).Distinct().OrderBy(d => d).ToList()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading resources directory");
                return View(new ResourceListViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var resource = await _resourceService.GetResourceByIdAsync(id);
            if (resource == null) return NotFound();

            return View(new ResourceDetailsViewModel { Resource = resource });
        }

        [HttpGet]
        public async Task<IActionResult> QuickView(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var resource = await _resourceService.GetResourceByIdAsync(id);
            if (resource == null) return NotFound();

            return PartialView("_QuickViewModal", resource);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadResume(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var resource = await _resourceService.GetResourceByIdAsync(id);
            if (resource == null) return NotFound();

            var stream = await _resourceService.DownloadResumeAsync(id);
            if (stream == null)
            {
                var content = System.Text.Encoding.UTF8.GetBytes($"Microsoft Practice Hub - Resume for {resource.Name} ({resource.Designation})\nPrimary Skill: {resource.PrimarySkill}\nExperience: {resource.ExperienceYears} Years\nLocation: {resource.Location}\nStatus: {resource.Availability}");
                stream = new MemoryStream(content);
            }

            var fileName = $"{resource.EmployeeId}_{resource.Name.Replace(" ", "_")}_Resume.pdf";
            return File(stream, "application/pdf", fileName);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ResourceDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResourceDto model)
        {
            if (!ModelState.IsValid) return View(model);

            if (string.IsNullOrWhiteSpace(model.EmployeeId))
            {
                model.EmployeeId = $"EMP-{new Random().Next(2000, 9999)}";
            }

            await _resourceService.AddResourceAsync(model);
            TempData["SuccessMessage"] = $"Resource {model.Name} ({model.EmployeeId}) added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var resource = await _resourceService.GetResourceByIdAsync(id);
            if (resource == null) return NotFound();

            return View(resource);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ResourceDto model)
        {
            if (!ModelState.IsValid) return View(model);

            await _resourceService.UpdateResourceAsync(model);
            TempData["SuccessMessage"] = $"Resource {model.Name} updated successfully.";
            return RedirectToAction(nameof(Details), new { id = model.EmployeeId });
        }
    }
}
