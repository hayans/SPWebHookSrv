using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPWebHookSrv.Helpers
{
    public class SPHelperLib
    {

        private static string AccessToken { get; set; }

        private void SetToken()
        {

            var sharePointUrl = new Uri("https://docufan.sharepoint.com/sites/DocuSign");
            string SHAREPOINT_PID = "00000003-0000-0ff1-ce00-000000000000";

            string sharePointRealm = TokenHelper.GetRealmFromTargetUrl(sharePointUrl);
            AccessToken = TokenHelper.GetAppOnlyAccessToken(SHAREPOINT_PID, sharePointUrl.Authority, sharePointRealm).AccessToken;

        }
    }
}