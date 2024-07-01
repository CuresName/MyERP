using System.ComponentModel.DataAnnotations;

namespace 南岩ERP.Models
{
    public class 計畫表Model
    {
        public int 計畫表內容流水號 { get; set; }
        public string 計畫表編號 { get; set; }
        public string 提出單位 { get; set; }
        public string 類別 { get; set; }
        public string 課程名稱 { get; set; }
        public string 課程大綱 { get; set; }
        public List<CheckBoxListItem> 基層別 { get; set; } = new List<CheckBoxListItem>
        {
            new(){ ID = 0, Text = "管理人員", IsChecked = false },
            new(){ ID = 1, Text = "工管理師", IsChecked = false},
            new(){ ID = 2, Text = "作業人員", IsChecked = false}
        };
        public List<CheckBoxListItem> 訓練單位 { get; set; } = new List<CheckBoxListItem>
        {
            new(){ ID = 0, Text = "業務部", IsChecked = false },
            new(){ ID = 1, Text = "研發部", IsChecked = false },
            new(){ ID = 2, Text = "製造部", IsChecked = false },
            new(){ ID = 3, Text = "品保部", IsChecked = false },
            new(){ ID = 4, Text = "測試部", IsChecked = false },
            new(){ ID = 5, Text = "財務部", IsChecked = false },
            new(){ ID = 6, Text = "管理部", IsChecked = false }
        };
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime 預計上課日期 { get; set; }
        public int 上課時數 { get; set; }
        public string 訓練方式 { get; set; }
        public string 講師 { get; set; }
        public string 預計費用 { get; set; }
        public string 預計參訓人數 { get; set; }
        public string 預計總費用 { get; set; }


    }
}
