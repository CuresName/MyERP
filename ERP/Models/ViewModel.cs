#nullable disable
namespace ERP.Models
{
    public class ViewModel
    {
        public Dictionary<string, List<string>> MenuItemsByCategory { get; set; }
        public string 系統別 { get; set; }
        public string 程式分類 { get; set; }
        public int 顯示順序 { get; set; }
        public string PRO_ID { get; set; }
        public string PRO_NAME { get; set; }
        public List<string> Menu { get;set; }
    }
}
