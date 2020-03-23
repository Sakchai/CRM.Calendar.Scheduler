using AutoMapper;
using CRM.Dto;
using CRM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Calendar
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<CrmActivity, CrmActivityDto>();
        }
    }
}
