using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using �n��ERP.Controllers;
using �n��ERP.Filter;
using �n��ERP.Models;
using �n��ERP.�Ш|�V�mEFModels;
using �n��ERP.MisEFModels;
using �n��ERP.TableTest;
using �n��ERP.Models.WIPEFModels;
using NLog.Web;
using �n��ERP.TESTModels.TESTEFModels;
using System.Net;

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddLogging(logging =>
{  //�NNLog���U�즹�M�פ�
    logging.ClearProviders();
    //�]�wlog�������̤p����
    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
}); 
builder.Host.UseNLog();

builder.Services.AddControllersWithViews();

//�Ϩ�LController�i�H�ϥθ�controller��function
builder.Services.AddScoped<DocumentController>();
builder.Services.AddScoped<UserController>();

builder.Services.AddDbContext<nanoerpEntities>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("NanoEntity")));//�s����Ʈw
builder.Services.AddDbContext<nanodevContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("Nanodev")));//�s����Ʈw
builder.Services.AddDbContext<misContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("Nanodev")));//�s����Ʈw
builder.Services.AddDbContext<nanotableContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("Nanodev")));//�s����Ʈw
builder.Services.AddDbContext<WIPContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("NanoEntity")));//�s����Ʈw

builder.Services.AddDbContext<TESTContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("NanoTest")));//�s����Ʈw

builder.Services.AddSession(); // �t�mSession�A��


//�NAuthorizeFilter�M�Φܾ�Өt��
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
        options.LoginPath = "/User/Login"; // �]�m�n�����������|
    });

//�HUnicode�s�g�A��Html���ýX�i���T��ܤ�r
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));


builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts(); //�t�mhttps���w�V
}

app.UseSession(); // �ϥ�Session�����n��


app.UseHttpsRedirection(); //�t�mhttps���w�V
app.UseStaticFiles();
app.UseAuthentication(); // �ҥΨ������Ҥ����n��
app.UseAuthorization();

app.UseRouting();
app.UseCookiePolicy();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");

app.Run();
