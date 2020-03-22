using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static ZoomApp.ZoomApiClient;

namespace ZoomApp
{
    public partial class _Default : Page
    {
        protected String ApiKey = Creds.ApiKey;
        protected String Sig = null;
        protected String MeetingNumber = null;
        protected String UserName = null;
        protected String MeetingUrl = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            ZoomApiClient client = new ZoomApiClient(Creds.ApiKey, Creds.Secret, "Pool");
            List<ZoomUsers.ZoomUser> usersPool = client.GetUsersPool();
            ZoomMeetings.ZoomMeeting meeting = client.GetNewMeeting(usersPool[0].id, "BlaBla", "BlaBla", true);
            this.Sig = MeetingUtils.GenerateSignature(Creds.ApiKey, Creds.Secret, meeting.id, MeetingUtils.AtendeeRole);
  
            this.MeetingNumber = meeting.id;
            this.UserName = "Bla Bla";
            this.MeetingUrl = meeting.join_url;
        }
    }
}