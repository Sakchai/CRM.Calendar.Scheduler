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
                    var activities = activityService.GetActivityByOwner(item.EmpId, GetBeforeMinuitesModifiedDate());

                    foreach (var activity in activities)
                    {
                        string calendarId = string.Empty;
                        if (IsFacebookCalendarType())
                            calendarId = item.Facebook;
                        else
                            calendarId = item.Email;
                        //calendarId = "primary";
                        var followers = activityService.GetEmailsFollowID(activity.ActivityId);
                        string eventId = FindEventId(service, calendarId, activity.ActivityId);
                        if (!string.IsNullOrEmpty(eventId))
                            UpdateNewEvent(service, calendarId, activity, followers, eventId, logger);
                        else
                            CreateNewEvent(service, calendarId, activity, followers, logger);
                    }

                }
            }

        }


        private static string FindEventId(CalendarService service, string calendarId, string iCalUID)
        {
            EventsResource.ListRequest request = service.Events.List(calendarId);
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.ICalUID = iCalUID;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            string eventId = string.Empty;
            foreach (var item in events.Items)
            {
                eventId = item.Id;
                break;
            }
            return eventId;

        }

        private static int GetBeforeMinuitesModifiedDate()
        {
            return Int32.Parse(ConfigurationManager.AppSettings["BeforeMinuitesModifiedDate"]);
        }
        private static string GetTimeZone()
        {
            return ConfigurationManager.AppSettings["TimeZone"];
        }
        private static bool IsFacebookCalendarType()
        {
            return ConfigurationManager.AppSettings["CalendarType"].Equals("Facebook");
        }

        private static bool GetEventReminderEmail()
        {
            return ConfigurationManager.AppSettings["EventReminderEmail"].Equals("Y");
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

        private static void CreateNewEvent(CalendarService service, string calendarId, CrmActivityDto activity, List<EventAttendee> eventAttendees, ILog logger)
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
                    DateTime = activity.EndDateTime,
                    TimeZone = GetTimeZone(),
                },
                Attendees = eventAttendees,
                Reminders = new Event.RemindersData()
                {
                    UseDefault = false,
                    Overrides = overrides
                }
            };

            EventsResource.InsertRequest insertRequest = service.Events.Insert(newEvent, calendarId);
            try
            {
                Event createdEvent = insertRequest.Execute();
                logger.Info($"Activity Id:{createdEvent.Id}");
            }
            catch (Exception e)
            {
                logger.Error($"Insert Activity:{newEvent.ICalUID} CalendarId:{calendarId}  Error:{e.Message}{e.StackTrace}");
            }

        }

        private static void UpdateNewEvent(CalendarService service, string calendarId, CrmActivityDto activity, List<EventAttendee> eventAttendees, string eventId, ILog logger)
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
                    DateTime = activity.EndDateTime,
                    TimeZone = GetTimeZone(),
                },
                Attendees = eventAttendees,
                Reminders = new Event.RemindersData()
                {
                    UseDefault = false,
                    Overrides = overrides
                }
            };

            EventsResource.UpdateRequest updateRequest = service.Events.Update(newEvent, calendarId, eventId);
            try
            {
                Event updatedEvent = updateRequest.Execute();
                logger.Info($"Activity Id:{updatedEvent.Id}");
            }
            catch (Exception e)
            {
                logger.Error($"Insert Activity:{newEvent.ICalUID} CalendarId:{calendarId}  Error:{e.Message}{e.StackTrace}");
            }

        }


        private static void DeleteNewEvent(CalendarService service, string calendarId, CrmActivityDto activity, string eventId, ILog logger)
        {
            EventsResource.DeleteRequest deleteRequest = service.Events.Delete(calendarId, eventId);
            try
            {
                deleteRequest.Execute();
            }
            catch (Exception e)
            {
                logger.Error($"Delete Activity Id:{activity.ActivityId} CalendarId:{calendarId}  Error:{e.Message}{e.StackTrace}");
            }
        }
    }
}



