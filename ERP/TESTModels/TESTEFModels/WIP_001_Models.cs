using Microsoft.AspNetCore.Mvc.Rendering;

namespace ERP.TESTModels.TESTEFModels
{
    public class WIP_001_Models
    {
        public string 流程卡號 { get; set; }
        public string 生產站別 { get; set; }
        public WIP_001_生產回報_產出資料 產出資料 { get; set; }
        public WIP_001_生產回報資料來源 WIP_001_生產回報資料來源 { get; set; }
        public WIP_001_產線現有班別 WIP_001_產線現有班別 { get; set; }
        public List<WIP_001_生產回報_不良品> WIP_001_生產回報_不良品 { get; set; }
        public List<WIP_001_生產回報_產出資料> WIP_001_生產回報_產出資料 { get; set; }
        public List<WIP_001_生產回報_Cassette_NoResult> WIP_001_生產回報_Cassette_NoResult {  get; set; }
        public List<WIP_001_流程卡資料_各站異常代號Result> 不良品_異常代號 { get; set; }
        public List<SelectListItem> WIP_001_生產回報_流程卡號_SelectListItem { get; set; }
        public List<SelectListItem> 各站異常代號 { get; set; }
        public List<SelectListItem> 產線現有班別_SelectListItem { get; set; }
        public List<SelectListItem> 機台編號_SelectListItem { get; set; }

        public List<SelectListItem> 現有人員_SelectListItem { get; set; }
    }
}
