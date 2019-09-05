using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using SPWebHookSrv.Authentication;
using SPWebHookSrv.Models;

namespace SPWebHookSrv.Helpers
{
    public class SharePointService
    {
        private HTTPRequestor requestor;

        public SharePointService(IAuthenticator auth)
        {
            requestor = new HTTPRequestor(auth);
        }
      
        internal string GetObjectAPIUrl => WebConfigurationManager.AppSettings.Get("SPURIAPI");
      
        
        public async Task<D> CreateFolder(string path, string FOLDER_PRE, string folderName)
        {
            string stringData = "{'__metadata': { 'type': 'SP.Folder' }, 'ServerRelativeUrl': '" + FOLDER_PRE + "_" + folderName + "'}";

           

            return await requestor.Post<D>($"{GetObjectAPIUrl}/_api/web/GetFolderByServerRelativeUrl('{path}')/Folders", stringData);
        }
    }
}