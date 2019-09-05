using Newtonsoft.Json;
using SPWebHookSrv.Authentication;
using SPWebHookSrv.Helpers;
using SPWebHookSrv.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Xml;
using log4net;

namespace SPWebHookSrv.Controllers
{
    public class SPWebHookSrvController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SPWebHookSrvController));

        static string URI = WebConfigurationManager.AppSettings.Get("SPURI");
        string SHAREPOINT_PID = WebConfigurationManager.AppSettings.Get("SPSID"); //"00000003-0000-0ff1-ce00-000000000000";
        string FOLDER_PRE = WebConfigurationManager.AppSettings.Get("DSFPRE");
    //    string SPSCPATH = WebConfigurationManager.AppSettings.Get("SPSCPATH");

     //   private SharePointService svc;

        Uri sharePointUrl = new Uri(URI);
        
        public APIUserAuthenticator getapiAuth()
        {
            APIUserAuthenticator auth = new APIUserAuthenticator("client_credentials", "c14c4618-f753-4693-bcd4-f236ad6d6dd3@9684067a-6190-4cbc-8a17-476c83670507", "cGO5UZrfAePpUjJnaayPoM+c+exzPK95AXCofn/iM0E=", "00000003-0000-0ff1-ce00-000000000000/docufan.sharepoint.com@9684067a-6190-4cbc-8a17-476c83670507", "https://accounts.accesscontrol.windows.net/9684067a-6190-4cbc-8a17-476c83670507/tokens/OAuth/2");
            return auth;
        }

        public SharePointService GetSharePointService(APIUserAuthenticator auth)
        {
           // APIUserAuthenticator auth = getapiAuth();
            return new SharePointService(auth);

        }

        public async Task<T> PostFile<T>(string url, string accessToken, Stream file, string fileName, Dictionary<string, string> headers = null, int retrycount = 0)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {accessToken}");
            MultipartFormDataContent form = new MultipartFormDataContent();
            httpClient.Timeout = new TimeSpan(0, 30, 0);
            form.Add(new StreamContent(file), fileName, fileName);
           
            HttpResponseMessage response;

            try
            {
                response = await httpClient.PostAsync(url, form);
            }
            catch (HttpRequestException ex)
            {
                throw ex;
            }
           
            response.EnsureSuccessStatusCode();
            httpClient.Dispose();
            string sd = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(sd, JSONService.Settings);
        }

        public async Task<string> UploadDocCall(string url, string accessToken, string body, Dictionary<string, string> headers, string method)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {accessToken}");
                
                foreach (var header in headers)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                var request = new HttpRequestMessage(new HttpMethod(method), url);
                if (!string.IsNullOrEmpty(body))
                {

                    request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                }
                httpClient.Timeout = new TimeSpan(0, 0, 30, 0);
                HttpResponseMessage response = new HttpResponseMessage();
                
                string res = "";
                try
                {
                    response = await httpClient.SendAsync(request);
                    res = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                
                if (!response.IsSuccessStatusCode)
                {
                    ApiError ae = JsonConvert.DeserializeObject<ApiError>(res, JSONService.Settings);
                    throw new ApiException(ae.Error.Code) { ApiError = ae };
                }
                return res;
            }


        }

        private bool UploadSignedDocument(string appOnlyAccessToken, string repository, string folderName, string fileName, string documentId, byte[] byteFile)
        {
            HttpWebResponse response = null;
            Folder jsonC = null;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(sharePointUrl + "/_api/web/GetFolderByServerRelativeUrl('" + repository + "/DocuSign/" + FOLDER_PRE + "_" + folderName + "')/Files/add(url='" + fileName + "_" + documentId + ".pdf',overwrite=true)");
            request.Method = "POST";
            request.Headers.Add("binaryStringRequestBody", "true");
            request.Headers.Add("Authorization", "Bearer " + appOnlyAccessToken);
            request.GetRequestStream().Write(byteFile, 0, byteFile.Length);
            request.Accept = "application/json;odata=verbose";

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                Stream sr = response.GetResponseStream();
                StreamReader read = new StreamReader(sr);
                Char[] vrb = new Char[256];
                // Read 256 charcters at a time.
                int count = read.Read(vrb, 0, 256);
                String json = "";

                while (count > 0)
                {
                    // Dump the 256 characters on a string and display the string onto the console.
                    String str = new String(vrb, 0, count);
                    json = json + str;
                    count = read.Read(vrb, 0, 256);
                }

                jsonC = JsonConvert.DeserializeObject<Folder>(json, JSONService.Settings);


            }
            catch (System.Net.WebException ex)
            {
                response = (HttpWebResponse)ex.Response;

                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound: // 404
                        return false;

                    case HttpStatusCode.InternalServerError: // 500
                        return false;

                    default:
                        throw;
                }
            }

            return jsonC.D.Exists;




        }

        private bool FolderExist(string appOnlyAccessToken, string repository, string folderName)
        {
            HttpWebResponse response = null;
            Folder jsonC = null;
            // Create Folder
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(sharePointUrl + "/_api/web/GetFolderByServerRelativeUrl('" + repository + "/DocuSign/" + FOLDER_PRE + "_" + folderName + "')");
            request.Method = "GET";
            request.Headers.Add("Authorization", "Bearer " + appOnlyAccessToken);
            request.ContentType = "application/json;odata=verbose";
            request.Accept = "application/json;odata=verbose";

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                Stream sr = response.GetResponseStream();
                StreamReader read = new StreamReader(sr);
                Char[] vrb = new Char[256];
                // Read 256 charcters at a time.
                int count = read.Read(vrb, 0, 256);
                String json = "";

                while (count > 0)
                {
                    // Dump the 256 characters on a string and display the string onto the console.
                    String str = new String(vrb, 0, count);
                    json = json + str;
                    count = read.Read(vrb, 0, 256);
                }

                jsonC = JsonConvert.DeserializeObject<Folder>(json, JSONService.Settings);


            }
            catch (System.Net.WebException ex)
            {
                response = (HttpWebResponse)ex.Response;

                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound: // 404
                        return false;

                    case HttpStatusCode.InternalServerError: // 500
                        return false;

                    default:
                        throw;
                }
            }

                return jsonC.D.Exists;


            }

        private bool CreateFolder(string appOnlyAccessToken, string repository, string folderName)
        {
            HttpWebResponse response = null;
            Folder jsonC = null;
            // Create Folder
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(sharePointUrl + "/_api/web/GetFolderByServerRelativeUrl('" + repository + "/DocuSign')/Folders");
            request.Method = "POST";
            request.Headers.Add("Authorization", "Bearer " + appOnlyAccessToken);
            request.ContentType = "application/json;odata=verbose";
            request.Accept = "application/json;odata=verbose";

            string stringData = "{'__metadata': { 'type': 'SP.Folder' }, 'Ser" +
                "verRelativeUrl': '" + FOLDER_PRE + "_" + folderName + "'}";
            request.ContentLength = stringData.Length;
            StreamWriter writer = new StreamWriter(request.GetRequestStream());
            writer.Write(stringData);
            writer.Flush();

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                Stream sr = response.GetResponseStream();
                StreamReader read = new StreamReader(sr);
                Char[] vrb = new Char[256];
                // Read 256 charcters at a time.
                int count = read.Read(vrb, 0, 256);
                String json = "";

                while (count > 0)
                {
                    // Dump the 256 characters on a string and display the string onto the console.
                    String str = new String(vrb, 0, count);
                    json = json + str;
                    count = read.Read(vrb, 0, 256);
                }

                jsonC = JsonConvert.DeserializeObject<Folder>(json, JSONService.Settings);


            }
            catch (System.Net.WebException ex)
            {
                response = (HttpWebResponse)ex.Response;

                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound: // 404
                        return false;

                    case HttpStatusCode.InternalServerError: // 500
                        return false;

                    default:
                        throw;
                }
            }

            return jsonC.D.Exists;

        }

        //private object CreateRequest(string folderPath)
        //{
        //    var type = new { type = "SP.Folder" };
        //    var request = new { __metadata = type, ServerRelativeUrl = folderPath };
        //    return request;
        //}

        //private async Task CreateFolder(HttpClient client, string digest)
        //{
        //    client.DefaultRequestHeaders.Add("X-RequestDigest", digest);
        //    var request = CreateRequest("foo");
        //    string json = JsonConvert.SerializeObject(request);
        //    StringContent strContent = new StringContent(json);
        //    strContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=verbose");
        //    HttpResponseMessage response = await client.PostAsync("_api/web/getfolderbyserverrelativeurl('test/test123/')/folders", strContent);
        //    //response.EnsureSuccessStatusCode();
        //    if (response.IsSuccessStatusCode)
        //    {
        //        string content = await response.Content.ReadAsStringAsync();
        //        Console.WriteLine(content);
        //    }
        //    else
        //    {
        //        Console.WriteLine(response.StatusCode);
        //        Console.WriteLine(response.ReasonPhrase);
        //        string content = await response.Content.ReadAsStringAsync();
        //        Console.WriteLine(content);
        //    }
        //}

        public HttpResponseMessage Post(HttpRequestMessage request)
        {
            string site = null;
            string repository = null;
            string templateName = null;
            String responseMessage = null;

            log4net.Config.XmlConfigurator.Configure();

            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(request.Content.ReadAsStreamAsync().Result);

            var mgr = new XmlNamespaceManager(xmldoc.NameTable);
            mgr.AddNamespace("a", "http://www.docusign.net/API/3.0");

            XmlNode envelopeStatus = xmldoc.SelectSingleNode("//a:EnvelopeStatus", mgr);
            XmlNode envelopeId = envelopeStatus.SelectSingleNode("//a:EnvelopeID", mgr);
            XmlNode status = envelopeStatus.SelectSingleNode("./a:Status", mgr);

            XmlNode userName = envelopeStatus.SelectSingleNode("./a:UserName", mgr);
            XmlNode signed = envelopeStatus.SelectSingleNode("./a:Signed", mgr);

            if (status.InnerText == "Completed")
            {
                // get Custom Fields
                XmlNode custFlds = envelopeStatus.SelectSingleNode("//a:CustomFields[a:CustomField]", mgr);
                foreach (XmlNode custFld in custFlds.ChildNodes)
                {
                    if (custFld.ChildNodes[0].InnerText.Equals("Site"))
                    {
                        site = custFld.ChildNodes[3].InnerText;
                    }

                    if (custFld.ChildNodes[0].InnerText.Equals("Repository"))
                    {
                        repository = custFld.ChildNodes[3].InnerText;
                    }

                    if (custFld.ChildNodes[0].InnerText.Equals("Template Name"))
                    {
                        templateName = custFld.ChildNodes[3].InnerText;
                    }
                }

                if (site.Equals("SharePoint"))
                {
                    try
                    {
                        //get access token
                        string sharePointRealm = TokenHelper.GetRealmFromTargetUrl(sharePointUrl);
                        var appOnlyAccessToken = TokenHelper.GetAppOnlyAccessToken(SHAREPOINT_PID, sharePointUrl.Authority, sharePointRealm).AccessToken;

                        //TokenRequest tokenRequest = new TokenRequest();
                        //tokenRequest.AccessToken = appOnlyAccessToken;
                        //APIUserAuthenticator auth = new APIUserAuthenticator(tokenRequest);

                        string fileName = null;
                        string folderName = DateTime.Today.Month + "_" + DateTime.Today.Year;

                        if (!FolderExist(appOnlyAccessToken, repository, folderName))
                        {
                            if (!CreateFolder(appOnlyAccessToken, repository, folderName))
                            {
                                //log error can not create folder
                                Log.Error("Unable to Create Folder :: " + folderName);
                            }
                        }

                        ////Check if the first day of the month to create the folder
                        //if (!DateTime.Now.Day.Equals("1"))
                        //{
                        //    // Create Folder
                        //    CreateFolder(appOnlyAccessToken);
                        //   // getapiAuth();
                        // //   svc = GetSharePointService(auth);
                        ////   D test = svc.CreateFolder("OSI/DocuSign", "TestWithNoel", "10_2019").Result;

                        //}

                        fileName = templateName + "_" + userName.InnerText.Replace(" ", "_") + "_" + signed.InnerText.Substring(0, 10).Replace("-", "_"); ;

                        // Loop through the DocumentPDFs element, storing each document.

                        XmlNode docs = xmldoc.SelectSingleNode("//a:DocumentPDFs", mgr);

                        foreach (XmlNode doc in docs.ChildNodes)
                        {
                            string documentName = doc.ChildNodes[0].InnerText; // pdf.SelectSingleNode("//a:Name", mgr).InnerText;
                            string documentId = doc.ChildNodes[2].InnerText; // pdf.SelectSingleNode("//a:DocumentID", mgr).InnerText;
                            string byteStr = doc.ChildNodes[1].InnerText; // pdf.SelectSingleNode("//a:PDFBytes", mgr).InnerText;
                            byte[] byteFile = Convert.FromBase64String(byteStr);

                            if (!UploadSignedDocument(appOnlyAccessToken, repository, folderName, fileName, documentId, byteFile))
                            {
                                //Log Error Can not Upload Document
                                Log.Error("Unable to Upload Document :: " + fileName);
                            }

                           
                            responseMessage = responseMessage + "Documents Named - " + fileName + " - Was Uploaded Successfully Into - " + repository + " | ";
                            
                        }

                        Log.Info(responseMessage.Substring(0, responseMessage.Length - 3));
                    }
                    catch (Exception ex)
                    {
                        Log.Error("No Document Library Was Found :: " + repository);
                        return this.Request.CreateResponse<string>(HttpStatusCode.InternalServerError, "No Document Library Was Found :: " + repository );
                    }
                }
                else
                {
                    //Not In Scope For this Phase
                    //Google Drive
                    responseMessage = "This Route is not Currently Supported. Site : " + site;
                    Log.Info(responseMessage.Substring(0, responseMessage.Length - 3));
                }
            }

            return this.Request.CreateResponse<String>(HttpStatusCode.OK, responseMessage.Substring(0, responseMessage.Length - 3));

        }
    }
}
