using SPWebHookSrv.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPWebHookSrv.Helpers
{
    public class ApiException : Exception
    {
        public ApiException(string message) : base(message)
        {

        }
        public ApiError ApiError { get; set; }

    }
}