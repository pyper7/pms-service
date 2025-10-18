using Microsoft.EntityFrameworkCore;
using PMS.Domain.Entities;
using PMS.Infrastructure.Data;

namespace PMS.Infrastructure.Data;

public static class OrgUnitSeeder
{
    public static async Task SeedAsync(PmsDbContext context)
    {
        // Check if Board of Trustees already exists (our main indicator)
        if (await context.OrgUnits.AnyAsync(o => o.Code == "BOT"))
        {
            return; // Data already seeded
        }

        var orgUnits = new List<OrgUnit>
        {
            // Level 1: Board of Trustees
            new OrgUnit
            {
                Name = "Board of Trustees",
                Type = OrgUnitType.BOARD,
                Description = "The governing body of the organization",
                Code = "BOT",
                Level = 1,
                Order = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            
            // Level 2: Executive Secretary
            new OrgUnit
            {
                Name = "Executive Secretary",
                Type = OrgUnitType.EXECUTIVE,
                Description = "Executive leadership of the organization",
                Code = "EXEC",
                Level = 2,
                Order = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            
            // Level 3: Sample Departments
            new OrgUnit
            {
                Name = "Human Resources Department",
                Type = OrgUnitType.DEPT,
                Description = "Manages human resources and personnel matters",
                Code = "HR",
                Level = 3,
                Order = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            
            new OrgUnit
            {
                Name = "Finance Department",
                Type = OrgUnitType.DEPT,
                Description = "Manages financial operations and budgeting",
                Code = "FIN",
                Level = 3,
                Order = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            
            new OrgUnit
            {
                Name = "Information Technology Department",
                Type = OrgUnitType.DEPT,
                Description = "Manages IT infrastructure and systems",
                Code = "IT",
                Level = 3,
                Order = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            
            new OrgUnit
            {
                Name = "Administration Department",
                Type = OrgUnitType.DEPT,
                Description = "Handles administrative and operational matters",
                Code = "ADMIN",
                Level = 3,
                Order = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            
            new OrgUnit
            {
                Name = "Legal Department",
                Type = OrgUnitType.DEPT,
                Description = "Provides legal counsel and compliance oversight",
                Code = "LEGAL",
                Level = 3,
                Order = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            }
        };

        // Set up hierarchy relationships
        var boardOfTrustees = orgUnits[0];
        var executiveSecretary = orgUnits[1];
        var departments = orgUnits.Skip(2).ToList();

        // Executive Secretary reports to Board of Trustees
        executiveSecretary.ParentId = boardOfTrustees.Id;
        executiveSecretary.Parent = boardOfTrustees;

        // All departments report to Executive Secretary
        foreach (var dept in departments)
        {
            dept.ParentId = executiveSecretary.Id;
            dept.Parent = executiveSecretary;
        }

        await context.OrgUnits.AddRangeAsync(orgUnits);
        await context.SaveChangesAsync();
    }
}
