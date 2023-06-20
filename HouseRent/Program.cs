using Microsoft.AspNetCore.Authentication.Cookies;
using ServiceLayer.Interface;
using ServiceLayer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // Set a short timeout for easy testing.
    options.IdleTimeout = TimeSpan.FromDays(3);
    options.Cookie.HttpOnly = true;
    // Make the session cookie essential
    options.Cookie.IsEssential = true;
});
//Code required for session END
//----------------------------------------------------------------------------------------


builder.Services.Configure<CookiePolicyOptions>(options =>
{
    //Auto Generated
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

string connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Account/Login";
    });

/////////////////////////////////////////////// Dependency Injection //////////////////////////////////////

//Services
builder.Services.AddTransient<ICommonService>(x => new CommonService(connectionString));
builder.Services.AddTransient<IUserService>(x => new UserService(connectionString, new CommonService(connectionString)));
builder.Services.AddTransient<IAdminService>(x => new AdminService(connectionString, new CommonService(connectionString)));
builder.Services.AddTransient<IOwnerService>(x => new OwnerService(connectionString, new CommonService(connectionString)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=HouseRent}/{action=Index}/{id?}"
    );

app.Run();
