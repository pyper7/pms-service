using PMS.Domain.Entities;

namespace PMS.Infrastructure.Data;

public static class PermissionSeeder
{
    public static async Task SeedHrAdminPermissionsAsync(PmsDbContext context)
    {
        var permissions = new List<Permission>
        {
            // User Management Module
            new Permission { Name = "user_management:read", Description = "Read user management data including user profiles, roles, and basic information", Module = "user_management" },
            new Permission { Name = "user_management:create", Description = "Create new user accounts and profiles", Module = "user_management" },
            new Permission { Name = "user_management:update", Description = "Update existing user profiles, roles, and personal information", Module = "user_management" },
            new Permission { Name = "user_management:delete", Description = "Delete user accounts and deactivate user access", Module = "user_management" },
            new Permission { Name = "user_management:assign_roles", Description = "Assign and modify user roles (HR_ADMIN, DIRECTOR, OFFICER)", Module = "user_management" },
            new Permission { Name = "user_management:view_all", Description = "View all users across the organization regardless of department", Module = "user_management" },
            new Permission { Name = "user_management:export", Description = "Export user data to CSV, Excel, or PDF formats", Module = "user_management" },

            // Post Management Module
            new Permission { Name = "post_management:read", Description = "Read post information, hierarchies, and organizational structure", Module = "post_management" },
            new Permission { Name = "post_management:create", Description = "Create new posts and positions within the organization", Module = "post_management" },
            new Permission { Name = "post_management:update", Description = "Update post details, descriptions, and requirements", Module = "post_management" },
            new Permission { Name = "post_management:delete", Description = "Delete posts and remove positions from organization", Module = "post_management" },
            new Permission { Name = "post_management:assign_staff", Description = "Assign staff members to specific posts and positions", Module = "post_management" },
            new Permission { Name = "post_management:view_hierarchy", Description = "View organizational hierarchy and reporting structures", Module = "post_management" },
            new Permission { Name = "post_management:manage_departments", Description = "Create, update, and manage department structures", Module = "post_management" },

            // Performance Management Module
            new Permission { Name = "performance_management:read", Description = "Read performance data, appraisals, and KPI information", Module = "performance_management" },
            new Permission { Name = "performance_management:create_appraisals", Description = "Create new performance appraisal periods and cycles", Module = "performance_management" },
            new Permission { Name = "performance_management:update_appraisals", Description = "Update existing performance appraisals and ratings", Module = "performance_management" },
            new Permission { Name = "performance_management:delete_appraisals", Description = "Delete performance appraisals and related data", Module = "performance_management" },
            new Permission { Name = "performance_management:view_all_appraisals", Description = "View all performance appraisals across the organization", Module = "performance_management" },
            new Permission { Name = "performance_management:approve_appraisals", Description = "Approve or reject performance appraisals", Module = "performance_management" },
            new Permission { Name = "performance_management:generate_reports", Description = "Generate performance reports and analytics", Module = "performance_management" },
            new Permission { Name = "performance_management:set_kpis", Description = "Set and manage Key Performance Indicators for staff", Module = "performance_management" },
            new Permission { Name = "performance_management:view_analytics", Description = "View performance analytics and dashboard data", Module = "performance_management" },

            // Department Management Module
            new Permission { Name = "department_management:read", Description = "Read department information and organizational structure", Module = "department_management" },
            new Permission { Name = "department_management:create", Description = "Create new departments and organizational units", Module = "department_management" },
            new Permission { Name = "department_management:update", Description = "Update department details and organizational structure", Module = "department_management" },
            new Permission { Name = "department_management:delete", Description = "Delete departments and reorganize structure", Module = "department_management" },
            new Permission { Name = "department_management:assign_directors", Description = "Assign directors and department heads", Module = "department_management" },
            new Permission { Name = "department_management:view_org_chart", Description = "View organizational chart and reporting relationships", Module = "department_management" },

            // Role Management Module
            new Permission { Name = "role_management:read", Description = "Read role definitions and permission structures", Module = "role_management" },
            new Permission { Name = "role_management:create", Description = "Create new roles and define their permissions", Module = "role_management" },
            new Permission { Name = "role_management:update", Description = "Update existing roles and modify permissions", Module = "role_management" },
            new Permission { Name = "role_management:delete", Description = "Delete roles and remove from system", Module = "role_management" },
            new Permission { Name = "role_management:assign_permissions", Description = "Assign specific permissions to roles", Module = "role_management" },
            new Permission { Name = "role_management:view_all", Description = "View all roles and their permission mappings", Module = "role_management" },

            // Dashboard and Analytics Module
            new Permission { Name = "dashboard:view_hr_dashboard", Description = "Access HR Admin dashboard with key metrics and analytics", Module = "dashboard" },
            new Permission { Name = "dashboard:view_user_analytics", Description = "View user activity and engagement analytics", Module = "dashboard" },
            new Permission { Name = "dashboard:view_performance_metrics", Description = "View performance metrics and KPI dashboards", Module = "dashboard" },
            new Permission { Name = "dashboard:export_data", Description = "Export dashboard data and reports", Module = "dashboard" },

            // System Administration Module
            new Permission { Name = "system_admin:view_logs", Description = "View system logs and audit trails", Module = "system_admin" },
            new Permission { Name = "system_admin:manage_settings", Description = "Manage system-wide settings and configurations", Module = "system_admin" },
            new Permission { Name = "system_admin:view_audit_trails", Description = "View user activity and system audit trails", Module = "system_admin" },
            new Permission { Name = "system_admin:backup_data", Description = "Create and manage system data backups", Module = "system_admin" },
            new Permission { Name = "system_admin:restore_data", Description = "Restore system data from backups", Module = "system_admin" },

            // Notification Management Module
            new Permission { Name = "notifications:read", Description = "Read system notifications and alerts", Module = "notifications" },
            new Permission { Name = "notifications:create", Description = "Create and send notifications to users", Module = "notifications" },
            new Permission { Name = "notifications:update", Description = "Update notification settings and preferences", Module = "notifications" },
            new Permission { Name = "notifications:delete", Description = "Delete notifications and clear notification history", Module = "notifications" },
            new Permission { Name = "notifications:send_bulk", Description = "Send bulk notifications to multiple users", Module = "notifications" },

            // Profile Management Module
            new Permission { Name = "profile:read_own", Description = "Read own profile information and personal details", Module = "profile" },
            new Permission { Name = "profile:update_own", Description = "Update own profile information and personal details", Module = "profile" },
            new Permission { Name = "profile:read_all", Description = "Read all user profiles across the organization", Module = "profile" },
            new Permission { Name = "profile:update_all", Description = "Update any user profile in the organization", Module = "profile" },
            new Permission { Name = "profile:change_passwords", Description = "Change user passwords and reset password requests", Module = "profile" },

            // Reports and Analytics Module
            new Permission { Name = "reports:generate_user_reports", Description = "Generate comprehensive user reports and statistics", Module = "reports" },
            new Permission { Name = "reports:generate_performance_reports", Description = "Generate performance reports and analytics", Module = "reports" },
            new Permission { Name = "reports:generate_department_reports", Description = "Generate department-wise reports and analytics", Module = "reports" },
            new Permission { Name = "reports:export_all", Description = "Export reports in various formats (PDF, Excel, CSV)", Module = "reports" },
            new Permission { Name = "reports:schedule_reports", Description = "Schedule automatic report generation and delivery", Module = "reports" },

            // Security and Access Control Module
            new Permission { Name = "security:view_access_logs", Description = "View user access logs and login activities", Module = "security" },
            new Permission { Name = "security:manage_sessions", Description = "Manage active user sessions and force logout", Module = "security" },
            new Permission { Name = "security:reset_passwords", Description = "Reset user passwords and manage password policies", Module = "security" },
            new Permission { Name = "security:view_alerts", Description = "View security alerts and suspicious activities", Module = "security" },
            new Permission { Name = "security:manage_api_keys", Description = "Manage API keys and external integrations", Module = "security" },

            // Data Management Module
            new Permission { Name = "data:import_users", Description = "Import user data from external sources (CSV, Excel)", Module = "data" },
            new Permission { Name = "data:export_users", Description = "Export user data to external formats", Module = "data" },
            new Permission { Name = "data:bulk_operations", Description = "Perform bulk operations on user data", Module = "data" },
            new Permission { Name = "data:validate_data", Description = "Validate and clean imported data", Module = "data" },
            new Permission { Name = "data:backup_data", Description = "Create backups of critical data", Module = "data" },

            // Integration Management Module
            new Permission { Name = "integration:manage_external_systems", Description = "Manage integrations with external HR systems", Module = "integration" },
            new Permission { Name = "integration:configure_apis", Description = "Configure API connections and endpoints", Module = "integration" },
            new Permission { Name = "integration:view_logs", Description = "View integration logs and error reports", Module = "integration" },
            new Permission { Name = "integration:test_connections", Description = "Test external system connections and APIs", Module = "integration" },

            // Workflow Management Module
            new Permission { Name = "workflow:create_workflows", Description = "Create approval workflows and business processes", Module = "workflow" },
            new Permission { Name = "workflow:update_workflows", Description = "Update existing workflows and processes", Module = "workflow" },
            new Permission { Name = "workflow:delete_workflows", Description = "Delete workflows and processes", Module = "workflow" },
            new Permission { Name = "workflow:view_status", Description = "View workflow status and progress", Module = "workflow" },
            new Permission { Name = "workflow:approve_workflows", Description = "Approve or reject workflow requests", Module = "workflow" },

            // Audit and Compliance Module
            new Permission { Name = "audit:view_logs", Description = "View comprehensive audit logs and activities", Module = "audit" },
            new Permission { Name = "audit:generate_compliance_reports", Description = "Generate compliance and audit reports", Module = "audit" },
            new Permission { Name = "audit:track_changes", Description = "Track and monitor system changes", Module = "audit" },
            new Permission { Name = "audit:export_data", Description = "Export audit data for external analysis", Module = "audit" }
        };

        var existingPermissionNames = context.Permissions.Select(p => p.Name).ToHashSet();
        var newPermissions = permissions.Where(p => !existingPermissionNames.Contains(p.Name)).ToList();
        if (newPermissions.Any())
        {
            context.Permissions.AddRange(newPermissions);
            await context.SaveChangesAsync();
        }

        // Assign all HR Admin permissions to HR role
        var hrRole = context.Roles.First(r => r.Name == "HR");
        var targetPermissionNames = permissions.Select(x => x.Name).ToHashSet();
        var allPermissionIds = context.Permissions
            .Where(p => targetPermissionNames.Contains(p.Name))
            .Select(p => p.Id)
            .ToList();

        var existingRolePermissionPairs = context.RolePermissions
            .Where(rp => rp.RoleId == hrRole.Id)
            .Select(rp => rp.PermissionId)
            .ToHashSet();

        var rolePermissions = allPermissionIds
            .Where(pid => !existingRolePermissionPairs.Contains(pid))
            .Select(pid => new RolePermission
            {
                RoleId = hrRole.Id,
                PermissionId = pid,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "system"
            })
            .ToList();

        if (rolePermissions.Any())
        {
            context.RolePermissions.AddRange(rolePermissions);
            await context.SaveChangesAsync();
        }
    }
}


