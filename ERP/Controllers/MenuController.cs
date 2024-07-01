using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using 南岩ERP.MisEFModels;
using 南岩ERP.Models;

namespace 南岩ERP.Controllers
{

    public class MenuController(nanoerpEntities db, IHttpContextAccessor httpContextAccessor) : Controller
    {
        private readonly string UserID = httpContextAccessor.HttpContext.Session.GetString("UserID");
        private readonly string UserIP = httpContextAccessor.HttpContext.Session.GetString("UserIP"); 
        private readonly string UserPC = httpContextAccessor.HttpContext.Session.GetString("電腦名稱");
        private readonly nanoerpEntities db = db;
        // GET: Menu
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Menu(string 程式分類)
        {
            //給予View目前使用者資訊
            ViewBag.ID = UserID;
            ViewBag.電腦名稱 = UserPC;
            ViewBag.IP = UserIP;
            ViewBag.程式分類 = 程式分類;
            // 從 Session 中讀取值
            var array = HttpContext.Session.Get("系統");
            // 將讀取的值轉換為原始的 List<string> 類型
            var decodedMenu = JsonConvert.DeserializeObject<List<string>>(Encoding.UTF8.GetString(array));
            ViewBag.Menu = decodedMenu;
            return View();
        }
        public ActionResult 選單頁面(string 程式分類)
        {
            //透過MenuList Function取得目前使用者可瀏覽之系統
            var model = MenuList(UserID, 程式分類);
            
            return View(model);
        }

        public ViewModel MenuList(string account, string SYS_Name)
        {
            //透過sql函式取得目前使用者 可瀏覽選單
            var menu = db.SYS_MENU_選單驗證(account, "MVC").ToList();
            //初始化model
            var model = new ViewModel
            {
                MenuItemsByCategory = []
            };
            //將資料存入model
            model.MenuItemsByCategory[SYS_Name] = [];
            foreach (var line in menu)
            {
                if (line.程式分類 == SYS_Name)
                {
                    if (!model.MenuItemsByCategory[SYS_Name].Contains(line.PRO_NAME))
                    {
                        model.MenuItemsByCategory[SYS_Name].Add(line.PRO_NAME);
                    }
                }
            }
            return model;
        }
        public ActionResult 物質調查記錄()
        {
            //網頁導向該系統
            return RedirectToAction("List", "QA_018_物質調查記錄");
        }
        public ActionResult 教育訓練()
        {
            //網頁導向該系統
            return RedirectToAction("Index", "教育訓練");
        }
        public ActionResult Mis()
        {
            //網頁導向該系統
            return RedirectToAction("Index", "Mis");
        }
        public ActionResult WIP()
        {
            //網頁導向該系統
            return RedirectToAction("Index", "WIP");
        }
        public ActionResult Test()
        {
            //網頁導向該系統
            return RedirectToAction("Index", "TableTest");
        }
    }
}
