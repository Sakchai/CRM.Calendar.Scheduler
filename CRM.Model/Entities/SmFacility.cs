﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using LinqToDB.Mapping;
using System;
using System.Collections.Generic;

namespace CRM.Model
{
    public partial class SmFacility : BaseEntity
    {
        [PrimaryKey, NotNull]
        public string FacilityId { get; set; }
        public string FacilityNo { get; set; }
        public string FacilityName { get; set; }
        public string Remark { get; set; }
        public bool? IsInactive { get; set; }
        public bool? IsDelete { get; set; }
        public string CreatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedById { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}