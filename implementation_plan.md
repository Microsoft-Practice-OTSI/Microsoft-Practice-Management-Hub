# Implementation Plan: Microsoft Practice Hub (.NET 8 | ASP.NET Core MVC | 3-Layer Architecture)

Build a production-ready enterprise web application called **Microsoft Practice Hub** with the subtitle *"People • Projects • Skills • Delivery • Growth"*. The application follows a 3-Layer Architecture (Web, Service, Data), connects to **Azure Table Storage** via `Azure.Data.Tables` as primary data store and **Azure Blob Storage** via `Azure.Storage.Blobs` for documents, with no SQL Server or Entity Framework. It features a Microsoft Fluent-inspired enterprise UI with high information density, rich Chart.js dashboards, Gantt-style allocation timelines, comprehensive directory filtering, skills matrix, practice health scoring, and realistic seed data (250+ resources, 35+ projects, 500+ allocations, skills, certifications, case studies, timesheets, appraisals).

---

## User Review Required

> [!IMPORTANT]
> - **Storage Backend**: Primary data access is built using `Azure.Data.Tables` and `Azure.Storage.Blobs`. We provide a built-in automated fallback and seed mechanism (`DevelopmentStorage` / `Azurite` / local in-memory storage manager) so that the application runs immediately out of the box without requiring manual Azure setup or Azurite pre-installation, while still fully supporting real Azure Storage connection strings in production.
> - **Solution Project Names**: We will structure the solution with `MicrosoftPracticeManagement.Web`, `MicrosoftPracticeManagement.Services`, and `MicrosoftPracticeManagement.Data` (or aliases mapped to `MicrosoftPracticeHub`) with full solution references.

---

## Architecture & Layer Responsibilities

```
MicrosoftPracticeHub
│
├── MicrosoftPracticeHub.Web (ASP.NET Core MVC .NET 8)
│   ├── Controllers (Dashboard, Resources, Projects, Skills, PracticeHealth, Timesheets, Leave, Appraisals, Innovation, Reports)
│   ├── ViewModels (Strongly-typed ViewModels for each view and partial)
│   ├── Views (Fluent UI layout, Razor views, reusable partial views, modal dialogs)
│   ├── wwwroot (css/fluent.css, js/dashboard-charts.js, js/timeline-gantt.js, js/site.js, icons)
│   └── Program.cs (DI registrations, Azure Storage client singletons)
│
├── MicrosoftPracticeHub.Service (Business Logic Layer)
│   ├── Interfaces (IDashboardService, IResourceService, IProjectService, ISkillService, IPracticeHealthService, ITimesheetService, ILeaveService, IAppraisalService, IInnovationService, IReportService)
│   ├── Services (Calculations, Aggregations, Utilization, Bench detection, Practice Health score, Staffing gaps)
│   ├── DTOs (ResourceDto, ProjectDto, AllocationDto, SkillDto, HealthScoreDto, DashboardSummaryDto, etc.)
│   └── Helpers & Mappings (Auto-mapping between Data Entities and Service DTOs)
│
└── MicrosoftPracticeHub.Data (Data Access Layer)
    ├── Entities (ResourceEntity, ProjectEntity, AllocationEntity, SkillEntity, InnovationEntity, TimesheetEntity, etc. implementing ITableEntity)
    ├── Storage (TableStorageContext, BlobStorageContext, StorageInitializer, SeedDataGenerator)
    ├── Repositories (Generic TableRepository<T>, ResourceRepository, ProjectRepository, AllocationRepository, SkillRepository, InnovationRepository, etc.)
    └── BlobStorage (AzureBlobStorageService, DocumentManager)
```

---

## Proposed Changes

### 1. Data Layer (`MicrosoftPracticeManagement.Data`)

#### [MODIFY] [`MicrosoftPracticeManagement.Data.csproj`](file:///c:/Users/Suman%20Bhavansi/source/repos/MicrosoftPracticeManagement/MicrosoftPracticeManagement.Data/MicrosoftPracticeManagement.Data.csproj)
- Add NuGet dependencies:
  - `Azure.Data.Tables` (v12.8.3+)
  - `Azure.Storage.Blobs` (v12.20.0+)
  - `Microsoft.Extensions.Configuration.Abstractions`
  - `Microsoft.Extensions.Logging.Abstractions`

#### [NEW] Entities (`MicrosoftPracticeManagement.Data/Entities/`)
- `ResourceEntity.cs`: `PartitionKey = ManagerId`, `RowKey = EmployeeId`, `Name`, `Designation`, `PrimarySkill`, `SecondarySkill`, `AllocationPercent`, `Billable`, `ExperienceYears`, `Location`, `Availability`, `Email`, `Department`, `ResumeBlobUrl`, `ProfileImageUrl`, `HireDate`, `Status`.
- `ProjectEntity.cs`: `PartitionKey = Account`, `RowKey = ProjectId`, `ProjectName`, `PM`, `StartDate`, `EndDate`, `Status`, `StaffingGap`, `Description`, `TotalAllocated`, `RequiredResources`, `Health`, `TechnologyStack`.
- `AllocationEntity.cs`: `PartitionKey = ProjectId`, `RowKey = EmployeeId`, `AllocationPercent`, `Role`, `StartDate`, `EndDate`, `Billable`, `Notes`.
- `SkillEntity.cs`: `PartitionKey = EmployeeId`, `RowKey = SkillName`, `Level` (Beginner, Intermediate, Advanced, Expert), `Certification`, `ExpiryDate`, `VerifiedDate`, `YearsOfExperience`.
- `InnovationEntity.cs`: `PartitionKey = Category` (CaseStudy, Accelerator), `RowKey = Id`, `Title`, `BusinessProblem`, `Solution`, `TechnologyTags`, `SharePointLink`, `DocumentBlobUrl`, `DownloadsCount`.
- `TimesheetEntity.cs`: `PartitionKey = YearMonth`, `RowKey = EmployeeId`, `SubmissionStatus`, `HoursLogged`, `SubmissionDate`, `ApproverName`.
- `LeaveEntity.cs`: `PartitionKey = YearMonth`, `RowKey = LeaveId`, `EmployeeId`, `EmployeeName`, `StartDate`, `EndDate`, `LeaveType`, `Status`.
- `AppraisalEntity.cs`: `PartitionKey = CycleYear`, `RowKey = EmployeeId`, `Rating`, `PromotionReadiness`, `FeedbackSummary`, `ReviewStatus`.

#### [NEW] Repositories (`MicrosoftPracticeManagement.Data/Repositories/`)
- `ITableRepository<T>.cs` & `TableRepository<T>.cs`: Generic async CRUD and OData query wrapper for `TableClient`.
- `IResourceRepository.cs` & `ResourceRepository.cs`
- `IProjectRepository.cs` & `ProjectRepository.cs`
- `IAllocationRepository.cs` & `AllocationRepository.cs`
- `ISkillRepository.cs` & `SkillRepository.cs`
- `IInnovationRepository.cs` & `InnovationRepository.cs`
- `ITimesheetRepository.cs`, `ILeaveRepository.cs`, `IAppraisalRepository.cs`

#### [NEW] Blob Storage (`MicrosoftPracticeManagement.Data/BlobStorage/`)
- `IBlobStorageService.cs` & `AzureBlobStorageService.cs`: Container initialization (`resumes`, `certificates`, `case-studies`, `accelerators`, `exports`), file upload, stream read, download URL generation.

#### [NEW] Storage Context & Seeder (`MicrosoftPracticeManagement.Data/Storage/`)
- `StorageContext.cs`: Manages `TableServiceClient` and `BlobServiceClient`.
- `DataSeeder.cs`: Seeds ~250 realistic consulting resources (with authentic Microsoft ecosystem roles, managers, locations, utilization rates), 35+ projects across realistic enterprise accounts, 500+ allocations, 120+ skill matrices, 40+ certifications, plus innovation case studies, timesheet data, leave records, and appraisals.

---

### 2. Service Layer (`MicrosoftPracticeManagement.Services`)

#### [MODIFY] [`MicrosoftPracticeManagement.Services.csproj`](file:///c:/Users/Suman%20Bhavansi/source/repos/MicrosoftPracticeManagement/MicrosoftPracticeManagement.Services/MicrosoftPracticeManagement.Services.csproj)
- Add project reference to `MicrosoftPracticeManagement.Data`.

#### [NEW] DTOs (`MicrosoftPracticeManagement.Services/DTOs/`)
- `DashboardSummaryDto.cs`: KPI metrics, trend percentages, health score, charts data payloads, attention items.
- `ResourceDto.cs`, `ProjectDto.cs`, `AllocationDto.cs`, `SkillDto.cs`, `CertificationDto.cs`
- `PracticeHealthDto.cs`, `GanttTimelineDto.cs`, `SkillsMatrixDto.cs`, `TimesheetComplianceDto.cs`, `LeaveSummaryDto.cs`, `AppraisalSummaryDto.cs`, `InnovationDto.cs`, `ExportResultDto.cs`.

#### [NEW] Services & Business Logic (`MicrosoftPracticeManagement.Services/Services/`)
- `IDashboardService.cs` & `DashboardService.cs`:
  - Aggregations: Total resources, billable count, bench count, average experience, average utilization, active projects, active accounts.
  - Practice Health Score (0-100) weighted algorithm: Utilization (35%), Billability (25%), Bench ratio (20%), Skill/Cert readiness (20%).
  - Attention Required analysis (bench > 30 days, expiring allocations in 30d, timesheet gaps, certification expiries in 60d, staffing gaps).
  - Chart payload generators for Chart.js (Donut, Bar, Stacked Bar, Line).
- `IResourceService.cs` & `ResourceService.cs`: Multi-facet filtering, sorting, pagination, profile view with allocation history and skills.
- `IProjectService.cs` & `ProjectService.cs`: Project cards/table, staffing gap calculation, timeline/Gantt data builder, overallocated & bench highlights.
- `ISkillService.cs` & `SkillService.cs`: Skills matrix, certification expiry tracking, tech gap analysis.
- `IPracticeHealthService.cs` & `PracticeHealthService.cs`: 12-month utilization trends, billability trends, experience tier distribution (0-3, 3-5, 5-8, 8-12, 12+), attrition metrics, future hiring demand.
- `ITimesheetService.cs`, `ILeaveService.cs`, `IAppraisalService.cs`, `IInnovationService.cs`
- `IReportService.cs` & `ReportService.cs`: Generates Excel/CSV and printable reports for Resources, Projects, Utilization, and Health, storing exports into the `exports` Blob container.

---

### 3. Web Layer (`MicrosoftPracticeManagement.Web`)

#### [MODIFY] [`MicrosoftPracticeManagement.Web.csproj`](file:///c:/Users/Suman%20Bhavansi/source/repos/MicrosoftPracticeManagement/MicrosoftPracticeManagement.Web/MicrosoftPracticeManagement.Web.csproj)
- Add project reference to `MicrosoftPracticeManagement.Services` and `MicrosoftPracticeManagement.Data`.

#### [NEW] ViewModels (`MicrosoftPracticeManagement.Web/ViewModels/`)
- `DashboardViewModel.cs`, `ResourceListViewModel.cs`, `ResourceDetailsViewModel.cs`, `ProjectListViewModel.cs`, `ProjectDetailsViewModel.cs`, `GanttTimelineViewModel.cs`, `SkillsMatrixViewModel.cs`, `PracticeHealthViewModel.cs`, `TimesheetsViewModel.cs`, `LeaveViewModel.cs`, `AppraisalsViewModel.cs`, `InnovationViewModel.cs`, `ReportsViewModel.cs`.

#### [NEW] Controllers (`MicrosoftPracticeManagement.Web/Controllers/`)
- `DashboardController.cs` (Index, GetChartDataJson)
- `ResourcesController.cs` (Index, Details, QuickView, Create, Edit, DownloadResume)
- `ProjectsController.cs` (Index, Details, Timeline, Create, Edit)
- `SkillsController.cs` (Index, Matrix, Certifications, MissingSkills)
- `PracticeHealthController.cs` (Index, HiringDemand)
- `TimesheetsController.cs` (Index, Approvals)
- `LeaveController.cs` (Index, Calendar)
- `AppraisalsController.cs` (Index, Matrix)
- `InnovationController.cs` (Index, Details, DownloadAsset)
- `ReportsController.cs` (Index, ExportResources, ExportProjects, ExportHealth)

#### [NEW] Views & UI Layout (`MicrosoftPracticeManagement.Web/Views/`)
- `_Layout.cshtml`: Microsoft Fluent enterprise design with:
  - Left navigation sidebar (Dashboard, Resources, Projects & Allocation, Practice Health, Timesheets, Leave, Appraisals, Skills, Case Studies, Accelerators, Opportunities, Reports, Administration).
  - Top header: Global Search box with live keyboard shortcuts, Notifications center with badges, Practice Selector dropdown ("Microsoft Azure & Cloud", "Data & AI", "Power Platform"), User Profile flyout.
- `Dashboard/Index.cshtml`:
  - 8 KPI cards with trend indicators (Total Resources, Billable, Non-Billable, Bench, Avg Experience, Avg Utilization, Active Projects, Active Accounts).
  - Practice Health Score visual dial (e.g. 87/100, Healthy/Attention/Critical).
  - 4 Chart.js charts: Utilization Donut, Technology Distribution Bar, Capacity vs Demand Stacked Bar, 12-Month Utilization Trend Line.
  - Interactive Attention Required panel with badge counters.
- `Resources/Index.cshtml` & `Resources/Details.cshtml`:
  - Searchable directory with multi-select filters (Account, Project, Technology, Location, Designation, Status).
  - Rich table with pagination, sorting, Quick View modal, avatar badges, billable indicators.
  - Details page with experience breakdown, skills matrix, certifications with expiry alerts, project timeline, and resume download.
- `Projects/Index.cshtml` & `Projects/Timeline.cshtml`:
  - Switchable Grid Cards & Table views.
  - Interactive Gantt timeline visualization highlighting overallocated, underallocated, and bench resources.
- `Skills/Index.cshtml` & `Skills/Matrix.cshtml`:
  - Beginner, Intermediate, Advanced, Expert heatmaps, certification tracker, missing skills gap report.
- `PracticeHealth/Index.cshtml`:
  - Experience tier graphs, 12-month billability trends, attrition curves, future hiring needs forecast.
- `Timesheets/Index.cshtml`, `Leave/Index.cshtml`, `Appraisals/Index.cshtml`, `Innovation/Index.cshtml`, `Reports/Index.cshtml`.

#### [NEW] Styling & Scripts (`MicrosoftPracticeManagement.Web/wwwroot/`)
- `css/fluent-theme.css`: Microsoft Fluent 2 design tokens, Segoe UI typography, elevation shadows, modern card layouts, status colors (Green `#107c41`, Amber `#d83b01`, Red `#d13438`, Azure Blue `#0078d4`, Purple `#5c2d91`).
- `js/dashboard.js`: Chart.js configuration, real-time KPI animations, attention filter triggers.
- `js/timeline.js`: Gantt chart timeline rendering with hover tooltips and resource capacity bars.
- `js/site.js`: Global search, quick view modal handler, export triggers, toast notifications.

#### [MODIFY] [`appsettings.json`](file:///c:/Users/Suman%20Bhavansi/source/repos/MicrosoftPracticeManagement/MicrosoftPracticeManagement.Web/appsettings.json) & [`Program.cs`](file:///c:/Users/Suman%20Bhavansi/source/repos/MicrosoftPracticeManagement/MicrosoftPracticeManagement.Web/Program.cs)
- Connection string settings: `TableStorageConnection`, `BlobStorageConnection`, `StorageAccountName`, `BlobContainerPrefix`.
- Singleton registration of `TableServiceClient` and `BlobServiceClient`.
- Service and Repository registrations for clean Dependency Injection.
- Automatic database table creation and seeder execution on startup.

---

## Verification Plan

### Automated Build & Test
- Build all 3 projects via `dotnet build MicrosoftPracticeManagement.slnx` or `dotnet build MicrosoftPracticeManagement.Web`.
- Verify 0 compile errors and 0 warnings on .NET 8.

### Functional & UI Verification
- Run the web application using `dotnet run --project MicrosoftPracticeManagement.Web`.
- Verify database tables initialized and 250+ resources, 35+ projects, 500+ allocations, skills, certifications seeded.
- Validate:
  1. **Dashboard**: All 8 KPI cards calculate correct percentages; Health Score dial displays calculated score; 4 Chart.js charts render correctly; Attention Required panel displays active alerts.
  2. **Resources**: Filter by technology, designation, location; test Quick View modal and Details view with resume download.
  3. **Projects & Timeline**: Verify card/table views, verify Gantt timeline with capacity and bench highlights.
  4. **Skills Matrix**: Test level filters, certification expiry tracker, missing skills view.
  5. **Practice Health**: Check experience distribution, hiring requirements, billability trend.
  6. **Innovation, Timesheets, Leave, Appraisals**: Verify solution cards and compliance tracking.
  7. **Reports**: Verify CSV/Excel export download for resources and project summaries.
