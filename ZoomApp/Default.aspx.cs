using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ZoomApp
{
    public partial class _Default : Page
    {
        protected String ApiKey = Creds.ApiKey;
        protected String Sig = null;
        protected String MeetingNumber = null;
        protected String UserName = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            ZoomApiClient client = new ZoomApiClient(Creds.ApiKey, Creds.Secret, "Pool");
            string meetingNumber = client.GetNewMeetingNumber("xOgTIVD2SxGpOrXS4kF_0A", "BlaBla", "BlaBla");
            this.Sig = MeetingUtils.GenerateSignature(Creds.ApiKey, Creds.Secret, meetingNumber, "0");
  
            this.MeetingNumber = meetingNumber;
            this.UserName = "Bla Bla";
        }
    }
}