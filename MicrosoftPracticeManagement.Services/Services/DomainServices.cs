using Microsoft.Extensions.Logging;
using MicrosoftPracticeManagement.Data.BlobStorage;
using MicrosoftPracticeManagement.Data.Entities;
using MicrosoftPracticeManagement.Data.Repositories;
using MicrosoftPracticeManagement.Services.DTOs;
using MicrosoftPracticeManagement.Services.Interfaces;

namespace MicrosoftPracticeManagement.Services.Services
{
    public class TimesheetService : ITimesheetService
    {
        private readonly ITimesheetRepository _timesheetRepository;
        private readonly ILogger<TimesheetService> _logger;

        public TimesheetService(ITimesheetRepository timesheetRepository, ILogger<TimesheetService> logger)
        {
            _timesheetRepository = timesheetRepository;
            _logger = logger;
        }

        public async Task<TimesheetComplianceDto> GetComplianceDashboardAsync(string? yearMonth)
        {
            var ym = string.IsNullOrWhiteSpace(yearMonth) ? DateTime.UtcNow.ToString("yyyy-MM") : yearMonth;
            var list = (await _timesheetRepository.GetByYearMonthAsync(ym)).ToList();

            var total = list.Count > 0 ? list.Count : 1;
            var approvedOrSubmitted = list.Count(t => t.Status == "Approved" || t.Status == "Submitted");
            var late = list.Count(t => t.Status == "Late");
            var pending = list.Count(t => t.Status == "Pending Approval");

            var complianceRate = Math.Round(((decimal)approvedOrSubmitted / total) * 100, 1);

            return new TimesheetComplianceDto
            {
                ComplianceRatePercent = complianceRate,
                TotalSubmissions = list.Count,
                LateSubmissionsCount = late,
                PendingApprovalsCount = pending,
                RecentTimesheets = list.Select(t => new TimesheetDto
                {
                    YearMonth = t.YearMonth,
                    EmployeeId = t.EmployeeId,
                    EmployeeName = t.EmployeeName,
                    ProjectName = t.ProjectName,
                    HoursLogged = t.HoursLogged,
                    ExpectedHours = t.ExpectedHours,
                    Status = t.Status,
                    SubmissionDate = t.SubmissionDate,
                    ApproverName = t.ApproverName,
                    Comments = t.Comments
                }).ToList()
            };
        }

        public async Task SubmitTimesheetAsync(TimesheetDto dto)
        {
            var entity = new TimesheetEntity
            {
                PartitionKey = dto.YearMonth,
                RowKey = dto.EmployeeId,
                EmployeeName = dto.EmployeeName,
                ProjectName = dto.ProjectName,
                HoursLogged = dto.HoursLogged,
                ExpectedHours = dto.ExpectedHours,
                Status = "Submitted",
                SubmissionDate = DateTime.UtcNow,
                ApproverName = dto.ApproverName,
                Comments = dto.Comments
            };

            await _timesheetRepository.UpsertAsync(entity);
        }

        public async Task ApproveTimesheetAsync(string yearMonth, string employeeId, string approverName)
        {
            var entity = await _timesheetRepository.GetByIdAsync(yearMonth, employeeId);
            if (entity != null)
            {
                entity.Status = "Approved";
                entity.ApproverName = approverName;
                await _timesheetRepository.UpdateAsync(entity);
            }
        }
    }

    public class LeaveService : ILeaveService
    {
        private readonly ILeaveRepository _leaveRepository;
        private readonly ILogger<LeaveService> _logger;

        public LeaveService(ILeaveRepository leaveRepository, ILogger<LeaveService> logger)
        {
            _leaveRepository = leaveRepository;
            _logger = logger;
        }

        public async Task<LeaveSummaryDto> GetLeaveSummaryAsync(string? yearMonth)
        {
            var ym = string.IsNullOrWhiteSpace(yearMonth) ? DateTime.UtcNow.ToString("yyyy-MM") : yearMonth;
            var leaves = (await _leaveRepository.GetByYearMonthAsync(ym)).ToList();

            var today = DateTime.UtcNow.Date;
            var onLeaveToday = leaves.Count(l => l.StartDate.Date <= today && l.EndDate.Date >= today && l.Status == "Approved");

            return new LeaveSummaryDto
            {
                TotalOnLeaveToday = onLeaveToday,
                TotalOnLeaveThisMonth = leaves.Count(l => l.Status == "Approved"),
                UpcomingLeaves = leaves.Select(l => new LeaveDto
                {
                    LeaveId = l.LeaveId,
                    EmployeeId = l.EmployeeId,
                    EmployeeName = l.EmployeeName,
                    ManagerName = l.ManagerName,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    DaysCount = l.DaysCount,
                    LeaveType = l.LeaveType,
                    Status = l.Status,
                    Reason = l.Reason
                }).OrderBy(l => l.StartDate).ToList()
            };
        }

        public async Task RequestLeaveAsync(LeaveDto dto)
        {
            var entity = new LeaveEntity
            {
                PartitionKey = dto.StartDate.ToString("yyyy-MM"),
                RowKey = string.IsNullOrWhiteSpace(dto.LeaveId) ? $"LV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6]}" : dto.LeaveId,
                EmployeeId = dto.EmployeeId,
                EmployeeName = dto.EmployeeName,
                ManagerName = dto.ManagerName,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                DaysCount = dto.DaysCount,
                LeaveType = dto.LeaveType,
                Status = "Pending",
                Reason = dto.Reason
            };

            await _leaveRepository.AddAsync(entity);
        }
    }

    public class AppraisalService : IAppraisalService
    {
        private readonly IAppraisalRepository _appraisalRepository;
        private readonly ILogger<AppraisalService> _logger;

        public AppraisalService(IAppraisalRepository appraisalRepository, ILogger<AppraisalService> logger)
        {
            _appraisalRepository = appraisalRepository;
            _logger = logger;
        }

        public async Task<AppraisalSummaryDto> GetAppraisalCycleSummaryAsync(string? cycleYear)
        {
            var cy = string.IsNullOrWhiteSpace(cycleYear) ? "FY26-Annual" : cycleYear;
            var list = (await _appraisalRepository.GetByCycleYearAsync(cy)).ToList();

            var completed = list.Count(a => a.ReviewStatus == "Completed");
            var promotionReady = list.Count(a => a.PromotionReadiness.Contains("Ready Now") || a.PromotionReadiness.Contains("High Performer"));
            var avgRating = list.Count > 0 ? Math.Round(list.Average(a => a.PerformanceRating), 2) : 0;

            return new AppraisalSummaryDto
            {
                CurrentCycle = cy,
                TotalAppraisals = list.Count,
                CompletedAppraisals = completed,
                PromotionReadyCount = promotionReady,
                AverageRating = avgRating,
                Appraisals = list.Select(a => new AppraisalDto
                {
                    CycleYear = a.CycleYear,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.EmployeeName,
                    Designation = a.Designation,
                    ManagerName = a.ManagerName,
                    PerformanceRating = a.PerformanceRating,
                    PromotionReadiness = a.PromotionReadiness,
                    TargetDesignation = a.TargetDesignation,
                    ReviewStatus = a.ReviewStatus,
                    KeyStrengths = a.KeyStrengths,
                    DevelopmentAreas = a.DevelopmentAreas,
                    FeedbackSummary = a.FeedbackSummary
                }).OrderByDescending(a => a.PerformanceRating).ToList()
            };
        }
    }

    public class InnovationService : IInnovationService
    {
        private readonly IInnovationRepository _innovationRepository;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<InnovationService> _logger;

        public InnovationService(
            IInnovationRepository innovationRepository,
            IBlobStorageService blobStorageService,
            ILogger<InnovationService> logger)
        {
            _innovationRepository = innovationRepository;
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        public async Task<IEnumerable<InnovationDto>> GetAllAssetsAsync(string? category)
        {
            var all = (await _innovationRepository.GetAllAsync()).ToList();
            if (!string.IsNullOrWhiteSpace(category))
            {
                all = all.Where(i => i.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return all.Select(i => new InnovationDto
            {
                Id = i.RowKey,
                Category = i.PartitionKey,
                Title = i.Title,
                ClientAccount = i.ClientAccount,
                BusinessProblem = i.BusinessProblem,
                Solution = i.Solution,
                BusinessValue = i.BusinessValue,
                TechnologyTags = i.TechnologyTags,
                SharePointLink = i.SharePointLink,
                DocumentBlobUrl = i.DocumentBlobUrl,
                AuthorName = i.AuthorName,
                AuthorRole = i.AuthorRole,
                DownloadsCount = i.DownloadsCount,
                RatingStars = i.RatingStars,
                PublishedDate = i.PublishedDate
            }).ToList();
        }

        public async Task<InnovationDto?> GetAssetByIdAsync(string category, string id)
        {
            var entity = await _innovationRepository.GetByIdAsync(category, id);
            if (entity == null) return null;

            return new InnovationDto
            {
                Id = entity.RowKey,
                Category = entity.PartitionKey,
                Title = entity.Title,
                ClientAccount = entity.ClientAccount,
                BusinessProblem = entity.BusinessProblem,
                Solution = entity.Solution,
                BusinessValue = entity.BusinessValue,
                TechnologyTags = entity.TechnologyTags,
                SharePointLink = entity.SharePointLink,
                DocumentBlobUrl = entity.DocumentBlobUrl,
                AuthorName = entity.AuthorName,
                AuthorRole = entity.AuthorRole,
                DownloadsCount = entity.DownloadsCount,
                RatingStars = entity.RatingStars,
                PublishedDate = entity.PublishedDate
            };
        }

        public async Task<Stream?> DownloadAssetDocumentAsync(string category, string id)
        {
            var container = category.Equals("Accelerator", StringComparison.OrdinalIgnoreCase) ? "accelerators" : "case-studies";
            var fileName = $"{id}_Whitepaper.pdf";
            return await _blobStorageService.DownloadFileAsync(container, fileName);
        }
    }
}
