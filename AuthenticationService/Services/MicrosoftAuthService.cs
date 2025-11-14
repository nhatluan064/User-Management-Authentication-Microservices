using System.DirectoryServices;
using System.Net;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Authentication.Azure;
using Azure.Identity;
using AuthenticationService.Models;

namespace AuthenticationService.Services;

public class MicrosoftAuthService : IMicrosoftAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MicrosoftAuthService> _logger;

    public MicrosoftAuthService(IConfiguration configuration, ILogger<MicrosoftAuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> ValidateAdCredentialsAsync(string username, string password, string? domain = null)
    {
        try
        {
            domain ??= _configuration["MicrosoftAuth:Domain"] ?? throw new InvalidOperationException("AD Domain not configured");

            // Try LDAP bind to validate credentials
            var ldapPath = $"LDAP://{domain}";
            using var entry = new DirectoryEntry(ldapPath, username, password);
            
            // Force authentication by accessing native object
            var nativeObject = entry.NativeObject;
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AD authentication failed for user {Username}", username);
            return false;
        }
    }

    public async Task<MicrosoftUserInfo?> GetUserFromAdAsync(string username, string? domain = null)
    {
        try
        {
            domain ??= _configuration["MicrosoftAuth:Domain"] ?? throw new InvalidOperationException("AD Domain not configured");

            var ldapPath = $"LDAP://{domain}";
            using var entry = new DirectoryEntry(ldapPath);
            using var searcher = new DirectorySearcher(entry)
            {
                Filter = $"(&(objectClass=user)(sAMAccountName={username}))",
                SearchScope = System.DirectoryServices.SearchScope.Subtree
            };

            searcher.PropertiesToLoad.Add("objectGUID");
            searcher.PropertiesToLoad.Add("sAMAccountName");
            searcher.PropertiesToLoad.Add("mail");
            searcher.PropertiesToLoad.Add("displayName");
            searcher.PropertiesToLoad.Add("cn");

            var result = searcher.FindOne();
            if (result == null) return null;

            var objectGuid = result.Properties["objectGUID"][0] as byte[];
            var objectId = objectGuid != null ? new Guid(objectGuid).ToString() : string.Empty;

            return new MicrosoftUserInfo
            {
                ObjectId = objectId,
                Username = result.Properties["sAMAccountName"][0]?.ToString() ?? username,
                Email = result.Properties["mail"]?[0]?.ToString(),
                FullName = result.Properties["displayName"]?[0]?.ToString() ?? result.Properties["cn"]?[0]?.ToString(),
                DisplayName = result.Properties["displayName"]?[0]?.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user from AD: {Username}", username);
            return null;
        }
    }

    public async Task<MicrosoftUserInfo?> GetUserFromMicrosoft365Async(string entityId)
    {
        try
        {
            var tenantId = _configuration["MicrosoftAuth:TenantId"];
            var clientId = _configuration["MicrosoftAuth:ClientId"];
            var clientSecret = _configuration["MicrosoftAuth:ClientSecret"];

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogWarning("Microsoft 365 credentials not configured");
                return null;
            }

            // Create GraphServiceClient with client credentials
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var authProvider = new AzureIdentityAuthenticationProvider(credential);
            var graphClient = new GraphServiceClient(authProvider);

            // Get user by ID
            var user = await graphClient.Users[entityId].GetAsync();
            if (user == null) return null;

            return new MicrosoftUserInfo
            {
                ObjectId = user.Id ?? entityId,
                Username = user.UserPrincipalName ?? user.Mail ?? string.Empty,
                Email = user.Mail ?? user.UserPrincipalName,
                FullName = user.DisplayName,
                DisplayName = user.DisplayName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user from Microsoft 365: {EntityId}", entityId);
            return null;
        }
    }
}

