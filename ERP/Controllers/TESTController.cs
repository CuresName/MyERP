using Azure.Core;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Protocol;
using ERP.TESTModels.TESTEFModels;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Net.Sockets;
using System.Net;
using Microsoft.AspNetCore.Http;


namespace ERP.Controllers
{
    [AllowAnonymous]
    public class TESTController(TESTContext db, IHttpContextAccessor httpContextAccessor) : Controller
    {
        [DllImport("Iphlpapi.dll")]
        private static extern int SendARP(Int32 dest, Int32 host, ref Int64 mac, ref Int32 length);
        [DllImport("Ws2_32.dll")]
        private static extern Int32 inet_addr(string ip);

        private void GetMACAddressA()
        {
            // initial page
            try
            {
                string userip = HttpContext.Connection.RemoteIpAddress?.ToString();
                if (string.IsNullOrEmpty(userip))
                {
                    userip = "127.0.0.1"; // Default to localhost if IP address is not available
                }
                string strClientIP = userip.Trim();
                Int32 ldest = inet_addr(strClientIP); //client ip
                Int32 lhost = inet_addr(""); //server ip
                Int64 macinfo = new Int64();
                Int32 len = 6;
                int res = SendARP(ldest, 0, ref macinfo, ref len);
                string mac_src = macinfo.ToString("X");
                if (mac_src == "0")
                {
                    if (userip == "127.0.0.1")
                        Console.WriteLine("Localhost!");
                    else
                        Console.WriteLine("welcome IP = " + userip + "<br>");
                    return;
                }

                while (mac_src.Length < 12)
                {
                    mac_src = mac_src.Insert(0, "0");
                }

                string mac_dest = "";

                for (int i = 0; i < 11; i++)
                {
                    if (0 == (i % 2))
                    {
                        if (i == 10)
                        {
                            mac_dest = mac_dest.Insert(0, mac_src.Substring(i, 2));
                        }
                        else
                        {
                            mac_dest = "-" + mac_dest.Insert(0, mac_src.Substring(i, 2));
                        }
                    }
                }

                Console.WriteLine("welcome IP = " + userip + "<br>" + ",MAC = " + mac_dest

                + "<br>");
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }

        private readonly TESTContext db = db;
        public string HOST_NAME = httpContextAccessor.HttpContext.Session.GetString("電腦名稱");
        public string HOST_IP = httpContextAccessor.HttpContext.Session.GetString("UserIP");

        public IActionResult Index(string 流程卡號, string 生產站別)
        {
            return View();
        }
        public IActionResult WB()
        {
            return View();
        }
        public IActionResult DB()
        {
            return View();
        }
        public IActionResult AllView(string 流程卡號, string 生產站別)
        {


            var 生產回報 = new WIP_001_Models
            {
                流程卡號 = 流程卡號,
                生產站別 = 生產站別,
                //獲取數據並轉換為 SelectListItem 列表
                WIP_001_生產回報_流程卡號_SelectListItem = db.WIP_001_生產回報資料來源
              .Where(m => m.生產站別 == 生產站別 && m.建檔日期 > new DateTime(2023, 1, 1))
              .OrderByDescending(m => m.回報編號)
              .Select(m => new SelectListItem
              {
                  Value = m.流程卡號.ToString(),
                  Text = m.流程卡號.ToString() + " | " + m.客戶編號,
                  Selected = 流程卡號 == m.流程卡號.ToString()
              }).ToList()
            };
            if (流程卡號 == null || 生產站別 == null)
            {
            }
            else
            {

                var 各站異常代號 = db.WIP_001_流程卡資料_各站異常代號(生產站別).ToList();
                var b = 各站異常代號.Where(m => m.不良編號 == 8);


                var 生產回報資料來源 = db.WIP_001_生產回報資料來源.Where(m => m.流程卡號 == 流程卡號 && m.生產站別 == 生產站別).FirstOrDefault();
                生產回報.WIP_001_生產回報資料來源 = 生產回報資料來源;
                生產回報.WIP_001_生產回報_Cassette_NoResult = db.WIP_001_生產回報_Cassette_No(生產回報資料來源.工單編號).ToList();
                生產回報.WIP_001_生產回報_產出資料 = db.WIP_001_生產回報_產出資料.Where(m => m.回報編號 == 生產回報資料來源.回報編號).ToList();
                生產回報.WIP_001_生產回報_不良品 = db.WIP_001_生產回報_不良品.Where(m => m.回報編號 == 生產回報資料來源.回報編號).ToList();
                var 不良編號集合 = 生產回報.WIP_001_生產回報_不良品.Select(m => m.不良編號);
                if (不良編號集合.Count() != 0)
                {
                    try
                    {
                        生產回報.不良品_異常代號 = 各站異常代號.Where(m => 不良編號集合.Contains(m.不良編號)).ToList();
                    }
                    catch (Exception ex) { }
                }
                生產回報.各站異常代號 = db.WIP_001_流程卡資料_各站異常代號(生產站別).Select(m => new SelectListItem
                {
                    Value = m.不良編號.ToString(),
                    Text = m.不良代號 + " | " + m.不良原因 + " | " + m.不良英文原因
                }).ToList();
                生產回報.產線現有班別_SelectListItem = db.WIP_001_產線現有班別.Select(m => new SelectListItem
                {
                    Value = m.生產班別,
                    Text = m.生產班別 + " | " + m.中文說明 + " | " + m.英文說明
                }).ToList();
                生產回報.機台編號_SelectListItem = db.WIP_001_流程卡資料_機台編號(生產回報資料來源.工單編號, 生產站別).Select(m => new SelectListItem
                {
                    Value = m.機台編號,
                    Text = m.機台編號
                }).ToList();
                生產回報.現有人員_SelectListItem =db.現有人員查詢.Select(m=>new SelectListItem
                {
                    Value=m.員工代號,
                    Text=m.員工代號+"  |  "+m.姓名+"  |  "+m.英文姓名,
                }).ToList();
            }

            return View(生產回報);

        }

        [HttpPost]
        public IActionResult Page_Load()
        {
            try
            {
                var files = Request.Form.Files;
                var 流程卡號 = Request.Form["流程卡號"]; // 獲取流程卡號
                var FileType = Request.Form["fileType"];
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "WIP", 流程卡號);
                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }
                        var filePath = Path.Combine(uploadPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        var fileContent = Request.Form.Files[file.Name];
                        return Json(new { message = "Success", fileName = fileName, filePath = filePath, FileType = FileType });
                    }

                }
                return Json(new { message = "No files uploaded" });
            }
            catch (Exception ex)
            {
                return Json(new { message = "Upload failed", error = ex.Message });
            }
        }

        #region 產出資料_操作
        [HttpPost]
        public async Task<IActionResult> 產出資料_更新(string 流程卡號, string 生產站別, int? 產出編號, DateTime? 生產日期, string 生產班別, string 生產機台, string 生產人員1, string 生產人員2, int? 產出數量, int? 不良數量)
        {
            try
            {
                await db.Procedures.WIP_001_生產回報_產出資料_記錄更新Async(產出編號, 生產日期, 生產班別, 生產機台, 生產人員1, 生產人員2, 產出數量, 不良數量, HOST_NAME, HOST_IP);
            }
            catch (Exception ex) { ViewBag.Error = "" + ex; }

            return RedirectToAction("AllView", new { 流程卡號 = 流程卡號, 生產站別 = 生產站別 });
        }
        [HttpPost]
        public async Task<IActionResult> 產出資料_新增(string 流程卡號, string 生產站別, int? 回報編號, DateTime? 生產日期, string 生產班別, string 生產機台, string 生產人員1, string 生產人員2, int? 產出數量, int? 不良數量)
        {
            try
            {
                await db.Procedures.WIP_001_生產回報_產出資料_記錄新增Async(回報編號, 生產日期, 生產班別, 生產機台, 生產人員1, 生產人員2, 產出數量, 不良數量, HOST_NAME, HOST_IP);
            }
            catch (Exception ex) {TempData["Error"] = "" + ex; }

            return RedirectToAction("AllView", new { 流程卡號 = 流程卡號, 生產站別 = 生產站別 });
        }
        [HttpPost]
        public async Task<IActionResult> 產出資料_刪除(string 流程卡號, string 生產站別, int? 產出編號)
        {
            try
            {
                await db.Procedures.WIP_001_生產回報_產出資料_記錄刪除Async(產出編號);
            }
            catch (Exception ex) {TempData["Error"] = "" + ex; }

            return RedirectToAction("AllView", new { 流程卡號 = 流程卡號, 生產站別 = 生產站別 });
        }
        #endregion
        #region 不良品操作
        [HttpPost]
        public async Task<IActionResult> 不良品_新增(string 流程卡號, string 生產站別, int? 回報編號, int? 不良編號, int? 不良數量)
        {
            try
            {
                await db.Procedures.WIP_001_生產回報_不良品_記錄新增Async(回報編號, 不良編號, 不良數量, HOST_NAME, HOST_IP);
            }
            catch (Exception ex) { TempData["Error"] = "" + ex; }
            return RedirectToAction("AllView", new { 流程卡號 = 流程卡號, 生產站別 = 生產站別 });
        }
        [HttpPost]
        public async Task<IActionResult> 不良品_更新(string 流程卡號, string 生產站別, int 不良品流水號, int? 回報編號, int? 不良編號, int? 不良數量)
        {
            try
            {
                await db.Procedures.WIP_001_生產回報_不良品_記錄更新Async(不良品流水號, 不良編號, 不良數量, HOST_NAME, HOST_IP);
            }
            catch (Exception ex) {TempData["Error"] = "" + ex; }
            return RedirectToAction("AllView", new { 流程卡號 = 流程卡號, 生產站別 = 生產站別 });
        }
        [HttpPost]
        public async Task<IActionResult> 不良品_刪除(string 流程卡號, string 生產站別, int 不良品流水號)
        {
            try
            {
                await db.Procedures.WIP_001_生產回報_不良品_記錄刪除Async(不良品流水號);
            }
            catch (Exception ex) {TempData["Error"] = "" + ex; }
            return RedirectToAction("AllView", new { 流程卡號 = 流程卡號, 生產站別 = 生產站別 });
        }
        #endregion

    }


}
