using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using ERP.Controllers;
using ERP.Filter;
using ERP.Models;
<<<<<<< HEAD
using ERP.EFModels;
=======
using ERP.教育訓練EFModels;
>>>>>>> 57a69941cd5a539dc541b21b1090b20fae847344
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
<<<<<<< HEAD
{  //Register NLog into this project
    logging.ClearProviders();
    //setting log minimunLevel 
=======
{  //將NLog註冊到此專案內
    logging.ClearProviders();
    //設定log紀錄的最小等級
>>>>>>> 57a69941cd5a539dc541b21b1090b20fae847344
    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
});
builder.Host.UseNLog();

builder.Services.AddControllersWithViews();

<<<<<<< HEAD
//Register Controller into this project
=======
//使其他Controller可以使用該controller的function
>>>>>>> 57a69941cd5a539dc541b21b1090b20fae847344
builder.Services.AddScoped<DocumentController>();
builder.Services.AddScoped<UserController>();

builder.Services.AddDbContext<erpEntities>(option =>
<<<<<<< HEAD
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


//AuthorizeFilter
=======
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
>>>>>>> 57a69941cd5a539dc541b21b1090b20fae847344
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
<<<<<<< HEAD
        options.LoginPath = "/User/Login"; // Set login page
    });

//Unicode 
=======
        options.LoginPath = "/User/Login"; // 設置登錄頁面的路徑
    });

//以Unicode編寫，使Html的亂碼可正確顯示文字
>>>>>>> 57a69941cd5a539dc541b21b1090b20fae847344
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));


builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
<<<<<<< HEAD
    app.UseHsts(); //set https
}

app.UseSession(); // set Session


app.UseHttpsRedirection(); //set https
app.UseStaticFiles();

app.UseAuthentication(); // Enable authentication
4
=======
    app.UseHsts(); //配置https重定向
}

app.UseSession(); // 使用Session中介軟體


app.UseHttpsRedirection(); //配置https重定向
app.UseStaticFiles();
app.UseAuthentication(); // 啟用身份驗證
>>>>>>> 57a69941cd5a539dc541b21b1090b20fae847344
app.UseAuthorization();

app.UseRouting();
app.UseCookiePolicy();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");

app.Run();