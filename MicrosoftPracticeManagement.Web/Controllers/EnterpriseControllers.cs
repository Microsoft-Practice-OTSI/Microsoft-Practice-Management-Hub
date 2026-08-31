using Microsoft.AspNetCore.Mvc;
using MicrosoftPracticeManagement.Services.DTOs;
using MicrosoftPracticeManagement.Services.Interfaces;
using MicrosoftPracticeManagement.Web.ViewModels;

namespace MicrosoftPracticeManagement.Web.Controllers
{
    public class TimesheetsController : Controller
    {
        private readonly ITimesheetService _timesheetService;
        private readonly ILogger<TimesheetsController> _logger;

        public TimesheetsController(ITimesheetService timesheetService, ILogger<TimesheetsController> logger)
        {
            _timesheetService = timesheetService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? month)
        {
            var ym = string.IsNullOrWhiteSpace(month) ? DateTime.UtcNow.ToString("yyyy-MM") : month;
            var compliance = await _timesheetService.GetComplianceDashboardAsync(ym);
            return View(new TimesheetsViewModel { Compliance = compliance, SelectedMonth = ym });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string month, string employeeId)
        {
            await _timesheetService.ApproveTimesheetAsync(month, employeeId, "Practice Director");
            TempData["SuccessMessage"] = $"Timesheet for {employeeId} approved successfully.";
            return RedirectToAction(nameof(Index), new { month });
        }
    }

    public class LeaveController : Controller
    {
        private readonly ILeaveService _leaveService;
        private readonly ILogger<LeaveController> _logger;

        public LeaveController(ILeaveService leaveService, ILogger<LeaveController> logger)
        {
            _leaveService = leaveService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? month)
        {
            var ym = string.IsNullOrWhiteSpace(month) ? DateTime.UtcNow.ToString("yyyy-MM") : month;
            var summary = await _leaveService.GetLeaveSummaryAsync(ym);
            return View(new LeaveViewModel { Summary = summary, SelectedMonth = ym });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestLeave(LeaveDto model)
        {
            if (string.IsNullOrWhiteSpace(model.EmployeeId) || string.IsNullOrWhiteSpace(model.EmployeeName))
            {
                TempData["ErrorMessage"] = "Please fill in all required leave details.";
                return RedirectToAction(nameof(Index));
            }

            await _leaveService.RequestLeaveAsync(model);
            TempData["SuccessMessage"] = "Leave request submitted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class AppraisalsController : Controller
    {
        private readonly IAppraisalService _appraisalService;
        private readonly ILogger<AppraisalsController> _logger;

        public AppraisalsController(IAppraisalService appraisalService, ILogger<AppraisalsController> logger)
        {
            _appraisalService = appraisalService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? cycle)
        {
            var cy = string.IsNullOrWhiteSpace(cycle) ? "FY26-Annual" : cycle;
            var summary = await _appraisalService.GetAppraisalCycleSummaryAsync(cy);
            return View(new AppraisalsViewModel { Summary = summary });
        }
    }

    public class InnovationController : Controller
    {
        private readonly IInnovationService _innovationService;
        private readonly ILogger<InnovationController> _logger;

        public InnovationController(IInnovationService innovationService, ILogger<InnovationController> logger)
        {
            _innovationService = innovationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? category)
        {
            var assets = await _innovationService.GetAllAssetsAsync(category);
            return View(new InnovationViewModel { Assets = assets, SelectedCategory = category });
        }

        [HttpGet]
        public async Task<IActionResult> Details(string category, string id)
        {
            if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(id)) return NotFound();

            var asset = await _innovationService.GetAssetByIdAsync(category, id);
            if (asset == null) return NotFound();

            return View(asset);
        }

        [HttpGet]
        public async Task<IActionResult> Download(string category, string id)
        {
            if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(id)) return NotFound();

            var stream = await _innovationService.DownloadAssetDocumentAsync(category, id);
            if (stream == null)
            {
                var dummyBytes = System.Text.Encoding.UTF8.GetBytes($"Microsoft Practice Innovation Asset: {category}/{id}\nConfidential - Microsoft Consulting Delivery.");
                stream = new MemoryStream(dummyBytes);
            }

            return File(stream, "application/pdf", $"{id}_Whitepaper.pdf");
        }
    }

    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ReportsViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> ExportResources()
        {
            var result = await _reportService.ExportResourcesCsvAsync();
            return File(result.Data, result.ContentType, result.FileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportProjects()
        {
            var result = await _reportService.ExportProjectsCsvAsync();
            return File(result.Data, result.ContentType, result.FileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportUtilization()
        {
            var result = await _reportService.ExportUtilizationReportCsvAsync();
            return File(result.Data, result.ContentType, result.FileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportHealth()
        {
            var result = await _reportService.ExportPracticeHealthCsvAsync();
            return File(result.Data, result.ContentType, result.FileName);
        }
    }
}
