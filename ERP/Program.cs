using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using ERP.Controllers;
using ERP.Filter;
using ERP.Models;
using ERP.教育訓練EFModels;
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
{  //將NLog註冊到此專案內
    logging.ClearProviders();
    //設定log紀錄的最小等級
    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
}); 
builder.Host.UseNLog();

builder.Services.AddControllersWithViews();

//使其他Controller可以使用該controller的function
builder.Services.AddScoped<DocumentController>();
builder.Services.AddScoped<UserController>();

builder.Services.AddDbContext<erpEntities>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DBEntity")));//連接資料庫
builder.Services.AddDbContext<devContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DBdev")));//連接資料庫
builder.Services.AddDbContext<misContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DBdev")));//連接資料庫
builder.Services.AddDbContext<tableContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DBdev")));//連接資料庫
builder.Services.AddDbContext<WIPContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DBEntity")));//連接資料庫

builder.Services.AddDbContext<TESTContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DBTest")));//連接資料庫

builder.Services.AddSession(); // 配置Session服務


//將AuthorizeFilter套用至整個系統
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
        options.LoginPath = "/User/Login"; // 設置登錄頁面的路徑
    });

//以Unicode編寫，使Html的亂碼可正確顯示文字
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));


builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts(); //配置https重定向
}

app.UseSession(); // 使用Session中介軟體


app.UseHttpsRedirection(); //配置https重定向
app.UseStaticFiles();
app.UseAuthentication(); // 啟用身份驗證
app.UseAuthorization();

app.UseRouting();
app.UseCookiePolicy();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");

app.Run();
