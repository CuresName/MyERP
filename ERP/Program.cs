using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using ERP.Controllers;
using ERP.Filter;
using ERP.Models;
using ERP.EFModels;
using ERP.MisEFModels;
using ERP.TableTest;
using ERP.Models.WIPEFModels;
using NLog.Web;
using ERP.TESTModels.TESTEFModels;
using System.Net;

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddLogging(logging =>
{  //Register NLog into this project
    logging.ClearProviders();
    //setting log minimunLevel 
    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
});
builder.Host.UseNLog();

builder.Services.AddControllersWithViews();

//Register Controller into this project
builder.Services.AddScoped<DocumentController>();
builder.Services.AddScoped<UserController>();

builder.Services.AddDbContext<erpEntities>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DBEntity")));//Connect SQL
builder.Services.AddDbContext<devContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DBdev")));//Connect SQL
builder.Services.AddDbContext<misContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DBdev")));//Connect SQL
builder.Services.AddDbContext<tableContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DBdev")));//Connect SQL
builder.Services.AddDbContext<WIPContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DBEntity")));//Connect SQL
builder.Services.AddDbContext<TESTContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DBTest")));//Connect SQL


builder.Services.AddSession(); // Set Session




builder.Services.AddMvc(option =>
{
    option.Filters.Add(new AuthorizeFilter());
}).AddSessionStateTempDataProvider();

builder.Services.AddMvc(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());


builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login"; // Set login page
    });

//Unicode 
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));


builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts(); //set https
}

app.UseSession(); // set Session


app.UseHttpsRedirection(); //set https
app.UseStaticFiles();
app.UseAuthentication(); // Enable authentication
app.UseAuthorization();

app.UseRouting();
app.UseCookiePolicy();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");

app.Run();