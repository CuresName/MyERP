namespace 南岩ERP.Models
{
    public class QA_018_物質調查記錄_供應商未回覆清單
    {
        public List<MA_005_供應商資料查詢Result> 廠商 { get; set; }
        public List<QA_018_物質調查記錄_明細> 未回覆清單 { get;set; }
        public List<QA_018_物質調查記錄_主檔> 調查事項 { get;set; }
        public Dictionary<string, List<string>> 廠商未回覆清單 { get; set; }
    }
}
