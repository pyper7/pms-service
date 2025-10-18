using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Services;
using PMS.Domain.Entities;
using PMS.Infrastructure.Repositories;
using System.Text;
using PMS.API.Middlewares;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Filters;
using PMS.Infrastructure.Data;
using PMS.Application.Models.Notifications;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseWrapperFilter>();
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var response = ApiResponse.Fail("Validation failed", errorCode: "VALIDATION_ERROR", errors: errors);
        return new BadRequestObjectResult(response);
    };
});
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "PMS API", 
        Version = "v1",
        Description = "Performance Management System API for OHCSF"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Entity Framework with database provider selection
builder.Services.AddDatabase(builder.Configuration);

// Configure JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Configure Email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSingleton<EmailSettings>(provider => 
    provider.GetRequiredService<IOptions<EmailSettings>>().Value);

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidAudience = jwtSettings?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings?.SecretKey ?? ""))
    };
});

builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<IRepository<User>, Repository<User>>();
builder.Services.AddScoped<IRepository<Role>, Repository<Role>>();
builder.Services.AddScoped<IRepository<Permission>, Repository<Permission>>();
builder.Services.AddScoped<IRepository<RolePermission>, Repository<RolePermission>>();
builder.Services.AddScoped<IRepository<Post>, Repository<Post>>();
builder.Services.AddScoped<IRepository<PostOccupancy>, Repository<PostOccupancy>>();
builder.Services.AddScoped<IRepository<OrgUnit>, Repository<OrgUnit>>();
builder.Services.AddScoped<IRepository<OrgUnitHead>, Repository<OrgUnitHead>>();
builder.Services.AddScoped<IRepository<Qualification>, Repository<Qualification>>();
builder.Services.AddScoped<IRepository<WorkHistory>, Repository<WorkHistory>>();
builder.Services.AddScoped<IRepository<StatusHistory>, Repository<StatusHistory>>();
builder.Services.AddScoped<IRepository<EmailLog>, Repository<EmailLog>>();
builder.Services.AddScoped<IRepository<Kra>, Repository<Kra>>();
builder.Services.AddScoped<IRepository<KraOrgUnitAssignment>, Repository<KraOrgUnitAssignment>>();
builder.Services.AddScoped<IRepository<Objective>, Repository<Objective>>();
builder.Services.AddScoped<IRepository<ObjectiveAssignedWeight>, Repository<ObjectiveAssignedWeight>>();
builder.Services.AddScoped<IRepository<Kpi>, Repository<Kpi>>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IOrgUnitRepository, OrgUnitRepository>();
builder.Services.AddScoped<IOfficerRepository, OfficerRepository>();
builder.Services.AddScoped<IKraRepository, KraRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IOrgUnitService, OrgUnitService>();
builder.Services.AddScoped<IOfficerService, OfficerService>();
builder.Services.AddScoped<IKraService, KraService>();
builder.Services.AddScoped<IObjectiveService, ObjectiveService>();
builder.Services.AddScoped<IKpiService, KpiService>();

// Appraisal Settings services
builder.Services.AddScoped<IRepository<Competency>, Repository<Competency>>();
builder.Services.AddScoped<IRepository<Process>, Repository<Process>>();
builder.Services.AddScoped<IRepository<AppraisalPeriod>, Repository<AppraisalPeriod>>();
builder.Services.AddScoped<IRepository<ScoringWeight>, Repository<ScoringWeight>>();
builder.Services.AddScoped<IRepository<NotificationSettings>, Repository<NotificationSettings>>();
builder.Services.AddScoped<ICompetencyRepository, CompetencyRepository>();
builder.Services.AddScoped<IProcessRepository, ProcessRepository>();
builder.Services.AddScoped<IAppraisalPeriodRepository, AppraisalPeriodRepository>();
builder.Services.AddScoped<IScoringWeightRepository, ScoringWeightRepository>();
builder.Services.AddScoped<INotificationSettingsRepository, NotificationSettingsRepository>();
builder.Services.AddScoped<ICompetencyService, CompetencyService>();
builder.Services.AddScoped<IProcessService, ProcessService>();
builder.Services.AddScoped<IAppraisalPeriodService, AppraisalPeriodService>();
builder.Services.AddScoped<IScoringWeightService, ScoringWeightService>();
builder.Services.AddScoped<INotificationSettingsService, NotificationSettingsService>();

// Email services
builder.Services.AddScoped<IEmailLogRepository, EmailLogRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddSingleton<IEmailQueueService, EmailQueueService>();

// Reservation and caching for onboarding
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IReservationService, ReservationService>();

// Background services
builder.Services.AddHostedService<EmailRetryBackgroundService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PMS API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseGlobalExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PmsDbContext>();
    context.Database.EnsureCreated();
    
    // Clear departments before seeding (commented out to keep HRGA department)
    // await DatabaseCleanup.ClearDepartmentsAsync(context);
    
    // Use comprehensive seeder to avoid conflicts
    await ComprehensiveSeeder.SeedAllAsync(context);
    await PermissionSeeder.SeedHrAdminPermissionsAsync(context);
}

await app.RunAsync();
