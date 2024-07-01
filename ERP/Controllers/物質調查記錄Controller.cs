using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using ERP.Models;
using System.Diagnostics;
using X.PagedList;
using ERP.Filter;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace ERP.Controllers
{
    [選單驗證("MQA_018_物質調查記錄")]
    public partial class QA_018_物質調查記錄Controller(erpEntities db, ILogger<QA_018_物質調查記錄Controller> logger, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : Controller
    {
        private readonly nerpEntities db = db;

        private readonly string UserID = httpContextAccessor.HttpContext.Session.GetString("UserID");
        private readonly string UserIP = httpContextAccessor.HttpContext.Session.GetString("UserIP");
        private readonly string UserPC = httpContextAccessor.HttpContext.Session.GetString("電腦名稱");
        private static readonly object thisLock = new();

        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

        public IActionResult Index()
        {
            return View();
        }
        #region 調查內容清單
        readonly int pageSize = 10;
        public IActionResult List(string Search, string 結案, string 寄送狀態, string currentFilter, int page = 1)
        {
            Search ??= currentFilter;
            SetRolePermissions("MQA_018_物質調查記錄");

            ViewBag.CurrentFilter = Search;
            ViewBag.結案 = 結案;
            ViewBag.寄送狀態 = 寄送狀態;

            var ask = from s in db.QA_018_物質調查記錄_主檔 select s;
            if (!string.IsNullOrEmpty(Search))
            {
                ask = ask.Where(m => m.調查事項.Contains(Search) || m.物質調查編號.Contains(Search));
            }
            //ViewBag.NameSortParm = String.IsNullOrEmpty(current) ? "Y" : "";
            ViewBag.DoneOrNot = 結案 == "Y" ? "N" : "Y";
            ask = 結案 switch
            {
                "Y" => ask.Where(m => m.結案 == "Y"),
                "N" => ask.Where(m => m.結案 == "N"),
                _ => ask.OrderByDescending(m => m.物質調查流水號),
            };
            ask = 寄送狀態 switch
            {
                "Y" => ask.Where(m => m.寄送狀態 == "Y" || m.寄送狀態 == "S"),
                "N" => ask.Where(m => m.寄送狀態 == "N"),
                _ => ask.OrderByDescending(m => m.物質調查流水號),
            };
            ask = ask.OrderByDescending(m => m.物質調查流水號);
            int currentPage = page < 1 ? 1 : page;
            var pages = ask.ToPagedList(currentPage, pageSize);


            return View(pages);
        }
        #endregion

        #region 調查內容

        public ActionResult Detail(int Id)
        {
            //設定資料
            var assk = db.QA_018_物質調查記錄_主檔.Where(m => m.物質調查流水號 == Id).FirstOrDefault(); //回傳第一筆相符資料，若無則回傳null
            var companyReplay = db.QA_018_物質調查記錄_明細.Where(m => m.物質調查流水號 == Id).Select(m => m.供應商編號).ToList();
            var Company = db.MA_005_供應商資料查詢().ToList();
            //教資料儲存在Model並給予View資料
            QA_018_ModelsManager mm = new QA_018_ModelsManager()
            {
                Company = Company,
                廠商回覆 = db.QA_018_物質調查記錄_明細.Where(m => m.物質調查流水號 == Id).ToList(),
                Ask = db.QA_018_物質調查記錄_主檔.Where(m => m.物質調查流水號 == Id).FirstOrDefault(),
                AskFile = db.QA_018_物質調查記錄_附件.Where(m => m.物質調查流水號 == assk!.物質調查流水號).ToList(),
            };

            //取得使用者系統權限
            SetRolePermissions("MQA_018_物質調查記錄");
            if (ViewBag.新增 == 0 && ViewBag.修改 == 0 && ViewBag.刪除 == 0)
            {
                TempData["目前狀態"] = "唯讀狀態";
            }

            return View(mm);
        }
        #endregion

        # region 新增調查
        [HttpGet]
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "MQA_018_物質調查記錄", "新增" }])]
        public ActionResult Create()
        {
            //初始化資料庫，在使用者為儲存前都暫存在.net core的Model裡
            QA_018_ModelsManager viewModel = new()
            {
                // 初始化需要的屬性，例如廠商回覆、詢問內容、公司等
                廠商回覆 = [],
                詢問內容 = [],
                Ask = new QA_018_物質調查記錄_主檔
                {

                    物質調查編號 = "",
                    調查人員 = UserID,
                    調查日期 = DateTime.Now,
                    回覆期限 = DateTime.Now,
                    結案 = "N",
                    寄送狀態 = "N",
                    建檔日期 = DateTime.Now,
                    KEYIN_DATE = DateTime.Now,
                    HOST_IP = UserIP,
                    HOST_NAME = UserPC,
                    MAN_ID = UserID,
                    物質調查流水號 = 0
                },
                Replay = new QA_018_物質調查記錄_明細(),
                AskFile = [],
                Company = []
            };

            SetRolePermissions("MQA_018_物質調查記錄");
            //View設定目前狀態"新增調查"
            TempData["目前狀態"] = "新增調查";
            //給予空白表格
            return View("Detail", viewModel);
        }
        #endregion

        #region 調查內容更新
        [HttpPost]
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "MQA_018_物質調查記錄", "修改" }])]
        public ActionResult Upload(QA_018_ModelsManager vask)
        {
            //取得目前使用者讀取的檔案
            var i = Get調查編號();
            var Id = vask.Ask.物質調查流水號;
            var ask = db.QA_018_物質調查記錄_主檔.Where(m => m.物質調查流水號 == Id).FirstOrDefault();
            if (ask == null)
            {
                QA_018_物質調查記錄_主檔 qa_018 = new QA_018_物質調查記錄_主檔()
                {
                    物質調查編號 = i,
                    調查人員 = vask.Ask.調查人員,
                    調查日期 = vask.Ask.調查日期,
                    調查事項 = vask.Ask.調查事項,
                    調查事項英文 = vask.Ask.調查事項英文,
                    內文2 = vask.Ask.內文2,
                    內文2英文 = vask.Ask.內文2英文,
                    內文3 = vask.Ask.內文3,
                    內文3英文 = vask.Ask.內文3英文,
                    回覆期限 = vask.Ask.回覆期限,
                    備註 = vask.Ask.備註,
                    結案 = "N",
                    寄送狀態 = "N",
                    建檔日期 = DateTime.Now,
                    KEYIN_DATE = DateTime.Now,
                    HOST_IP = UserIP,
                    HOST_NAME = UserPC,
                    MAN_ID = UserID
                };
                db.QA_018_物質調查記錄_主檔.Add(qa_018);
            }
            else
            {
                ask.回覆期限 = vask.Ask.回覆期限;
                ask.調查事項 = vask.Ask.調查事項;
                ask.調查事項英文 = vask.Ask.調查事項英文;
                ask.結案 = vask.Ask.結案;
                ask.結案日期 = vask.Ask.結案日期;
                ask.內文2 = vask.Ask.內文2;
                ask.內文2英文 = vask.Ask.內文2英文;
                ask.內文3 = vask.Ask.內文3;
                ask.內文3英文 = vask.Ask.內文3英文;
                ask.備註 = vask.Ask.備註;
                ask.KEYIN_DATE = DateTime.Now;
                ask.HOST_IP = UserIP;
                ask.MAN_ID = UserID;
                ask.HOST_NAME = UserPC;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.LogInformation("QA_Upload: {Id}  {IP}  {Error}", UserID, UserIP, ex.Message);
                return Content(ex.Message);
            }
            if (ask == null)
            {
                Id = db.QA_018_物質調查記錄_主檔.Where(m => m.物質調查編號 == i).FirstOrDefault().物質調查流水號;
            }
            return RedirectToAction("Detail", new { Id });
        }
        #endregion

        #region 上傳檔案
        [HttpPost]
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "MQA_018_物質調查記錄", "新增" }])]
        public async Task<IActionResult> UploadByAjax(int 物質調查流水號)
        {
            string folderPath = CreateFloder(物質調查流水號.ToString());
            QA_018_物質調查記錄_附件 askFile = new();
            //取得目前 HTTP 要求的 HttpRequestBase 物件
            foreach (var files in Request.Form.Files)
            {
                bool isValidFileName = MyRegex().IsMatch(files.Name);
                var fileContent = Request.Form.Files[files.Name];
                if (isValidFileName)
                {
                    if (fileContent != null)
                    {
                        // 取得的檔案是stream
                        var stream = fileContent.OpenReadStream;
                        var fileName = Path.GetFileName(files.Name);
                        var path = Path.Combine(folderPath, fileName);
                        var Id = db.QA_018_物質調查記錄_附件.Where(m => m.物質調查流水號 == 物質調查流水號 && m.附件檔名 == fileName).FirstOrDefault();
                        //判斷是否有相同檔名 如果相同存入已建立sql資料
                        if (Id != null)
                        {
                            TempData["message"] = fileName + "  已有相同名稱文件，請刪除後在重新上傳";

                            return Json("error");
                        }
                        else
                        {
                            askFile.物質調查流水號 = 物質調查流水號;
                            askFile.附件檔名 = fileName;
                            askFile.建檔日期 = DateTime.Now;
                            askFile.KEYIN_DATE = DateTime.Now;
                            askFile.HOST_IP = UserIP;
                            askFile.HOST_NAME = UserPC;
                            askFile.MAN_ID = UserID;
                            db.QA_018_物質調查記錄_附件.Add(askFile);
                        }
                        try
                        {
                            using var fileStream = System.IO.File.Create(path);
                            await files.CopyToAsync(fileStream);
                        }
                        catch (Exception ex)
                        {
                            TempData["Error"] = "檔案儲存失敗  " + ex;
                            return Json("檔案儲存失敗XXX" + ex);
                        }
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            logger.LogInformation("QA_018_CreateFloder: {Id}  {IP}  {Error}", UserID,UserIP, ex.Message);
                            TempData["Error"] = "檔案儲存失敗DBSaveChanges  " + ex;
                            return Json("檔案儲存失敗DBSaveChanges" + ex);
                        }
                    }
                }
                else
                {
                    TempData["Error"] = "檔案名稱只能為中文或英文!";
                }

            }
            return Json(new { status = "Successed" });
        }
        #endregion

        #region 建立資料夾
        public string CreateFloder(string 物質調查流水號)
        {
            //在儲存檔案時，如果沒有該單號的資料夾就建立一個
            string folderPath = Path.Combine("\\\\fileserver\\erp-files\\QA_018_物質調查記錄", 物質調查流水號);
            DirectoryInfo dirInfo = new(folderPath);

            if (dirInfo.Exists == false)
            {
                Directory.CreateDirectory(folderPath);
            }
            return folderPath;
        }
        #endregion

        #region 下載檔案
        public ActionResult Downloads(string path)
        {
            //供使用者下載以上傳之檔案

            string fileName = Path.GetFileName(path); //取得該檔案存放位置
            byte[] fileBytes = System.IO.File.ReadAllBytes(path); //讀取檔案資料
            var contentType = "application/force-download";
            return File(fileBytes, contentType, fileName);
        }
        #endregion

        #region 刪除上傳檔案
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "MQA_018_物質調查記錄", "刪除" }])]
        public JsonResult DeleteFile(int 物質調查流水號, string FileName)
        {
            //取得該檔案存放在sql裡的內容
            var askFile = db.QA_018_物質調查記錄_附件.Where(m => m.物質調查流水號 == 物質調查流水號 && m.附件檔名 == FileName).FirstOrDefault();
            //取得檔案位置
            string filePath = Path.Combine("\\\\fileserver\\erp-files\\QA_018_物質調查記錄", 物質調查流水號.ToString(), askFile!.附件檔名);
            //刪除該檔案
            System.IO.File.Delete(filePath);
            //刪除該檔案存放在sql裡的內容
            db.QA_018_物質調查記錄_附件.Remove(askFile);
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex) {
                logger.LogInformation("QA_Upload: {Id}  {IP}  {Error}",UserID,UserIP, ex.Message);
                TempData["Error"]= ex.Message;
            }

            return Json(new { Result = FileName });
        }
        #endregion

        #region 新增供應商
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "MQA_018_物質調查記錄", "修改" }])]
        public IActionResult AddCompany(List<string> receiver, int Id)
        {
            var ask = db.QA_018_物質調查記錄_主檔.Where(m => m.物質調查流水號 == Id).FirstOrDefault();
            QA_018_物質調查記錄_明細 Replay = new();
            if (receiver == null)
            {
                TempData["message"] = "請選擇供應商!";
                return RedirectToAction("Detail", new { Id });
            }
            foreach (var maill in receiver)
            {
                //設定收件者
                var 供應商編號 = db.MA_005_供應商資料查詢().Where(m => m.ISO_EMAIL == maill).First();
                Replay.物質調查明細流水號 = 0;
                Replay.物質調查流水號 = Id;
                Replay.ISO_EMAIL = maill;
                Replay.供應商編號 = 供應商編號.供應商編號;
                Replay.建檔日期 = DateTime.Now;
                Replay.KEYIN_DATE = DateTime.Now;
                Replay.MAN_ID = UserID;
                Replay.HOST_IP = UserIP;
                Replay.HOST_NAME = UserPC;
                db.QA_018_物質調查記錄_明細.Add(Replay);
                var replayTime = ask.回覆期限.Value.ToString("yyyy/MM/dd");
                try
                {
                    db.SaveChanges();
                    logger.LogInformation("QA_018_AddCompany: {Id}  {IP}  更新 QA_018_物質調查記錄_主檔", UserID, UserIP );
                }
                catch (DbUpdateException ex)
                {
                    // 檢查內部異常
                    if (ex.InnerException != null)
                    {
                        var innerException = ex.InnerException;
                        // 輸出異常訊息
                        var innerException1 = "Inner Exception Message: " + innerException.Message;
                        var innerException2 = "Inner Exception Stack Trace: " + innerException.StackTrace;
                        logger.LogInformation("QA_018_AddCompany: {Id}  {IP}  {Error}", UserID, UserIP, ex.Message);
                        TempData["Error"] = ex.Message + " innerException1:" + innerException1 + " innerException2:" + innerException2;

                    }

                    return RedirectToAction("Detail", new { Id });
                }
            }
            return RedirectToAction("Detail", new { Id });
        }
        #endregion

        #region 供應商回覆內容修改
        [HttpPost]
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "MQA_018_物質調查記錄", "修改" }])]

        public ActionResult ReplayContext(DateTime?[] replayDate, string[] replayState, string[] replayRemark, string[] 供應商編號, int[] 物質調查流水號)
        {
            int i = 0;
            foreach (var id in 物質調查流水號)
            {
                string a = 供應商編號[i];
                var replay = db.QA_018_物質調查記錄_明細.Where(m => m.物質調查流水號 == id && m.供應商編號 == a).FirstOrDefault();
                replay.回覆日期 = replayDate[i];
                replay.回覆狀態 = replayState[i];
                replay.備註 = replayRemark[i];
                replay.KEYIN_DATE = DateTime.Now;
                replay.HOST_IP = UserIP;
                replay.HOST_NAME = UserPC;
                replay.MAN_ID = UserID;
                db.SaveChanges();
                i++;
            }
            i = 物質調查流水號[0];
            var replays = db.QA_018_物質調查記錄_明細.Where(m => m.物質調查流水號 == i).FirstOrDefault();

            return RedirectToAction("Detail", new { Id = replays.物質調查流水號 });
        }
        #endregion

        #region 寄出Email
        [HttpPost]
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "MQA_018_物質調查記錄", "修改" }])]
        public JsonResult SendMail(int Id)
        {
            //從資料庫查詢，確認目前單號的供應商清單是否為空
            var repaly = db.QA_018_物質調查記錄_明細.Where(m => m.物質調查流水號 == Id).FirstOrDefault();
            if (repaly == null)
            {
                TempData["Error"] = "未選擇供應商!!";
                return Json(new { status = "未選擇供應商!!" });
            }
            if (CheckMail(Id))
            {
                var ask = db.QA_018_物質調查記錄_主檔.Where(m => m.物質調查流水號 == Id).FirstOrDefault();
                ask.寄送狀態 = "Y";
                ask.寄送日期 = DateTime.Now;
                ask.HOST_IP = UserIP;
                ask.HOST_NAME = UserPC;
                ask.MAN_ID = UserID;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return Json(ex.Message);
                }
                return Json("Successed");
            }
            else
            {
                TempData["Error"] = "內容尚未完成!!";
                return Json(new { status = "內容尚未完成!!" });
            }

        }
        #endregion

        #region 刪除供應商回覆
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "MQA_018_物質調查記錄", "修改" }])]
        public JsonResult DeleteReplay(string 供應商編號, int 物質調查流水號)
        {
            var Replay = db.QA_018_物質調查記錄_明細.Where(m => m.物質調查流水號 == 物質調查流水號 && m.供應商編號 == 供應商編號).FirstOrDefault();
            if (Replay != null)
            {
                db.QA_018_物質調查記錄_明細.Remove(Replay);
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new { status = ex });
            }
            return Json(new { status = "Successed" });
        }
        #endregion

        #region 檢查信件內容
        public bool CheckMail(int Id)
        {
            //取得該單號資料庫內容
            var ask = db.QA_018_物質調查記錄_主檔.Where(m => m.物質調查流水號 == Id).FirstOrDefault();
            var replayTime = ask!.回覆期限.Value.ToString("yyyy/MM/dd");
            var 內容 = ask.調查事項.IsNullOrEmpty();
            var 主旨 = ask.調查事項.IsNullOrEmpty();
            var 內容英文 = ask.調查事項英文.IsNullOrEmpty();
            var 調查日期 = ask.調查日期.ToString().IsNullOrEmpty();
            var 回覆期限 = ask.回覆期限.ToString().IsNullOrEmpty();
            //檢查該單號的內容是否完整
            if (內容 || 主旨 || 內容英文 || 調查日期 || 回覆期限)
            {
                TempData["Error"] = "內容尚未完成";
                return false;
            }

            return true;
        }
        #endregion

        #region 取得調查編號
        public string Get調查編號()
        {
            lock (thisLock)
            {
                //調查編號為 西元年 月 + 四碼
                var a = DateTime.Now.ToString("yyyyMM") + "0001";
                var i = Int32.Parse(a);
                var mm = db.QA_018_物質調查記錄_主檔.OrderByDescending(m => m.物質調查流水號).FirstOrDefault();
                if (mm != null)
                {
                    while (i <= Int32.Parse(mm!.物質調查編號))
                    {
                        i++;
                    }
                }
                else
                {
                    while (i <= Int32.Parse(a))
                    {
                        i++;
                    }
                }

                return i.ToString();
            }

        }
        #endregion

        #region 轉給採購
        [HttpPost]
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "MQA_018_物質調查記錄", "修改" }])]
        public JsonResult SendA(int Id)
        {
            if (CheckMail(Id))
            {
                var ask = db.QA_018_物質調查記錄_主檔.Where(m => m.物質調查流水號 == Id).FirstOrDefault();
                ask.寄送狀態 = "A";

            }
            else
            {
                TempData["Error"] = "內容尚未完成!!";
                return Json(new { status = "內容尚未完成!!" });
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                TempData["Error"] = e.Message;
            }
            return Json("Successed");
        }
        #endregion

        public IActionResult 廠商未回覆清單()
        {
            var 廠商清單 = db.MA_005_供應商資料查詢().ToList();
            var 供應商清單 = db.MA_005_供應商資料查詢().Select(m => m.供應商編號).ToList();

            var 未回覆供應商 = db.QA_018_物質調查記錄_明細.Where(m => m.回覆狀態 == null).OrderBy(m => m.供應商編號).ThenBy(m => m.物質調查流水號).ToList();
            var 調查 = db.QA_018_物質調查記錄_主檔.ToList();
            QA_018_物質調查記錄_供應商未回覆清單 未回覆清單 = new()
            {
                廠商 = 廠商清單,
                未回覆清單 = 未回覆供應商,
                調查事項 = 調查,
                廠商未回覆清單 = []
            };
            foreach (var item in 未回覆供應商)
            {
                未回覆清單.廠商未回覆清單[item.供應商編號] = [];
                foreach (var line in 未回覆供應商)
                {
                    if (line.供應商編號 == item.供應商編號)
                    {
                        未回覆清單.廠商未回覆清單[item.供應商編號].Add(line.物質調查流水號.ToString());
                    }
                }
            }

            return View(未回覆清單);
        }

        [GeneratedRegex(@"^[\u4e00-\u9fa5a-zA-Z0-9\s\p{P}]+$")]
        private static partial Regex MyRegex();

        #region 刪除
        //未使用
        public IActionResult Delete(int a)
        {
            var aa = db.QA_018_物質調查記錄_主檔.Where(m => m.物質調查流水號 == a).FirstOrDefault();
            if (aa != null)
                db.Remove(aa);
            db.SaveChanges();
            return RedirectToAction("List");
        }
        #endregion
        public void SetRolePermissions(string 使用程式)
        {
            var SYS_RP_角色驗證 = db.SYS_RP_角色驗證(UserID, "MVC", 使用程式).FirstOrDefault();
            ViewBag.新增 = SYS_RP_角色驗證.新增;
            ViewBag.修改 = SYS_RP_角色驗證.修改;
            ViewBag.刪除 = SYS_RP_角色驗證.刪除;
        }
    }
}
