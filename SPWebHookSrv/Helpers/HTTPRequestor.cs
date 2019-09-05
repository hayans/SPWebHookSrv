using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using SPWebHookSrv.Models;
using SPWebHookSrv.Authentication;

namespace SPWebHookSrv.Helpers
{
    public class HTTPRequestor
    {
        private IAuthenticator _auth;
        private const int MAXREQUESTS = 19;
        private static int RunningRequests;
        private readonly object lck = new object();
        public int AutomaticRetrys { get; set; }
        public HTTPRequestor() { }
        public HTTPRequestor(IAuthenticator authenticator)
        {
            _auth = authenticator;
            AutomaticRetrys = 10;
        }
        public async Task<string> Post(string url, string body, Dictionary<string, string> headers)
        {
            var json = await MakeCall(url, body, headers, "POST");
            return json;
        }
        public async Task<T> Post<T>(string url, string body, Dictionary<string, string> headers = null)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            var json = await MakeCall(url, body, headers, "POST");
            return JsonConvert.DeserializeObject<T>(json, JSONService.Settings);
        }

        public async Task<string> Post(string url, Dictionary<string, string> headers = null)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            var json = await MakeCall(url, headers, "POST");
            return json;
        }
        public async Task<string> Get(string url, Dictionary<string, string> headers = null)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            var json = await MakeCall(url, null, headers, "GET");
            return json;
        }
        public async Task<T> Get<T>(string url, Dictionary<string, string> headers = null)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            var json = await MakeCall(url, null, headers, "GET");
            return JsonConvert.DeserializeObject<T>(json, JSONService.Settings);
        }

        public async Task<T> Delete<T>(string url, Dictionary<string, string> headers = null)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            var json = await MakeCall(url, null, headers, "DELETE");
            return JsonConvert.DeserializeObject<T>(json, JSONService.Settings);
        }

        public async Task<string> Delete(string url, Dictionary<string, string> headers = null)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            var json = await MakeCall(url, null, headers, "DELETE");
            return json;
        }
        public async Task<T> Patch<T>(string url, string body, Dictionary<string, string> headers = null)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            var json = await MakeCall(url, body, headers, "PATCH");
            return JsonConvert.DeserializeObject<T>(json, JSONService.Settings);
        }

        public async Task<T> PostFile<T>(string url, Stream file, string fileName, Dictionary<string, string> headers = null, int retrycount = 0)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }

            HttpClient httpClient = new HttpClient();
            if (_auth != null)
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {_auth.AccessToken}");
            }
            MultipartFormDataContent form = new MultipartFormDataContent();

            httpClient.Timeout = new TimeSpan(0, 30, 0);

            form.Add(new StreamContent(file), fileName, fileName);
            bool canRun = false;
            lock (lck)
            {
                canRun = RunningRequests < MAXREQUESTS;
                if (canRun)
                {
                    RunningRequests++;
                }
            }
            while (!canRun)
            {

                Console.WriteLine($"Waiting to send request there are {RunningRequests} running requests.");
                await Task.Delay(500);

                lock (lck)
                {
                    canRun = RunningRequests < MAXREQUESTS;
                    if (canRun)
                    {
                        RunningRequests++;
                    }
                }
            }
            HttpResponseMessage response;
            try
            {
                response = await httpClient.PostAsync(url, form);
            }
            catch (HttpRequestException ex)
            {
                if (retrycount < AutomaticRetrys)
                {
                    return await PostFile<T>(url, file, fileName, null, retrycount + 1);
                }
                throw ex;
                // response = await httpClient.PostAsync(url, form);
            }
            //HttpResponseMessage response = await httpClient.PostAsync(url, form);

            response.EnsureSuccessStatusCode();
            httpClient.Dispose();
            string sd = await response.Content.ReadAsStringAsync();
            lock (lck)
            {

                RunningRequests--;
            }
            return JsonConvert.DeserializeObject<T>(sd, JSONService.Settings);
        }

        public async Task<T> Put<T>(string url, string body, Dictionary<string, string> headers = null)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            var json = await MakeCall(url, body, headers, "PUT");
            return JsonConvert.DeserializeObject<T>(json, JSONService.Settings);
        }

        //public async Task<Stream> GetStream(string url, int numberofTrys = 0)
        //{
        //    using (var httpClient = new HttpClient())
        //    {
        //        //httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
        //        if (_auth != null)
        //        {
        //            httpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {_auth.AccessToken}");
        //        }

        //        var request = new HttpRequestMessage(new HttpMethod("GET"), url);

        //        httpClient.Timeout = new TimeSpan(0, 0, 30, 0);
        //        HttpResponseMessage response = new HttpResponseMessage();

        //        response = await httpClient.SendAsync(request);


        //        var res = await response.Content.ReadAsStreamAsync();
        //        if (!response.IsSuccessStatusCode)
        //        {
        //            ApiError ae = JsonConvert.DeserializeObject<ApiError>(response.Content.ReadAsStringAsync().Result, JSONService.Settings);
        //            if (ae.Error.HttpStatusCode == 429 && numberofTrys < AutomaticRetrys)
        //            {
        //                Thread.Sleep(2000);
        //                var newTry = numberofTrys + 1;
        //                return await GetStream(url, newTry);

        //            }
        //            throw new ApiException(ae.Error.DeveloperMessage) { ApiError = ae };
        //        }
        //        return res;
        //    }
        //}

        //public async Task<string> MakeCallAsync(string url, string body, Dictionary<string, string> headers, string method, int numberofTrys = 0)
        //{
        //    var task = Task<String>.Run(() => MakeCall(url,body,headers,method,numberofTrys));
        //    return await task;
        //}

        public async Task<string> MakeCall(string url, string body, Dictionary<string, string> headers, string method, int numberofTrys = 0)
        {
            using (var httpClient = new HttpClient())
            {
                //httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (_auth != null)
                {


                    httpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {_auth.AccessToken}");
                }
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
                bool canRun = false;
                lock (lck)
                {
                    canRun = RunningRequests < MAXREQUESTS;
                    if (canRun)
                    {
                        RunningRequests++;
                    }
                }
                while (!canRun)
                {

                    Console.WriteLine($"Waiting to send request there are {RunningRequests} running requests.");
                    await Task.Delay(500);
                    lock (lck)
                    {
                        canRun = RunningRequests < MAXREQUESTS;
                        if (canRun)
                        {
                            RunningRequests++;
                        }
                    }
                }
                string res = "";
                try
                {
                    var makeCall = new Thread(() =>
                    {
                        try
                        {
                            response =  httpClient.SendAsync(request).Result;
                            res =  response.Content.ReadAsStringAsync().Result;
                        }
                        catch (Exception e)
                        {
                           
                        }
                    });
                    makeCall.Start();
                    makeCall.Join();
                    
                }
                catch (Exception ex)
                {
                    if (numberofTrys < AutomaticRetrys)
                    {
                        return await MakeCall(url, headers, method, numberofTrys + 1);
                    }
                    throw ex;
                }
                lock (lck)
                {
                    canRun = RunningRequests < MAXREQUESTS;
                    RunningRequests--;
                }
                if (!response.IsSuccessStatusCode)
                {
                    ApiError ae = JsonConvert.DeserializeObject<ApiError>(res, JSONService.Settings);
                    //if (response.StatusCode ==  && numberofTrys < AutomaticRetrys)
                    //{
                    //    Thread.Sleep(2000);
                    //    var newTry = numberofTrys + 1;
                    //    return await MakeCall(url, body, headers, method, newTry);
                    //}
                    throw new ApiException(ae.Error.Message.Value) { ApiError = ae };
                }
                return res;
            }


        }

        public async Task<string> MakeCall(string url, Dictionary<string, string> headers, string method, int numberofTrys = 0)
        {
            using (var httpClient = new HttpClient())
            {
                //httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (_auth != null)
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {_auth.AccessToken}");
                }
                foreach (var header in headers)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                var request = new HttpRequestMessage(new HttpMethod(method), url);

                httpClient.Timeout = new TimeSpan(0, 0, 30, 0);
                HttpResponseMessage response = new HttpResponseMessage();
                bool canRun = false;
                lock (lck)
                {
                    canRun = RunningRequests < MAXREQUESTS;
                    if (canRun)
                    {
                        RunningRequests++;
                    }
                }
                while (!canRun)
                {

                    Console.WriteLine($"Waiting to send request there are {RunningRequests} running requests.");
                    await Task.Delay(500);
                    lock (lck)
                    {
                        canRun = RunningRequests < MAXREQUESTS;
                        if (canRun)
                        {
                            RunningRequests++;
                        }
                    }
                }
                string res;
                try
                {
                    response = await httpClient.SendAsync(request);
                    res = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    if (numberofTrys < AutomaticRetrys)
                    {
                        return await MakeCall(url, headers, method, numberofTrys + 1);
                    }
                    throw ex;
                }

                lock (lck)
                {

                    RunningRequests--;
                }
                if (!response.IsSuccessStatusCode)
                {
                    ApiError ae = JsonConvert.DeserializeObject<ApiError>(res, JSONService.Settings);
                    //if (ae.Error.HttpStatusCode == 429 && numberofTrys < AutomaticRetrys)
                    //{
                    //    await Task.Delay(2000);
                    //    var newTry = numberofTrys + 1;
                    //    return await MakeCall(url, headers, method, newTry);
                    //}
                    throw new ApiException(ae.Error.Message.Value) { ApiError = ae };
                }
                return res;
            }


        }

    }
}

