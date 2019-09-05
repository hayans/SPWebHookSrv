﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using SPWebHookSrv.Models;
//
//    var iAuthenticator = IAuthenticator.FromJson(jsonString);

namespace SPWebHookSrv.Authentication
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public interface IAuthenticator
    {

        string AccessToken { get; }
        DateTime TokenExpiration { get; }
        DateTime LastTokenRequest { get; set; }

        //string TokenType { get; set; }

        //long ExpiresIn { get; set; }

        //long NotBefore { get; set; }

        //long ExpiresOn { get; set; }

        //string Resource { get; set; }

        //string AccessToken { get; set; }
    }


}