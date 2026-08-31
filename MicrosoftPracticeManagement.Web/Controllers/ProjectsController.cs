using Microsoft.AspNetCore.Mvc;
using MicrosoftPracticeManagement.Services.DTOs;
using MicrosoftPracticeManagement.Services.Interfaces;
using MicrosoftPracticeManagement.Web.ViewModels;

namespace MicrosoftPracticeManagement.Web.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? account, string? status, string? health)
        {
            try
            {
                var projects = await _projectService.GetFilteredProjectsAsync(search, account, status, health);
                var allProjects = await _projectService.GetAllProjectsAsync();

                var vm = new ProjectListViewModel
                {
                    Projects = projects,
                    Search = search,
                    Account = account,
                    Status = status,
                    Health = health,
                    AvailableAccounts = allProjects.Select(p => p.Account).Distinct().OrderBy(a => a).ToList()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading projects directory");
                return View(new ProjectListViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string account, string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var project = await _projectService.GetProjectByIdAsync(account ?? string.Empty, id);
            if (project == null) return NotFound();

            return View(new ProjectDetailsViewModel { Project = project });
        }

        [HttpGet]
        public async Task<IActionResult> Timeline()
        {
            try
            {
                var timeline = await _projectService.GetGanttTimelineAsync();
                return View(new GanttTimelineViewModel { Timeline = timeline });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Gantt timeline");
                return View(new GanttTimelineViewModel());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ProjectDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectDto model)
        {
            if (!ModelState.IsValid) return View(model);

            if (string.IsNullOrWhiteSpace(model.ProjectId))
            {
                model.ProjectId = $"PRJ-{new Random().Next(200, 999)}";
            }

            await _projectService.AddProjectAsync(model);
            TempData["SuccessMessage"] = $"Project {model.ProjectName} created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string account, string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var project = await _projectService.GetProjectByIdAsync(account ?? string.Empty, id);
            if (project == null) return NotFound();

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProjectDto model)
        {
            if (!ModelState.IsValid) return View(model);

            await _projectService.UpdateProjectAsync(model);
            TempData["SuccessMessage"] = $"Project {model.ProjectName} updated successfully.";
            return RedirectToAction(nameof(Details), new { account = model.Account, id = model.ProjectId });
        }
    }
}
