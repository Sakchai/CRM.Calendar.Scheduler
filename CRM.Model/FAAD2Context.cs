﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CRM.Model
{
    public partial class FAAD2Context : DbContext
    {
        public FAAD2Context()
        {
        }

        public FAAD2Context(DbContextOptions<FAAD2Context> options)
            : base(options)
        {
        }

        public virtual DbSet<CrmActivity> CrmActivities { get; set; }
        public virtual DbSet<SmEmployee> SmEmployees { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=FAAD2;Integrated Security=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity<CrmActivity>(entity =>
            {
                entity.HasKey(e => e.ActivityId)
                    .HasName("PK__crmActiv__45F4A7F148398B79");
                //entity.HasKey(e => e.Id)
                //     .HasName("PK__crmActiv__45F4A7F148398B79");
                entity.ToTable("crmActivity");

                //entity.Property(e => e.Id)
                //    .HasColumnName("ActivityID")
                //    .HasMaxLength(50)
                //    .ValueGeneratedNever();

                entity.Property(e => e.ActivityId)
                    .HasColumnName("ActivityID")
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.ActCost).HasColumnType("decimal(28, 6)");

                entity.Property(e => e.Address).HasColumnType("ntext");

                entity.Property(e => e.BranchId)
                    .HasColumnName("BranchID")
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedById)
                    .HasColumnName("CreatedByID")
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Detail).HasColumnType("ntext");

                entity.Property(e => e.EmailBcc)
                    .HasColumnName("EmailBCC")
                    .HasColumnType("ntext");

                entity.Property(e => e.EmailBccid)
                    .HasColumnName("EmailBCCID")
                    .HasColumnType("ntext");

                entity.Property(e => e.EmailCc)
                    .HasColumnName("EmailCC")
                    .HasColumnType("ntext");

                entity.Property(e => e.EmailCcid)
                    .HasColumnName("EmailCCID")
                    .HasColumnType("ntext");

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.Latitude).HasMaxLength(50);

                entity.Property(e => e.Longtitude).HasMaxLength(50);

                entity.Property(e => e.ModifiedById)
                    .HasColumnName("ModifiedByID")
                    .HasMaxLength(50);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.NotiDate).HasColumnType("date");

                entity.Property(e => e.OwnerId)
                    .HasColumnName("OwnerID")
                    .HasMaxLength(50);

                entity.Property(e => e.Phone).HasMaxLength(50);

                entity.Property(e => e.PriorityEnumId)
                    .HasColumnName("PriorityEnumID")
                    .HasMaxLength(50);

                entity.Property(e => e.RefDocId)
                    .HasColumnName("RefDocID")
                    .HasMaxLength(50);

                entity.Property(e => e.RefDocNo).HasMaxLength(50);

                entity.Property(e => e.RefMenuId)
                    .HasColumnName("RefMenuID")
                    .HasMaxLength(50);

                entity.Property(e => e.RelateActId)
                    .HasColumnName("RelateActID")
                    .HasColumnType("ntext");

                entity.Property(e => e.RelateActName).HasColumnType("ntext");

                entity.Property(e => e.RelateActType).HasMaxLength(50);

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.StatusEnumId)
                    .HasColumnName("StatusEnumID")
                    .HasMaxLength(50);

                entity.Property(e => e.Topic).HasMaxLength(255);
                //entity.Ignore(e => e.Id);
            });

            modelBuilder.Entity<SmEmployee>(entity =>
            {
                entity.HasKey(e => e.EmpId);
                //entity.HasKey(e => e.Id);

                entity.ToTable("smEmployee");

                entity.Property(e => e.EmpId)
                    .HasColumnName("EmpID")
                    .HasMaxLength(50)
                    .ValueGeneratedNever();
                //entity.Property(e => e.Id)
                //    .HasColumnName("EmpID")
                //    .HasMaxLength(50)
                //    .ValueGeneratedNever();
                entity.Property(e => e.AddrLine).HasColumnType("ntext");

                entity.Property(e => e.BloodGroupEnumId)
                    .HasColumnName("BloodGroupEnumID")
                    .HasMaxLength(50);

                entity.Property(e => e.Country).HasMaxLength(50);

                entity.Property(e => e.CreatedById)
                    .HasColumnName("CreatedByID")
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DeptId)
                    .HasColumnName("DeptID")
                    .HasMaxLength(50);

                entity.Property(e => e.District).HasMaxLength(50);

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.EmpBirthDate).HasColumnType("date");

                entity.Property(e => e.EmpFirstName).HasMaxLength(255);

                entity.Property(e => e.EmpGroupId)
                    .HasColumnName("EmpGroupID")
                    .HasMaxLength(50);

                entity.Property(e => e.EmpLastName).HasMaxLength(255);

                entity.Property(e => e.EmpLeaderId)
                    .HasColumnName("EmpLeaderID")
                    .HasMaxLength(50);

                entity.Property(e => e.EmpLeaveDate).HasColumnType("date");

                entity.Property(e => e.EmpNationality).HasMaxLength(50);

                entity.Property(e => e.EmpNickName).HasMaxLength(50);

                entity.Property(e => e.EmpNo).HasMaxLength(50);

                entity.Property(e => e.EmpProfileImg).HasColumnType("ntext");

                entity.Property(e => e.EmpRace).HasMaxLength(50);

                entity.Property(e => e.EmpReligion).HasMaxLength(50);

                entity.Property(e => e.EmpSignerImg).HasColumnType("ntext");

                entity.Property(e => e.EmpStartWordDate).HasColumnType("date");

                entity.Property(e => e.EmpTaxId)
                    .HasColumnName("EmpTaxID")
                    .HasMaxLength(50);

                entity.Property(e => e.EmpTypeEnumId)
                    .HasColumnName("EmpTypeEnumID")
                    .HasMaxLength(50);

                entity.Property(e => e.Facebook).HasMaxLength(50);

                entity.Property(e => e.Fax).HasMaxLength(50);

                entity.Property(e => e.GenderEnumId)
                    .HasColumnName("GenderEnumID")
                    .HasMaxLength(50);

                entity.Property(e => e.Latitude).HasMaxLength(50);

                entity.Property(e => e.LineId)
                    .HasColumnName("LineID")
                    .HasMaxLength(50);

                entity.Property(e => e.Longitude).HasMaxLength(50);

                entity.Property(e => e.MaritalEnumId)
                    .HasColumnName("MaritalEnumID")
                    .HasMaxLength(50);

                entity.Property(e => e.MilitaryEnumId)
                    .HasColumnName("MilitaryEnumID")
                    .HasMaxLength(50);

                entity.Property(e => e.Mobile).HasMaxLength(50);

                entity.Property(e => e.ModifiedById)
                    .HasColumnName("ModifiedByID")
                    .HasMaxLength(50);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Phone).HasMaxLength(50);

                entity.Property(e => e.PositionId)
                    .HasColumnName("PositionID")
                    .HasMaxLength(50);

                entity.Property(e => e.PostCode).HasMaxLength(50);

                entity.Property(e => e.Province).HasMaxLength(50);

                entity.Property(e => e.SubDistrict).HasMaxLength(50);

                entity.Property(e => e.TitleNameEnumId)
                    .HasColumnName("TitleNameEnumID")
                    .HasMaxLength(50);

                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkStatusEnumId)
                    .HasColumnName("WorkStatusEnumID")
                    .HasMaxLength(50);
               // entity.Ignore(e => e.Id);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}