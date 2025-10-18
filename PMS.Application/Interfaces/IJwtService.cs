using PMS.Application.Models.Auth;
using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user, IList<string> posts);
    string GenerateRefreshToken();
    bool ValidateRefreshToken(string refreshToken);
    UserDto CreateUserDto(User user);
}
