﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace 南岩ERP.教育訓練EFModels;

public partial class 教育訓練計畫表內容
{
    public int 計畫表內容流水號 { get; set; }

    public string 計畫表編號 { get; set; }

    public string 提出單位 { get; set; }

    public string 類別 { get; set; }

    public string 課程名稱 { get; set; }

    public string 課程大綱 { get; set; }

    public bool? 管理人員 { get; set; }

    public bool? 工管理師 { get; set; }

    public bool? 作業人員 { get; set; }

    public bool? 業務部 { get; set; }

    public bool? 研發部 { get; set; }

    public bool? 製造部 { get; set; }

    public bool? 品保部 { get; set; }

    public bool? 測試部 { get; set; }

    public bool? 財務部 { get; set; }

    public bool? 管理部 { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime 預計上課日期 { get; set; }

    public int? 上課時數 { get; set; }

    public string 訓練方式 { get; set; }

    public string 講師 { get; set; }

    public string 預計費用 { get; set; }

    public string 預計參訓人數 { get; set; }

    public string 預計總費用 { get; set; }

    public DateTime? 建檔日期 { get; set; }

    public DateTime? KEYIN_DATE { get; set; }

    public string MAN_ID { get; set; }

    public string HOST_NAME { get; set; }

    public string HOST_IP { get; set; }
}