using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZoomApp
{
    public class ZoomApiClient
    {
        private string token = null;

        public ZoomApiClient(string apiKey, string secret)
        {
            var payload = new Dictionary<string, object>
            {
                { "iss", apiKey },
                { "exp", ExpiryToTimestamp(DateTime.UtcNow.ToUniversalTime(), 1) }
            };

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            this.token = encoder.Encode(payload, secret);
        }

        public string GetNewMeetingId(string topic, string agenda)
        {
            ZoomMeeting meeting = CreateMeeting(topic, agenda);
            
            if (meeting != null)
            {
                return meeting.id;
            } 
            else
            {
                return null;
            }
        }

        private ZoomMeeting CreateMeeting(string topic, string agenda)
        {
            ZoomUsers users = GetUsers();

            if (users != null)
            {
                IRestResponse res = MakeMeetingCall(users.users[0].id, topic, agenda);
                if (res.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    return JsonConvert.DeserializeObject<ZoomMeeting>(res.Content);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private ZoomUsers GetUsers()
        {
            IRestResponse res = MakeGetUsersCall();
 
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ZoomUsers>(res.Content);
            }
            else
            {
                return null;
            }
        }

        private IRestResponse MakeCreateUserCall(string email, string name, string lastName)
        {
            var payload = new Dictionary<string, object>
            {
                { "action", "create" },
                { "user_info", new Dictionary<string, object>
                    {
                        { "email", email },
                        { "type", 1 },
                        { "first_name", name },
                        { "last_name", lastName }
                    }
                }
            };
 
            return MakeCall("https://api.zoom.us/v2/users", payload);
        }

        private IRestResponse MakeMeetingCall(string userId, string topic, string agenda)
        {         
            var payload = new Dictionary<string, object>
            {
                { "topic", topic },
                { "type", 2 },
                { "agenda", agenda },
                { "settings", new Dictionary<string, object>
                    {
                        { "join_before_host", true }
                    }
                }
            };
            return MakeCall("https://api.zoom.us/v2/users/" + userId + "/meetings", payload);
        }

        private IRestResponse MakeGetUsersCall()
        {
            var client = new RestClient("https://api.zoom.us/v2/users?status=active&page_size=10&page_number=1");
            var request = new RestRequest(Method.GET);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", "Bearer " + this.token);
            return client.Execute(request);
        }

        private IRestResponse MakeCall(string url, object obj)
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", "Bearer " + this.token);
            request.AddJsonBody(obj);
            return client.Execute(request);
        }

        private static long ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Convert.ToInt64(Math.Floor(diff.TotalSeconds));
        }

        private static long ExpiryToTimestamp(DateTime value, int days)
        {
            long epoch = ConvertToUnixTimestamp(DateTime.UtcNow.ToUniversalTime());
            return ConvertToUnixTimestamp(DateTime.UtcNow.ToUniversalTime()) + (86400 * days);
        }

        public class ZoomUsers
        {
            public int page_count;
            public int page_number;
            public int page_size;
            public int total_records;
            public ZoomUser[] users;

            public class ZoomUser
            {
                public string id;
                public string first_name;
                public string last_name;
                public string email;
                public int type;
            }
        }

        public class ZoomMeeting
        {
            public string uuid;
            public string id;
            public string start_url;
        }
    }
}