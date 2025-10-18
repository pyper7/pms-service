using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.Auth;
using PMS.Domain.Entities;

namespace PMS.Application.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public string GenerateToken(User user, IList<string> posts)
    {
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
        var claims = new List<Claim>
        {
             new("nameid", user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new("staffId", user.StaffId ?? ""),
            new("gradeLevel", user.GradeLevel ?? ""),
            new("userType", user.UserType)
           
            
        };

        // Add post claims
        foreach (var post in posts)
        {
            claims.Add(new Claim("post", post));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }

    public bool ValidateRefreshToken(string refreshToken)
    {
        return !string.IsNullOrEmpty(refreshToken) && Guid.TryParse(refreshToken, out _);
    }

    public UserDto CreateUserDto(User user)
    {
        var primaryPost = user.PostOccupancies
            .Where(po => po.IsPrimary && po.IsCurrentlyOccupying)
            .Select(po => po.Post)
            .FirstOrDefault();

        var postTitle = primaryPost?.Title ?? string.Empty;
        var orgUnitName = primaryPost?.OrgUnit?.Name;
        
        // Get all posts from user's active post occupancies
        var posts = user.PostOccupancies
            .Where(po => po.IsCurrentlyOccupying)
            .Select(po => po.Post?.Title)
            .Where(postTitle => !string.IsNullOrEmpty(postTitle))
            .Cast<string>()
            .Distinct()
            .ToList();

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.FullName,
            Post = postTitle,
            StaffId = user.StaffId,
            PhoneNumber = user.PhoneNumber,
            Posts = posts,
            OrgUnit = orgUnitName,
            GradeLevel = user.GradeLevel,
            LastLoginDate = user.LastLoginDate
        };
    }
}
