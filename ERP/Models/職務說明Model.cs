using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace 南岩ERP.Models
{
    public class 職務說明Model
    {
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
        public string 部門 {  get; set; }
        public string 職務 { get; set; }
        public string 職位類別 { get; set; }
        public List<CheckBoxListItem> 學歷 { get; set; } =
        [
            new() { ID = 0, Text = "不限", IsChecked = false },
            new() { ID = 1, Text = "國中", IsChecked = false },
            new() { ID = 2, Text = "高中(職)", IsChecked = false },
            new() { ID = 3, Text = "專科", IsChecked = false },
            new() { ID = 4, Text = "大學", IsChecked = false },
            new() { ID = 5, Text = "碩士", IsChecked = false },
            new() { ID = 6, Text = "博士", IsChecked = false },
        ];
        public string 科系 { get; set; }
        public string 經歷 { get; set; }

        public string 專業證照 { get; set; }
        public List<CheckBoxListItem> 人格特質 { get; set; } =
        [

            new(){ ID = 0, Text = "主動積極" , IsChecked = false },
            new(){ ID = 1, Text = "持續學習", IsChecked = false },
            new(){ ID = 2, Text = "責任感", IsChecked = false },
            new(){ ID = 3, Text = "抗壓性", IsChecked = false },
            new(){ ID = 4, Text = "創新力", IsChecked = false },
            new(){ ID = 5, Text = "正直誠信", IsChecked = false },
            new(){ ID = 6, Text = "親和力", IsChecked = false },
            new(){ ID = 7, Text = "協調性", IsChecked = false },
            new(){ ID = 8, Text = "EQ自制", IsChecked = false },
            new(){ ID = 9, Text = "責任感", IsChecked = false },
            new(){ ID = 10, Text = "內向成穩", IsChecked = false },
            new(){ ID = 11, Text = "活潑外向", IsChecked = false },
            new(){ ID = 12, Text = "細心", IsChecked = false },
            new(){ ID = 13, Text = "樂觀", IsChecked = false },
            new(){ ID = 14, Text = "其它", IsChecked = false }
        ];
        public string[] 教育訓練 { get; set; }
        public string[] 技術面 { get; set; }
        public string[] 專業知識 { get; set; }
        public List<語言> 語言 { get; set; } =
        [
            new() { ID = 0, Text = "國語", IsChecked = false},
            new() { ID = 1, Text = "英語", IsChecked = false},
            new() { ID = 2, Text = "台語", IsChecked = false },
            new() { ID = 3, Text = "其它", IsChecked = false },
        ];

    }

    public class 語言能力
    {
        public bool 聽 { get; set; } = false;
        public bool 說 { get; set; } = false;
        public bool 讀 { get; set; } = false;
        public bool 寫 { get; set; } = false;
    }
    public class 語言
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public bool IsChecked { get; set; }
        public 語言能力 語言能力 { get; set; } =
           new 語言能力 { 聽 = false, 說 = false, 讀 = false, 寫 = false };
    };

}

