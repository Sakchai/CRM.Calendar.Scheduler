// Copyright 2018 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// [START calendar_quickstart]
using Autofac;
using CRM.Services;
using CRM.Model;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Configuration;
using System.Collections.Generic;
using AutoMapper;
using CRM.Calendar;
using CRM.Dto;
using log4net;
using System.Reflection;
using System.Globalization;

namespace FAAD.Calendar
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/calendar-dotnet-quickstart.json
        static string[] Scopes = { CalendarService.Scope.Calendar };
        static string ApplicationName = "CRM Calendar";


        static void Main(string[] args)
        {
            ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            var builder = new ContainerBuilder();
            //data layer
            builder.RegisterType<DataProviderManager>().As<IDataProviderManager>().InstancePerDependency();
            builder.Register(context => context.Resolve<IDataProviderManager>().DataProvider).As<IFAADDataProvider>().InstancePerDependency();

            //repositories
            builder.RegisterGeneric(typeof(EntityRepository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();

            builder.RegisterType<EmployeeService>().As<IEmployeeService>().InstancePerLifetimeScope();
            builder.RegisterType<ActivityService>().As<IActivityService>().InstancePerLifetimeScope();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => typeof(Profile).IsAssignableFrom(t) && !t.IsAbstract && t.IsPublic)
                .As<Profile>();

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapping());
            });

            builder.Register(c => new MapperConfiguration(cfg =>
            {
                foreach (var profile in c.Resolve<IEnumerable<Profile>>())
                {
                    cfg.AddProfile(profile);
                }
            })).AsSelf().SingleInstance();

            builder.Register(ctx => ctx.Resolve<MapperConfiguration>().CreateMapper()).As<IMapper>().InstancePerLifetimeScope();

            UserCredential credential;

            using (var stream =
                new FileStream(@"credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });


            using (var container = builder.Build())
            {
                var employeeService = container.Resolve<IEmployeeService>();
                var activityService = container.Resolve<IActivityService>();
                var employees = employeeService.GetEmployeesList();
                foreach (var item in employees)
                {
                    string calendarId = string.Empty;
                    if (IsFacebookCalendarType())
                        calendarId = item.Facebook;
                    else
                        calendarId = item.Email;
                    bool foundGoogleCalendar = (GetCalendar(logger, service, calendarId) != null);
                    if (UsedDefaultCalendarId())  {
                        if (!foundGoogleCalendar) calendarId = GetDefaultCalendarId();
                        else CreateNewCalendar(logger, service, item);
                        foundGoogleCalendar = true;
                    }
                    if (foundGoogleCalendar)
                    {
                        CreateOrUpdateEventsToGoogleCalendar(logger, service, item, activityService, calendarId);
                        UpdateActivitiesFromGoogleCalendar(logger, service, activityService, calendarId);
                    }
                }
            }

        }

        private static void CreateOrUpdateEventsToGoogleCalendar(ILog logger, CalendarService service, SmEmployee item, IActivityService activityService, string calendarId)
        {
            var activities = activityService.GetActivityByOwner(item.EmpId, CheckMinutesModifiedDate(), GetBeforeMinutesModifiedDate());
            foreach (var activity in activities)
            {
                var followers = activityService.GetEmailsFollowID(activity.ActivityId);
                var notes = GetNotes(activityService, activity.ActivityId);
                string eventId = FindEventId(logger, service, calendarId, activity.ActivityId);
                if (!string.IsNullOrEmpty(eventId))
                    UpdateNewEvent(service, calendarId, item, activity, followers, eventId, logger,notes);
                else
                    CreateNewEvent(service, calendarId, item, activity, followers, logger,notes);
            }
        }

        private static void UpdateActivitiesFromGoogleCalendar(ILog logger, CalendarService service, IActivityService activityService, string calendarId)
        {
            Events events = FindEvents(logger, service, calendarId);
            if (events != null)
            {
                logger.Info($"Update Calendar Id from Google:{calendarId} count:{events.Items.Count} ");
                foreach (var item in events.Items)
                {
                    var activity = new CrmActivity();
                    try
                    {
                        bool updated = false;
                        if (!string.IsNullOrWhiteSpace(item.ICalUID))
                             activity = activityService.GetActivityById(item.ICalUID);
                        //activity = activityService.GetActivityById("23e8f5c6-283c-4826-9e80-e5dce15fdefc");
                        if (!string.IsNullOrWhiteSpace(activity.ActivityId))
                        {
                            var startDateTime = activity.StartDate + activity.StartTime;
                            var endDateTime = activity.EndDate + activity.EndTime;
                            if (startDateTime != item.Start.DateTime.Value)
                            {
                                updated = true;
                                activity.StartDate = item.Start.DateTime.Value.Date;
                                activity.StartTime = item.Start.DateTime.Value.TimeOfDay;
                            }
                            if (endDateTime != item.End.DateTime.Value)
                            {
                                updated = true;
                                activity.StartDate = item.Start.DateTime.Value.Date;
                                activity.StartTime = item.Start.DateTime.Value.TimeOfDay;
                            }
                            if (!string.IsNullOrWhiteSpace(item.Summary) && !item.Summary.Contains(activity.Topic))
                            {
                                updated = true;
                                activity.Topic = item.Summary;
                            }
                            if (!string.IsNullOrWhiteSpace(item.Description) && (!item.Description.Contains(activity.Detail)))
                            {
                                updated = true;
                                activity.Detail = item.Description;
                            }
                        }

                        if (updated) activityService.UpdateActivity(activity);

                    }
                    catch (Exception e)
                    {
                        logger.Error($"Update Activity Id:{activity.ActivityId} CalendarId:{calendarId}  Error:{e.Message}{e.StackTrace}");
                    }
                }
            }
        }

        private static Events FindEvents(ILog logger, CalendarService service, string calendarId)
        {
            EventsResource.ListRequest request = service.Events.List(calendarId);
            //request.TimeMin = DateTime.Now;
            request.UpdatedMin = DateTime.Now.AddMinutes(-GetBeforeMinutesModifiedDate());
            request.TimeZone = GetTimeZone();
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 100;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            try
            {
                // List events.
                return request.Execute();
            } catch (Exception e)
            {
                logger.Info($"Not found data:CalendarId:{calendarId}  Error:{e.Message}{e.StackTrace}");
                return null;
            }
        }

        private static string FindEventId(ILog logger, CalendarService service, string calendarId, string iCalUID)
        {
            EventsResource.ListRequest request = service.Events.List(calendarId);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.ICalUID = iCalUID;
            request.MaxResults = 1;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            try
            {
                Events events = request.Execute();
                string eventId = string.Empty;
                foreach (var item in events.Items)
                {
                    eventId = item.Id;
                    break;
                }
                return eventId;
            } catch (Exception e)
            {
                logger.Info($"Now found Activity Id:{iCalUID} CalendarId:{calendarId} in Google Calendar Error:{e.Message}{e.StackTrace}");
                return string.Empty;
            }
        }

        private static int GetBeforeMinutesModifiedDate()
        {
            return Int32.Parse(ConfigurationManager.AppSettings["BeforeMinutesModifiedDate"]);
        }
        private static string GetTimeZone()
        {
            return ConfigurationManager.AppSettings["TimeZone"];
        }
        private static string GetDefaultCalendarId()
        {
            return ConfigurationManager.AppSettings["DefaultCalendarId"];
        }
        private static bool IsFacebookCalendarType()
        {
            return ConfigurationManager.AppSettings["CalendarType"].Equals("Facebook");
        }
        private static bool CheckMinutesModifiedDate()
        {
            return ConfigurationManager.AppSettings["CheckMinutesModifiedDate"].Equals("Y");
        }
        
        private static bool GetEventReminderEmail()
        {
            return ConfigurationManager.AppSettings["EventReminderEmail"].Equals("Y");
        }

        private static bool UsedDefaultCalendarId()
        {
            return ConfigurationManager.AppSettings["UsedDefaultCalendarId"].Equals("Y");
        }
        private static int GetEventReminderEmailMinutes()
        {
            return Int32.Parse(ConfigurationManager.AppSettings["EventReminderEmailMinutes"]);
        }

        private static bool GetEventReminderSMS()
        {
            return ConfigurationManager.AppSettings["EventReminderSMS"].Equals("Y");
        }

        private static int GetEventReminderSMSMinutes()
        {
            return Int32.Parse(ConfigurationManager.AppSettings["EventReminderSMSMinutes"]);
        }

        private static string GetPriorityColor(string priority)
        {
            if (priority.Equals("ต่ำ"))
                return "8"; // "black";
            else if (priority.Equals("ปานกลาง"))
                return "7"; //"blue";
            else
                return "6"; // "red";

        }
        private static string GetStatus(string status)
        {
            if (status.Equals("กำลังดำเนินการ") || status.Equals("ยังไม่ได้เริ่ม"))
                return "tentative";
            else if (status.Equals("เสร็จสิ้น"))
                return "confirmed";
            else
                return "cancelled";

        }

        private static void CreateNewEvent(CalendarService service, string calendarId, SmEmployee employee, CrmActivityDto activity, List<EventAttendee> eventAttendees, ILog logger, string notes)
        {

            var overrides = new List<EventReminder>();
            if (GetEventReminderEmail())
            {
                overrides.Add(new EventReminder { Method = "email", Minutes = GetEventReminderEmailMinutes() });
            }
            if (GetEventReminderSMS())
            {
                overrides.Add(new EventReminder { Method = "sms", Minutes = GetEventReminderSMSMinutes() });
            }


            string summary = $"{activity.Topic} [ความสำคัญ:{activity.PriorityEnumName}][สถานะ:{activity.StatusEnumName}] ".Replace(System.Environment.NewLine, string.Empty);
            var endDateTime = activity.EndDateTime == null ? activity.StartDateTime : activity.EndDateTime.Value;
            Event newEvent = new Event()
            {
                ICalUID = activity.ActivityId,
                Summary = summary,
                Location = activity.RelateActName,
                Description = activity.Detail,
                Created = activity.ModifiedDate,
                ColorId = GetPriorityColor(activity.PriorityEnumName),
                Status = GetStatus(activity.StatusEnumName),
                Start = new EventDateTime()
                {
                    DateTime = activity.StartDateTime,
                    TimeZone = GetTimeZone(),
                },
                End = new EventDateTime()
                {
                    DateTime = endDateTime,
                    TimeZone = GetTimeZone(),
                },
                Attendees = eventAttendees,
                CreatedRaw = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo),
                Visibility = "default",
                Organizer = new Event.OrganizerData
                {
                    Email = employee.Facebook,
                    DisplayName = $"{employee.EmpFirstName} {employee.EmpLastName}",
                },
                Reminders = new Event.RemindersData()
                {
                    UseDefault = false,
                    Overrides = overrides
                },
                ConferenceData = new ConferenceData
                {
                    Notes = notes,
                },
                AnyoneCanAddSelf = true,
            };


            EventsResource.InsertRequest insertRequest = service.Events.Insert(newEvent, calendarId);
            try
            {
                Event createdEvent = insertRequest.Execute();
                logger.Info($"CalendarId:{calendarId} Event Id:{newEvent.ICalUID} has been inserted successful.");
            }
            catch (Exception e)
            {
                logger.Error($"Insert Event Id:{newEvent.ICalUID} CalendarId:{calendarId}  Error:{e.Message}{e.StackTrace}");
            }

        }

        private static string GetNotes(IActivityService activityService, string activityId)
        {
            string description = string.Empty;
            List<string> notes = activityService.GetFacilities(activityId);
            notes.AddRange(activityService.GetNotes(activityId));
            foreach (var note in notes)
            {
                description += note;
            }
            return description;
        }
        private static void CreateNewCalendar(ILog logger, CalendarService service, SmEmployee smEmployee)
        {
            string calendarId = IsFacebookCalendarType() ? smEmployee.Facebook : smEmployee.Email;
            var newCalendar = new Google.Apis.Calendar.v3.Data.Calendar()
            {
                Summary = $"[{smEmployee.EmpNo}] {smEmployee.EmpFirstName} {smEmployee.EmpLastName} ({smEmployee.EmpNickName})",
                Location = $"{smEmployee.AddrLine} แขวง{smEmployee.SubDistrict} เขต{smEmployee.District} จังหวัด{smEmployee.Province} {smEmployee.PostCode}",
                TimeZone = GetTimeZone(),
                Id = calendarId
            };

            CalendarsResource.InsertRequest insertCalendar = service.Calendars.Insert(newCalendar);
            try
            {
                insertCalendar.Execute();
            }
            catch (Exception e)
            {
                logger.Error($"Insert Calendar:{smEmployee.Email} Error:{e.Message}{e.StackTrace}");
            }
        }

        private static Google.Apis.Calendar.v3.Data.Calendar GetCalendar(ILog logger,CalendarService service, string calendarId)
        {
            try
            {
                return service.Calendars.Get(calendarId).Execute();
            } catch (Exception e)
            {
                logger.Info($"System don't found Calendar: {calendarId} Message:{e.Message}");
                return null;
            }
        }


        private static void UpdateNewEvent(CalendarService service, string calendarId,SmEmployee employee, CrmActivityDto activity, List<EventAttendee> eventAttendees, string eventId, ILog logger, string notes)
        {
            var overrides = new List<EventReminder>();
            if (GetEventReminderEmail())
            {
                overrides.Add(new EventReminder { Method = "email", Minutes = GetEventReminderEmailMinutes() });
            }
            if (GetEventReminderSMS())
            {
                overrides.Add(new EventReminder { Method = "sms", Minutes = GetEventReminderSMSMinutes() });
            }


            string summary = $"{activity.Topic} [ความสำคัญ:{activity.PriorityEnumName}][สถานะ:{activity.StatusEnumName}] ".Replace(System.Environment.NewLine, string.Empty);
            var endDateTime = activity.EndDateTime == null ? activity.StartDateTime : activity.EndDateTime.Value;
              Event newEvent = new Event()
            {
                ICalUID = activity.ActivityId,
                Summary = summary,
                Location = activity.RelateActName,
                Description = activity.Detail,
                Created = activity.ModifiedDate,
                ColorId = GetPriorityColor(activity.PriorityEnumName),
                Status = GetStatus(activity.StatusEnumName),
                Start = new EventDateTime()
                {
                    DateTime = activity.StartDateTime,
                    TimeZone = GetTimeZone(),
                },
                End = new EventDateTime()
                {
                    DateTime = endDateTime,
                    TimeZone = GetTimeZone(),
                },

                
                Updated = DateTime.Now,
                Attendees = eventAttendees,
                Visibility = "default",
                Organizer = new Event.OrganizerData
                {
                    Email = employee.Facebook,
                    DisplayName = $"{employee.EmpFirstName} {employee.EmpLastName}",
                },
                Reminders = new Event.RemindersData()
                {
                    UseDefault = false,
                    Overrides = overrides
                },
                ConferenceData = new ConferenceData
                {
                    Notes = notes
                }
            };

            EventsResource.UpdateRequest updateRequest = service.Events.Update(newEvent, calendarId, eventId);
            try
            {
                Event updatedEvent = updateRequest.Execute();
                logger.Info($"CalendarId:{calendarId} Event Id:{newEvent.ICalUID} has been updated successful.");
            }
            catch (Exception e)
            {
                logger.Error($"Update Event Id:{newEvent.ICalUID} CalendarId:{calendarId}  Error:{e.Message}{e.StackTrace}");
            }

        }



    }
}



