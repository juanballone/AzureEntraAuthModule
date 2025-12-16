using AzureEntraAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using TestWebApp.Data;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// This calls your DLL extension method.
builder.Services.AddAzureEntraAuthentication(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.TestItems.Any())
    {
        db.TestItems.Add(new TestItem { Name = "Hello from SQL" });
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Use authentication/authorization from your DLL extension
app.UseAzureEntraAuthentication();

app.MapRazorPages();

app.Run();
