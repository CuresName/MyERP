using Microsoft.AspNetCore.Mvc.Rendering;
using ERP.教育訓練EFModels;

namespace ERP.Models
{
    public class MultiSkillTestTableModel
    {
        public string 職務名稱 { get; set; }
        public string 部門名稱 { get; set; }
        public 員工表 員工表 { get; set; }
        public 職務文件關聯表 職務文件關聯表 { get; set; }
        public List<員工表> 人員 { get; set; }
        public List<文件表> ISO文件 { get; set; }
        public List<員工職務表> 職務 { get; set; }
        public List<職務文件關聯表> 檢定內容 { get; set; }
        public List<部門表> 部門 { get; set; } 

    }
}
