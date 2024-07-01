using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using 南岩ERP.Models;
using 南岩ERP.教育訓練EFModels;
using ClosedXML.Excel;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using Break = DocumentFormat.OpenXml.Wordprocessing.Break;
using Microsoft.AspNetCore.Hosting;
using DocumentFormat.OpenXml.Spreadsheet;


namespace 南岩ERP.Controllers
{
    public class DocumentController(nanodevContext db, ILogger<DocumentController> logger, IWebHostEnvironment webHostEnvironment) : Controller
    {

        private static readonly object thisLock = new();
        private readonly nanodevContext db = db;

        public IActionResult Index()
        {
            return View();
        }

        #region 建立資料夾
        public string CreateFloder(string 資料夾名稱)
        {
            //在儲存檔案時，如果沒有該單號的資料夾就建立一個

            string rootPath = "\\wwwroot\\uploads\\";

            string folderPath = Path.Combine(rootPath, 資料夾名稱);
            DirectoryInfo dirInfo = new(folderPath);

            if (dirInfo.Exists == false)
            {
                Directory.CreateDirectory(folderPath);
            }
            return folderPath;
        }
        #endregion
        [HttpPost]
        public IActionResult CopyFileToServer(byte[] docxBytes, string FileName)
        {
            var absolutePath = Path.Combine(webHostEnvironment.WebRootPath, "/uploads/");
            var destinationPath = absolutePath + FileName;
            var Document = db.教育訓練檔案.Where(m => m.檔案名稱 == FileName).FirstOrDefault();
            if (Document == null)
            {
                Document = new 教育訓練檔案()
                {
                    檔案名稱 = FileName,
                };
                db.Add(Document);
                try
                {
                    db.SaveChanges();
                    logger.LogInformation("CopyFileToServer:db.Savechange: {Id}  {IP} 更新 教育訓練檔案", HttpContext.Session.GetString("UserID"), HttpContext.Session.GetString("UserIP"));
                }
                catch (Exception ex)
                {
                }
                
            }
            try
            {
                // 將字串單元加入文件
                System.IO.File.WriteAllBytes(destinationPath, docxBytes);

                // 返回操作结果
                return Ok("文件复制成功");
            }
            catch (Exception ex)
            {
                return BadRequest("文件复制失败：" + ex.Message);
            }
        }

        [HttpPost]
        public IActionResult FileUpload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                try
                {
                    var FileName = Path.GetFileName(file.FileName);
                    var uploadsFolder = CreateFloder("職務說明");
                    var filePath = Path.Combine(uploadsFolder, FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    if (db.教育訓練檔案.Where(m => m.檔案名稱 == FileName).FirstOrDefault() == null)
                    {
                        var Files = new 教育訓練檔案
                        {
                            檔案名稱 = FileName,
                        };
                        db.Add(Files);
                        db.SaveChanges();
                    }
                    logger.LogInformation("CopyFileToServer:db.Savechange: {Id}  {IP} 更新 教育訓練檔案", HttpContext.Session.GetString("UserID"), HttpContext.Session.GetString("UserIP"));
                    return RedirectToAction("職務說明");
                }
                catch (Exception ex)
                {
                    // 處理錯誤
                    return Content(ex.Message + "Error");
                }
            }
            return RedirectToAction("職務說明");
        }
        public ActionResult Downloads(string FileName)
        {
            //供使用者下載以上傳之檔案
            var absolutePath = Path.Combine(webHostEnvironment.WebRootPath, "/uploads/");
            var path = Path.Combine(absolutePath, FileName);
            byte[] fileBytes = System.IO.File.ReadAllBytes(path); //讀取檔案資料
            var contentType = "application/force-download";
            return File(fileBytes, contentType, FileName);
        }

        [HttpPost]
        public IActionResult Download職務說明(職務說明Model 職務說明Model)
        {
            if(職務說明Model.部門==null|| 職務說明Model.職務==null)
            {
                TempData["Error"] = "請輸入部門/職務";

            }
            var Filepath = "1111.docx";
            var absolutePath = Path.Combine(webHostEnvironment.WebRootPath, Filepath);
            var docxBytes = WordRender.職務說明word(absolutePath, 職務說明Model);
            var FileName = 職務說明Model.職務 + "職務說明.docx";
            CopyFileToServer(docxBytes, FileName);
            return File(docxBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", FileName);
        }
        public IActionResult Download申請表(string 申請表編號)
        {
            var 申請表 = db.申請表.Where(m => m.申請表編號 == 申請表編號).FirstOrDefault();
            var Filepath = "教育訓練申請表.docx";
            var absolutePath = Path.Combine(webHostEnvironment.WebRootPath, Filepath);
            var docxBytes = WordRender.申請表Word(absolutePath, 申請表!);
            var FileName = 申請表編號 + "申請表.docx";
            CopyFileToServer(docxBytes, FileName);
            return File(docxBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", FileName);
        }


        public FileResult Download計畫表(string 計畫表編號)
        {
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = DateTime.Now.Year + 1 + "年度教育訓練計畫書.xlsx";
            var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add("總表"); // 新增分頁 分頁名稱
            worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape; //頁面橫向
            worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper; // 設置紙張大小 A4
            worksheet.PageSetup.FitToPages(1, 1); //1頁長1頁寬
            計畫表excel(計畫表編號, worksheet);
            var content = SaveExcelFile(workbook);
            CopyFileToServer(content, fileName);
            return File(content, contentType, fileName);
        }
        public void 計畫表excel(string 計畫表編號, IXLWorksheet worksheet)
        {
            worksheet.PageSetup.Margins.SetTop(0.7).SetLeft(0.2).SetBottom(0).SetRight(0.2).SetHeader(0.3).SetFooter(0);
            worksheet.PageSetup.Header.Center.AddText("南岩半導體股份有限公司2024年度教育訓練計劃", XLHFOccurrence.AllPages).SetFontSize(26).SetFontName("標楷體");
            var 計畫書 = db.教育訓練計畫表內容.Where(m => m.計畫表編號 == 計畫表編號).ToList();
            //設定標題列名稱與樣式
            for (int i = 1; i <= 5; i++)
            {
                worksheet.Range(1, i, 7, i).Merge();//欄合併
            }
            worksheet.Range(1, 6, 2, 8).Merge();//欄合併
            worksheet.Range(1, 9, 2, 15).Merge();//欄合併
            worksheet.Range(1, 16, 2, 20).Merge();//欄合併
            worksheet.Range(1, 21, 2, 22).Merge();//欄合併
            for (int i = 6; i <= 22; i++)
            {
                worksheet.Range(3, i, 7, i).Merge();//欄合併
            }

            string[] a =[ "管理人員", "工管理師" , "作業人員"
                    , "業務部", "研發部" , "製造部", "品保部", "測試部", "財務部", "管理部"
                    , "預計上課\r\n日 期", "上課\r\n時數", "訓練方式\r\n內訓／外訓"
                    , "講師", "預計\r\n費用", "預計\r\n參訓人數", "預計總費用"];

            worksheet.Cell(1, 1).Value = "項 次";//內容
            worksheet.Cell(1, 2).Value = "提出單位";//內容
            worksheet.Cell(1, 3).Value = "類  別";//內容
            worksheet.Cell(1, 4).Value = "課程名稱";//內容
            worksheet.Cell(1, 5).Value = "課程大綱";//內容
            worksheet.Cell(1, 6).Value = "階層別";//內容
            worksheet.Cell(1, 9).Value = "訓練單位 / 對象";//內容
            worksheet.Cell(1, 16).Value = "訓練資訊";//內容
            worksheet.Cell(1, 21).Value = "訓練規劃";//內容
            for (int i = 1; i <= 5; i++)
            {
                worksheet.Cell(1, i).Style.Alignment.SetTextRotation(255);
            }
            for (int i = 5; i <= 15; i++)
            {
                worksheet.Cell(3, i).Style.Alignment.SetTextRotation(255);
            }
            for (int i = 0; i < a.Length; i++)
            {
                worksheet.Cell(3, i + 6).Value = a[i];//內容
            }
            worksheet.Range(1, 1, 7, 22).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD9D9D9);
            worksheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;//文字置中
            worksheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;//垂直置中

            worksheet.Style.Font.SetFontSize(10);//字體大小
            worksheet.Style.Font.FontName = "新細明體";



            //行列屬性
            worksheet.Rows().Style.Alignment.WrapText = true;
            worksheet.Rows(1, 7).Height = 14.25;//列高

            worksheet.Columns(1, 1).Width = 3.14;//行寬
            worksheet.Columns(2, 2).Width = 6.57;//行寬
            worksheet.Columns(3, 3).Width = 8.43;//行寬
            worksheet.Columns(4, 4).Width = 26.71;//行寬
            worksheet.Columns(5, 5).Width = 39.41;//行寬
            for (int i = 6; i <= 15; i++)
            {
                worksheet.Columns(i, i).Width = 5.71;//行寬
            }
            worksheet.Columns(16, 16).Width = 13.14;//行寬
            worksheet.Columns(17, 17).Width = 6;//行寬
            worksheet.Columns(18, 18).Width = 10.29;//行寬
            worksheet.Columns(19, 19).Width = 8.43;//行寬
            worksheet.Columns(20, 20).Width = 6;//行寬
            worksheet.Columns(21, 21).Width = 8.43;//行寬
            worksheet.Columns(22, 22).Width = 10.29;//行寬

            //worksheet.Range(24, 1, 24, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            //worksheet.Range(4, 1, 4, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            //worksheet.Range(22, 1, 23, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            var row = 1;

            foreach (var item in 計畫書)
            {
                worksheet.Cell(row + 7, 1).Value = row;//內容
                worksheet.Cell(row + 7, 2).Value = item.提出單位;//內容
                worksheet.Cell(row + 7, 3).Value = item.類別;//內容
                worksheet.Cell(row + 7, 4).Value = item.課程名稱;//內容
                worksheet.Cell(row + 7, 5).Value = item.課程大綱;//內容
                worksheet.Cell(row + 7, 6).Value = item.管理人員 == true ? "█" : "";//內容
                worksheet.Cell(row + 7, 7).Value = item.工管理師 == true ? "█" : "";//內容
                worksheet.Cell(row + 7, 8).Value = item.作業人員 == true ? "█" : "";//內容
                worksheet.Cell(row + 7, 9).Value = item.業務部 == true ? "█" : "";//內容
                worksheet.Cell(row + 7, 10).Value = item.研發部 == true ? "█" : "";//內容
                worksheet.Cell(row + 7, 11).Value = item.製造部 == true ? "█" : "";//內容
                worksheet.Cell(row + 7, 12).Value = item.品保部 == true ? "█" : "";//內容
                worksheet.Cell(row + 7, 13).Value = item.測試部 == true ? "█" : "";//內容
                worksheet.Cell(row + 7, 14).Value = item.財務部 == true ? "█" : "";//內容
                worksheet.Cell(row + 7, 15).Value = item.管理部 == true ? "█" : "";//內容
                worksheet.Cell(row + 7, 16).Value = item.預計上課日期;//內容
                worksheet.Cell(row + 7, 17).Value = item.上課時數;//內容
                worksheet.Cell(row + 7, 18).Value = item.訓練方式;//內容
                worksheet.Cell(row + 7, 19).Value = item.講師;//內容
                worksheet.Cell(row + 7, 20).Value = item.預計費用;//內容
                worksheet.Cell(row + 7, 21).Value = item.預計參訓人數;//內容
                worksheet.Cell(row + 7, 22).Value = item.預計總費用;//內容
                row++;
            }
            //行列範圍
            worksheet.Range(1, 1, 6 + row, 22).Style.Border.InsideBorder = XLBorderStyleValues.Thin; //內框
            worksheet.Range(1, 1, 6 + row, 22).Style.Border.OutsideBorder = XLBorderStyleValues.Medium; //外框 細線
        }
        public byte[] SaveExcelFile(XLWorkbook workbook)
        {
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return content;
        }
    }
    #region Word文件套版
    public static partial class WordRender
    {
        #region 申請表Word
        public static byte[] 申請表Word(string Filepath, 申請表 申請表)
        {

            var data = new Dictionary<string, string>()
            {
                ["申請人"] = 申請表.申請人,
                ["申請部門"] = 申請表.申請部門,
                ["申請日期"] = 申請表.申請日期.ToString("yyyy-MM-dd"),
                ["內訓"] = 申請表.訓練方式 == "內訓" ? "█" : "",
                ["外訓"] = 申請表.訓練方式 == "外訓" ? "█" : "",
                ["訓練費用"] = 申請表.訓練費用,
                ["課程名稱"] = 申請表.課程名稱,
                ["訓練目的"] = 申請表.訓練目的,
                ["訓練日期"] = 申請表.訓練日期.ToString("yyyy-MM-dd"),
                ["時間"] = 申請表.時間,
                ["時數"] = 申請表.時數?.ToString() ?? "0",
                ["主辦單位"] = 申請表.主辦單位,
                ["聯絡單位"] = 申請表.聯絡單位,
                ["上課地點"] = 申請表.上課地點,
                ["上課人員"] = 申請表.上課人員,
                ["講師"] = 申請表.講師,
                //["檢附資料"] = 申請表.講師,
                //["成效評估"] = 申請表.講師,
                ["訓練管理單位協助事項"] = 申請表.訓練管理單位協助事項,
                ["備註"] = 申請表.備註
            };
            var docxBytes = WordRender.申請表GenerateDocx(File.ReadAllBytes(Filepath), data);
            return docxBytes;
        }
        public static byte[] 申請表GenerateDocx(byte[] template, Dictionary<string, string> data)
        {

            using var ms = new MemoryStream();
            ms.Write(template, 0, template.Length);

            using (var docx = WordprocessingDocument.Open(ms, true))
            {
                docx.MainDocumentPart?.Document.Body?.申請表GenerateDocx(data);
                docx.Save();
            }
            return ms.ToArray();
        }
        static void 申請表GenerateDocx(this OpenXmlElement elem, Dictionary<string, string> data)
        {
            var pool = new List<Run>();
            var matchText = string.Empty;
            var YellowhiliteRuns = elem.Descendants<Run>() //找出鮮明提示(將要替換位置螢光筆上色)
                .Where(o => o.RunProperties?.Elements<Highlight>().Where(h => h.Val == HighlightColorValues.Yellow).Any() ?? false).ToList();
            foreach (var run in YellowhiliteRuns)
            {
                var t = run.InnerText;
                if (t.StartsWith('['))
                {
                    pool = [run];
                    matchText = t;
                }
                else
                {
                    matchText += t;
                    pool.Add(run);
                }
                if (t.EndsWith(']'))
                {
                    var m = MyRegex().Match(matchText);
                    if (!m.Success || !data.ContainsKey(m.Groups["n"].Value))
                    {
                        continue;
                    }
                    var firstRun = pool.First();
                    firstRun.RemoveAllChildren<Text>();
                    firstRun.RunProperties?.RemoveAllChildren<Highlight>();
                    var newText = data[m.Groups["n"].Value];
                    var firstLine = true;
                    foreach (var line in MyRegex1().Split(newText))
                    {
                        if (firstLine) firstLine = false;
                        else firstRun.Append(new Break());
                        firstRun.Append(new Text(line));
                    }
                    pool.Skip(1).ToList().ForEach(o => o.Remove());
                }

            }
        }
        #endregion
        #region 職務說明word
        public static byte[] 職務說明word(string Filepath, 職務說明Model 職務說明Model)
        {
            var studys = "";
            foreach (var item in 職務說明Model.學歷)
            {
                studys += (bool)item.IsChecked ? "  ■" + item.Text : "  □" + item.Text;
            }
            var personality = "";
            foreach (var item in 職務說明Model.人格特質)
            {
                personality += (bool)item.IsChecked ? "  ■" + item.Text : "  □" + item.Text;
            }
            var 語言能力 = "";
            foreach (var item in 職務說明Model.語言)
            {
                語言能力 += item.IsChecked ? "■" : "□";
                語言能力 += item.Text + "(";

                語言能力 += item.語言能力.聽 ? "■聽" : "□聽";
                語言能力 += item.語言能力.說 ? "■說" : "□說";
                語言能力 += item.語言能力.讀 ? "■讀" : "□讀";
                語言能力 += item.語言能力.寫 ? "■寫" : "□寫";

                語言能力 += ")\r\n";
            }
            var data = new Dictionary<string, string>()
            {
                ["Title"] = 職務說明Model.部門,
                ["職務"] = 職務說明Model.職務,
                ["職位類別"] = 職務說明Model.職位類別,
                ["學歷"] = studys,
                ["科系"] = 職務說明Model.科系,
                ["經歷"] = 職務說明Model.經歷,
                ["語言能力"] = 語言能力,
                ["專業證照"] = 職務說明Model.專業證照,
                ["人格特質"] = personality,
            };
            var docxBytes = WordRender.職務說明GenerateDocx(File.ReadAllBytes(Filepath), data, 職務說明Model);
            return docxBytes;
        }
        public static byte[] 職務說明GenerateDocx(byte[] template, Dictionary<string, string> data, 職務說明Model 職務說明Model)
        {

            using var ms = new MemoryStream();
            ms.Write(template, 0, template.Length);

            using (var docx = WordprocessingDocument.Open(ms, true))
            {
                docx.MainDocumentPart?.Document.Body?.職務說明ReplaceParserTag(data, 職務說明Model);
                docx.MainDocumentPart?.HeaderParts.HeaderReplaceParserTag(職務說明Model.Date);
                docx.Save();
            }
            return ms.ToArray();
        }
        #endregion

        static void HeaderReplaceParserTag(this IEnumerable<HeaderPart> headers, DateTime Date)
        {
            var date = new Dictionary<string, string>()
            {
                ["Date"] = Date.ToString("yyyy-MM-dd"),

            };
            foreach (var header in headers)
            {
                header.Header.HeaderReplaceParserTag(date);
            }
        }
        static void HeaderReplaceParserTag(this OpenXmlElement elem, Dictionary<string, string> data)
        {
            var pool = new List<Run>();
            var matchText = string.Empty;
            var hiliteRuns = elem.Descendants<Run>() //找出鮮明提示(將要替換位置螢光筆上色)
                .Where(o => o.RunProperties?.Elements<Highlight>().Where(h => h.Val == HighlightColorValues.Yellow).Any() ?? false).ToList();

            foreach (var run in hiliteRuns)
            {
                var t = run.InnerText;
                if (t.StartsWith('['))
                {
                    pool = [run];
                    matchText = t;
                }
                else
                {
                    matchText += t;
                    pool.Add(run);
                }
                if (t.EndsWith(']'))
                {
                    var m = MyRegex().Match(matchText);
                    if (m.Success && data.TryGetValue(m.Groups["n"].Value, out string? value))
                    {
                        var firstRun = pool.First();
                        firstRun.RemoveAllChildren<Text>();
                        firstRun.RunProperties.RemoveAllChildren<Highlight>();
                        var newText = value;
                        var firstLine = true;
                        foreach (var line in MyRegex1().Split(newText))
                        {
                            if (firstLine) firstLine = false;
                            else firstRun.Append(new Break());
                            firstRun.Append(new Text(line));
                        }
                        pool.Skip(1).ToList().ForEach(o => o.Remove());
                    }
                }

            }
        }
        static void 職務說明ReplaceParserTag(this OpenXmlElement elem, Dictionary<string, string> data, 職務說明Model 職務說明Model)
        {
            var pool = new List<Run>();
            var matchText = string.Empty;
            var YellowhiliteRuns = elem.Descendants<Run>() //找出鮮明提示 黃色(將要替換位置螢光筆上色)
                .Where(o => o.RunProperties?.Elements<Highlight>().Where(h => h.Val == HighlightColorValues.Yellow).Any() ?? false).ToList();
            var GreenhiliteRuns = elem.Descendants<Run>() //找出鮮明提示 綠色(將要替換位置螢光筆上色)
                .Where(o => o.RunProperties?.Elements<Highlight>().Where(h => h.Val == HighlightColorValues.Green).Any() ?? false).ToList();
            var BluehiliteRuns = elem.Descendants<Run>() //找出鮮明提示 藍色(將要替換位置螢光筆上色)
                .Where(o => o.RunProperties?.Elements<Highlight>().Where(h => h.Val == HighlightColorValues.Blue).Any() ?? false).ToList();
            var RedhiliteRuns = elem.Descendants<Run>() //找出鮮明提示 紅色(將要替換位置螢光筆上色)
                .Where(o => o.RunProperties?.Elements<Highlight>().Where(h => h.Val == HighlightColorValues.Red).Any() ?? false).ToList();

            foreach (var run in YellowhiliteRuns)
            {
                var t = run.InnerText;
                if (t.StartsWith('['))
                {
                    pool = [run];
                    matchText = t;
                }
                else
                {
                    matchText += t;
                    pool.Add(run);
                }
                if (t.EndsWith(']'))
                {
                    var m = MyRegex().Match(matchText);
                    if (m.Success && data.TryGetValue(m.Groups["n"].Value, out string? value))
                    {
                        var firstRun = pool.First();
                        firstRun.RemoveAllChildren<Text>();
                        firstRun.RunProperties.RemoveAllChildren<Highlight>();
                        var newText = value;
                        var firstLine = true;
                        foreach (var line in MyRegex1().Split(newText))
                        {
                            if (firstLine) firstLine = false;
                            else firstRun.Append(new Break());
                            firstRun.Append(new Text(line));
                        }
                        pool.Skip(1).ToList().ForEach(o => o.Remove());
                    }
                }

            }
            var Green = 0;
            foreach (var run in GreenhiliteRuns)
            {
                var t = run.InnerText;
                if (t.StartsWith('['))
                {
                    pool = [run];
                    matchText = t;
                }
                else
                {
                    matchText += t;
                    pool.Add(run);
                }
                if (t.EndsWith(']'))
                {
                    var m = MyRegex().Match(matchText);
                    if (m.Success && "[$教育訓練$]" == matchText)
                    {
                        var firstRun = pool.First();
                        firstRun.RemoveAllChildren<Text>();
                        firstRun.RunProperties.RemoveAllChildren<Highlight>();
                        var newText = "";
                        if (Green < 職務說明Model.教育訓練.Length)
                        {
                            newText = 職務說明Model.教育訓練[Green];
                            Green++;
                        }
                        else
                        {
                            newText = "";
                        }
                        var firstLine = true;
                        foreach (var line in MyRegex1().Split(newText))
                        {
                            if (firstLine) firstLine = false;
                            else firstRun.Append(new Break());
                            firstRun.Append(new Text(line));
                        }

                        pool.Skip(1).ToList().ForEach(o => o.Remove());
                    }
                }

            }
            var Blue = 0;
            foreach (var run in BluehiliteRuns)
            {
                var t = run.InnerText;
                if (t.StartsWith('['))
                {
                    pool = [run];
                    matchText = t;
                }
                else
                {
                    matchText += t;
                    pool.Add(run);
                }
                if (t.EndsWith(']'))
                {
                    var m = MyRegex().Match(matchText);
                    if (m.Success && "[$技術面$]" == matchText)
                    {
                        var firstRun = pool.First();
                        firstRun.RemoveAllChildren<Text>();
                        firstRun.RunProperties.RemoveAllChildren<Highlight>();
                        var newText = "";
                        if (Blue < 職務說明Model.技術面.Length)
                        {
                            newText = 職務說明Model.技術面[Blue];
                            Blue++;
                        }
                        else
                        {
                            newText = "";
                        }
                        var firstLine = true;
                        foreach (var line in MyRegex1().Split(newText))
                        {
                            if (firstLine) firstLine = false;
                            else firstRun.Append(new Break());
                            firstRun.Append(new Text(line));
                        }
                        pool.Skip(1).ToList().ForEach(o => o.Remove());
                    }
                }

            }
            var Red = 0;
            foreach (var run in RedhiliteRuns)
            {
                var t = run.InnerText;
                if (t.StartsWith('['))
                {
                    pool = [run];
                    matchText = t;
                }
                else
                {
                    matchText += t;
                    pool.Add(run);
                }
                if (t.EndsWith(']'))
                {
                    var m = MyRegex().Match(matchText);
                    if (m.Success && "[$專業知識$]" == matchText)
                    {
                        var firstRun = pool.First();
                        firstRun.RemoveAllChildren<Text>();
                        firstRun.RunProperties.RemoveAllChildren<Highlight>();
                        var newText = "";
                        if (Red < 職務說明Model.專業知識.Length)
                        {
                            newText = 職務說明Model.專業知識[Red];
                            Red++;
                        }
                        else
                        {
                            newText = "";
                        }
                        var firstLine = true;
                        foreach (var line in MyRegex1().Split(newText))
                        {
                            if (firstLine) firstLine = false;
                            else firstRun.Append(new Break());
                            firstRun.Append(new Text(line));
                        }
                        pool.Skip(1).ToList().ForEach(o => o.Remove());
                    }
                }

            }

        }

        [GeneratedRegex(@"\[\$(?<n>\w+)\$\]")]
        private static partial Regex MyRegex();
        [GeneratedRegex(@"\\n")]
        private static partial Regex MyRegex1();
    }
    #endregion
}
