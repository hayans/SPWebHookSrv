using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SPWebHookSrv.Helpers;
using SPWebHookSrv.Models;

namespace SPWebHookSrv.Authentication
{
    public class APIUserAuthenticator: IAuthenticator
    {
        private string _grantType;
        private string _clientId;
        private string _clientSecret;
        private string _resource;
        private string _url;
        private TokenRequest _tokenRequest;

        public APIUserAuthenticator(TokenRequest tokenRequest)
        {
            _tokenRequest = tokenRequest;
        }
        public APIUserAuthenticator(string grant_type, string client_id, string client_secret, string resource, string url)
        {
            _grantType = grant_type;
            _clientId = client_id;
            _clientSecret = client_secret;
            _resource = resource;
            _url = url;

        }

        public string AccessToken {
            get
            {
                //if (_tokenRequest == null || DateTime.Now > TokenExpiration)
                //{
                //    GetNewToken();
                //}
                return _tokenRequest.AccessToken;
            }
        }
        public DateTime TokenExpiration => LastTokenRequest.AddSeconds(_tokenRequest.ExpiresIn);
        public DateTime LastTokenRequest { get; set; }

        public void GetNewToken()
        {
            HTTPRequestor req = new HTTPRequestor();

            System.Collections.Generic.Dictionary<string, string> dic = new System.Collections.Generic.Dictionary<string, string>();


               dic.Add("grant_type", _grantType);
            dic.Add("client_id", _clientId);
            dic.Add("client_secret", _clientSecret);
            dic.Add("resource", _resource);

            var resp = req.Post<TokenRequest>(_url, "", dic);

            _tokenRequest = resp.Result;
            LastTokenRequest = DateTime.Now;
        }
    }
}