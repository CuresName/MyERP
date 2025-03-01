﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ERP.MisEFModels;

public partial class misContext : DbContext
{
    public misContext(DbContextOptions<misContext> options)
        : base(options)
    {
    }

    public virtual DbSet<機器設備硬碟備份紀錄> 機器設備硬碟備份紀錄 { get; set; }

    public virtual DbSet<資訊設備明細表> 資訊設備明細表 { get; set; }

    public virtual DbSet<軟體版權明細> 軟體版權明細 { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<機器設備硬碟備份紀錄>(entity =>
        {
            entity.HasKey(e => e.流水號);

            entity.Property(e => e.HOST_IP).HasMaxLength(50);
            entity.Property(e => e.HOST_NAME).HasMaxLength(50);
            entity.Property(e => e.KEYIN_DATE).HasColumnType("date");
            entity.Property(e => e.MAN_ID).HasMaxLength(50);
            entity.Property(e => e.備份日期).HasColumnType("date");
            entity.Property(e => e.備註).HasMaxLength(50);
            entity.Property(e => e.建檔日期).HasColumnType("date");
            entity.Property(e => e.設備名稱)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<資訊設備明細表>(entity =>
        {
            entity.HasKey(e => e.流水號);

            entity.Property(e => e.AGP).HasMaxLength(255);
            entity.Property(e => e.CPU).HasMaxLength(255);
            entity.Property(e => e.F31).HasMaxLength(255);
            entity.Property(e => e.HOST_IP).HasMaxLength(255);
            entity.Property(e => e.HOST_NAME).HasMaxLength(255);
            entity.Property(e => e.IP_Address)
                .HasMaxLength(255)
                .HasColumnName("IP Address");
            entity.Property(e => e.ISA).HasMaxLength(255);
            entity.Property(e => e.KEYIN_DATE).HasColumnType("date");
            entity.Property(e => e.MAC_ADDrEss)
                .HasMaxLength(255)
                .HasColumnName("MAC ADDrEss");
            entity.Property(e => e.MAN_ID).HasMaxLength(255);
            entity.Property(e => e.Mail_Address)
                .HasMaxLength(255)
                .HasColumnName("Mail Address");
            entity.Property(e => e.Office版本).HasMaxLength(255);
            entity.Property(e => e.PCI).HasMaxLength(255);
            entity.Property(e => e.PCI_E_2_0_x16)
                .HasMaxLength(255)
                .HasColumnName("PCI-E 2#0 x16");
            entity.Property(e => e.PCI_E_3_0_x1)
                .HasMaxLength(255)
                .HasColumnName("PCI-E 3#0 x1");
            entity.Property(e => e.PCI_E_3_0_x16)
                .HasMaxLength(255)
                .HasColumnName("PCI-E 3#0 x16");
            entity.Property(e => e.PCI_E_4_0_x16)
                .HasMaxLength(255)
                .HasColumnName("PCI-E 4#0 x16");
            entity.Property(e => e.PCI_E_x1)
                .HasMaxLength(255)
                .HasColumnName("PCI-E x1");
            entity.Property(e => e.PCI_E_x4)
                .HasMaxLength(255)
                .HasColumnName("PCI-E x4");
            entity.Property(e => e.RAM).HasMaxLength(255);
            entity.Property(e => e.Win10_Microsoft帳戶)
                .HasMaxLength(255)
                .HasColumnName("Win10 Microsoft帳戶");
            entity.Property(e => e.Win10_Microsoft帳戶密碼)
                .HasMaxLength(255)
                .HasColumnName("Win10 Microsoft帳戶密碼");
            entity.Property(e => e.主機版).HasMaxLength(255);
            entity.Property(e => e.使用者_or_設備名稱_員工編號_姓名_分機)
                .HasMaxLength(255)
                .HasColumnName("使用者 or 設備名稱_員工編號/姓名/分機");
            entity.Property(e => e.使用部門).HasMaxLength(255);
            entity.Property(e => e.備註).HasMaxLength(255);
            entity.Property(e => e.公司).HasMaxLength(50);
            entity.Property(e => e.原始作業系統_免費升級系統).HasMaxLength(255);
            entity.Property(e => e.建檔日期).HasMaxLength(255);
            entity.Property(e => e.放置位置).HasMaxLength(255);
            entity.Property(e => e.硬碟容量).HasMaxLength(255);
            entity.Property(e => e.網卡1).HasMaxLength(50);
            entity.Property(e => e.網卡2).HasMaxLength(50);
            entity.Property(e => e.網卡3).HasMaxLength(50);
            entity.Property(e => e.網路接線盒號碼).HasMaxLength(255);
            entity.Property(e => e.記億體_插槽_速度_最大)
                .HasMaxLength(255)
                .HasColumnName("記億體-插槽/速度/最大");
            entity.Property(e => e.設備序號).HasMaxLength(255);
            entity.Property(e => e.郵件軟體).HasMaxLength(255);
            entity.Property(e => e.電腦名稱_or_設備型號)
                .HasMaxLength(255)
                .HasColumnName("電腦名稱 or 設備型號");
            entity.Property(e => e.電話接線盒號碼).HasMaxLength(255);
        });

        modelBuilder.Entity<軟體版權明細>(entity =>
        {
            entity.HasKey(e => e.流水號);

            entity.Property(e => e.HOST_IP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.HOST_NAME)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.KEYIN_DATE).HasColumnType("date");
            entity.Property(e => e.MAC)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MAN_ID)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.備註)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.公司)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.安裝電腦名稱)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.密碼)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.帳號)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.建檔日期)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.產品序號)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.軟體名稱)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}