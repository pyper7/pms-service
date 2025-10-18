using Microsoft.EntityFrameworkCore;
using PMS.Domain.Entities;
using PMS.Infrastructure.Data;
using BCrypt.Net;

namespace PMS.Infrastructure.Data;

public static class ComprehensiveSeeder
{
    public static async Task SeedAllAsync(PmsDbContext context)
    {
        
        // 2. Seed Organizational Units (no dependencies)
        await SeedOrgUnitsAsync(context);
        
        // 3. Seed Roles (no dependencies)
        await SeedRolesAsync(context);
        
        // 4. Seed Permissions (no dependencies)
        await SeedPermissionsAsync(context);
        
        // 5. Seed Role-Permission relationships
        await SeedRolePermissionsAsync(context);
        
        // 6. Seed Users and Posts (depends on OrgUnits, Cadres, Roles)
        await SeedUsersAndPostsAsync(context);
    }

   

    private static async Task SeedOrgUnitsAsync(PmsDbContext context)
    {
        if (await context.OrgUnits.AnyAsync(o => o.Code == "BOT"))
            return;

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
            
            // Level 3: Human Resources & General Administration Department
            new OrgUnit
            {
                Name = "Human Resources & General Administration Department",
                Type = OrgUnitType.DEPT,
                Description = "Manages human resources and general administration matters",
                Code = "HRGA",
                Level = 3,
                Order = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            }
        };

        // Set up hierarchy relationships
        var boardOfTrustees = orgUnits[0];
        var executiveSecretary = orgUnits[1];
        var hrgaDepartment = orgUnits[2];

        // Executive Secretary reports to Board of Trustees
        executiveSecretary.ParentId = boardOfTrustees.Id;
        executiveSecretary.Parent = boardOfTrustees;

        // HRGA Department reports to Executive Secretary
        hrgaDepartment.ParentId = executiveSecretary.Id;
        hrgaDepartment.Parent = executiveSecretary;

        await context.OrgUnits.AddRangeAsync(orgUnits);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(PmsDbContext context)
    {
        if (await context.Roles.AnyAsync(r => r.Name == "HR"))
            return;

        var roles = new List<Role>
        {
            new Role
            {
                Name = "HR",
                Description = "Human Resources Officer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Role
            {
                Name = "Director",
                Description = "Director",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Role
            {
                Name = "Assistant Director",
                Description = "Assistant Director",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Role
            {
                Name = "Officer",
                Description = "Officer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            }
        };
        
        context.Roles.AddRange(roles);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPermissionsAsync(PmsDbContext context)
    {
        if (await context.Permissions.AnyAsync(p => p.Name == "user_management:read"))
            return;

        var permissions = new List<Permission>
        {
            // User Management Permissions
            new Permission
            {
                Name = "user_management:read",
                Description = "Read user management",
                Module = "user_management",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            
            // Officer Management Permissions
            new Permission
            {
                Name = "officers:view",
                Description = "View officers list",
                Module = "officers",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Permission
            {
                Name = "officers:create",
                Description = "Create new officers",
                Module = "officers",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Permission
            {
                Name = "officers:edit",
                Description = "Update officer information",
                Module = "officers",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Permission
            {
                Name = "officers:delete",
                Description = "Delete officers",
                Module = "officers",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Permission
            {
                Name = "officers:export",
                Description = "Export officers data",
                Module = "officers",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Permission
            {
                Name = "officers:bulk_operations",
                Description = "Perform bulk operations",
                Module = "officers",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            }
        };
        
        context.Permissions.AddRange(permissions);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolePermissionsAsync(PmsDbContext context)
    {
        var hrRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "HR");
        var allPermissions = await context.Permissions.ToListAsync();
        
        if (hrRole == null || !allPermissions.Any()) return;
        
        // Get existing role permissions to avoid duplicates
        var existingRolePermissions = await context.RolePermissions
            .Where(rp => rp.RoleId == hrRole.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        // Add all permissions to HR role that don't already exist
        var newRolePermissions = allPermissions
            .Where(p => !existingRolePermissions.Contains(p.Id))
            .Select(p => new RolePermission
            {
                RoleId = hrRole.Id,
                PermissionId = p.Id,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "system"
            })
            .ToList();

        if (newRolePermissions.Any())
        {
            context.RolePermissions.AddRange(newRolePermissions);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedUsersAndPostsAsync(PmsDbContext context)
    {
        // Check if we need to create a new user or assign existing user to post
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@tetfund.gov.ng");
        var hasActivePostOccupancy = existingUser != null && await context.PostOccupancies
            .AnyAsync(po => po.UserId == existingUser.Id && po.EndDate == null);
        
        if (hasActivePostOccupancy)
            return; // User already has active post assignment

        var hrgaOrgUnit = await context.OrgUnits.FirstOrDefaultAsync(o => o.Code == "HRGA");
        var hrRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "HR");

        if (hrRole == null)
            throw new InvalidOperationException("Required entities not found for user creation");

        // If HRGA department doesn't exist, create it
        if (hrgaOrgUnit == null)
        {
            var execOrgUnit = await context.OrgUnits.FirstOrDefaultAsync(o => o.Code == "EXEC");
            if (execOrgUnit == null)
                throw new InvalidOperationException("Executive Secretary not found. Please ensure organizational units are seeded first.");

            hrgaOrgUnit = new OrgUnit
            {
                Name = "Human Resources & General Administration Department",
                Type = OrgUnitType.DEPT,
                Description = "Manages human resources and general administration matters",
                Code = "HRGA",
                Level = 3,
                Order = 1,
                IsActive = true,
                ParentId = execOrgUnit.Id,
                Parent = execOrgUnit,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };
            context.OrgUnits.Add(hrgaOrgUnit);
            await context.SaveChangesAsync();
        }

        // Create Agency Admin Post (Role is assigned to Post)
        var agencyAdminPost = new Post
        {
            Title = "Agency Admin",
            GradeLevel = "17",
            OrgUnitId = hrgaOrgUnit.Id,
            RoleId = hrRole.Id, // HR Role is assigned to Post
            IsUnique = true,
            Status = PostStatus.Active,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };
        context.Posts.Add(agencyAdminPost);
        await context.SaveChangesAsync();

        User user;
        if (existingUser != null)
        {
            // Use existing user and update if needed
            user = existingUser;
            
            // Update user if they don't have a password or correct post assignment status
            bool needsUpdate = false;
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
                needsUpdate = true;
            }
            if (!user.IsPostAssigned)
            {
                user.IsPostAssigned = true;
                needsUpdate = true;
            }
            
            if (needsUpdate)
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
        }
        else
        {
            // Create new admin user
            user = new User
            {
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@tetfund.gov.ng",
                StaffId = "ADMIN001",
                PhoneNumber = "08012345678",
                GradeLevel = "17",
                IsActive = true,
                IsPostAssigned = true, // User will be assigned to a post
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), // Default password
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Clear any existing inactive post occupancies for this user
        var existingOccupancies = await context.PostOccupancies
            .Where(po => po.UserId == user.Id)
            .ToListAsync();
            
        if (existingOccupancies.Any())
        {
            context.PostOccupancies.RemoveRange(existingOccupancies);
            await context.SaveChangesAsync();
        }

        // Create PostOccupancy (User is assigned to Post)
        var postOccupancy = new PostOccupancy
        {
            PostId = agencyAdminPost.Id, // User is assigned to Agency Admin Post
            UserId = user.Id,
            StartDate = DateTime.UtcNow,
            EndDate = null, // This makes IsCurrentlyOccupying = true
            IsPrimary = true, // This is the user's primary post
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };
        context.PostOccupancies.Add(postOccupancy);
        await context.SaveChangesAsync();
        
        // Verify the post occupancy was created
        var verifyOccupancy = await context.PostOccupancies
            .Include(po => po.User)
            .Include(po => po.Post)
            .FirstOrDefaultAsync(po => po.UserId == user.Id);
            
        Console.WriteLine($"Post Occupancy Created - User: {verifyOccupancy?.User?.FullName}, Post: {verifyOccupancy?.Post?.Title}, EndDate: {verifyOccupancy?.EndDate}, IsCurrentlyOccupying: {verifyOccupancy?.IsCurrentlyOccupying}");
    }
}
