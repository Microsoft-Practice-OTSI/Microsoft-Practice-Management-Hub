using Microsoft.AspNetCore.Mvc;
using MicrosoftPracticeManagement.Services.Interfaces;
using MicrosoftPracticeManagement.Web.ViewModels;

namespace MicrosoftPracticeManagement.Web.Controllers
{
    public class SkillsController : Controller
    {
        private readonly ISkillService _skillService;
        private readonly ILogger<SkillsController> _logger;

        public SkillsController(ISkillService skillService, ILogger<SkillsController> logger)
        {
            _skillService = skillService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? tech, string? level)
        {
            try
            {
                var matrix = await _skillService.GetSkillsMatrixAsync(tech, level);
                var vm = new SkillsMatrixViewModel
                {
                    Matrix = matrix,
                    TechFilter = tech,
                    LevelFilter = level
                };
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading skills matrix");
                return View(new SkillsMatrixViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Certifications()
        {
            var expiring = await _skillService.GetExpiringCertificationsAsync(60);
            return View(expiring);
        }

        [HttpGet]
        public async Task<IActionResult> MissingSkills()
        {
            var gaps = await _skillService.GetMissingSkillsReportAsync();
            return View(gaps);
        }
    }

    public class PracticeHealthController : Controller
    {
        private readonly IPracticeHealthService _healthService;
        private readonly ILogger<PracticeHealthController> _logger;

        public PracticeHealthController(IPracticeHealthService healthService, ILogger<PracticeHealthController> logger)
        {
            _healthService = healthService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var health = await _healthService.GetPracticeHealthMetricsAsync();
                return View(new PracticeHealthViewModel { Health = health });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading practice health dashboard");
                return View(new PracticeHealthViewModel());
            }
        }
    }
}
