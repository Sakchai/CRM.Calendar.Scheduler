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

            var builder = new ContainerBuilder();
            //data layer
            builder.RegisterType<DataProviderManager>().As<IDataProviderManager>().InstancePerDependency();
            builder.Register(context => context.Resolve<IDataProviderManager>().DataProvider).As<IFAADDataProvider>().InstancePerDependency();

            //repositories
            builder.RegisterGeneric(typeof(EntityRepository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();

            builder.RegisterType<EmployeeService>().As<IEmployeeService>().InstancePerLifetimeScope();
            builder.RegisterType<ActivityService>().As<IActivityService>().InstancePerLifetimeScope();

            UserCredential credential;

            using (var stream =
                new FileStream(@"credentials\credentials.json", FileMode.Open, FileAccess.Read))
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

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;


            // List events.
            Events events = request.Execute();
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (string.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                }
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }

            


            using (var container = builder.Build())
            {
                var employeeService = container.Resolve<IEmployeeService>();
                var activityService = container.Resolve<IActivityService>();
                var employees = employeeService.GetEmployeesList();
                foreach (var item in employees)
                {
                    var activities = activityService.GetActivityByOwner(item.EmpId);
                    if (IsFacebookCalendarType()) {
                        foreach (var act in activities)
                        {
                            var emails = activityService.GetEmailsFollowID(act.ActivityId);
                            CreateNewEvent(service, item.Facebook, act, emails);
                        }
                    }
                    else
                    {
                        foreach (var act in activities)
                        {
                            var emails = activityService.GetEmailsFollowID(act.ActivityId);
                            CreateNewEvent(service, item.Email, act, emails);
                        }
                    }
                }
            }

            Console.Read();

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

        private static void CreateNewEvent(CalendarService service,string calendarId, CrmActivity activity, List<EventAttendee> eventAttendees )
        {
            //new EventAttendee[] {
            //        new EventAttendee() { Email = "lpage@example.com" },
            //        new EventAttendee() { Email = "sbrin@example.com" },
            //    }
            //new EventReminder[] {
            //            new EventReminder() { Method = "email", Minutes = 24 * 60 },
            //            new EventReminder() { Method = "sms", Minutes = 10 },
            //        }

            var overrides = new List<EventReminder>();
            if (GetEventReminderEmail())
            {
                overrides.Add(new EventReminder { Method = "email", Minutes = GetEventReminderEmailMinutes() });
            }
            if (GetEventReminderSMS())
            {
                overrides.Add(new EventReminder { Method = "sms", Minutes = GetEventReminderSMSMinutes() });
            }
            string summary = $" {activity.Topic} [ความสำคัญ:{activity.PriorityEnumName}][สถานะ:{activity.StatusEnumName}] ";
            Event newEvent = new Event()
            {
                Summary = summary,
                Location = activity.RelateActName,
                Description = activity.Detail,
                Start = new EventDateTime()
                {
                    DateTime = DateTime.Now.AddSeconds(-60),
                    TimeZone = GetTimeZone(),
                },
                End = new EventDateTime()
                {
                    DateTime = DateTime.Now,
                    TimeZone = GetTimeZone(),
                },
                Attendees = eventAttendees,
                Reminders = new Event.RemindersData()
                {
                    UseDefault = false,
                    Overrides = overrides
                }
            };

            //String calendarId = "primary";
            EventsResource.InsertRequest insertRequest = service.Events.Insert(newEvent, calendarId);
            Event createdEvent = insertRequest.Execute();
            Console.WriteLine("Event created: {0}", createdEvent.HtmlLink);
        }
    }
}
// [END calendar_quickstart]
