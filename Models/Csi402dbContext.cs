using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace warat_0583_l005.Models;

public partial class Csi402dbContext : DbContext
{
public Csi402dbContext(DbContextOptions<Csi402dbContext> options)
        : base(options)
    {
    }

    // ====================================================
    // DbSet สำหรับระบุตารางทั้งหมดในฐานข้อมูล
    // ====================================================
    public virtual DbSet<UserModel> UserModels { get; set; }
    public virtual DbSet<LoginHistoryModel> LoginHistories { get; set; }
    public virtual DbSet<BookingModel> Bookings { get; set; }
    
    // 📌 เพิ่ม DbSet สำหรับตาราง cars (รถยนต์)
    public virtual DbSet<CarModel> Cars { get; set; }
    public virtual DbSet<PaymentModel> Payments { get; set; }
    public virtual DbSet<PromotionModel> Promotions { get; set; }
    public virtual DbSet<CarInspectionModel> CarInspections { get; set; }
    public virtual DbSet<MaintenanceModel> MaintenanceRecords { get; set; }
    public virtual DbSet<AddOnServiceModel> AddOnServices { get; set; }
    public virtual DbSet<SystemSettingModel> SystemSettings { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ตั้งค่า Charset และ Collation เริ่มต้นของฐานข้อมูล
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        // ====================================================
        // 1. การตั้งค่า Fluent API สำหรับตาราง usermodels
        // ====================================================
        modelBuilder.Entity<UserModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("usermodels");

            entity.Property(e => e.Id).HasColumnName("Id");
            
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .IsRequired();
                
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsRequired();
                
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Gender).HasMaxLength(50);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.Property(e => e.RegisterDate)
                .HasColumnType("datetime")
                .HasColumnName("RegisterDate");
        });

        // ====================================================
        // 2. การตั้งค่า Fluent API สำหรับตาราง login_history
        // ====================================================
        modelBuilder.Entity<LoginHistoryModel>(entity =>
        {
            entity.HasKey(e => e.LoginHistoryId).HasName("PRIMARY");
            entity.ToTable("login_history");

            entity.Property(e => e.LoginHistoryId).HasColumnName("LoginHistoryID");
            
            entity.Property(e => e.UserId)
                .HasColumnName("UserID")
                .IsRequired();
                
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsRequired();
                
            entity.Property(e => e.Email).HasMaxLength(100);
            
            entity.Property(e => e.LoggedInAtUtc)
                .HasColumnType("datetime(6)")
                .HasColumnName("LoggedInAtUTC");
        });

        // ====================================================
        // 3. การตั้งค่า Fluent API สำหรับตาราง bookings
        // ====================================================
        modelBuilder.Entity<BookingModel>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PRIMARY");
            entity.ToTable("bookings");

            entity.Property(e => e.BookingId).HasColumnName("BookingId");
            entity.Property(e => e.UserId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CarModel).HasMaxLength(150).IsRequired();
            entity.Property(e => e.PickupLocation).HasMaxLength(150).IsRequired();
            entity.Property(e => e.PickupDate).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RentalDays).IsRequired();
            entity.Property(e => e.PricePerDay).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.BookingDate).HasColumnType("datetime").HasColumnName("BookingDate");
            entity.Property(e => e.CarId).HasColumnName("CarId");
            entity.Property(e => e.ReturnLocation).HasMaxLength(150);
            entity.Property(e => e.PickupTime).HasColumnType("time");
            entity.Property(e => e.ReturnDate).HasColumnType("date");
            entity.Property(e => e.ReturnTime).HasColumnType("time");
            entity.Property(e => e.DepositAmount).HasColumnType("decimal(10,2)").HasDefaultValue(0m);
            entity.Property(e => e.BookingStatus).HasMaxLength(30).HasDefaultValue("pending");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10,2)").HasDefaultValue(0m);
            entity.Property(e => e.PromotionCode).HasMaxLength(50);
            entity.Property(e => e.LicenseImagePath).HasMaxLength(255);
            entity.Property(e => e.LicenseStatus).HasMaxLength(30).HasDefaultValue("pending");
            entity.Property(e => e.LicenseReviewNote).HasMaxLength(500);
            entity.Property(e => e.ReturnMileage);
            entity.Property(e => e.ReturnImagePaths).HasMaxLength(500);
            entity.Property(e => e.ReturnSubmittedAt).HasColumnType("datetime");
            entity.HasIndex(e => e.CarId);
            entity.HasOne<CarModel>().WithMany().HasForeignKey(e => e.CarId).OnDelete(DeleteBehavior.Restrict);
        });

        // ====================================================
        // 4. การตั้งค่า Fluent API สำหรับตาราง cars (📌 เพิ่มใหม่)
        // ====================================================
        modelBuilder.Entity<CarModel>(entity =>
        {
            entity.HasKey(e => e.CarId).HasName("PRIMARY");
            entity.ToTable("cars");

            entity.Property(e => e.CarId).HasColumnName("CarId");
            entity.Property(e => e.CarName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.PricePerDay).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.ImageUrl).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("ว่าง");
            entity.Property(e => e.Details).HasMaxLength(255).IsRequired();
            entity.Property(e => e.TransmissionType).HasMaxLength(20);
            entity.Property(e => e.Location).HasMaxLength(150);
        });

        modelBuilder.Entity<PaymentModel>(entity =>
        {
            entity.HasKey(e => e.PaymentId);
            entity.ToTable("payments");
            entity.Property(e => e.PaymentType).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Amount).HasColumnType("decimal(10,2)");
            entity.Property(e => e.PaymentStatus).HasMaxLength(30).HasDefaultValue("pending");
            entity.Property(e => e.ProofFileName).HasMaxLength(255);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.ReviewedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ReviewedAt).HasColumnType("datetime");
            entity.HasOne<BookingModel>().WithMany().HasForeignKey(e => e.BookingId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PromotionModel>(entity =>
        {
            entity.HasKey(e => e.PromotionId);
            entity.ToTable("promotions");
            entity.Property(e => e.PromotionName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.PromotionCode).HasMaxLength(50);
            entity.Property(e => e.ConditionType).HasMaxLength(30).IsRequired();
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)");
            entity.Property(e => e.StartDate).HasColumnType("date");
            entity.Property(e => e.EndDate).HasColumnType("date");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<CarInspectionModel>(entity =>
        {
            entity.HasKey(e => e.InspectionId);
            entity.ToTable("car_inspections");
            entity.Property(e => e.InspectionStage).HasMaxLength(30).IsRequired();
            entity.Property(e => e.FuelLevel).IsRequired();
            entity.Property(e => e.ConditionNotes).HasMaxLength(2000);
            entity.Property(e => e.DamageAmount).HasColumnType("decimal(10,2)");
            entity.Property(e => e.InspectionStatus).HasMaxLength(30);
            entity.Property(e => e.InspectedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.InspectedAt).HasColumnType("datetime");
            entity.HasOne<BookingModel>().WithMany().HasForeignKey(e => e.BookingId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CarModel>().WithMany().HasForeignKey(e => e.CarId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MaintenanceModel>(entity =>
        {
            entity.HasKey(e => e.MaintenanceId);
            entity.ToTable("maintenance_records");
            entity.Property(e => e.IssueDescription).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.RepairCost).HasColumnType("decimal(10,2)");
            entity.Property(e => e.MaintenanceStatus).HasMaxLength(30);
            entity.Property(e => e.ReportedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ResolvedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ResolvedAt).HasColumnType("datetime");
            entity.HasOne<CarModel>().WithMany().HasForeignKey(e => e.CarId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<BookingModel>().WithMany().HasForeignKey(e => e.BookingId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AddOnServiceModel>(entity =>
        {
            entity.HasKey(e => e.ServiceId);
            entity.ToTable("add_on_services");
            entity.Property(e => e.ServiceCode).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.ServiceCode).IsUnique();
            entity.Property(e => e.ServiceName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.PricePerDay).HasColumnType("decimal(10,2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<SystemSettingModel>(entity =>
        {
            entity.HasKey(e => e.SettingKey);
            entity.ToTable("system_settings");
            entity.Property(e => e.SettingKey).HasMaxLength(100);
            entity.Property(e => e.SettingValue).HasMaxLength(500).IsRequired();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
