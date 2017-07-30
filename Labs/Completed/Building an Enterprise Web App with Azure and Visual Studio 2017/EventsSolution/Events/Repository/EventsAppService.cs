using Events.ApplicationService.Validators;
using Events.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using StackExchange.Redis;
using Events.RedisExtensions;


namespace Events.ApplicationService
{
    public class EventsAppService : IEventsAppService
    {
        private string _currentUserName = string.Empty;

        private IEventsRepository _repository;

        public EventsAppService()
        {
        }

        public EventsAppService(string userName)
        {
            this._currentUserName = userName;
            this._repository = new EventsRepository(userName);
        }

        public EventsAppService(IEventsRepository repository, string userName)
        {
        }

        public Event GetEvent(int eventId)
        {
            return this._repository.GetEvent(eventId);
        }

        public IEnumerable<Event> UpcomingEvents(int count)
        {
            return this._repository.UpcomingEvents(count);
        }

        public IEnumerable<Event> GetUserEvents(string activeDirectoryId)
        {
            IDatabase cache = MvcApplication.RedisCache.GetDatabase();
            var events = cache.Get<IEnumerable<Event>>(activeDirectoryId);
            if (events == null)
            {
                events = this._repository.GetUserEvents(activeDirectoryId);
                cache.Set(activeDirectoryId, events, TimeSpan.FromMinutes(5));
            }
            return events;
        }

        public Event CreateEvent(Event eventData)
        {
            if (EventsValidator.ValidateEventData(eventData))
            {
                return this._repository.CreateEvent(eventData);
            }
            else
            {
                throw new ApplicationException("Invalid Data");
            }
        }

        public bool RegisterUser(string activeDirectoryId, int eventId)
        {
            return this._repository.RegisterUser(activeDirectoryId, eventId);
        }

        async public Task<IEnumerable<ActiveDirectoryUser>> ActiveDirectoryUsersAsync()
        {
            List<ActiveDirectoryUser> activeDirectoryUsers = new List<ActiveDirectoryUser>();
            ActiveDirectoryClient client = Helpers.AuthenticationHelper.GetActiveDirectoryClient();
            IPagedCollection<IUser> pagedCollection = await client.Users.ExecuteAsync();
            if (pagedCollection != null)
            {
                do
                {
                    List<IUser> usersList = pagedCollection.CurrentPage.ToList();
                    foreach (IUser user in usersList)
                    {
                        ActiveDirectoryUser adUser = new Models.ActiveDirectoryUser();
                        adUser.ActiveDirectoryId = user.ObjectId;
                        adUser.FullName = user.DisplayName;
                        adUser.Position = user.JobTitle;
                        adUser.Location = user.City + ", " + user.State;
                        adUser.ImageUrl = "/Users/ShowThumbnail/" + user.ObjectId;
                        adUser.ObjectId = user.ObjectId;
                        activeDirectoryUsers.Add(adUser);
                    }
                    pagedCollection = await pagedCollection.GetNextPageAsync();
                } while (pagedCollection != null);
            }

            return activeDirectoryUsers;
        }


    }
}