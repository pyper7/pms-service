using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PMS.Domain.Entities;

namespace PMS.Infrastructure.Data;

public class PmsDbContext : DbContext
{
    private readonly IConfiguration? _configuration;

    public PmsDbContext(DbContextOptions<PmsDbContext> options) : base(options)
    {
    }

    public PmsDbContext(DbContextOptions<PmsDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostOccupancy> PostOccupancies { get; set; }
    public DbSet<OrgUnit> OrgUnits { get; set; }
    public DbSet<OrgUnitHead> OrgUnitHeads { get; set; }
    // Cadre removed
    public DbSet<Qualification> Qualifications { get; set; }
    public DbSet<WorkHistory> WorkHistories { get; set; }
    public DbSet<StatusHistory> StatusHistories { get; set; }
    public DbSet<EmailLog> EmailLogs { get; set; }
    public DbSet<Kra> Kras { get; set; }
    public DbSet<KraOrgUnitAssignment> KraOrgUnitAssignments { get; set; }
    public DbSet<Objective> Objectives { get; set; }
    public DbSet<ObjectiveOrgUnitAssignment> ObjectiveOrgUnitAssignments { get; set; }
    public DbSet<ObjectiveAssignedWeight> ObjectiveAssignedWeights { get; set; }
    public DbSet<Kpi> Kpis { get; set; }
    
    // Appraisal Settings entities
    public DbSet<Competency> Competencies { get; set; }
    public DbSet<Process> Processes { get; set; }
    public DbSet<AppraisalPeriod> AppraisalPeriods { get; set; }
    public DbSet<ScoringWeight> ScoringWeights { get; set; }
    public DbSet<NotificationSettings> NotificationSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure database-specific settings
        ConfigureDatabaseSpecificSettings(modelBuilder);
            // KRA
            modelBuilder.Entity<Kra>(entity =>
            {
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.WorkingYearId).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AppraisalPeriodId).IsRequired().HasMaxLength(50);
                entity.HasMany(e => e.Objectives).WithOne(o => o.Kra!).HasForeignKey(o => o.KraId);
                entity.HasMany(e => e.AssignedOrgUnits).WithOne(a => a.Kra!).HasForeignKey(a => a.KraId);
            });

            modelBuilder.Entity<KraOrgUnitAssignment>(entity =>
            {
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.Property(e => e.OrgUnitType).IsRequired().HasMaxLength(10);
            });

            modelBuilder.Entity<Objective>(entity =>
            {
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.HasMany(e => e.Kpis).WithOne(k => k.Objective!).HasForeignKey(k => k.ObjectiveId);
                entity.HasMany(e => e.AssignedOrgUnits).WithOne(a => a.Objective!).HasForeignKey(a => a.ObjectiveId);
                entity.HasMany(e => e.AssignedWeights).WithOne(a => a.Objective!).HasForeignKey(a => a.ObjectiveId);
            });

            modelBuilder.Entity<ObjectiveOrgUnitAssignment>(entity =>
            {
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.Property(e => e.OrgUnitType).IsRequired().HasMaxLength(10);
            });

            modelBuilder.Entity<ObjectiveAssignedWeight>(entity =>
            {
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.Property(e => e.Scope).IsRequired().HasMaxLength(10);
            });

            modelBuilder.Entity<Kpi>(entity =>
            {
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Unit).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MeasurementType).IsRequired().HasMaxLength(20);
            });


        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.StaffId).IsUnique();
                
            // Cadre relation removed
        });

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure Permission entity
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });


        // Configure RolePermission entity
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
            
            entity.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Post entity
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);
                
            entity.HasOne(e => e.OrgUnit)
                .WithMany()
                .HasForeignKey(e => e.OrgUnitId)
                .OnDelete(DeleteBehavior.Restrict);                
          
                
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Posts)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure PostOccupancy entity
        modelBuilder.Entity<PostOccupancy>(entity =>
        {
            entity.HasKey(e => e.Id);
                
            entity.HasOne(e => e.Post)
                .WithMany()
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.User)
                .WithMany(u => u.PostOccupancies)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure OrgUnit entity
        modelBuilder.Entity<OrgUnit>(entity =>
        {
            entity.HasKey(e => e.Id);
                
            entity.HasOne(e => e.Parent)
                .WithMany(o => o.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure OrgUnitHead entity
        modelBuilder.Entity<OrgUnitHead>(entity =>
        {
            entity.HasKey(e => e.Id);
                
            entity.HasOne(e => e.OrgUnit)
                .WithMany(o => o.HeadHistory)
                .HasForeignKey(e => e.OrgUnitId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Cadre entity removed


        // Configure Qualification entity
        modelBuilder.Entity<Qualification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Qualifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure WorkHistory entity
        modelBuilder.Entity<WorkHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany(u => u.WorkHistory)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure StatusHistory entity
        modelBuilder.Entity<StatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany(u => u.StatusHistory)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure EmailLog entity
        modelBuilder.Entity<EmailLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.EmailType);
            entity.HasIndex(e => new { e.RelatedEntityId, e.RelatedEntityType });
            entity.HasIndex(e => e.NextRetryAt);
        });


        // Configure Appraisal Settings entities
        modelBuilder.Entity<Competency>(entity =>
        {
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Process>(entity =>
        {
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Weight).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<AppraisalPeriod>(entity =>
        {
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<ScoringWeight>(entity =>
        {
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.Section).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Weight).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.HasIndex(e => e.Section).IsUnique();
        });

        modelBuilder.Entity<NotificationSettings>(entity =>
        {
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure global query filters for soft delete
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Permission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Post>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OrgUnit>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OrgUnitHead>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Qualification>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WorkHistory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<StatusHistory>().HasQueryFilter(e => !e.IsDeleted);
    }

    private void ConfigureDatabaseSpecificSettings(ModelBuilder modelBuilder)
    {
        // Database-specific configurations can be added here if needed
        // For now, we'll rely on the default EF Core behavior
    }
}
