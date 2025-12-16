# Azure Entra ID App Registration Setup Guide

## Step 1: Create App Registration

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Entra ID** (formerly Azure Active Directory)
3. Click **App registrations** → **New registration**
4. Configure:
   - **Name**: Your Application Name (e.g., "MyApp Authentication")
   - **Supported account types**: 
     - Single tenant (most common for internal apps)
     - Or multi-tenant if needed
   - **Redirect URI**: 
     - Platform: **Web**
     - URI: `https://yourdomain.com/signin-oidc`
5. Click **Register**

## Step 2: Note the IDs

After registration, copy these values:
- **Application (client) ID** → Use as `ClientId`
- **Directory (tenant) ID** → Use as `TenantId`

## Step 3: Create Client Secret

1. In your app registration, go to **Certificates & secrets**
2. Click **New client secret**
3. Add description: "App Authentication Secret"
4. Set expiration (recommend 24 months)
5. Click **Add**
6. **IMMEDIATELY COPY THE SECRET VALUE** → Use as `ClientSecret`
   - ⚠️ You can't see this again after leaving the page!

## Step 4: Configure API Permissions

1. Go to **API permissions**
2. Click **Add a permission**
3. Select **Microsoft Graph**
4. Choose **Delegated permissions**
5. Add these permissions:
   - `User.Read` (usually already there)
   - `Directory.Read.All` (to read group memberships)
   - `GroupMember.Read.All` (to read group details)
6. Click **Add permissions**
7. Click **Grant admin consent for [Your Organization]** (requires admin)
   - ⚠️ This step requires Global Admin or Privileged Role Admin

## Step 5: Configure Authentication Settings

1. Go to **Authentication**
2. Under **Implicit grant and hybrid flows**, enable:
   - ✅ **ID tokens** (for authentication)
3. Under **Redirect URIs**, add:
   - `https://yourdomain.com/signin-oidc`
   - `https://yourdomain.com/signout-callback-oidc` (for logout)
4. **Front-channel logout URL**: `https://yourdomain.com/signout-oidc`
5. Click **Save**

## Step 6: Configure Token Configuration (Optional but Recommended)

1. Go to **Token configuration**
2. Click **Add groups claim**
3. Select:
   - ✅ **Security groups**
   - Token type: **ID** and **Access**
   - Group ID format: **Group ID**
4. Click **Add**

This ensures group IDs are included in the authentication token.

## Step 7: Find Azure AD Group IDs

To use group-based authorization, you need the Object IDs of your groups:

1. In Azure Portal, go to **Azure Entra ID** → **Groups**
2. Find your group (e.g., "Administrators", "Users")
3. Click on the group
4. Copy the **Object ID**
5. Use this ID in your configuration:
   ```json
   "AdminGroupId": "12345678-1234-1234-1234-123456789abc"
   ```

## Step 8: Configure Application Settings

Update your application configuration with the values:

### For ASP.NET Core (appsettings.json):
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "YOUR_TENANT_ID_FROM_STEP_2",
    "ClientId": "YOUR_CLIENT_ID_FROM_STEP_2",
    "ClientSecret": "YOUR_CLIENT_SECRET_FROM_STEP_3",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc",
    "AdminGroupId": "YOUR_ADMIN_GROUP_OBJECT_ID_FROM_STEP_7"
  }
}
```

### For ASP.NET Framework (Web.config):
```xml
<appSettings>
  <add key="AzureAd:TenantId" value="YOUR_TENANT_ID_FROM_STEP_2" />
  <add key="AzureAd:ClientId" value="YOUR_CLIENT_ID_FROM_STEP_2" />
  <add key="AzureAd:ClientSecret" value="YOUR_CLIENT_SECRET_FROM_STEP_3" />
  <add key="AzureAd:RedirectUri" value="https://yourdomain.com/signin-oidc" />
  <add key="AzureAd:AdminGroupId" value="YOUR_ADMIN_GROUP_OBJECT_ID_FROM_STEP_7" />
</appSettings>
```

## Step 9: Update IIS Application Settings

1. Open **IIS Manager**
2. Select your application
3. Double-click **Configuration Editor**
4. Section: `system.webServer/security/authentication/anonymousAuthentication`
5. Set `enabled` to `True`
6. Section: `system.webServer/security/authentication/windowsAuthentication`
7. Set `enabled` to `False`
8. Click **Apply**

## Step 10: Test the Integration

1. Navigate to your application URL
2. You should be redirected to Microsoft login
3. Sign in with your Azure AD credentials
4. After successful authentication, you'll be redirected back to your app

## Troubleshooting

### Common Issues:

**"AADSTS50011: The reply URL specified in the request does not match"**
- Ensure Redirect URI in Azure matches your application URL exactly
- Check for http vs https
- Verify the path is `/signin-oidc`

**"User not authorized / 403 Forbidden"**
- Check that admin consent was granted for API permissions
- Verify user is in the required Azure AD group
- Check group ID is correct in configuration

**Groups not appearing in token:**
- Ensure "Token configuration" was set up (Step 6)
- If user is in many groups (150+), Azure sends a groups overage claim
  - Solution: Use Microsoft Graph API to retrieve groups

**Cannot acquire token:**
- Verify Client Secret hasn't expired
- Check that API permissions include User.Read
- Ensure token endpoint is accessible from server

## Security Best Practices

1. **Store secrets securely**: Use Azure Key Vault or environment variables
2. **Rotate secrets regularly**: Set reminders for secret expiration
3. **Use HTTPS only**: Never use HTTP for authentication
4. **Limit permissions**: Only request necessary API permissions
5. **Monitor sign-ins**: Check Azure AD sign-in logs regularly
6. **Enable Conditional Access**: Add MFA requirements if needed
