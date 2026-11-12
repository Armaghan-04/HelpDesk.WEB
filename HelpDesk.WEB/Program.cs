using FluentValidation;
using HelpDesk.WEB.Areas.Identity;
using HelpDesk.WEB.Configuration;
using HelpDesk.WEB.Data;
using HelpDesk.WEB.Mappings;
using HelpDesk.WEB.Models.Entities;
using HelpDesk.WEB.Services;
using HelpDesk.WEB.Services.Interfaces;
using HelpDesk.WEB.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 2FA is not offered by this application. Removing the authenticator token provider
// means authenticator tokens cannot be generated or verified, even if code asks for them.
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    options.Tokens.ProviderMap.Remove(TokenOptions.DefaultAuthenticatorProvider);
});

builder.Services.Configure<AuthenticationSettings>(
    builder.Configuration.GetSection(AuthenticationSettings.SectionName));

// A modest per-IP limit on sign-in submissions. Page loads are not counted, and
// Identity's own lockout remains the per-account protection.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", httpContext =>
    {
        if (!HttpMethods.IsPost(httpContext.Request.Method))
            return RateLimitPartition.GetNoLimiter("login-page-load");

        var clientKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            clientKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});

builder.Services.Configure<RazorPagesOptions>(options =>
{
    options.Conventions.AddAreaPageApplicationModelConvention(
        "Identity",
        "/Account/Login",
        model => model.EndpointMetadata.Add(new EnableRateLimitingAttribute("login")));
});

// The password-recovery pages come from the Identity UI package, so guard their
// handlers with a filter instead of scaffolding and duplicating the pages.
builder.Services.Configure<RazorPagesOptions>(options =>
{
    foreach (var page in new[]
    {
        "/Account/ForgotPassword",
        "/Account/ForgotPasswordConfirmation",
        "/Account/ResetPassword",
        "/Account/ResetPasswordConfirmation"
    })
    {
        options.Conventions.AddAreaPageApplicationModelConvention(
            "Identity",
            page,
            model => model.Filters.Add(new PasswordRecoveryDisabledFilter()));
    }
});

// Pick up promotions/demotions within about a minute without forcing a full re-login delay.
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(1);
});
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

    // FluentValidation supplies the required/length rules for controller-bound models.
    // Without this, MVC adds its own generic "field is required" error first and hides
    // the more specific FluentValidation message.
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Run the registered FluentValidation validators as part of MVC model validation.
// Plain MVC form posts do not declare [FromForm], so their binding source is null;
// enable that case explicitly. The custom result factory keeps failures in the
// normal view/redirect flow instead of returning a JSON 400.
builder.Services.AddFluentValidationAutoValidation(options =>
{
    options.EnableNullBindingSourceAutomaticValidation = true;
    options.OverrideDefaultResultFactoryWith<MvcValidationResultFactory>();
});

builder.Services.AddSingleton<TicketMapper>();
builder.Services.AddSingleton<DepartmentMapper>();
builder.Services.AddSingleton<TeamMapper>();

builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ITicketService, TicketService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await IdentitySeeder.SeedRolesAsync(services);
    // Runs before the bootstrap check so an enabled demo Manager satisfies it.
    await DemoAccountSeeder.SeedAsync(services);
    await IdentitySeeder.SeedBootstrapManagerAsync(services);
    await DepartmentSeeder.SeedAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
