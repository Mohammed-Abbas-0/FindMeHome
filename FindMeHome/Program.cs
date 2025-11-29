using FindMeHome.AppContext;
using FindMeHome.Mappers;
using FindMeHome.Repositories.AbstractionLayer;
using FindMeHome.Repositories.ImplementationLayer;
using FindMeHome.Services.Abstraction;
using FindMeHome.Services.Implementation;
using FindMeHome.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Mapper
builder.Services.AddAutoMapper(typeof(MappingHelper));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepositories<>), typeof(Repositories<>));
builder.Services.AddScoped<IRealStateService, RealStateService>();
// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// 🟢 MVC مع Runtime Compilation (لو كنت ضايفها)
builder.Services.AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization()
    .AddRazorRuntimeCompilation();

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDBContext>()
    .AddDefaultTokenProviders();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICraftsmanService, CraftsmanService>();

// JWT Authentication
builder.Services.AddAuthentication()
.AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false;
    o.SaveToken = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
    };
});

// Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/RealEstate/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

var supportedCultures = new[] { "ar-EG", "en-US" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("ar-EG")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=RealEstate}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDBContext>();  
    // Ensure database is created/migrated
    // context.Database.Migrate(); 
    await FindMeHome.Data.CraftsmanSeeder.SeedAsync(context);
    await FindMeHome.Data.RoleSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
