using Data_Layer;
using Infrastructure_Layer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<MaindbContext>(options => options.UseMySql(builder.Configuration.GetConnectionString("MainDB"),
 new MySqlServerVersion(version: new Version())));

//builder.Services.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));

builder.Services.AddTransient(typeof(IUnitOfWork), typeof(UnitOfWork));

builder.Services.AddAuthentication("MyCookie").AddCookie("MyCookie", options =>
{
    options.Cookie.Name = "MyCookie";
    options.LoginPath = "/Account/Login";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeAdmin",
        policy => policy.RequireClaim("Admin", "1"));
    options.AddPolicy("LogCustomer",
        policy => policy.RequireClaim("Customer", "1"));
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddRazorPages();

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

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
