using System.Collections.Specialized;
using System.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO;

namespace APILibrary
{
    /// <summary>
    /// This class is used to make shop API calls 
    /// </summary>
    /// <remarks>
    /// You will first need to use the APIAuthorizer to obtain the required authorization.
    /// </remarks>
    public class APIClient
    {
        protected AuthState State { get; private set; }

        /// <summary>
        /// Creates an instance of this class for use with making API Calls
        /// </summary>
        /// <param name="state">the authorization state required to make the API Calls</param>
        public APIClient(AuthState state)
        {
            this.State = state;
        }

        /// <summary>
        /// Make a Get method HTTP request to API
        /// </summary>
        /// <param name="path">path where the API call will be made</param>
        /// <param name="callPrams">the querystring params</param>
        /// <returns>an object (APIResponse) that represents server response</returns>
        public APIResponse Get(string path, NameValueCollection callPrams = null)
        {
            return Call(HttpMethods.GET, path, callPrams);
        }

        /// <summary>
        /// Make a Post method HTTP request to API
        /// </summary>
        /// <param name="path">path where the API call will be made</param>
        /// <param name="data">>data that this path will be expecting</param>
        /// <returns>an object (APIResponse) that represents server response</returns>
        public APIResponse Post(string path, Object data)
        {
            return Call(HttpMethods.POST, path, data);
        }

        /// <summary>
        /// Make a Put method HTTP request to API
        /// </summary>
        /// <param name="path">path where the API call will be made</param>
        /// <param name="data">>data that this path will be expecting</param>
        /// <returns>an object (APIResponse) that represents server response</returns>
        public APIResponse Put(string path, Object data)
        {
            return Call(HttpMethods.PUT, path, data);
        }

        /// <summary>
        /// Make a Delete method HTTP request to API
        /// </summary>
        /// <param name="path">path where the API call will be made</param>
        /// <returns>an object that represents server response</returns>
        public APIResponse Delete(string path)
        {
            return Call(HttpMethods.DELETE, path);
        }

        private APIResponse Call(HttpMethods method, string path)
        {
            return Call(method, path, null);
        }

        internal APIResponse Call(HttpMethods method, string path, object callParams)
        {
            string queryString = string.Empty;
            string content = string.Empty;

            if (method == HttpMethods.GET || method == HttpMethods.DELETE)
                queryString = ExtractQueryString(callParams);
            else if (method == HttpMethods.POST || method == HttpMethods.PUT)
                content = (new JsonTranslator()).Encode(callParams);

            string url = String.Format("{0}/{1}{2}",
                                                    this.State.ApiUrl,
                                                    path,
                                                    queryString);

            return Call(method, path, content, new HttpHelper(url));
        }

        internal APIResponse Call(HttpMethods method, string path, string content, HttpHelper httpHelper)
        {
            var request = httpHelper.HttpWebRequest;
            request.Method = method.ToString();

            request.ContentType = "application/json";
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", "Basic Bearer:" + this.State.AccessToken);

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(content);
                writer.Close();
            }

            return GetResponse((HttpWebResponseWrapper)request.GetResponse());
        }

        internal APIResponse GetResponse(HttpWebResponseWrapper response)
        {
            APIResponse apiResponse = new APIResponse();

            //Extract content
            using (Stream stream = response.GetResponseStream())
            {
                StreamReader sr = new StreamReader(stream);
                apiResponse.Content = (new JsonTranslator()).Decode(sr.ReadToEnd());
                sr.Close();
            }

            apiResponse.Header = response.Headers;

            apiResponse.StatusCode = (int)response.StatusCode;

            if (response.Headers != null)
            {
                bool hasMore;
                if (response.Headers.Get("x-hasmore") != null && Boolean.TryParse(response.Headers.GetValues("x-hasmore").First().ToString(), out hasMore))
                    apiResponse.HasMore = hasMore;

                int apiLimit = 0;
                if (response.Headers.Get("x-apilimit-remaining") != null && int.TryParse(response.Headers.GetValues("x-apilimit-remaining").First().Split('/')[0], out apiLimit))
                    apiResponse.ApiLimit = apiLimit;

                int apiLimitRemaining = 0;
                if (response.Headers.Get("x-apilimit-remaining") != null && int.TryParse(response.Headers.GetValues("x-apilimit-remaining").First().Split('/')[1], out apiLimitRemaining))
                    apiResponse.ApiLimitRemaining = apiLimitRemaining;
            }

            //Recover object if error
            if (apiResponse.Content is JObject && apiResponse.Content.responseCode != null)
            {
                apiResponse.StatusCode = apiResponse.Content.responseCode;
                apiResponse.Error.Message = apiResponse.Content.message;
                if (apiResponse.Content.errors != null)
                {
                    foreach (var itemError in apiResponse.Content.errors)
                    {
                        apiResponse.Error.ErrorDetails.Add(
                            new ErrorDetail
                            {
                                Message = itemError.message,
                                Code = itemError.code,
                                MoreInfo = itemError.moreInfo
                            }
                            );
                    }
                }
            }

            return apiResponse;
        }

        internal string ExtractQueryString(object callParams)
        {
            if (callParams == null)
                return string.Empty;

            if (callParams is NameValueCollection)
            {
                List<string> queryString = new List<string>();
                NameValueCollection parameters = (NameValueCollection)callParams;

                foreach (string key in parameters)
                    queryString.Add(String.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(parameters[key])));

                return string.Join("&", queryString).Insert(0, "?");
            }
            else
            {
                return callParams.ToString();
            }

        }
    }
}
