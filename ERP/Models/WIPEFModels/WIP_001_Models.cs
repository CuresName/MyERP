using Microsoft.AspNetCore.Mvc.Rendering;

namespace 南岩ERP.Models.WIPEFModels
{
    public class WIP_001_Models
    {
        public List<WIP_001_生產回報_不良品> WIP_001_生產回報_不良品 { get; set; }
        public List<WIP_001_生產回報_產出資料> WIP_001_生產回報_產出資料 { get; set; }
        public WIP_001_生產回報資料來源 WIP_001_生產回報資料來源 { get; set; }
        public WIP_001_產線現有班別 WIP_001_產線現有班別 { get;set; }
        public List<WIP_001_生產回報_Cassette_NoResult> WIP_001_生產回報_Cassette_NoResult {  get; set; }
        public List<SelectListItem> WIP_001_生產回報_流程卡號_SelectListItem { get; set; }
    }
}
