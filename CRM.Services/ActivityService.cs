using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CRM.Dto;
using CRM.Model;
using Google.Apis.Calendar.v3.Data;

namespace CRM.Services
{
    /// <summary>
    /// Activity service
    /// </summary>
    public partial class ActivityService : IActivityService
    {
        #region Fields
        private readonly IRepository<CrmActivityFollower> _activityFollowerRepository;
        private readonly IEmployeeService _employeeService;
        private readonly IRepository<CrmActivity> _activityRepository;
        private readonly IRepository<SmEnum> _enumRepository;
        private readonly IRepository<CrmActivityFacility> _crmActivityFacility;
        private readonly IRepository<CrmActivityNote> _crmActivityNote;
        private readonly IRepository<SmFacility> _smFacility;
        private readonly IMapper _mapper;
        #endregion

        #region Ctor

        public ActivityService(IRepository<CrmActivity> activityRepository,
            IRepository<CrmActivityFollower> activityFollowerRepository,
            IEmployeeService employeeService,
            IRepository<SmEnum> enumRepository,
            IMapper mapper,
            IRepository<CrmActivityFacility> crmActivityFacility,
            IRepository<CrmActivityNote> crmActivityNote,
            IRepository<SmFacility> smFacility
            )
        {
            _activityRepository = activityRepository;
            _activityFollowerRepository = activityFollowerRepository;
            _employeeService = employeeService;
            _enumRepository = enumRepository;
            _mapper = mapper;
            _crmActivityFacility = crmActivityFacility;
            _crmActivityNote = crmActivityNote;
            _smFacility = smFacility;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets an activity by activity identifier
        /// </summary>
        /// <param name="activityId">Activity identifier</param>
        /// <returns>Activity</returns>
        public virtual IList<CrmActivityDto> GetActivityByOwner(string ownerId, bool checkMinutes, int BeforeMinuitesModifiedDate)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
                return null;

            var query = _activityRepository.Table;
            query = query.Where(x => x.OwnerId == ownerId);
            if (checkMinutes) query = query.Where(x => x.ModifiedDate >= DateTime.Now.AddMinutes(-BeforeMinuitesModifiedDate));
            var activitys = query.ToList();
            var activitysByOwner = new List<CrmActivityDto>();
            foreach (var item in activitys)
            {
                var activity = _mapper.Map<CrmActivity, CrmActivityDto>(item);
                activity.PriorityEnumName = GetStatusOrPriorityName(item.PriorityEnumId);
                activity.StatusEnumName = GetStatusOrPriorityName(item.StatusEnumId);

                activitysByOwner.Add(activity);
            }

            return activitysByOwner;

        }



        /// <summary>
        /// Marks activity as deleted 
        /// </summary>
        /// <param name="activity">Activity</param>
        public virtual void DeleteActivity(CrmActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));

            activity.IsDelete = true;
            UpdateActivity(activity);

        }

        /// <summary>
        /// Inserts an activity
        /// </summary>
        /// <param name="activity">Activity</param>
        public virtual void InsertActivity(CrmActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));

            _activityRepository.Insert(activity);

        }

        /// <summary>
        /// Updates the activity
        /// </summary>
        /// <param name="activity">Activity</param>
        public virtual void UpdateActivity(CrmActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));

            _activityRepository.Update(activity);

        }

        public string GetStatusOrPriorityName(string enumId)
        {
            var query = _enumRepository.Table;
            var enums = query.Where(x => x.EnumId == enumId).FirstOrDefault();

            return enums.EnumName;
        }

        public List<EventAttendee> GetEmailsFollowID(string activityId)
        {
            var query = _activityFollowerRepository.Table;
            var followers = query.Where(x => x.ActivityId == activityId).ToList();
            //var employeeFollowers = new List<EmployeeFollowerDto>();
            var employeeFollowers = new List<EventAttendee>();
            foreach (var item in followers)
            {
                var emp = _employeeService.GetEmployeeByID(item.EmpId);
                string fullname = $"{emp.EmpFirstName} {emp.EmpLastName}";
                //employeeFollowers.Add(new EmployeeFollowerDto { DisplayName = fullname, Email = emp.Email });
                //employeeFollowers.Add(new EventAttendee
                //{ DisplayName = fullname, Email = emp.Email });
                if (!string.IsNullOrWhiteSpace(emp.Facebook))
                    employeeFollowers.Add(new EventAttendee
                    { DisplayName = fullname, Email = emp.Facebook });
            }
            return employeeFollowers;
        }

        public CrmActivity GetActivityById(string activityId)
        {
            if (string.IsNullOrWhiteSpace(activityId))
                return null;

            var query = _activityRepository.Table;
            return query.Where(x => x.ActivityId == activityId).FirstOrDefault();
        }

        public List<string> GetNotes(string activityId)
        {
            if (string.IsNullOrWhiteSpace(activityId))
                return null;
            var query = _crmActivityNote.Table;
            query = query.Where(x => x.ActivityId == activityId && !x.IsInactive.Value);
            var notes = query.OrderBy(y => y.ListNo).ToList();
            var result = new List<string>();
            if (notes.Count > 0)
            {
                result.Add("<h4>Note</h4>");
                result.Add("<table>");
                result.Add($"<tr><th>ลำดับ</th><th>บันทึกเพิ่มเติม</th></tr>");
                foreach (var item in notes)
                {
                    result.Add($"<tr><td>{item.ListNo}</td><td>{item.Note}</td></tr>");
                }
                result.Add("</table>");
            }
            return result;
        }

        private SmFacility GetFacility(string facilityId)
        {
            if (string.IsNullOrWhiteSpace(facilityId))
                return null;
            var query = _smFacility.Table;
            return query.Where(x => x.FacilityId == facilityId).FirstOrDefault();
        }

        public List<string> GetFacilities(string activityId)
        {
            if (string.IsNullOrWhiteSpace(activityId))
                return null;
            var query = _crmActivityFacility.Table;
            query = query.Where(x => x.ActivityId == activityId && !x.IsInactive.Value);
            var facilities = query.OrderBy(y => y.FacilityId).ToList();
            var result = new List<string>();
            if (facilities.Count > 0)
            {
                result.Add("<h4>Facility</h4>");
                result.Add("<table>");
                result.Add($"<tr><th>หมายเลข</th><th>ชื่อ Facility</th><th>จำนวน(ชิ้น)</th></tr>");
                foreach (var item in facilities)
                {
                    var facility = this.GetFacility(item.FacilityId);
                    result.Add($"<tr><td>{facility.FacilityNo}</td><td>{facility.FacilityName}</td><td>{item.Qty}</td>");
                }
                result.Add("</table>");
            }
            return result;
        }





        #endregion
    }
}