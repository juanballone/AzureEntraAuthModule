using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Microsoft.IdentityModel.Tokens;

namespace AzureEntraAuth
{
    /// <summary>
    /// Main class to configure Azure Entra ID authentication
    /// </summary>
    public static class AzureEntraAuthModule
    {
        /// <summary>
        /// Configure Azure Entra ID authentication
        /// Call this from your Startup.cs Configuration method
        /// </summary>
        public static void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                CookieName = "AzureEntraAuth",
                ExpireTimeSpan = TimeSpan.FromHours(8),
                SlidingExpiration = true
            });

            string clientId = ConfigurationManager.AppSettings["AzureAd:ClientId"];
            string tenantId = ConfigurationManager.AppSettings["AzureAd:TenantId"];
            string clientSecret = ConfigurationManager.AppSettings["AzureAd:ClientSecret"];
            string redirectUri = ConfigurationManager.AppSettings["AzureAd:RedirectUri"];
            string authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = clientId,
                Authority = authority,
                RedirectUri = redirectUri,
                PostLogoutRedirectUri = redirectUri,
                Scope = "openid profile email User.Read",
                ResponseType = "code id_token",
                
                TokenValidationParameters = new TokenValidationParameters // <-- change namespace
                {
                    ValidateIssuer = true,
                    NameClaimType = "name",
                    RoleClaimType = "roles"
                },

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthorizationCodeReceived = async context =>
                    {
                        var code = context.Code;
                        string signedInUserId = context.AuthenticationTicket.Identity
                            .FindFirst(ClaimTypes.NameIdentifier).Value;

                        IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
                            .Create(clientId)
                            .WithClientSecret(clientSecret)
                            .WithAuthority(authority)
                            .WithRedirectUri(redirectUri)
                            .Build();

                        var result = await app.AcquireTokenByAuthorizationCode(
                            new[] { "User.Read" }, code).ExecuteAsync();

                        // Store tokens for later use
                        context.AuthenticationTicket.Identity.AddClaim(
                            new Claim("access_token", result.AccessToken));
                        
                        // Get user groups
                        var groups = await GetUserGroups(result.AccessToken);
                        foreach (var group in groups)
                        {
                            context.AuthenticationTicket.Identity.AddClaim(
                                new Claim("groups", group));
                        }
                    },

                    AuthenticationFailed = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect($"/Error?message={context.Exception.Message}");
                        return Task.FromResult(0);
                    },

                    RedirectToIdentityProvider = context =>
                    {
                        return Task.FromResult(0);
                    }
                }
            });
        }

        /// <summary>
        /// Get user's Azure AD group memberships
        /// </summary>
        private static async Task<string[]> GetUserGroups(string accessToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", accessToken);
                    
                    var response = await client.GetAsync(
                        "https://graph.microsoft.com/v1.0/me/memberOf");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var json = JObject.Parse(content);
                        var groups = json["value"] as JArray;
                        
                        return groups?.Select(g => g["id"].ToString()).ToArray() 
                            ?? new string[0];
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Trace.TraceError(
                    $"Failed to get user groups: {ex.Message}");
            }
            
            return new string[0];
        }
    }

    /// <summary>
    /// Authorization helper for checking group membership
    /// </summary>
    public static class AzureGroupAuthorizationHelper
    {
        /// <summary>
        /// Check if current user is in specified Azure AD group
        /// </summary>
        public static bool IsUserInGroup(ClaimsPrincipal user, string groupId)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            var groupClaims = user.FindAll("groups");
            return groupClaims.Any(c => c.Value == groupId);
        }

        /// <summary>
        /// Get all groups current user belongs to
        /// </summary>
        public static List<string> GetUserGroups(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return new List<string>();

            return user.FindAll("groups")
                .Select(c => c.Value)
                .ToList();
        }

        /// <summary>
        /// Require user to be in specific group
        /// Throws UnauthorizedAccessException if not authorized
        /// </summary>
        public static void RequireGroup(ClaimsPrincipal user, string groupId)
        {
            if (!IsUserInGroup(user, groupId))
            {
                throw new UnauthorizedAccessException(
                    $"User is not a member of required group: {groupId}");
            }
        }
    }
}