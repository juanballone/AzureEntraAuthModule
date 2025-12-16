# AzureEntraAuthModule
AzureEntraAuthModule Test Project

# Build the project
dotnet build -c Release

# Publish the DLL
```
dotnet publish -c Release -o ./publish
```
The DLL will be in ./publish folder
Copy AzureEntraAuth.dll to your vendor

# Package as NuGet (optional, for easier distribution)
```
dotnet pack -c Release
```

## Complete Checklist for Vendor

**Package to provide to vendor:**

1. **AzureEntraAuth.dll** - The compiled library
2. **All dependent DLLs** from the publish folder:
   - Microsoft.Identity.Web.dll
   - Microsoft.Identity.Client.dll
   - And other dependencies
3. **Integration documentation** (the markdown files I provided)
4. **Sample configuration** (JSON/XML snippets)

**Information to provide vendor:**
```
📦 Azure Entra Auth Package Contents:
├── AzureEntraAuth.dll
├── Dependencies/
│   ├── Microsoft.Identity.Web.dll
│   ├── Microsoft.Graph.dll
│   └── [other dependencies]
├── Documentation/
│   ├── IntegrationGuide.md
│   └── AzureSetupGuide.md
└── Configuration/
    ├── appsettings.example.json (for Core)
    └── web.config.example (for Framework)
```

## Required from your side:
- Azure App Registration (Client ID, Tenant ID, Secret)
- Azure AD Group Object IDs for authorization
- Redirect URI configuration in Azure