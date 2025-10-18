using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByStaffIdAsync(string staffId);
    Task<User?> GetWithPostsAsync(long id);
    Task<List<string>> GetPermissionNamesAsync(long userId);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> StaffIdExistsAsync(string staffId);
}
