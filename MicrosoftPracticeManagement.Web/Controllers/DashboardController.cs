using Microsoft.AspNetCore.Mvc;
using MicrosoftPracticeManagement.Services.Interfaces;
using MicrosoftPracticeManagement.Web.ViewModels;

namespace MicrosoftPracticeManagement.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var summary = await _dashboardService.GetDashboardSummaryAsync();
                var vm = new DashboardViewModel
                {
                    Summary = summary,
                    SelectedPractice = "Microsoft Azure & Cloud Practice"
                };
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading executive dashboard");
                return View(new DashboardViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData()
        {
            var summary = await _dashboardService.GetDashboardSummaryAsync();
            return Json(new
            {
                utilizationDonut = summary.UtilizationDonutChart,
                techDistribution = summary.TechnologyDistributionBarChart,
                capacityVsDemand = summary.CapacityVsDemandChart,
                utilizationTrend = summary.UtilizationTrendLineChart
            });
        }
    }
}
