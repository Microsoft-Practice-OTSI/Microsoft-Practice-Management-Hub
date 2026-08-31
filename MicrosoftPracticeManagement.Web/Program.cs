using MicrosoftPracticeManagement.Data.BlobStorage;
using MicrosoftPracticeManagement.Data.Repositories;
using MicrosoftPracticeManagement.Data.Storage;
using MicrosoftPracticeManagement.Services.Interfaces;
using MicrosoftPracticeManagement.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// Storage Configuration
var storageConfig = new StorageConfiguration();
builder.Configuration.GetSection("Storage").Bind(storageConfig);

// Support environment variables override
var envTableConn = Environment.GetEnvironmentVariable("TableStorageConnection");
if (!string.IsNullOrWhiteSpace(envTableConn)) storageConfig.TableStorageConnection = envTableConn;

var envBlobConn = Environment.GetEnvironmentVariable("BlobStorageConnection");
if (!string.IsNullOrWhiteSpace(envBlobConn)) storageConfig.BlobStorageConnection = envBlobConn;

builder.Services.AddSingleton(storageConfig);
builder.Services.AddSingleton<StorageContext>();
builder.Services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();

// Register Repositories
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IAllocationRepository, AllocationRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<IInnovationRepository, InnovationRepository>();
builder.Services.AddScoped<ITimesheetRepository, TimesheetRepository>();
builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();
builder.Services.AddScoped<IAppraisalRepository, AppraisalRepository>();

// Register Seeder
builder.Services.AddScoped<DataSeeder>();

// Register Domain Services
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IPracticeHealthService, PracticeHealthService>();
builder.Services.AddScoped<ITimesheetService, TimesheetService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<IAppraisalService, AppraisalService>();
builder.Services.AddScoped<IInnovationService, InnovationService>();
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

// Seed initial sample data if empty
// using (var scope = app.Services.CreateScope())
// {
//     var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
//     await seeder.SeedAsync();
// }

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
