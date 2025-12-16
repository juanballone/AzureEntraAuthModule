using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureEntraAuth
{
    public static class AzureEntraAuthExtensions
    {
        /// <summary>
        /// Adds Azure Entra ID authentication to the application
        /// </summary>
        public static IServiceCollection AddAzureEntraAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Read configuration from appsettings.json
            var azureAdSection = configuration.GetSection("AzureAd");

            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options =>
                {
                    configuration.Bind("AzureAd", options);

                    // Request additional scopes and claims
                    options.Scope.Add("User.Read");
                    options.TokenValidationParameters.NameClaimType = "name";
                    options.TokenValidationParameters.RoleClaimType = "roles";

                    // Handle group claims
                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;

                            // Add custom logic here if needed
                            // For example, map Azure AD groups to application roles
                            await Task.CompletedTask;
                        }
                    };
                })
                .EnableTokenAcquisitionToCallDownstreamApi(
                    options => configuration.Bind("AzureAd", options))
                .AddMicrosoftGraph(
                    configuration.GetSection("MicrosoftGraph"))
                .AddInMemoryTokenCaches();

            // Add authorization policies based on Azure AD groups
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireClaim(
                        "groups",
                        configuration["AzureAd:AdminGroupId"]!   // null-forgiving operator
                    ));

                options.AddPolicy("UserAccess", policy =>
                    policy.RequireAuthenticatedUser());
            });

            services.AddRazorPages()
                .AddMicrosoftIdentityUI();

            return services;
        }

        /// <summary>
        /// Configures the authentication middleware
        /// </summary>
        public static IApplicationBuilder UseAzureEntraAuthentication(
            this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }

    /// <summary>
    /// Service to handle Azure AD group operations
    /// </summary>
    public interface IAzureGroupService
    {
        Task<List<string>> GetUserGroupsAsync(string userId);
        Task<bool> IsUserInGroupAsync(string userId, string groupId);
    }

    public class AzureGroupService : IAzureGroupService
    {
        private readonly Microsoft.Graph.GraphServiceClient _graphClient;

        public AzureGroupService(Microsoft.Graph.GraphServiceClient graphClient)
        {
            _graphClient = graphClient;
        }

        public async Task<List<string>> GetUserGroupsAsync(string userId)
        {
            try
            {
                var groups = await _graphClient.Users[userId]
                    .MemberOf
                    .GetAsync();

                return groups.Value
                    .Select(g => g.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                // Log error
                throw new Exception($"Failed to retrieve user groups: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsUserInGroupAsync(string userId, string groupId)
        {
            var groups = await GetUserGroupsAsync(userId);
            return groups.Contains(groupId);
        }
    }

    /// <summary>
    /// Attribute for group-based authorization
    /// </summary>
    public class RequireAzureGroupAttribute : AuthorizeAttribute
    {
        public RequireAzureGroupAttribute(string groupId)
        {
            Policy = $"RequireGroup_{groupId}";
        }
    }
}