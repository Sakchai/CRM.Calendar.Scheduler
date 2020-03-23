﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Model
{
    public partial class CrmActivity : BaseEntity
    {

        //[Key]
        //[Column("ActivityId")]
        public string ActivityId { get; set; }

        public byte? ActivityType { get; set; }
        public byte? Type { get; set; }
        public string PriorityEnumId { get; set; }
        public string StatusEnumId { get; set; }
        public string RelateActType { get; set; }
        public string RelateActId { get; set; }
        public string RelateActName { get; set; }
        public string Topic { get; set; }
        public string Detail { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public bool? IsAllDay { get; set; }
        public string RefMenuId { get; set; }
        public string RefDocId { get; set; }
        public string OwnerId { get; set; }
        public string Phone { get; set; }
        public decimal? ActCost { get; set; }
        public string EmailCcid { get; set; }
        public string EmailCc { get; set; }
        public string EmailBccid { get; set; }
        public string EmailBcc { get; set; }
        public bool? IsSendEmailAuto { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longtitude { get; set; }
        public DateTime? NotiDate { get; set; }
        public TimeSpan? NotiTime { get; set; }
        public bool? IsNotiOwner { get; set; }
        public bool? IsNotiEmp { get; set; }
        public bool? IsNotiAnother { get; set; }
        public bool? IsInactive { get; set; }
        public bool? IsDelete { get; set; }
        public string CreatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedById { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string BranchId { get; set; }
        public bool? IsNotiApp { get; set; }
        public bool? IsNotiEmail { get; set; }
        public string RefDocNo { get; set; }
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        //public virtual string PriorityEnumName { get; set; }
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        //public virtual string StatusEnumName { get; set; }
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        //public virtual DateTime? StartDateTime { get { return StartDate + StartTime; } }
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        //public virtual DateTime? EndDateTime { get { return EndDate + EndTime;  } }
    }
}