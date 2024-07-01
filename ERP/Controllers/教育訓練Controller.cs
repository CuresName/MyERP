using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using Microsoft.IdentityModel.Tokens;
using 南岩ERP.Models;
using 南岩ERP.教育訓練EFModels;
using DocumentFormat.OpenXml.Wordprocessing;
using X.PagedList;
using 南岩ERP.Filter;

namespace 南岩ERP.Controllers
{
    [選單驗證("HR_001_教育訓練")]
    public class 教育訓練Controller(nanodevContext db, nanoerpEntities _db, DocumentController documentController, IHttpContextAccessor httpContextAccessor) : Controller
    {

        private readonly string UserIP = httpContextAccessor.HttpContext.Session.GetString("UserIP");
        private readonly string UserID = httpContextAccessor.HttpContext.Session.GetString("UserID");
        private readonly string UserPC = httpContextAccessor.HttpContext.Session.GetString("電腦名稱");
        private static readonly object thisLock = new();
        readonly int pageSize = 30;
        private readonly DocumentController _documentController = documentController;
        private readonly nanodevContext db = db;
        private readonly nanoerpEntities _db = _db;
        public IActionResult Index()
        {
            return View();
        }
        #region ISO文件
        public IActionResult ISO文件()
        {
            SetRolePermissions("HR_001_教育訓練");
            var iso文件 = new ISO文件ModelManager()
            {
                文件表 = [.. db.文件表.OrderBy(m => m.文件編號)],
                職務文件 = [.. db.職務文件關聯表],
                部門 = [.. db.部門表]
            };

            return View(iso文件);
        }
        #endregion
        #region ISO_Edit
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "HR_001_教育訓練", "修改" }])]
        public IActionResult ISO_Edit(string 文件編號)
        {
            SetRolePermissions("HR_001_教育訓練");
            var iso文件 = new ISO文件ModelManager()
            {
                文件表 = [.. db.文件表.Where(m => m.文件編號 == 文件編號)],
                職務文件 = [.. db.職務文件關聯表.Where(m => m.文件編號 == 文件編號)],
                部門 = [.. db.部門表]
            };
            return View(iso文件);
        }
        #endregion
        #region ISO Updata 
        [HttpPost]
        public IActionResult ISOUpdata(string 原文件編號, string 文件編號, string 文件名稱, string[] 職務, string[] 檢定方式)
        {
            var iso = db.文件表.Where(m => m.文件編號 == 原文件編號).FirstOrDefault();
            var 職務文件關聯表 = db.職務文件關聯表.Where(m => m.文件編號 == 原文件編號).ToList();
            if (iso != null)
            {
                foreach (var a in 職務)
                {
                    var Contain = 職務文件關聯表.Select(m => m.職務).Contains(a);
                    var 部門編號 = db.職務表.Where(m => m.職務 == a).Select(m => m.部門編號).FirstOrDefault();
                    if (!Contain)
                    {
                        var new職務 = new 職務文件關聯表()
                        {
                            部門編號 = 部門編號,
                            職務 = a,
                            文件編號 = 文件編號
                        };
                        db.職務文件關聯表.Add(new職務);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            TempData["Error"] = "錯誤：" + ex;
                        }
                    }
                }
                // 移除不存在的職務
                foreach (var 現有職務 in 職務文件關聯表.Select(m => m.職務))
                {
                    if (!職務.Contains(現有職務))
                    {
                        var 職務移除 = db.職務文件關聯表.Where(m => m.文件編號 == 原文件編號 && m.職務 == 現有職務).FirstOrDefault();
                        if (職務移除 != null)
                        {
                            db.職務文件關聯表.Remove(職務移除);
                            // 進行儲存更動到資料庫
                            try
                            {
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                TempData["Error"] = "錯誤：" + ex;
                            }
                        }
                    }
                }
                iso.文件名稱 = 文件名稱;
                iso.筆試 = Convert.ToByte((檢定方式.Contains("筆試") ? 1 : 0));
                iso.口試 = Convert.ToByte((檢定方式.Contains("口試") ? 1 : 0));
                iso.實作 = Convert.ToByte((檢定方式.Contains("實作") ? 1 : 0));
                iso.KEYIN_DATE = DateTime.Now;
                iso.HOST_IP = UserIP;
                iso.MAN_ID = UserID;
                iso.HOST_NAME = UserPC;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "錯誤：" + ex;
                }
            }
            else if (原文件編號 == null)
            {
                TempData["Error"] = "文件編號錯誤";
            }
            else
            {
                TempData["Error"] = "錯誤：找不到文件";
            }


            return RedirectToAction("ISO_Edit", new { 文件編號 });
        }
        #endregion
        #region function
        public IActionResult ISO取得職務(string 部門)
        {
            var 部門編號 = db.部門表.Where(m => m.部門名稱 == 部門).Select(m => m.部門編號).FirstOrDefault();
            var 職務 = db.職務表.Where(m => m.部門編號 == 部門編號).Select(m => m.職務).ToList();
            return Json(職務);
        }
        public IActionResult 取得職務預設(string[] 部門)
        {
            Dictionary<string, List<string>> 部門職務 = [];

            foreach (var 部門名 in 部門)
            {
                var 部門編號 = db.部門表.Where(m => m.部門名稱 == 部門名).Select(m => m.部門編號).FirstOrDefault();
                var 職務 = db.職務表.Where(m => m.部門編號 == 部門編號).Select(m => m.職務).ToList();
                部門職務.Add(部門名, 職務);
            }

            return Json(部門職務);
        }
        #endregion
        # region ModelsManager初始化
        private HR_001_ModelsManager ModelsManager初始化(string 員工編號)
        {
            var 員工表 = db.員工表.Where(m => m.員工編號 == 員工編號).First();
            var 文件列表 = 取得文件列表(員工編號);
            var 職務 = 取得職務(員工編號);
            var Iso文件List = db.文件表.ToList();
            var 員工部門 = db.部門表.Where(m => m.部門編號 == 員工表.部門編號).First();
            var 員工職務表 = db.員工職務表.Where(m => m.員工編號 == 員工編號).First();

            return new HR_001_ModelsManager()
            {
                職務名稱 = 職務,
                員工表 = 員工表,
                文件列表 = 文件列表,
                文件表 = Iso文件List,
                部門名稱 = 員工部門.部門名稱,
                員工職務表 = 員工職務表,
            };
        }
        #endregion
        #region 訓練基準表
        public IActionResult 訓練基準表(int 部門, string 職務名稱)
        {
            var 職務表 = db.職務表.Where(m => m.職務 == 職務名稱 && m.部門編號 == 部門).FirstOrDefault();
            var 文件列表 = db.職務文件關聯表.Where(m => m.職務 == 職務名稱 && m.部門編號 == 部門).Select(m => m.文件編號).ToList();
            var Iso文件List = db.文件表.ToList();
            HR_001_ModelsManager mm = new()
            {
                職務表 = 職務表,
                文件列表 = 文件列表,
                文件表 = Iso文件List,
            };
            return View(mm);
        }

        #endregion
        #region 訓練基準表excel
        public FileResult 訓練基準表excel(HR_001_ModelsManager TrangingTable, string[] trainingType, string[] testType, string[] courseId, string[] courseName)
        {

            var 部門 = db.部門表.Where(m => m.部門編號 == TrangingTable.職務表.部門編號).Select(m => m.部門名稱).FirstOrDefault();
            var 職務 = TrangingTable.職務表.職務;
            var 部門編號 = TrangingTable.職務表.部門編號;
            string[] 檢定 = ["無", "筆試", "口試", "實作", "其它"];
            var x = "";
            var excel = new List<教育訓練Excels>()
             {
                new()  {Id=1,A="職級",B=TrangingTable.職務表.職級},
                new() { Id = 2, A = "職別", B = TrangingTable.職務表.職別 },
                new() { Id = 3, A = "職務", B = 職務 },
                new() { Id = 4, A = "", B = "" },
                new() { Id = 5, A = "課程編號", B = "課程名稱" },
                new() { Id = 6, A = "", B = "公司沿革及經營理念" },
                new() { Id = 7, A = "", B = "公司規章簡介" },
                new() { Id = 8, A = "", B = "工安衛暨消防課程" },
                new() { Id = 9, A = "", B = "ISO及HSF環境限用物質管制與查驗" }
            };
            for (int i = 0; i < courseId.Length; i++)
            {
                excel.Add(new 教育訓練Excels { Id = excel.Count + 1, A = courseId[i], B = courseName[i] });
            }

            excel.Add(new 教育訓練Excels { Id = excel.Count + 1, A = "訓練方式", B = trainingType[0] == "外訓" ? "□內訓█外訓" : "█內訓□外訓" });
            foreach (var item in 檢定)
            {
                x += (testType.Any(s => s == item) ? "█" : "□") + item;
            }
            excel.Add(new 教育訓練Excels { Id = excel.Count + 1, A = "檢定方式", B = x });
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = 部門 + "-" + 職務 + ".xlsx";
            //建立Excel
            int excelCount = excel.Count;
            var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add("1");//新增分頁 分頁名稱
            worksheet.SheetView.Worksheet.SetShowGridLines(false);//隱藏格線
            worksheet.PageSetup.PrintAreas.Add("A1:F" + (excelCount + 2));//設置列印範圍
            worksheet.SheetView.View = XLSheetViewOptions.PageBreakPreview;//分頁預覽
            //設定標題列名稱與樣式
            worksheet.Cell(1, 5).Value = "首頁";//內容
            worksheet.Cell(1, 5).Style.Font.FontColor = XLColor.Blue;//文字顏色
            worksheet.Range("A1", "E" + (excelCount - 1)).Style.Font.SetFontSize(12);//字體大小
            worksheet.Range("A7", "A" + (excelCount - 1)).Style.Font.SetFontSize(10);
            worksheet.Range("A7", "A" + (excelCount - 1)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(1, 5).Style.Font.Underline = XLFontUnderlineValues.Single;//文字底線
            worksheet.Range("A1", "E" + (excelCount + 1)).Style.Font.FontName = "新細明體";
            //行列範圍
            worksheet.Range("A2", "E2").Style.Border.BottomBorder = XLBorderStyleValues.Thin; //下方框線
            worksheet.Range("A2", "E" + (excelCount - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin; //內框
            worksheet.Range("A2", "E" + (excelCount - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin; //外框 細線
            worksheet.Cell("B10").Style.Font.FontColor = XLColor.Red;

            worksheet.Range("B7", "B9").Style.Font.FontColor = XLColor.Blue;
            worksheet.Cell("B2").Style.Font.FontColor = XLColor.Blue;
            worksheet.Cell("B2").Style.Font.Underline = XLFontUnderlineValues.Single;//文字底線
            worksheet.Range("A2", "B6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;//文字置中
            //行列屬性
            worksheet.Rows().Height = 16.5;//列高
            worksheet.Columns().Width = 10.14;//行寬
            worksheet.Columns("B:F").Width = 9.53;//行寬

            for (int i = 2; i <= excelCount + 1; i++)
            {
                worksheet.Range(i, 2, i, 5).Merge();//欄合併
                worksheet.Cell(i, 1).Value = excel[i - 2].A;
                worksheet.Cell(i, 2).Value = excel[i - 2].B;
            }
            worksheet.Range(5, 1, 5, 5).Merge();

            //寫入檔案
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return File(content, contentType, fileName);
        }
        #endregion
        #region 技能檢定卡
        public IActionResult 技能檢定卡(string 員工編號)
        {
            var mm = ModelsManager初始化(員工編號);
            return View(mm);
        }

        #endregion
        #region 技能檢定表
        public IActionResult 技能檢定表(string 員工編號)
        {
            var mm = ModelsManager初始化(員工編號);
            return View(mm);
        }
        #endregion
        #region 計畫表
        public IActionResult 計畫表清單(int page = 1)
        {
            SetRolePermissions("HR_001_教育訓練");
            var 計畫表 = db.教育訓練計畫表.OrderBy(m => m.計畫表流水號);
            int currentPage = page < 1 ? 1 : page;
            var pages = 計畫表.ToPagedList(currentPage, pageSize);
            return View(pages);
        }
        public IActionResult 計畫表(string 計畫表編號)
        {
            SetRolePermissions("HR_001_教育訓練");
            var model = new HR_001_ModelsManager
            {
                計畫表Model = new 計畫表Model()
            };
            model.計畫表Model.計畫表編號 = 計畫表編號;
            model.教育訓練計畫表內容 = [.. db.教育訓練計畫表內容.Where(m => m.計畫表編號 == 計畫表編號)];
            return View(model);
        }
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "HR_001_教育訓練", "修改" }])]
        public IActionResult 計畫表Edit內容(int 計畫表內容流水號, string 計畫表編號)
        {
            var 計畫表 = db.教育訓練計畫表內容.Where(m => m.計畫表內容流水號 == 計畫表內容流水號 && m.計畫表編號 == 計畫表編號).FirstOrDefault();
            var 計畫表內容 = new 計畫表Model();
            if (計畫表 != null)
            {
                List<CheckBoxListItem> 基層別 =
                [
                    new(){ ID = 0, Text = "管理人員", IsChecked = (bool)計畫表.管理人員 },
                    new(){ ID = 1, Text = "工管理師", IsChecked = (bool)計畫表.工管理師},
                    new(){ ID = 2, Text = "作業人員", IsChecked = (bool)計畫表.作業人員}
                ];
                List<CheckBoxListItem> 訓練單位 =
                [
                    new(){ ID = 0, Text = "業務部", IsChecked = (bool)計畫表.業務部 },
                    new(){ ID = 1, Text = "研發部", IsChecked = (bool)計畫表.研發部 },
                    new(){ ID = 2, Text = "製造部", IsChecked = (bool)計畫表.製造部 },
                    new(){ ID = 3, Text = "品保部", IsChecked = (bool)計畫表.品保部 },
                    new(){ ID = 4, Text = "測試部", IsChecked = (bool)計畫表.測試部 },
                    new(){ ID = 5, Text = "財務部", IsChecked = (bool)計畫表.財務部 },
                    new(){ ID = 6, Text = "管理部", IsChecked = (bool)計畫表.管理部 }
                ];
                計畫表內容 = new 計畫表Model
                {
                    提出單位 = 計畫表.提出單位,
                    類別 = 計畫表.類別,
                    課程名稱 = 計畫表.課程名稱,
                    課程大綱 = 計畫表.課程大綱,
                    預計上課日期 = (DateTime)計畫表.預計上課日期,
                    上課時數 = (int)計畫表.上課時數,
                    訓練方式 = 計畫表.訓練方式,
                    講師 = 計畫表.講師,
                    預計費用 = 計畫表.預計費用,
                    預計參訓人數 = 計畫表.預計參訓人數,
                    預計總費用 = 計畫表.預計總費用,
                    基層別 = 基層別,
                    訓練單位 = 訓練單位
                };
            }
            else
            {
                計畫表內容.預計上課日期 = DateTime.Now;
            }
            計畫表內容.計畫表編號 = 計畫表編號;
            計畫表內容.計畫表內容流水號 = 計畫表內容流水號;
            TempData["計畫表編號"] = 計畫表編號;
            return PartialView(計畫表內容);

        }
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "HR_001_教育訓練", "修改" }])]
        public IActionResult 計畫表Edit(int 計畫表內容流水號, string 計畫表編號, 計畫表Model model)
        {
            var 計畫表內容 = db.教育訓練計畫表內容.Where(m => m.計畫表內容流水號 == 計畫表內容流水號 && m.計畫表編號 == 計畫表編號).FirstOrDefault();
            if (計畫表內容流水號 == 0 && 計畫表編號 == null)
            {
                計畫表編號 = Get計畫表編號();
                計畫表內容 = new 教育訓練計畫表內容
                {
                    計畫表編號 = 計畫表編號

                };
                var 計畫表表單 = new 教育訓練計畫表();
                計畫表表單.計畫表編號 = 計畫表編號;
                計畫表表單.建檔日期 = DateTime.Now;
                db.Add(計畫表表單);
                db.SaveChanges();
            }
            else if (計畫表內容流水號 == 0 && 計畫表編號 != null)
            {
                計畫表內容 = new 教育訓練計畫表內容
                {
                    計畫表編號 = 計畫表編號,
                    建檔日期=DateTime.Now
                };
            }
            else
            {
                計畫表內容.計畫表編號 = 計畫表編號;
            }

            計畫表內容.提出單位 = model.提出單位;
            計畫表內容.類別 = model.類別;
            計畫表內容.課程名稱 = model.課程名稱;
            計畫表內容.課程大綱 = model.課程大綱;
            計畫表內容.預計上課日期 = model.預計上課日期;
            計畫表內容.上課時數 = model.上課時數;
            計畫表內容.訓練方式 = model.訓練方式;
            計畫表內容.講師 = model.講師;
            計畫表內容.預計費用 = model.預計費用;
            計畫表內容.預計參訓人數 = model.預計參訓人數;
            計畫表內容.預計總費用 = model.預計總費用;
            計畫表內容.管理人員 = model.基層別.FirstOrDefault(m => m.Text == "管理人員")?.IsChecked;
            計畫表內容.工管理師 = model.基層別.FirstOrDefault(x => x.Text == "工管理師")?.IsChecked;
            計畫表內容.作業人員 = model.基層別.FirstOrDefault(x => x.Text == "作業人員")?.IsChecked;
            計畫表內容.業務部 = model.訓練單位.FirstOrDefault(x => x.Text == "業務部")?.IsChecked;
            計畫表內容.研發部 = model.訓練單位.FirstOrDefault(x => x.Text == "研發部")?.IsChecked;
            計畫表內容.製造部 = model.訓練單位.FirstOrDefault(x => x.Text == "製造部")?.IsChecked;
            計畫表內容.品保部 = model.訓練單位.FirstOrDefault(x => x.Text == "品保部")?.IsChecked;
            計畫表內容.測試部 = model.訓練單位.FirstOrDefault(x => x.Text == "測試部")?.IsChecked;
            計畫表內容.財務部 = model.訓練單位.FirstOrDefault(x => x.Text == "財務部")?.IsChecked;
            計畫表內容.管理部 = model.訓練單位.FirstOrDefault(x => x.Text == "管理部")?.IsChecked;
            計畫表內容.KEYIN_DATE = DateTime.Now;
            計畫表內容.HOST_IP = UserIP;
            計畫表內容.MAN_ID = UserID;
            計畫表內容.HOST_NAME = UserPC;

            try
            {
                if (計畫表內容流水號 == 0)
                {
                    var 計畫表 = db.教育訓練計畫表.Where(m => m.計畫表編號 == 計畫表編號).FirstOrDefault();
                    計畫表.KEYIN_DATE = DateTime.Now;
                    計畫表.HOST_IP = UserIP;
                    計畫表.MAN_ID = UserID;
                    計畫表.HOST_NAME = UserPC;
                    db.Add(計畫表內容);
                    db.SaveChanges();
                    計畫表內容流水號 = 計畫表內容.計畫表內容流水號;
                    TempData["response"] = "修改成功";
                    
                }
                else
                {
                    db.SaveChanges();
                    TempData["response"] = "修改成功";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            TempData["計畫表編號"] = 計畫表編號;
            return Json(new { 計畫表編號 = 計畫表編號 });
        }
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "HR_001_教育訓練", "刪除" }])]
        public IActionResult 計畫表Delete(int 計畫表內容流水號, string 計畫表編號)
        {
            if (計畫表內容流水號 == 0)
            {
                var 計畫表內容 = db.教育訓練計畫表內容.Where(m => m.計畫表編號 == 計畫表編號);

                foreach (var item in 計畫表內容)
                {
                    db.Remove(item);
                }
                db.SaveChanges();
                var 計畫表 = db.教育訓練計畫表.Where(m => m.計畫表編號 == 計畫表編號).First();
                db.Remove(計畫表);
                db.SaveChanges();
                return RedirectToAction("計畫表清單");
            }
            else
            {
                var 計畫表內容 = db.教育訓練計畫表內容.Where(m => m.計畫表內容流水號 == 計畫表內容流水號 && m.計畫表編號 == 計畫表編號).First();
                db.Remove(計畫表內容);
                db.SaveChanges();
                return RedirectToAction("計畫表", new { 計畫表編號 });
            }

        }
        #endregion
        #region 申請表
        public IActionResult 申請表(string 申請表編號)
        {
            SetRolePermissions("HR_001_教育訓練");
            var 申請表 = db.申請表.Where(m => m.申請表編號 == 申請表編號).FirstOrDefault();
            if (申請表 == null)
            {
                申請表= new 申請表();
                申請表.申請日期 = DateTime.Now;
                申請表.訓練日期 = DateTime.Now;
            }
            
            
            return View(申請表);
        }
        public IActionResult 申請表清單(int page = 1)
        {
            SetRolePermissions("HR_001_教育訓練");
            var 申請表 = db.申請表.OrderBy(m => m.申請表流水號);
            int currentPage = page < 1 ? 1 : page;
            var pages = 申請表.ToPagedList(currentPage, pageSize);
            return View(pages);
        }
        [HttpGet]
        public IActionResult 申請表編輯()
        {
            // 返回一個視圖或進行其他相應的處理
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "HR_001_教育訓練", "修改" }])]
        public IActionResult 申請表編輯(申請表 model, string 申請表編號)
        {
            var 申請表 = db.申請表.Where(m => m.申請表編號 == 申請表編號).FirstOrDefault();
            if (申請表 == null)
            {
                申請表 = new 申請表
                {
                    申請表編號 = Get申請表編號(),
                    建檔日期=DateTime.Now
                };
            }
            else
            {
                申請表.申請表編號 = 申請表編號;
            }


            申請表.申請人 = model.申請人;
            申請表.申請部門 = model.申請部門;
            申請表.申請日期 = model.申請日期;
            申請表.訓練方式 = model.訓練方式;
            申請表.聯絡單位 = model.聯絡單位;
            申請表.訓練費用 = model.訓練費用;
            申請表.課程名稱 = model.課程名稱;
            申請表.訓練目的 = model.訓練目的;
            申請表.訓練日期 = model.訓練日期;
            申請表.時間 = model.時間;
            申請表.時數 = model.時數;
            申請表.主辦單位 = model.主辦單位;
            申請表.上課地點 = model.上課地點;
            申請表.上課人員 = model.上課人員;
            申請表.講師 = model.講師;
            申請表.報名表 = model.報名表;
            申請表.宣傳單 = model.宣傳單;
            申請表.其它 = model.其它;
            申請表.簽到表 = model.簽到表;
            申請表.測驗問卷 = model.測驗問卷;
            申請表.結業證書 = model.結業證書;
            申請表.心得報告 = model.心得報告;
            申請表.學員滿意度調查表 = model.學員滿意度調查表;
            申請表.實作 = model.實作;
            申請表.訓練管理單位協助事項 = model.申請部門;
            申請表.備註 = model.備註;
            申請表.KEYIN_DATE = DateTime.Now;
            申請表.HOST_IP = UserIP;
            申請表.MAN_ID = UserID;
            申請表.HOST_NAME = UserPC;
            try
            {
                if (申請表編號 == null)
                {
                    db.Add(申請表);
                    db.SaveChanges();
                }
                else
                {
                    db.SaveChanges();
                }
                TempData["response"] = "修改成功";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("申請表", new { 申請表.申請表編號 });
        }
        [TypeFilter(typeof(角色功能驗證), Arguments = [new string[] { "HR_001_教育訓練", "刪除" }])]
        public IActionResult 申請表Delete(string 申請表編號)
        {
            var 申請表 = db.申請表.Where(m => m.申請表編號 == 申請表編號).First();
            try
            {
                db.Remove(申請表);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("申請表清單");
        }
        #endregion
        #region 職務說明
        public IActionResult 職務說明()
        {
            var model = new 職務說明Model
            {
                Date = DateTime.Now,
                職位類別 = "管理職位"
            }; // 實例化職務說明Model
            return View(model);
        }
        #endregion
        #region 檔案列表
        public IActionResult FileList()
        {
            var model = new HR_001_ModelsManager
            {
                教育訓練檔案 = [.. db.教育訓練檔案]
            }; // 實例化職務說明Model
            return View(model);
        }
        #endregion
        #region DownloadExcelFile
        private FileContentResult DownloadExcelFile(HR_001_ModelsManager SkillTest, string 輸出表格, int 縮放比例)
        {
            var 部門名稱 = SkillTest.部門名稱;
            var 員工編號 = SkillTest.員工表.員工編號;
            var 文件列表 = 取得文件列表(員工編號);
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = "";
            var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add(部門名稱);//新增分頁 分頁名稱
            worksheet.SheetView.View = XLSheetViewOptions.PageBreakPreview;//分頁預覽
            worksheet.PageSetup.Scale = 縮放比例;// 設置縮放比例
            if (輸出表格 == "技能檢定表")
            {
                fileName = "技能檢定表-" + 員工編號 + ".xlsx";
                Multi技能檢定表excel(員工編號, 文件列表, worksheet, 0, 0);
            }
            else if (輸出表格 == "技能檢定卡")
            {
                fileName = "技能檢定卡-" + 員工編號 + ".xlsx";
                Multi技能檢定卡excel(員工編號, 文件列表, worksheet, 0, 0);
            }


            var content = SaveExcelFile(workbook);
            _documentController.CopyFileToServer(content, fileName);
            return File(content, contentType, fileName);
        }
        [HttpPost]
        public FileResult Download技能檢定表(HR_001_ModelsManager SkillTest)
        {

            return DownloadExcelFile(SkillTest, "技能檢定表", 65);
        }
        [HttpPost]
        public FileResult Download技能檢定卡(HR_001_ModelsManager SkillTest)
        {
            return DownloadExcelFile(SkillTest, "技能檢定卡", 77);
        }
        #endregion
        #region 技能檢定表、技能檢定卡 多選下載 畫面
        public IActionResult MultiSkillTestTable()
        {
            var 員工表 = db.員工表.ToList();
            var mm = new HR_001_ModelsManager()
            {
                員工表List = 員工表
            };
            return View(mm);
        }
        #endregion 
        #region 技能檢定表、技能檢定卡 多選下載 畫面
        [HttpPost]
        public IActionResult Multiple(string Multiple, int[] 部門, string[] 員工編號)
        {
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = Multiple + ".xlsx";
            var workbook = new XLWorkbook();
            var 部門List = new List<int>(); // 初始化部門List
            if (員工編號.IsNullOrEmpty())
            {
                TempData["Error"] = "請選擇至少一名員工";
                return RedirectToAction("MultiSkillTestTable");
            }
            if (部門.IsNullOrEmpty())
            {
                var 員工部門 = db.員工表.Where(m => 員工編號.Contains(m.員工編號)).Select(m => m.部門編號).Distinct().ToList();
                foreach (var i in 員工部門)
                {

                    部門List.Add((int)i);
                }

            }
            else
            {
                部門List.AddRange(部門);
            }
            foreach (var item in 部門List)
            {
                var 員工 = db.員工表.Where(m => m.部門編號 == item && 員工編號.Any(e => e == m.員工編號)).OrderBy(m => m.部門編號).ThenBy(m => m.職稱).ToList();
                var 部門名稱 = db.部門表.Where(m => m.部門編號 == item).Select(m => m.部門名稱).FirstOrDefault().ToString();

                IXLWorksheet worksheet = workbook.Worksheets.Add(部門名稱); // 新增分頁 分頁名稱
                worksheet.SheetView.View = XLSheetViewOptions.PageBreakPreview; // 分頁預覽

                int Count = 0;
                foreach (var 員工item in 員工)
                {
                    var 文件列表 = 取得文件列表(員工item.員工編號);
                    if (Multiple == "技能檢定卡")
                    {
                        worksheet.PageSetup.Scale = 77; // 設置縮放比例為 77%
                        Multi技能檢定卡excel(員工item.員工編號, 文件列表, worksheet, 0, Count);
                        Count += 文件列表.Count % 24 > 0 ? 文件列表.Count / 24 + 1 : 文件列表.Count / 24;//計算excel需要往下幾格，如果課程為14倍數 + 課程數/14，如課程不為14倍數 +課程數/14 + 1

                    }
                    else if ((Multiple == "技能檢定表"))
                    {
                        worksheet.PageSetup.Scale = 65; // 設置縮放比例為 65%
                        Multi技能檢定表excel(員工item.員工編號, 文件列表, worksheet, 0, Count);
                        Count += 文件列表.Count % 14 > 0 ? 文件列表.Count / 14 + 1 : 文件列表.Count / 14;//計算excel需要往下幾格，如果課程為14倍數 + 課程數/14，如課程不為14倍數 +課程數/14 + 1
                    }

                }

            }

            var content = SaveExcelFile(workbook);
            _documentController.CopyFileToServer(content, fileName);
            return File(content, contentType, fileName);
        }
        #endregion
        #region 技能檢定表 多選下載
        public void Multi技能檢定表excel(string 員工編號, List<string> 文件列表, IXLWorksheet worksheet, int CourseCount, int Count)
        {
            var RowTimes = Count * 24;

            var 員工 = db.員工表.Where(m => m.員工編號 == 員工編號).FirstOrDefault();
            var 職務名稱 = db.員工職務表.Where(m => m.員工編號 == 員工編號).ToList();
            var 職務 = 取得職務(員工編號);
            var 部門名稱 = db.部門表.Where(m => m.部門編號 == 員工.部門編號).Select(m => m.部門名稱).First();

            //設定標題列名稱與樣式
            worksheet.Range(1 + RowTimes, 1, 1 + RowTimes, 6).Merge();//欄合併
            worksheet.Range(2 + RowTimes, 1, 2 + RowTimes, 6).Merge();//欄合併
            worksheet.Cell(1 + RowTimes, 1).Value = "南岩半導體股份有限公司";//內容
            worksheet.Cell(2 + RowTimes, 1).Value = "技能檢定表";//內容

            worksheet.Range(1 + RowTimes, 1, 24 + RowTimes, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;//文字置中
            worksheet.Range(1 + RowTimes, 1, 24 + RowTimes, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;//垂直置中

            worksheet.Range(1 + RowTimes, 1, 2 + RowTimes, 1).Style.Font.SetFontSize(20);//字體大小
            worksheet.Range(5 + RowTimes, 1, 24 + RowTimes, 6).Style.Font.SetFontSize(14);//字體大小
            worksheet.Range(1 + RowTimes, 1, 24 + RowTimes, 6).Style.Font.FontName = "標楷體";

            //行列範圍
            worksheet.Range(5 + RowTimes, 1, 24 + RowTimes, 6).Style.Border.InsideBorder = XLBorderStyleValues.Thin; //內框
            worksheet.Range(5 + RowTimes, 1, 24 + RowTimes, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin; //外框 細線

            //行列屬性
            worksheet.Rows(1 + RowTimes, 24 + RowTimes).Height = 60;//列高
            worksheet.Rows(1 + RowTimes, 2 + RowTimes).Height = 27.75;//列高
            worksheet.Rows(4 + RowTimes, 6 + RowTimes).Height = 33;//列高
            worksheet.Rows(3 + RowTimes, 3 + RowTimes).Height = 33;//列高

            worksheet.Columns(1, 6).Width = 28.71;//行寬
            worksheet.Columns(3, 4).Width = 16;//行寬
            worksheet.Columns(5, 5).Width = 20.57;//行寬
            worksheet.Columns(6, 6).Width = 11.57;//行寬

            worksheet.Range(5 + RowTimes, 2, 5 + RowTimes, 3).Merge();
            worksheet.Range(6 + RowTimes, 2, 6 + RowTimes, 3).Merge();
            worksheet.Range(5 + RowTimes, 5, 5 + RowTimes, 6).Merge();
            worksheet.Range(6 + RowTimes, 5, 6 + RowTimes, 6).Merge();

            worksheet.Range(22 + RowTimes, 1, 22 + RowTimes, 6).Merge();
            worksheet.Range(23 + RowTimes, 1, 23 + RowTimes, 6).Merge();
            worksheet.Range(24 + RowTimes, 1, 24 + RowTimes, 6).Merge();
            worksheet.Range(4 + RowTimes, 5, 4 + RowTimes, 6).Merge();
            worksheet.Cell(4 + RowTimes, 5).Value = "檢定日期：";//內容
            worksheet.Cell(4 + RowTimes, 1).Value = "部門：" + 部門名稱;//內容
            worksheet.Cell(5 + RowTimes, 1).Value = "學員姓名";//內容
            worksheet.Cell(5 + RowTimes, 2).Value = 員工.姓名;//內容
            worksheet.Cell(5 + RowTimes, 4).Value = "員工編號";//內容
            worksheet.Cell(5 + RowTimes, 5).Value = 員工編號;//內容
            worksheet.Cell(6 + RowTimes, 1).Value = "主要工作";//內容
            worksheet.Cell(6 + RowTimes, 2).Value = 職務;//內容
            worksheet.Cell(6 + RowTimes, 2).Style.Alignment.SetWrapText();
            worksheet.Cell(6 + RowTimes, 4).Value = "工作職稱";//內容
            worksheet.Cell(6 + RowTimes, 5).Value = 員工.職稱;//內容
            worksheet.Cell(7 + RowTimes, 1).Value = "技能檢定項目";//內容
            worksheet.Cell(7 + RowTimes, 3).Value = "檢定方式\r\n";//內容
            var C73 = worksheet.Cell(7 + RowTimes, 3).CreateRichText();
            C73.AddText("筆試 口試 實作").SetFontSize(10);
            worksheet.Cell(7 + RowTimes, 4).Value = "檢定結果\r\n";//內容
            var C74 = worksheet.Cell(7 + RowTimes, 4).CreateRichText();
            C74.AddText("Ａ  Ｂ  Ｃ").SetFontSize(10);
            worksheet.Cell(7 + RowTimes, 5).Value = "檢定者簽名";//內容
            worksheet.Cell(7 + RowTimes, 6).Value = "備註";//內容
            worksheet.Cell(22 + RowTimes, 1).Value = "檢定說明：\r\n一、請檢定者在定結果□內打Ｖ，並簽名及加註日期。\r\n" +
                "　　＜Ａ：符合全部要求95分以上　Ｂ：符合要求94分至75分　Ｃ：須再行訓練或實習74分以下＞\r\n" +
                "二、全部檢定項目結果須達到Ｂ以上，檢定才通過。\r\n" +
                "三、若檢定結果為Ｃ，請檢定者擇日再行檢定，將再檢定結果填入該項備註欄，並簽名及加註日期";//內容
            worksheet.Cell(23 + RowTimes, 1).Value = "　　管理部：　　　　　　　部門主管：　　　　　　　單位主管：";//內容
            worksheet.Cell(24 + RowTimes, 1).Value = "QP-03-001-10-D";//內容
            worksheet.Range(24 + RowTimes, 1, 24 + RowTimes, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Range(4 + RowTimes, 1, 4 + RowTimes, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Range(22 + RowTimes, 1, 23 + RowTimes, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Rows(22 + RowTimes, 22 + RowTimes).Height = 115.50;
            worksheet.Rows(23 + RowTimes, 23 + RowTimes).Height = 30;
            worksheet.Rows(24 + RowTimes, 24 + RowTimes).Height = 30;
            worksheet.PageSetup.PrintAreas.Add(1 + RowTimes, 1, 24 + RowTimes, 6);//設置列印範圍

            worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper; // 設置紙張大小 A4

            worksheet.Rows(8 + RowTimes, 21 + RowTimes).Height = 45;//列高

            for (int i = 7 + RowTimes; i <= 21 + RowTimes; i++)
            {
                worksheet.Range(i, 1, i, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Range(i, 1, i, 2).Merge();//欄合併
            }
            worksheet.Range(1 + RowTimes, 7, 2 + RowTimes, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 0; i < 文件列表.Count && i < 14; i++)
            {
                var 文件名稱 = db.文件表.Where(m => m.文件編號 == 文件列表[i]).Select(m => m.文件名稱).FirstOrDefault().ToString();
                worksheet.Cell(8 + i + RowTimes, 1).Value = (i + 1 + (CourseCount)) + "、" + 文件列表[i] + 文件名稱;//內容
                var test = "";
                var testt = db.文件表.Where(m => 文件列表[i].ToString().Contains(m.文件編號)).FirstOrDefault();
                test += testt.筆試 == 1 ? "⬛" : "⬜";
                test += testt.口試 == 1 ? "⬛" : "⬜";
                test += testt.實作 == 1 ? "⬛" : "⬜";
                worksheet.Cell(8 + i + RowTimes, 3).Value = test;//內容
                worksheet.Cell(8 + i + RowTimes, 4).Value = "⬜⬜⬜";//內容
                worksheet.Cell(8 + i + RowTimes, 1).Style.Font.SetFontSize(12);
                worksheet.Cell(8 + i + RowTimes, 1).Style.Alignment.SetWrapText();//文字自動換行
            }
            if (文件列表.Count > 14)
            {
                文件列表 = 文件列表.Skip(14).ToList();

                Multi技能檢定表excel(員工編號, 文件列表, worksheet, CourseCount + 14, Count + 1);
            }
            worksheet.PageSetup.AddHorizontalPageBreak(RowTimes);//列印範圍分割

        }
        #endregion
        #region 技能檢定卡 多選下載
        public void Multi技能檢定卡excel(string 員工編號, List<string> 文件列表, IXLWorksheet worksheet, int CourseCount, int Count)
        {
            var 職務 = 取得職務(員工編號);

            var RowTimes = Count * 12;
            //行列屬性
            worksheet.Rows(1 + RowTimes, 1 + RowTimes).Height = 6;//列高
            worksheet.Rows(2 + RowTimes, 5 + RowTimes).Height = 25.5;//列高
            worksheet.Rows(6 + RowTimes, 11 + RowTimes).Height = 16.5;//列高
            worksheet.Rows(12 + RowTimes, 12 + RowTimes).Height = 6;//列高

            worksheet.Columns().Width = 9;//行寬
            worksheet.Columns(1, 1).Width = 1.25;//行寬
            worksheet.Columns(2, 3).Width = 2.75;//行寬
            worksheet.Columns(9, 9).Width = 2;//行寬
            worksheet.Columns(10, 15).Width = 7.75;//行寬
            worksheet.Columns(16, 16).Width = 2;//行寬
            //設定標題列名稱與樣式
            worksheet.Range(2 + RowTimes, 2, 2 + RowTimes, 8).Merge();//欄合併
            worksheet.Range(3 + RowTimes, 2, 3 + RowTimes, 8).Merge();//欄合併
            worksheet.Cell(2 + RowTimes, 2).Value = "南岩半導體股份有限公司";//內容
            worksheet.Cell(2 + RowTimes, 2).Style.Font.SetBold();
            worksheet.Cell(3 + RowTimes, 2).Value = "Nanotech Certification Card-Test";//內容
            worksheet.Cell(3 + RowTimes, 2).Style.Font.FontColor = XLColor.Blue;
            worksheet.Range(2 + RowTimes, 2, 12 + RowTimes, 16).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;//文字置中
            worksheet.Range(2 + RowTimes, 2, 12 + RowTimes, 16).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;//垂直置中

            worksheet.Range(2 + RowTimes, 2, 3 + RowTimes, 16).Style.Font.SetFontSize(14);//字體大小
            worksheet.Range(4 + RowTimes, 2, 11 + RowTimes, 16).Style.Font.SetFontSize(12);//字體大小
            worksheet.Range(3 + RowTimes, 10, 3 + RowTimes, 15).Style.Font.SetFontSize(12);//字體大小
            worksheet.Range(5 + RowTimes, 9, 11 + RowTimes, 16).Style.Font.SetFontSize(9);//字體大小
            worksheet.Cell(6 + RowTimes, 2).Style.Font.SetFontSize(10);//字體大小
            worksheet.Range(2 + RowTimes, 2, 12 + RowTimes, 16).Style.Font.FontName = "標楷體";
            worksheet.Range(3 + RowTimes, 2, 3 + RowTimes, 8).Style.Font.FontName = "Stencil";
            //行列範圍
            worksheet.Range(1 + RowTimes, 2, 12 + RowTimes, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin; //外框 細線
            worksheet.Range(1 + RowTimes, 9, 12 + RowTimes, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin; //外框 細線

            worksheet.Range(2 + RowTimes, 2, 11 + RowTimes, 8).Style.Border.InsideBorder = XLBorderStyleValues.Dotted; //內框 虛線
            worksheet.Range(2 + RowTimes, 10, 4 + RowTimes, 15).Style.Border.InsideBorder = XLBorderStyleValues.Dotted; //內框 虛線
            worksheet.Range(2 + RowTimes, 10, 4 + RowTimes, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Dotted; //內框 虛線

            worksheet.Range(4 + RowTimes, 2, 4 + RowTimes, 3).Merge();
            worksheet.Range(4 + RowTimes, 4, 4 + RowTimes, 5).Merge();
            worksheet.Range(4 + RowTimes, 7, 4 + RowTimes, 8).Merge();
            worksheet.Range(5 + RowTimes, 2, 5 + RowTimes, 3).Merge();
            worksheet.Range(5 + RowTimes, 4, 5 + RowTimes, 8).Merge();
            worksheet.Range(6 + RowTimes, 2, 11 + RowTimes, 3).Merge();
            worksheet.Range(6 + RowTimes, 4, 11 + RowTimes, 8).Merge();

            worksheet.Range(2 + RowTimes, 10, 2 + RowTimes, 15).Merge();//年度
            worksheet.Range(5 + RowTimes, 9, 11 + RowTimes, 16).Merge();//年度

            var 員工表 = db.員工表.Where(m => m.員工編號 == 員工編號).FirstOrDefault();
            var 員工職務表 = db.員工職務表.Where(m => m.員工編號 == 員工編號).ToList();

            worksheet.Cell(4 + RowTimes, 2).Value = "姓名\r\n";//內容
            var C42 = worksheet.Cell(4 + RowTimes, 2).CreateRichText();
            C42.AddText("Name").SetFontSize(8).SetFontName("Times New Roman");
            worksheet.Cell(4 + RowTimes, 4).Value = 員工表.姓名;//內容
            worksheet.Cell(4 + RowTimes, 6).Value = "工號\r\n";//內容
            var C46 = worksheet.Cell(4 + RowTimes, 6).CreateRichText();
            C46.AddText("Serial number").SetFontSize(8).SetFontName("Times New Roman");
            worksheet.Cell(4 + RowTimes, 7).Value = 員工表.員工編號;//內容
            worksheet.Cell(5 + RowTimes, 2).Value = "站別\r\n";//內容
            var C52 = worksheet.Cell(5 + RowTimes, 2).CreateRichText();
            C52.AddText("Process").SetFontSize(8).SetFontName("Times New Roman");
            worksheet.Cell(6 + RowTimes, 2).Value = 職務;//內容
            worksheet.Cell(6 + RowTimes, 2).Style.Alignment.SetWrapText();//文字自動換行
            worksheet.Cell(5 + RowTimes, 4).Value = "檢定項目\r\n";//內容
            var C54 = worksheet.Cell(5 + RowTimes, 4).CreateRichText();
            C54.AddText("Teat Item").SetFontSize(8).SetFontName("Times New Roman");
            var C64 = worksheet.Cell(6 + RowTimes, 4).CreateRichText();
            worksheet.Cell(6 + RowTimes, 4).Style.Alignment.SetWrapText();//文字自動換行
            for (int i = 0; i < 文件列表.Count && i < 22; i++)
            {
                var 文件名稱 = db.文件表.Where(m => m.文件編號 == 文件列表[i]).Select(m => m.文件名稱).First().ToString();

                if (文件列表.Count > 6 && 文件列表.Count <= 9)
                {
                    C64.AddText((i + 1 + CourseCount) + "、" + 文件列表[i] + " " + 文件名稱 + " ").SetFontSize(8);
                }
                else if (文件列表.Count > 9)
                {
                    C64.AddText((i + 1 + CourseCount) + "、" + 文件列表[i] + " " + 文件名稱 + " ").SetFontSize(6);
                }
                else
                {
                    C64.AddText((i + 1 + CourseCount) + "、" + 文件列表[i] + " " + 文件名稱 + " ").SetFontSize(12);
                }
                if ((文件列表.Count < 12 || 文件列表.Count >= 12 && i % 2 == 1) && i != 文件列表.Count - 1)
                {
                    C64.AddText("\r\n").SetFontSize(6);
                }
            }
            if (文件列表.Count > 22)
            {
                文件列表 = 文件列表.Skip(22).ToList();
                Multi技能檢定卡excel(員工編號, 文件列表, worksheet, CourseCount + 22, Count + 1);
            }

            worksheet.Cell(2 + RowTimes, 10).Value = "年      度";//內容
            for (int i = 0; i < 6; i++)
            {
                var Year = DateTime.Now.Year + i;
                worksheet.Cell(3 + RowTimes, 10 + i).Value = Year + "年";//內容
            }

            worksheet.Cell(5 + RowTimes, 9).Value = "　　備註：\r\n　　" +
                "一、本檢定卡僅供紀錄人員所需檢定技能之項目及檢定結果資\r\n　　　　" +
                "料不供其它用途使用。\r\n　　" +
                "二、當年度技能檢定不合格者不得從事該站職務。\r\n　　" +
                "三、檢定卡不得轉借供他人查核使用。\r\n　　" +
                "四、人員技能檢定於每年十二月份由單位主管執行檢定並將結\r\n        " +
                "果送至管理部查核後蓋章核發。\r\n　　" +
                "五、本卡遺失請儘速至管理部登記補發。\r\n" +
                "                                          QP-03-001-11-C";//內容
            worksheet.Cell(5 + RowTimes, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;//文字靠左
            worksheet.Cell(6 + RowTimes, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;//文字置中
            worksheet.PageSetup.PrintAreas.Add(1 + RowTimes, 2, 12 + RowTimes, 16);//設置列印範圍
            if (Count % 4 == 0 && Count != 0)
            {
                worksheet.PageSetup.AddHorizontalPageBreak(RowTimes);//列印範圍分割
            }
        }
        #endregion
        #region function
        public byte[] SaveExcelFile(XLWorkbook workbook)
        {
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return content;
        }
        public List<string> 取得文件列表(string 員工編號)
        {
            var 文件料表List = new List<string>();
            var 職務 = db.員工職務表.Where(m => m.員工編號 == 員工編號).ToList();
            if (職務.IsNullOrEmpty())
            {
                var 文件 = db.職務文件關聯表.Where(m => 員工編號 == m.職務).ToList();
                foreach (var 文件item in 文件)
                {
                    if (!文件料表List.Contains(文件item.文件編號))
                    {
                        文件料表List.Add(文件item.文件編號);
                    }
                }
            }
            else
            {
                foreach (var 職務item in 職務)
                {
                    var 文件 = db.職務文件關聯表.Where(m => 職務item.職務 == m.職務).ToList();
                    foreach (var 文件item in 文件)
                    {
                        if (!文件料表List.Contains(文件item.文件編號))
                        {
                            文件料表List.Add(文件item.文件編號);
                        }
                    }

                }

            }
            文件料表List.Sort();
            return 文件料表List;
        }
        public string 取得職務(string 員工編號)
        {
            var 職務名稱 = db.員工職務表.Where(m => m.員工編號 == 員工編號).ToList();
            var 職務 = "";
            foreach (var item in 職務名稱)
            {
                職務 += item.職務 + "/";
            }
            職務 = 職務[..^1];
            return 職務;
        }
        public ActionResult 取得職務名稱(int 部門編號)
        {
            var positions = db.職務表
                .Where(m => m.部門編號 == 部門編號)
                .Select(m => new { Value = m.職務, Text = m.職務 })
                .ToList();
            return Json(positions);
        }
        public ActionResult 取得人員(string 部門名稱)
        {
            var 部門編號 = db.部門表.Where(m => m.部門名稱 == 部門名稱).Select(m => m.部門編號).FirstOrDefault();
            var positions = db.員工表
                .Where(m => m.部門編號 == 部門編號)
                .Select(m => new { Value = m.員工編號, Text = m.員工編號 + m.姓名 })
                .ToList();
            return Json(positions);
        }
        //腳色驗證功能
        public void SetRolePermissions(string 使用程式)
        {
            var SYS_RP_角色驗證 = _db.SYS_RP_角色驗證(UserID, "MVC", 使用程式 ).FirstOrDefault();
            ViewBag.新增 = SYS_RP_角色驗證.新增;
            ViewBag.修改 = SYS_RP_角色驗證.修改;
            ViewBag.刪除 = SYS_RP_角色驗證.刪除;
        }
        #endregion
        #region 取得計畫表編號
        public string Get計畫表編號()
        {
            lock (thisLock)
            {
                //調查編號為 西元年 月 + 四碼
                var a = DateTime.Now.ToString("yyyyMM") + "000";
                var i = Int32.Parse(a);
                var mm = db.教育訓練計畫表.OrderByDescending(m => m.計畫表流水號).FirstOrDefault();
                if (mm != null)
                {
                    while (i <= Int32.Parse(mm!.計畫表編號))
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
        #region 取得申請表編號
        public string Get申請表編號()
        {
            lock (thisLock)
            {
                //調查編號為 西元年 月 + 四碼
                var a = DateTime.Now.ToString("yyyyMM") + "000";
                var i = Int32.Parse(a);
                var mm = db.申請表.OrderByDescending(m => m.申請表流水號).FirstOrDefault();
                if (mm != null)
                {
                    while (i <= Int32.Parse(mm!.申請表編號))
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

    }
}