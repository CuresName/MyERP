
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;
using ERP.Filter;
using ERP.MisEFModels;
using ERP.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;

namespace ERP.Controllers
{
    [選單驗證("SYS_資訊設備軟體明細")]
    public class MisController(misContext db, erpEntities _db, ILogger<MisController> logger, IHttpContextAccessor httpContextAccessor) : Controller
    {
        private readonly misContext db = db;
        private readonly erpEntities _db = _db;

        private readonly string UserID = httpContextAccessor.HttpContext.Session.GetString("UserID");

        private readonly string UserIP = httpContextAccessor.HttpContext.Session.GetString("UserIP");
        private readonly string UserPC = httpContextAccessor.HttpContext.Session.GetString("電腦名稱");

        readonly int pageSize = 20;

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ComputerDetails(string Search, string currentFilter, int page = 1)
        {
            SetRolePermissions("SYS_資訊設備軟體明細");
            Search ??= currentFilter;
            ViewBag.CurrentFilter = Search;
            var 資訊設備 = from s in db.資訊設備明細表 select s;
            if (!string.IsNullOrEmpty(Search))
            {
                資訊設備 = 資訊設備.Where(m => m.電腦名稱_or_設備型號.Contains(Search));
            }
            資訊設備 = 資訊設備.OrderBy(設備 => 設備.公司).ThenBy(設備 => 設備.放置位置);

            int currentPage = page < 1 ? 1 : page;
            var pages = 資訊設備.ToPagedList(currentPage, pageSize);
            return View(pages);
        }
        public IActionResult SoftToDetail(string equipmentName)
        {
            return RedirectToAction("ComputerDetails", new { Search = equipmentName });
        }
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "SYS_資訊設備軟體明細", "修改" }])]
        public IActionResult Detail(int 流水號, string equipmentName)
        {
            var 設備詳情 = db.資訊設備明細表.Where(m => m.流水號 == 流水號 && m.電腦名稱_or_設備型號 == equipmentName).FirstOrDefault();
            return View(設備詳情);
        }
        [HttpPost]
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "SYS_資訊設備軟體明細", "修改" }])]
        public IActionResult ComputerEdit(資訊設備明細表 資訊設備)
        {
            var 設備 = db.資訊設備明細表.Where(m => m.流水號 == 資訊設備.流水號 && m.電腦名稱_or_設備型號 == 資訊設備.電腦名稱_or_設備型號).FirstOrDefault();
            if (資訊設備.電腦名稱_or_設備型號 == null)
            {
                TempData["Error"] = "請輸入設備名稱";
                return RedirectToAction("Detail", new { 流水號 = 資訊設備.流水號, equipmentName = 資訊設備.電腦名稱_or_設備型號 });
            }
            if (設備 != null)
            {
                設備.公司 = 資訊設備.公司;
                設備.使用部門 = 資訊設備.使用部門;
                設備.使用者_or_設備名稱_員工編號_姓名_分機 = 資訊設備.使用者_or_設備名稱_員工編號_姓名_分機;
                設備.放置位置 = 資訊設備.放置位置;
                設備.電腦名稱_or_設備型號 = 資訊設備.電腦名稱_or_設備型號;
                設備.設備序號 = 資訊設備.設備序號;
                設備.IP_Address = 資訊設備.IP_Address;
                設備.MAC_ADDrEss = 資訊設備.MAC_ADDrEss;
                設備.原始作業系統_免費升級系統 = 資訊設備.原始作業系統_免費升級系統;
                設備.主機版 = 資訊設備.主機版;
                設備.CPU = 資訊設備.CPU;
                設備.記億體_插槽_速度_最大 = 資訊設備.記億體_插槽_速度_最大;
                設備.RAM = 資訊設備.RAM;
                設備.硬碟容量 = 資訊設備.硬碟容量;
                設備.PCI_E_4_0_x16 = 資訊設備.PCI_E_4_0_x16;
                設備.PCI_E_3_0_x16 = 資訊設備.PCI_E_3_0_x16;
                設備.PCI_E_3_0_x1 = 資訊設備.PCI_E_3_0_x1;
                設備.PCI_E_2_0_x16 = 資訊設備.PCI_E_2_0_x16;
                設備.PCI_E_x4 = 資訊設備.PCI_E_x4;
                設備.PCI_E_x1 = 資訊設備.PCI_E_x1;
                設備.PCI = 資訊設備.PCI;
                設備.AGP = 資訊設備.AGP;
                設備.ISA = 資訊設備.ISA;
                設備.上網 = 資訊設備.上網;
                設備.網卡1 = 資訊設備.網卡1;
                設備.網卡2 = 資訊設備.網卡2;
                設備.網卡3 = 資訊設備.網卡3;
                設備.網路接線盒號碼 = 資訊設備.網路接線盒號碼;
                設備.電話接線盒號碼 = 資訊設備.電話接線盒號碼;
                設備.Office版本 = 資訊設備.Office版本;
                設備.郵件軟體 = 資訊設備.郵件軟體;
                設備.Mail_Address = 資訊設備.Mail_Address;
                設備.Win10_Microsoft帳戶 = 資訊設備.Win10_Microsoft帳戶;
                設備.Win10_Microsoft帳戶密碼 = 資訊設備.Win10_Microsoft帳戶密碼;
                設備.備註 = 資訊設備.備註;
                設備.F31 = 資訊設備.F31;
                設備.KEYIN_DATE = DateTime.Now;
                設備.HOST_IP = UserIP;
                設備.MAN_ID = UserID;
                設備.HOST_NAME = UserPC;
            }
            else
            {
                資訊設備.建檔日期 = DateTime.Now;
                資訊設備.KEYIN_DATE = DateTime.Now;
                資訊設備.HOST_IP = UserIP;
                資訊設備.MAN_ID = UserID;
                資訊設備.HOST_NAME = UserPC;
                db.Add(資訊設備);
            }
            try
            {
                db.SaveChanges();
                logger.LogInformation("ComputerEdit: {Id}  {IP} 更新 資訊設備明細表", UserID, UserIP);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "儲存錯誤：" + ex;
            }
            //if (設備 == null) return RedirectToAction("Detail", new { 流水號 = 0 });
            return RedirectToAction("Detail", new { 資訊設備.流水號, equipmentName = 資訊設備.電腦名稱_or_設備型號 });
        }
        public IActionResult SoftwareDetails(string Search, string currentFilter, int page = 1)
        {
            SetRolePermissions("SYS_資訊設備軟體明細");
            Search ??= currentFilter;
            ViewBag.CurrentFilter = Search;
            var 軟體版權 = from s in db.軟體版權明細 select s;
            if (!string.IsNullOrEmpty(Search))
            {
                軟體版權 = 軟體版權.Where(m => m.安裝電腦名稱.Contains(Search) || m.軟體名稱.Contains(Search));
            }
            軟體版權 = 軟體版權.OrderBy(m => m.軟體名稱);
            int currentPage = page < 1 ? 1 : page;
            var pages = 軟體版權.ToPagedList(currentPage, pageSize);
            return View(pages);
        }
        public IActionResult EditSoftware(int 流水號, string SoftwareName)
        {
            var 軟體 = db.軟體版權明細.Where(m => m.軟體名稱 == SoftwareName && m.流水號 == 流水號).FirstOrDefault();
            return View(軟體);
        }
        [HttpPost]
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "SYS_資訊設備軟體明細", "修改" }])]
        public IActionResult EditSoftware(軟體版權明細 軟體版權明細)
        {
            var 軟體 = db.軟體版權明細.Where(m => m.軟體名稱 == 軟體版權明細.軟體名稱 && m.流水號 == 軟體版權明細.流水號).FirstOrDefault();
            if (軟體版權明細.軟體名稱 == null || 軟體版權明細.公司 == null)
            {
                TempData["Error"] = "請輸入軟體名稱/公司";
                return RedirectToAction("EditSoftware", new { 流水號 = 軟體版權明細.流水號, SoftwareName = 軟體版權明細.軟體名稱 });
            }

            if (軟體版權明細.流水號 == 0 && 軟體 == null)
            {
                軟體版權明細.建檔日期 = DateTime.Now;
                軟體版權明細.KEYIN_DATE = DateTime.Now;
                軟體版權明細.HOST_IP = UserIP;
                軟體版權明細.MAN_ID = UserID;
                軟體版權明細.HOST_NAME = UserPC;
                db.Add(軟體版權明細);
            }
            else
            {
                軟體.公司 = 軟體版權明細.公司;
                軟體.軟體名稱 = 軟體版權明細.軟體名稱;
                軟體.數量 = 軟體版權明細.數量;
                軟體.安裝電腦名稱 = 軟體版權明細.安裝電腦名稱;
                軟體.MAC = 軟體版權明細.MAC;
                軟體.產品序號 = 軟體版權明細.產品序號;
                軟體.備註 = 軟體版權明細.備註;
                軟體.帳號 = 軟體版權明細.帳號;
                軟體.密碼 = 軟體版權明細.密碼;
                軟體.已使用 = 軟體版權明細.已使用;
                軟體.主機板故障報廢 = 軟體版權明細.主機板故障報廢;
                軟體.未使用 = 軟體版權明細.未使用;
                軟體.KEYIN_DATE = DateTime.Now;
                軟體.HOST_IP = UserIP;
                軟體.MAN_ID = UserID;
                軟體.HOST_NAME = UserPC;
            }


            try
            {
                db.SaveChanges();
                logger.LogInformation("ComputerEdit: {Id}  {IP} 更新 軟體版權明細", UserID, UserIP);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "儲存錯誤：" + ex;
            }
            if (軟體 == null) return RedirectToAction("EditSoftware", new { 流水號 = 0 });

            return RedirectToAction("EditSoftware", new { 流水號 = 軟體版權明細.流水號, SoftwareName = 軟體版權明細.軟體名稱 });
        }
        [HttpGet]
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "SYS_資訊設備軟體明細", "刪除" }])]
        public IActionResult DeleteComputer(int 流水號, string equipmentName)
        {
            var 設備 = db.資訊設備明細表.Where(m => m.流水號 == 流水號 && m.電腦名稱_or_設備型號 == equipmentName).FirstOrDefault();
            if (設備 != null)
            {
                db.Remove(設備);
                try
                {
                    db.SaveChanges();
                    logger.LogInformation("DeleteComputer: {Id}  {IP}  刪除 資訊設備明細表 {PCName}", UserID, UserIP, 設備.電腦名稱_or_設備型號);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "錯誤：" + ex;
                }
            }

            return RedirectToAction("ComputerDetails", new { currentFilter = ViewBag.CurrentFilter });
        }
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "SYS_資訊設備軟體明細", "刪除" }])]
        public IActionResult DeleteSoftware(int 流水號, string SoftwareName)
        {
            var 軟體 = db.軟體版權明細.Where(m => m.流水號 == 流水號 && m.軟體名稱 == SoftwareName).FirstOrDefault();
            if (軟體 != null)
            {
                db.Remove(軟體);
            }
            try
            {
                db.SaveChanges();
                logger.LogInformation("DeleteSoftware: {Id}  {IP}  刪除 軟體版權明細 {軟體名稱}", UserID, UserIP,軟體.軟體名稱);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "錯誤：" + ex;
            }

            return RedirectToAction("SoftwareDetails", new { currentFilter = ViewBag.CurrentFilter });
        }
        public IActionResult DataBackup(string Search, string currentFilter, int page = 1)
        {
            SetRolePermissions("SYS_資訊設備軟體明細");
            Search ??= currentFilter;
            ViewBag.CurrentFilter = Search;
            var 備份設備 = from s in db.機器設備硬碟備份紀錄 select s;
            if (!string.IsNullOrEmpty(Search))
            {
                備份設備 = 備份設備.Where(m => m.設備名稱.Contains(Search));
            }
            備份設備 = 備份設備.OrderBy(m => m.設備名稱);
            int currentPage = page < 1 ? 1 : page;
            var pages = 備份設備.ToPagedList(currentPage, pageSize);

            return View(pages);
        }
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "SYS_資訊設備軟體明細", "修改" }])]
        public IActionResult EditDataBackup(int 流水號, string equipmentName)
        {
            var equipment = db.機器設備硬碟備份紀錄.Where(m => m.設備名稱 == equipmentName && m.流水號 == 流水號).FirstOrDefault();
            equipment ??= new 機器設備硬碟備份紀錄()
            {
                流水號 = 0,
                備份日期 = DateTime.Now,
            };
            return View(equipment);
        }
        [HttpPost]
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "SYS_資訊設備軟體明細", "修改" }])]
        public IActionResult EditDataBackup(機器設備硬碟備份紀錄 機器設備硬碟備份)
        {
            var 設備 = db.機器設備硬碟備份紀錄.Where(m => m.流水號 == 機器設備硬碟備份.流水號).FirstOrDefault();
            if (設備 == null)
            {
                機器設備硬碟備份.建檔日期 = DateTime.Now;
                機器設備硬碟備份.KEYIN_DATE = DateTime.Now;
                機器設備硬碟備份.HOST_IP = UserIP;
                機器設備硬碟備份.MAN_ID = UserID;
                機器設備硬碟備份.HOST_NAME = UserPC;
                db.Add(機器設備硬碟備份);
            }
            else
            {

                if (機器設備硬碟備份.IsConcurrencyToken)
                {
                    Console.WriteLine("有改");
                }
                else
                {
                    Console.WriteLine("媒改");
                }
                try
                {
                    
                    設備.設備名稱 = 機器設備硬碟備份.設備名稱;
                    設備.備註 = 機器設備硬碟備份.備註;
                    設備.備份日期 = 機器設備硬碟備份.備份日期;
                    設備.Ghost = 機器設備硬碟備份.Ghost;
                    設備.TrueImage = 機器設備硬碟備份.TrueImage;
                    設備.KEYIN_DATE = DateTime.Now;
                    設備.HOST_IP = UserIP;
                    設備.MAN_ID = UserID;
                    設備.HOST_NAME = UserPC;
                    db.SaveChanges();
                    Console.WriteLine("success");
                }
                catch(DBConcurrencyException ex)
                {
                    Console.WriteLine(ex.Message);
                }


            }
            try
            {
                db.SaveChanges();
                logger.LogInformation("DeleteComputer: {Id}  {IP}  更新 軟體版權明細 {設備名稱}", UserID, UserIP, 設備.設備名稱);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "儲存錯誤：" + ex;
            }
            if (設備 == null) return RedirectToAction("EditDataBackup", new { 流水號 = 0 });

            return RedirectToAction("EditDataBackup", new { 機器設備硬碟備份.流水號, equipmentName = 機器設備硬碟備份.設備名稱 });
        }
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "SYS_資訊設備軟體明細", "刪除" }])]
        public IActionResult DeleteDataBackup(int 流水號, string 設備名稱)
        {
            var 設備 = db.機器設備硬碟備份紀錄.Where(m => m.流水號 == 流水號 && m.設備名稱 == 設備名稱).FirstOrDefault();
            if (設備 != null)
                db.Remove(設備);
            try
            {
                db.SaveChanges();
                logger.LogInformation("DeleteComputer: {Id}  {IP}  刪除 軟體版權明細 {設備名稱}", UserID, UserIP, 設備.設備名稱);

            }
            catch (Exception ex)
            {
                TempData["Error"] = "錯誤" + ex;
            }

            return RedirectToAction("DataBackup");
        }

        public void SetRolePermissions(string 使用程式)
        {

            var SYS_RP_角色驗證 = _db.SYS_RP_角色驗證(UserID, "MVC", 使用程式).FirstOrDefault();

            ViewBag.新增 = SYS_RP_角色驗證.新增;
            ViewBag.修改 = SYS_RP_角色驗證.修改;
            ViewBag.刪除 = SYS_RP_角色驗證.刪除;
        }

    }
}
