#nullable disable

using Microsoft.AspNetCore.Mvc.Rendering;

namespace 南岩ERP.Models
{
    public class QA_018_ModelsManager
    {
        public IEnumerable<QA_018_物質調查記錄_明細> 廠商回覆 { get; set; }
        public IEnumerable<QA_018_物質調查記錄_主檔> 詢問內容 { get; set; }
        public QA_018_物質調查記錄_主檔 Ask { get; set; }
        public QA_018_物質調查記錄_明細 Replay { get; set; }
        public List<QA_018_物質調查記錄_附件> AskFile { get; set; }
        public List<MA_005_供應商資料查詢Result> Company { get; set; }
        public List<SelectListItem> 內文2 { get; set; } =
        [
            new SelectListItem { Value = "",Text="選擇內容範本"},
            new SelectListItem { Value = "請轉給貴司相關人員確認我司交易於貴司的產品材料是否符合前述法規，並於附件中填寫並簽字用印，煩請協助作業，謝謝。", Text = "範本1" },
        ];
        public List<SelectListItem> 內文2英文 { get; set; } =
        [
            new SelectListItem { Value = "",Text="選擇內容範本"},
            new SelectListItem { Value = "Please forward it to the relevant personnel of your company to confirm whether the product materials our company trades with your company comply with strict regulations, and fill in and sign in the attachment. Please assist in the operation, thank you.", Text = "範本1" },
        ];
        public List<SelectListItem> 內文3 { get; set; } =
        [
            new() { Value = "",Text="選擇內容範本"},
            new() { Value = "煩請於 +@回覆期限+ 前簽字用印回傳附件(必須單位負責人且經理級以上人員授權)，並於附件中填寫並簽字用印，煩請協助作業，謝謝。", Text = "範本1" },
        ];
        public List<SelectListItem> 內文3英文 { get; set; } =
        [
           new SelectListItem { Value = "",Text="選擇內容範本"},
           new SelectListItem { Value = "Please reply before +@回覆期限+ Thank you " ,Text="範本1"},
        ];
        public List<SelectListItem> 常用內文2 { get; set; } = [];

    }
}