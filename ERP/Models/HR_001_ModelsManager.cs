using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Mvc.Rendering;
using 南岩ERP.教育訓練EFModels;
namespace 南岩ERP.Models
{
    public class HR_001_ModelsManager
    {
        public List<文件表> 文件表 { get; set; }

        public 職務表 職務表 { get; set; }
       
        public List<SelectListItem> 部門 { get; set; } =
        [
            new() { Value = "210", Text = "管理部" },
            new() { Value = "230", Text = "財務部" },
            new() { Value = "500", Text = "製造部" },
            new() { Value = "530", Text = "測試部" },
            new() { Value = "700", Text = "研發部" },
            new() { Value = "800", Text = "業務部" },
        ];
        public string 職務名稱 { get; set; }
        public string 部門名稱 { get; set; }

        public 員工表 員工表 { get; set; }
        public List<員工表> 員工表List { get; set; }
        public 員工職務表 員工職務表 { get; set; }
        public List<string> 文件列表 { get; set; }
        public List<教育訓練檔案> 教育訓練檔案 { get;set; }
        public List<教育訓練計畫表內容> 教育訓練計畫表內容 { get;set; }
        public 計畫表Model 計畫表Model { get; set; }
    }
    public class CheckBoxListItem
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public bool IsChecked { get; set; }

    }
}
