using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Sockets;
using ERP.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;
using DocumentFormat.OpenXml.Spreadsheet;


namespace ERP.Controllers
{
    [AllowAnonymous]
    public class UserController(erpEntities db, ILogger<UserController> logger, IHttpContextAccessor httpContextAccessor) : Controller
    {

        private readonly erpEntities db = db;

        private readonly string UserIP = httpContextAccessor.HttpContext.Session.GetString("UserIP");
        private readonly string UserID = httpContextAccessor.HttpContext.Session.GetString("UserID");


        public IActionResult Login()
        {
            取得使用者IP();
            取得用戶端電腦名稱();
            ViewBag.電腦名稱 = httpContextAccessor.HttpContext.Session.GetString("電腦名稱");
            return PartialView();
        }
        //在VS裡加入自訂函數
        [HttpPost]
        public IActionResult Login(string account, string password)
        {
            if (account != null || password != null)
            {

                var result = db.SYS_USER_驗證查詢(account, password).FirstOrDefault();

                if (result == null)
                {
                    ViewBag.ErrorMessage = "帳號或密碼錯誤";
                    return PartialView("Login");
                }

                if (result.許可權 == "Y")
                {
                    // 登入成功，進行相應的處理

                    HttpContext.Session.SetString("UserID", account);

                    var 選單驗證 = db.SYS_MENU_選單驗證(account, "MVC").ToList();

                    var menu = 選單驗證.Select(m => m.PRO_ID).Distinct().ToList();
                    var 系統 = 選單驗證.Select(m => m.系統別).Distinct().ToList();
                    var menuBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(menu));
                    var 系統Bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(系統));
                    HttpContext.Session.Set("MenuList", menuBytes);
                    HttpContext.Session.Set("系統", 系統Bytes);

                    var menuList = "";
                    foreach (var item in menu)
                    {
                        menuList += item + "|";
                    }
                    HttpContext.Session.SetString("Menu", menuList);

                    HttpContext.Session.SetString("LoginTime", DateTime.Now.ToString("yyyy/MM/dd"));

                    // 紀錄日誌消息 UserID UserIP
                    logger.LogInformation("User Login: {sessionId}  {IP}", HttpContext.Session.GetString("UserID"), HttpContext.Session.GetString("UserIP"));

                    return RedirectToAction("Menu", "Menu");
                }
                else
                {
                    // 登入失敗，顯示錯誤訊息
                    ViewBag.ErrorMessage = "帳號或密碼錯誤";
                    return PartialView("Login");
                }

            }
            else
            {
                ViewBag.ErrorMessage = "請輸入帳號密碼";
                return PartialView("Login");
            }
        }

        #region 登出
        public Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            return Task.FromResult<IActionResult>(RedirectToAction("Login", "User"));
        }
        #endregion
        #region 取得使用者IP
        public void 取得使用者IP()
        {
            IPAddress? remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
            string 使用者IP = "";
            if (remoteIpAddress != null)
            {
                // If we got an IPV6 address, then we need to ask the network for the IPV4 address 
                // This usually only happens when the browser is on the same machine as the server.
                if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
            .First(x => x.AddressFamily == AddressFamily.InterNetwork);
                }
                使用者IP = remoteIpAddress.ToString();
            }
            HttpContext.Session.SetString("UserIP", 使用者IP);
        }
        #endregion
        #region 取得用戶端電腦名稱
        public void 取得用戶端電腦名稱()
        {
            string clientComputerName = Dns.GetHostEntry(HttpContext.Connection.RemoteIpAddress!).HostName;
            string[] parts = clientComputerName.Split('.'); // 使用句號作為分隔符
            string 電腦名稱 = parts[0].Trim(); // 提取第一部分並去除可能存在的空格
            HttpContext.Session.SetString("電腦名稱", 電腦名稱);
        }
        #endregion
        #region 更改密碼*
        //public ActionResult ChangePassword()
        //{
        //    if (Session["User"] != null)
        //    {
        //        string ID = Session["User"].ToString();
        //        var sysUser = db.Sys_USER.Where(m => m.MAN_ID == ID).FirstOrDefault();
        //        return View(sysUser);
        //    }
        //    else
        //    {
        //        return RedirectToAction("Login");
        //    }

        //}
        //[HttpPost]
        //public ActionResult ChangePassword(string ID, string password, string newPassword, string confirmNewPassword)
        //{
        //    string result = db.Sys_USER.Select(m => dbo.SYS_USER_驗證(ID, password)).FirstOrDefault().ToString();

        //    var sysUser = db.Sys_USER.Where(m => m.MAN_ID == ID).FirstOrDefault();
        //    var oldpsd = db.Sys_USER.Where(m => m.MAN_ID == ID).FirstOrDefault().MAN_PSWD;
        //    var psd = db.Sys_USER.Select(m => dbo.SYS_USER_解密(ID, oldpsd)).FirstOrDefault().ToString();
        //    if (result == "Y")
        //    {
        //        var UserPassword = db.Sys_USER.Where(m => m.MAN_PSWD == oldpsd).FirstOrDefault();
        //        if (UserPassword != null)
        //        {
        //            if (confirmNewPassword == newPassword)
        //            {
        //                string Newpsd = db.Sys_USER.Select(m => dbo.SYS_USER_密碼(ID, newPassword)).FirstOrDefault().ToString();
        //                UserPassword.MAN_PSWD = Newpsd;
        //                db.SaveChanges();
        //            }
        //            else
        //            {
        //                ViewBag.Error = "新密碼與確認密碼不符，請重新輸入!";
        //                return View(sysUser);
        //            }
        //        }
        //        else
        //        {
        //            ViewBag.Error = "密碼錯誤";
        //            return View(sysUser);
        //        }
        //    }
        //    else
        //    {
        //        ViewBag.Error = "密碼錯誤";
        //        return View(sysUser);
        //    }
        //    TempData["Success"] = "密碼更改成功!";
        //    return RedirectToAction("Menu", "Menu");
        //}
        #endregion
        #region 登入時顯示名稱*
        //[HttpPost]
        //public ActionResult GetID(string ID)
        //{
        //    var name = db.Sys_USER.Where(m => m.MAN_ID == ID).FirstOrDefault();
        //    return Json(name.MAN_CNM);
        //}
        #endregion

    }
}