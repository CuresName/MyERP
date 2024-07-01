using ERP.教育訓練EFModels;
namespace ERP.Models
{
    public class ISO文件ModelManager
    {
        public List<文件表> 文件表 { get; set; }
        public List<職務文件關聯表> 職務文件 { get; set; }
        public required List<部門表> 部門 { get; set; }
    }
}
