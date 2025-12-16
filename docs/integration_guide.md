# Azure Entra ID Authentication Integration Guide

## For ASP.NET Core Projects

### 1. Add the DLL Reference
Add reference to `AzureEntraAuth.dll` in your project file:
```xml
<ItemGroup>
  <Reference Include="AzureEntraAuth">
    <HintPath>path\to\AzureEntraAuth.dll</HintPath>
  </Reference>
</ItemGroup>
```

### 2. Update Program.cs or Startup.cs

**Program.cs (Minimal API - .NET 6+):**
```csharp
using AzureEntraAuth;

var builder = WebApplication.CreateBuilder(args);

// Add Azure Entra authentication
builder.Services.AddAzureEntraAuthentication(builder.Configuration);

// ... other services

var app = builder.Build();

// Configure middleware
app.UseAzureEntraAuthentication();

// ... other middleware

app.Run();
```

**Startup.cs (Traditional):**
```csharp
using AzureEntraAuth;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Azure Entra authentication
        services.AddAzureEntraAuthentication(Configuration);
        
        // ... other services
    }

    public void Configure(IApplicationBuilder app)
    {
        // ... other middleware
        
        app.UseAzureEntraAuthentication();
        
        // ... routing, endpoints, etc.
    }
}
```

### 3. Protect Controllers/Pages

**Using Authorize Attribute:**
```csharp
using Microsoft.AspNetCore.Authorization;
using AzureEntraAuth;

[Authorize] // Requires any authenticated user
public class HomeController : Controller
{
    public IActionResult Index() => View();
}

[RequireAzureGroup("group-id-here")] // Requires specific group
public class AdminController : Controller
{
    public IActionResult Index() => View();
}
```

---

## For ASP.NET Framework Projects

### 1. Add the DLL Reference
Right-click References → Add Reference → Browse to `AzureEntraAuth.dll`

### 2. Create/Update Startup.cs (OWIN)
```csharp
using Microsoft.Owin;
using Owin;
using AzureEntraAuth;

[assembly: OwinStartup(typeof(YourNamespace.Startup))]

namespace YourNamespace
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AzureEntraAuthModule.ConfigureAuth(app);
        }
    }
}
```

### 3. Update Web.config

Add required configuration in `<appSettings>`:
```xml
<appSettings>
  <add key="AzureAd:TenantId" value="YOUR_TENANT_ID" />
  <add key="AzureAd:ClientId" value="YOUR_CLIENT_ID" />
  <add key="AzureAd:ClientSecret" value="YOUR_CLIENT_SECRET" />
  <add key="AzureAd:RedirectUri" value="https://yourdomain.com/signin-oidc" />
</appSettings>
```

Add authentication module:
```xml
<system.webServer>
  <modules>
    <remove name="FormsAuthentication" />
  </modules>
</system.webServer>
```

### 4. Protect Controllers/Actions

```csharp
using System.Web.Mvc;
using AzureEntraAuth;

[Authorize] // Requires any authenticated user
public class HomeController : Controller
{
    public ActionResult Index()
    {
        return View();
    }
}

[RequireAzureGroup("admin-group-id")] // Requires specific group
public class AdminController : Controller
{
    public ActionResult Index()
    {
        // Check groups programmatically
        var user = User as System.Security.Claims.ClaimsPrincipal;
        bool isAdmin = AzureGroupAuthorizationHelper.IsUserInGroup(
            user, "admin-group-id");
        
        return View();
    }
}
```

---

## Common Integration Points

### Access User Information
```csharp
// In any controller/page
var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
var userName = User.Identity.Name;
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
```

### Check Group Membership
```csharp
// ASP.NET Core
[Authorize(Policy = "AdminOnly")]
public IActionResult AdminPage() { }

// ASP.NET Framework
if (AzureGroupAuthorizationHelper.IsUserInGroup(User, "group-id"))
{
    // User is in group
}
```

### Login/Logout Links

**ASP.NET Core:**
```html
<a asp-area="MicrosoftIdentity" asp-controller="Account" asp-action="SignIn">Login</a>
<a asp-area="MicrosoftIdentity" asp-controller="Account" asp-action="SignOut">Logout</a>
```

**ASP.NET Framework:**
```html
<a href="/Account/SignIn">Login</a>
<a href="/Account/SignOut">Logout</a>
```
