using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using PMS.Domain.Entities;

namespace PMS.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(PmsDbContext context)
    {
        

        if (!context.Users.Any())
        {
            // Get the HR Department from OrgUnitSeeder
            var orgUnit = await context.OrgUnits.FirstOrDefaultAsync(o => o.Code == "HR");
            if (orgUnit == null)
            {
                throw new InvalidOperationException("HR Department not found. Please ensure OrgUnitSeeder runs before SeedData.");
            }

          

            // Create roles
            var roles = new List<Role>
            {
                new Role
                {
                    Name = "HR",
                    Description = "Human Resources Officer",
                    IsActive = true
                },
                new Role
                {
                    Name = "Director",
                    Description = "Director",
                    IsActive = true
                },
                new Role
                {
                    Name = "Deputy Director",
                    Description = "Deputy Director",
                    IsActive = true
                },
                new Role
                {
                    Name = "Assistant Director",
                    Description = "Assistant Director",
                    IsActive = true
                },
                new Role
                {
                    Name = "Officer",
                    Description = "Officer",
                    IsActive = true
                }
            };
            
            context.Roles.AddRange(roles);
            await context.SaveChangesAsync();

            // Create test Permission
            var permission = new Permission
            {
                Name = "user_management:read",
                Description = "Read user management",
                Module = "user_management"
            };
            context.Permissions.Add(permission);
            await context.SaveChangesAsync();

            // Create RolePermission for HR role
            var hrRole = roles.First(r => r.Name == "HR");
            var rolePermission = new RolePermission
            {
                RoleId = hrRole.Id,
                PermissionId = permission.Id,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "system"
            };
            context.RolePermissions.Add(rolePermission);

            // Create HR Admin Post
            var hrAdminPost = new Post
            {
                Title = "HR Admin",
                GradeLevel = "13",
                OrgUnitId = orgUnit.Id,
                RoleId = hrRole.Id,
                IsUnique = true,
                Status = PostStatus.Active
            };
            context.Posts.Add(hrAdminPost);
            await context.SaveChangesAsync();

            // Create HR user
            var hrUser = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "hr@tetfund.gov.ng",
                PhoneNumber = "08012345678",
                StaffId = "TET001",
                Gender = "Male",
                DateOfBirth = new DateTime(1985, 1, 1),
                EmploymentDate = new DateTime(2020, 1, 1),
                GradeLevel = "13",
                IsSystemUser = true,
                IsActive = true,
                UserType = "Staff",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
            };
            context.Users.Add(hrUser);
            await context.SaveChangesAsync();


            // Create PostOccupancy to map HR user to HR Admin post
            var postOccupancy = new PostOccupancy
            {
                PostId = hrAdminPost.Id,
                UserId = hrUser.Id,
                StartDate = DateTime.UtcNow,
                IsPrimary = true,
                Remarks = "Initial assignment to HR Admin post"
            };
            context.PostOccupancies.Add(postOccupancy);

            // Update user to reflect post assignment
            hrUser.IsPostAssigned = true;
            context.Users.Update(hrUser);

            await context.SaveChangesAsync();
        }
    }
}
