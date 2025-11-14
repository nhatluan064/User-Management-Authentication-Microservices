using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AuthenticationService.Models;
using AuthenticationService.Models.DTOs;
using AuthenticationService.Repositories;

namespace AuthenticationService.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly IMicrosoftAuthService _microsoftAuthService;
    private readonly IDelegationRepository _delegationRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IConfiguration configuration,
        IUserRepository userRepository,
        IMicrosoftAuthService microsoftAuthService,
        IDelegationRepository delegationRepository,
        ILogger<AuthService> logger)
    {
        _configuration = configuration;
        _userRepository = userRepository;
        _microsoftAuthService = microsoftAuthService;
        _delegationRepository = delegationRepository;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        User? user = null;
        MicrosoftUserInfo? microsoftUser = null;

        if (request.UseLocalAuth)
        {
            // Local user authentication
            user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null || !user.IsLocalUser || !user.IsActive)
            {
                return null;
            }

            // Verify password
            if (string.IsNullOrEmpty(user.PasswordHash) || 
                !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return null;
            }
        }
        else
        {
            // Microsoft AD authentication
            var isValid = await _microsoftAuthService.ValidateAdCredentialsAsync(
                request.Username, 
                request.Password);
            
            if (!isValid)
            {
                return null;
            }

            microsoftUser = await _microsoftAuthService.GetUserFromAdAsync(request.Username);
            if (microsoftUser == null)
            {
                return null;
            }

            // Find or create user in database
            user = await _userRepository.GetByAdObjectIdAsync(microsoftUser.ObjectId);
            if (user == null)
            {
                // Create new user from AD
                user = new User
                {
                    Username = microsoftUser.Username,
                    Email = microsoftUser.Email,
                    FullName = microsoftUser.FullName,
                    IsLocalUser = false,
                    AdDomain = _configuration["MicrosoftAuth:Domain"],
                    AdObjectId = microsoftUser.ObjectId,
                    IsActive = true
                };
                user = await _userRepository.CreateAsync(user);
            }
            else if (!user.IsActive)
            {
                return null;
            }
        }

        // Load user with departments
        user = await _userRepository.GetByIdAsync(user.Id);
        if (user == null) return null;

        // Check for active delegations
        var activeDelegations = await _delegationRepository.GetActiveDelegationsForDelegateeAsync(user.Id);
        DelegationInfo? delegationInfo = null;

        if (activeDelegations.Any())
        {
            var delegation = activeDelegations.First();
            var delegator = await _userRepository.GetByIdAsync(delegation.DelegatorId);
            if (delegator != null)
            {
                delegationInfo = new DelegationInfo
                {
                    DelegationId = delegation.Id,
                    Delegator = MapToUserInfo(delegator),
                    StartDate = delegation.StartDate,
                    EndDate = delegation.EndDate,
                    Reason = delegation.Reason
                };
            }
        }

        var userInfo = MapToUserInfo(user);
        var token = GenerateToken(userInfo, delegationInfo);

        return new LoginResponse
        {
            Token = token,
            UserInfo = userInfo,
            DelegationInfo = delegationInfo
        };
    }

    public async Task<UserInfo?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            return await GetUserInfoAsync(userId);
        }
        catch
        {
            return null;
        }
    }

    public async Task<UserInfo?> GetUserInfoAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.IsActive) return null;

        // Check for active delegations
        var activeDelegations = await _delegationRepository.GetActiveDelegationsForDelegateeAsync(userId);
        
        return MapToUserInfo(user);
    }

    public string GenerateToken(UserInfo userInfo, DelegationInfo? delegationInfo = null)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
            new Claim(ClaimTypes.Name, userInfo.Username),
            new Claim("email", userInfo.Email ?? string.Empty),
            new Claim("fullName", userInfo.FullName ?? string.Empty),
            new Claim("isLocalUser", userInfo.IsLocalUser.ToString())
        };

        // Add delegation info if exists
        if (delegationInfo != null)
        {
            claims.Add(new Claim("delegationId", delegationInfo.DelegationId.ToString()));
            claims.Add(new Claim("delegatorId", delegationInfo.Delegator.Id.ToString()));
            claims.Add(new Claim("delegatorName", delegationInfo.Delegator.FullName ?? string.Empty));
        }

        // Add department and role info
        foreach (var dept in userInfo.Departments)
        {
            claims.Add(new Claim("department", dept.Id.ToString()));
            if (dept.Role != null)
            {
                claims.Add(new Claim("role", $"{dept.Id}:{dept.Role.Id}"));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private UserInfo MapToUserInfo(User user)
    {
        return new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            IsLocalUser = user.IsLocalUser,
            Departments = user.UserDepartments.Select(ud => new DepartmentInfo
            {
                Id = ud.Department.Id,
                Name = ud.Department.Name,
                Code = ud.Department.Code,
                Role = ud.Role != null ? new RoleInfo
                {
                    Id = ud.Role.Id,
                    Name = ud.Role.Name
                } : null
            }).ToList()
        };
    }
}

