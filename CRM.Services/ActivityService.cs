using System;
using System.Collections.Generic;
using System.Linq;
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

        #endregion

        #region Ctor

        public ActivityService(IRepository<CrmActivity> activityRepository,
            IRepository<CrmActivityFollower> activityFollowerRepository,
            IEmployeeService employeeService,
            IRepository<SmEnum> enumRepository
            )
        {
            _activityRepository = activityRepository;
            _activityFollowerRepository = activityFollowerRepository;
            _employeeService = employeeService;
            _enumRepository = enumRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets an activity by activity identifier
        /// </summary>
        /// <param name="activityId">Activity identifier</param>
        /// <returns>Activity</returns>
        public virtual IList<CrmActivity> GetActivityByOwner(string ownerId, int BeforeMinuitesModifiedDate)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
                return null;

            var query = _activityRepository.Table;
            var activitys = query.Where(x => x.OwnerId == ownerId 
                                        && x.ModifiedDate >= DateTime.Now.AddMinutes(-BeforeMinuitesModifiedDate) ).ToList();
            var activitysByOwner = new List<CrmActivity>();
            foreach (var item in activitys)
            {
                item.PriorityEnumName = GetStatusOrPriorityName(item.PriorityEnumId);
                item.StatusEnumName = GetStatusOrPriorityName(item.StatusEnumId);
                activitysByOwner.Add(item);
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
                employeeFollowers.Add(new EventAttendee
                { DisplayName = fullname, Email = emp.Email });
            }
            return employeeFollowers;
        }





        #endregion
    }
}