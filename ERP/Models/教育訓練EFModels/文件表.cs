﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace ERP.教育訓練EFModels;

public partial class 文件表
{
    public string 文件編號 { get; set; }

    public string 文件名稱 { get; set; }

    public byte? 筆試 { get; set; }

    public byte? 口試 { get; set; }

    public byte? 實作 { get; set; }

    public DateTime? 建檔日期 { get; set; }

    public DateTime? KEYIN_DATE { get; set; }

    public string MAN_ID { get; set; }

    public string HOST_NAME { get; set; }

    public string HOST_IP { get; set; }
}