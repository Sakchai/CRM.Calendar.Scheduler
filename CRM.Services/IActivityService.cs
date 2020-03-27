using CRM.Dto;
using CRM.Model;
using Google.Apis.Calendar.v3.Data;
using System;
using System.Collections.Generic;

namespace CRM.Services
{
    /// <summary>
    /// Activity service interface
    /// </summary>
    public partial interface IActivityService
    {
        /// <summary>
        /// Gets an activity by activity identifier
        /// </summary>
        /// <param name="email">Activity identifier</param>
        /// <returns>Activity</returns>
        IList<CrmActivityDto> GetActivityByOwner(string ownerId, int BeforeMinuitesModifiedDate);
        CrmActivity GetActivityById(string activityId);
        //IList<EmployeeFollowerDto> GetEmailsFollowID(string activityId);
        List<EventAttendee> GetEmailsFollowID(string activityId);
        string GetStatusOrPriorityName(string enumId);

        /// <summary>
        /// Marks activity as deleted 
        /// </summary>
        /// <param name="activity">Activity</param>
        void DeleteActivity(CrmActivity activity);

       

        /// <summary>
        /// Inserts an activity
        /// </summary>
        /// <param name="activity">Activity</param>
        void InsertActivity(CrmActivity activity);

        /// <summary>
        /// Updates the activity
        /// </summary>
        /// <param name="activity">Activity</param>
        void UpdateActivity(CrmActivity activity);



    }
}