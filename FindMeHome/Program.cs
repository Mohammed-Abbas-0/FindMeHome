using FindMeHome.AppContext;
using FindMeHome.Mappers;
using FindMeHome.Repositories.AbstractionLayer;
using FindMeHome.Repositories.ImplementationLayer;
using FindMeHome.Services.Abstraction;
using FindMeHome.Services.Implementation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Mapper
builder.Services.AddAutoMapper(typeof(MappingHelper));

builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
builder.Services.AddScoped(typeof(IRepositories<>), typeof(Repositories<>));
builder.Services.AddScoped<IRealStateService, RealStateService>();
// 🟢 MVC مع Runtime Compilation (لو كنت ضايفها)
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

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

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=RealEstate}/{action=Index}/{id?}");

app.Run();
