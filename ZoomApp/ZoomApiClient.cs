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
        private string poolDept = null;

        public ZoomApiClient(string apiKey, string secret, string poolDept)
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
            this.poolDept = poolDept;
        }

        public List<String> GetUsersPoolIds()
        {
            ZoomUsers users = GetUsers();
            List<String> ids = new List<string>();

            foreach (ZoomUsers.ZoomUser user in users.users)
            {
                if (user.dept == this.poolDept)
                {
                    ids.Add(user.id);
                }
            }

            return ids;
        }

        public string GetNewMeetingNumber(string userId, string topic, string agenda)
        {
            ZoomMeetings.ZoomMeeting meeting = CreateMeeting(userId, topic, agenda);
            
            if (meeting != null)
            {
                return meeting.id;
            } 
            else
            {
                return null;
            }
        }

        public bool EndMeeting(string meetingNumber)
        {
            IRestResponse res = MakeMeetingUpdateCall(meetingNumber);

            if (res.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private ZoomMeetings.ZoomMeeting CreateMeeting(string userId, string topic, string agenda)
        {
            IRestResponse res = MakeMeetingCall(userId, topic, agenda);
            if (res.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return JsonConvert.DeserializeObject<ZoomMeetings.ZoomMeeting>(res.Content);
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

        public ZoomMeetings GetLiveMeetings(string userId)
        {
            IRestResponse res = MakeListMeetingsCall(userId);

            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ZoomMeetings>(res.Content);
            }
            else
            {
                return null;
            }
        }

        private IRestResponse MakeMeetingUpdateCall(string meetingId)
        {
            var payload = new Dictionary<string, object>
            {
                { "action", "end" },
            };

            return MakeCall("https://api.zoom.us/v2/meetings/" + meetingId + "/status", payload);
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
            var client = new RestClient("https://api.zoom.us/v2/users?status=active&page_size=300&page_number=1");
            var request = new RestRequest(Method.GET);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", "Bearer " + this.token);
 
            return client.Execute(request);
        }

        private IRestResponse MakeListMeetingsCall(string userId)
        {
            var client = new RestClient("https://api.zoom.us/v2/users/" + userId + "/meetings?page_number=1&page_size=30&type=live");
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
                public string dept;
            }
        }

        public class ZoomMeetings
        {
            public int page_count;
            public int page_number;
            public int page_size;
            public int total_records;
            public ZoomMeeting[] meetings;

            public class ZoomMeeting
            {
                public string uuid;
                public string id;
                public string start_url;
            }
        }
    }
}