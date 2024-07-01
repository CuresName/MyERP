using Azure.Core;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using 南岩ERP.Models.WIPEFModels;
using NuGet.Protocol;
using System.Net.Sockets;
using System.Net;

namespace 南岩ERP.Controllers
{
    [AllowAnonymous]
    public class WIPController(WIPContext db) : Controller
    {
        private readonly WIPContext db = db;
        public IActionResult Index()
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
            var 生產回報 = new WIP_001_Models();

            //获取数据并转换为 SelectListItem 列表
            生產回報.WIP_001_生產回報_流程卡號_SelectListItem = db.WIP_001_生產回報資料來源
              .Where(m => m.生產站別 == 生產站別 && m.KEYIN_DATE > new DateTime(2024, 1, 1))
              .OrderByDescending(m => m.回報編號)
              .Select(m => new SelectListItem
              {
                  Value = m.流程卡號.ToString(),
                  Text = m.流程卡號.ToString()
              }).ToList();
            if (流程卡號 == null && 生產站別 == null)
            {
                生產回報.WIP_001_生產回報資料來源 = new WIP_001_生產回報資料來源();
                生產回報.WIP_001_生產回報_Cassette_NoResult = new List<WIP_001_生產回報_Cassette_NoResult>();
                生產回報.WIP_001_生產回報_不良品 = new List<WIP_001_生產回報_不良品>();
            }
            else
            {
                var 生產回報資料來源 = db.WIP_001_生產回報資料來源.Where(m => m.流程卡號 == 流程卡號 && m.生產站別 == 生產站別).FirstOrDefault();
                生產回報.WIP_001_生產回報資料來源 = 生產回報資料來源;
                生產回報.WIP_001_生產回報_Cassette_NoResult = db.WIP_001_生產回報_Cassette_No(生產回報資料來源.工單編號).ToList();
                生產回報.WIP_001_生產回報_產出資料 = db.WIP_001_生產回報_產出資料.Where(m => m.回報編號 == 生產回報資料來源.回報編號).ToList();
                生產回報.WIP_001_生產回報_不良品 = db.WIP_001_生產回報_不良品.Where(m => m.回報編號 == 生產回報資料來源.回報編號).ToList();
            }
            ViewBag.生產站別 = 生產站別;
            return View(生產回報);

        }


        //[HttpGet]
        //public JsonResult Get流程卡號Data(string 生產站別, string 流程卡號)
        //{
        //    var items = db.WIP_001_生產回報資料來源
        //    .Where(m => m.流程卡號.Contains(流程卡號) && m.生產站別 == 生產站別)
        //    .OrderByDescending(m => m.回報編號)
        //    .Select(m => new SelectListItem
        //    {
        //        Value = m.流程卡號.ToString(),
        //        Text = m.流程卡號.ToString()
        //    }).ToList();
        //    return Json(new { items, total_count = db.WIP_001_生產回報資料來源.Count() });
        //}
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
                        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads","WIP", 流程卡號);
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
                        return Json(new { fileName = fileName, filePath = filePath , FileType = FileType });
                    }

                }
                return Json(new { message = "No files uploaded" });
            }
            catch (Exception ex) { 
                return Json(new { message = "Upload failed", error = ex.Message }); 
            }
        }

        protected void MyUpload()
        {
            //for (int i = 0; i < Request.Form.Files.Count; i++)
            //{
            //    var postedFile = Request.Form.Files[i];

            //    if (postedFile.ContentLength > 0)
            //    {
            //        string fileName = Path.GetFileName(postedFile.FileName);
            //        postedFile.SaveAs(Path.Combine(("~/Uploads/") + fileName));
            //    }
            //}
        }
        [HttpPost]
        public IActionResult FileUpload()
        {
            var files = Request.Form.Files;
            foreach (var file in files)
            {
                var a = file;
                var filename = Path.GetFileName(file.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                var filePath = Path.Combine(uploadPath, filename);

                var fileContent = Request.Form.Files[file.Name];

            }


            return Json(new { status = "Successed" });
        }


    }


}
