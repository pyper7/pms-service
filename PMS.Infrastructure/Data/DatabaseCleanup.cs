using Microsoft.EntityFrameworkCore;
using PMS.Domain.Entities;
using PMS.Infrastructure.Data;

namespace PMS.Infrastructure.Data;

public static class DatabaseCleanup
{
    public static async Task ClearOrgUnitsAsync(PmsDbContext context)
    {
        // Remove all organizational units
        var orgUnits = await context.OrgUnits.ToListAsync();
        if (orgUnits.Any())
        {
            context.OrgUnits.RemoveRange(orgUnits);
            await context.SaveChangesAsync();
        }
    }

    public static async Task ClearDepartmentsAsync(PmsDbContext context)
    {
        // Get departments to delete
        var departments = await context.OrgUnits
            .Where(o => o.Type == OrgUnitType.DEPT)
            .ToListAsync();
            
        if (departments.Any())
        {
            var departmentIds = departments.Select(d => d.Id).ToList();
            
            // First, delete posts that reference these departments
            var postsToDelete = await context.Posts
                .Where(p => departmentIds.Contains(p.OrgUnitId))
                .ToListAsync();
                
            if (postsToDelete.Any())
            {
                // Delete post occupancies first (they reference posts)
                var postOccupanciesToDelete = await context.PostOccupancies
                    .Where(po => postsToDelete.Select(p => p.Id).Contains(po.PostId))
                    .ToListAsync();
                    
                if (postOccupanciesToDelete.Any())
                {
                    context.PostOccupancies.RemoveRange(postOccupanciesToDelete);
                }
                
                // Then delete the posts
                context.Posts.RemoveRange(postsToDelete);
                await context.SaveChangesAsync();
            }
            
            // Finally, delete the departments
            context.OrgUnits.RemoveRange(departments);
            await context.SaveChangesAsync();
        }
    }

    public static async Task ClearAllDataAsync(PmsDbContext context)
    {
        // Clear all data in the correct order to respect foreign key constraints
        var postOccupancies = await context.PostOccupancies.ToListAsync();
        if (postOccupancies.Any())
        {
            context.PostOccupancies.RemoveRange(postOccupancies);
        }

        var posts = await context.Posts.ToListAsync();
        if (posts.Any())
        {
            context.Posts.RemoveRange(posts);
        }

        var orgUnits = await context.OrgUnits.ToListAsync();
        if (orgUnits.Any())
        {
            context.OrgUnits.RemoveRange(orgUnits);
        }

        var users = await context.Users.ToListAsync();
        if (users.Any())
        {
            context.Users.RemoveRange(users);
        }

        var roles = await context.Roles.ToListAsync();
        if (roles.Any())
        {
            context.Roles.RemoveRange(roles);
        }

        var permissions = await context.Permissions.ToListAsync();
        if (permissions.Any())
        {
            context.Permissions.RemoveRange(permissions);
        }

        await context.SaveChangesAsync();
    }
}
