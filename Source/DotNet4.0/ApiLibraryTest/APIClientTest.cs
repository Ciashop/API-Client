using System;
using System.Text;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using APILibrary;
using System.Collections.Specialized;
using Moq;
using System.Net;
using System.IO;

namespace ApiLibraryTest
{
    [TestClass]
    public class APIClientTest
    {
        #region ExtractQueryString Tests
        [TestMethod]
        public void ExtractQueryString_Must_Return_Emprt_When_Parameters_Is_Null()
        {
            APIClient target = new APIClient(new AuthState
            {
                AccessToken = "token",
                ApiUrl = "https://www.mystore.com/api/v1"
            });

            Assert.AreEqual(String.Empty,target.ExtractQueryString(null));
        }

        [TestMethod]
        public void ExtractQueryString_Must_Return_Formatted_String_When_Valid_Parameter()
        {
            NameValueCollection param = new NameValueCollection();
            param.Add("attr1", "10");
            param.Add("attr2", "20");

            APIClient target = new APIClient(new AuthState
            {
                AccessToken = "token",
                ApiUrl = "https://www.mystore.com/api/v1"
            });

            Assert.AreEqual("?attr1=10&attr2=20", target.ExtractQueryString(param));
        }
        #endregion

        #region Call tests
        [TestMethod]
        public void Call_With_HttpMethod_Must_Return_Rigth_Method_and_RequestHeaders()
        {
            var mock = new Mock<HttpHelper>("https://www.mystore.com/api/v1");
            mock.SetupProperty(m => m.HttpWebRequest.Method);
            mock.SetupProperty(m => m.HttpWebRequest.ContentType);
            mock.SetupProperty(m => m.HttpWebRequest.Headers, new WebHeaderCollection());
            mock.Setup(m => m.HttpWebRequest.GetRequestStream()).Returns(new MemoryStream());
            mock.Setup(m => m.HttpWebRequest.GetResponse().GetResponseStream()).Returns(new MemoryStream());

            APIClient target = new APIClient(new AuthState
                                            {
                                                AccessToken = "token",
                                                ApiUrl = "https://www.mystore.com/api/v1"
                                            });

            target.Call(HttpMethods.GET, "departments", String.Empty, mock.Object);

            mock.VerifySet(h => h.HttpWebRequest.Method = "GET");
            mock.VerifySet(h => h.HttpWebRequest.ContentType = "application/json");

            Assert.AreEqual(mock.Object.HttpWebRequest.Headers.GetValues("Accept").First().ToString(), "application/json");
            Assert.AreEqual(mock.Object.HttpWebRequest.Headers.GetValues("Authorization").First().ToString(), "Basic Bearer:token");
        }

        [TestMethod]
        public void Call_With_HttpMethod_Post_Must_Write_Contents()
        {
            var content =   "{" +
                                "\"id\": \"1\"," +
                                "\"name\": \"Department\"" +
                            "}";

            string writedString = string.Empty;
            var mockMS = new Mock<MemoryStream>();
            mockMS.Setup(m => m.CanWrite).Returns(true);
            mockMS.Setup(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback(
                (byte[] b, int i1, int i2) =>
                {
                    writedString = new System.Text.ASCIIEncoding().GetString(b, i1, i2);
                });


            var mock = new Mock<HttpHelper>("https://www.mystore.com/api/v1");
            mock.SetupProperty(m => m.HttpWebRequest.Method);
            mock.SetupProperty(m => m.HttpWebRequest.ContentType);
            mock.SetupProperty(m => m.HttpWebRequest.Headers, new WebHeaderCollection());
            mock.Setup(m => m.HttpWebRequest.GetRequestStream()).Returns(mockMS.Object);
            mock.Setup(m => m.HttpWebRequest.GetResponse().GetResponseStream()).Returns(new MemoryStream());

            APIClient target = new APIClient(new AuthState
            {
                AccessToken = "token",
                ApiUrl = "https://www.mystore.com/api/v1"
            });

            target.Call(HttpMethods.POST, "departments", content, mock.Object);

            mock.VerifySet(h => h.HttpWebRequest.Method = "POST");
            Assert.AreEqual(content, writedString);
        }
        #endregion

        #region GetResponse tests
        [TestMethod]
        public void GetResponse_Must_Return_StatusCode_200()
        {
            var mock = new Mock<HttpWebResponseWrapper>();
            mock.Setup(m => m.StatusCode).Returns(HttpStatusCode.OK);
            mock.Setup(m => m.Headers).Returns(new WebHeaderCollection());
            mock.Setup(m => m.GetResponseStream()).Returns(new MemoryStream());

            APIClient target = new APIClient(new AuthState
            {
                AccessToken = "token",
                ApiUrl = "https://www.mystore.com/api/v1"
            });

            APIResponse apiResponse = target.GetResponse(mock.Object);

            Assert.AreEqual(200, apiResponse.StatusCode);
        }

        [TestMethod]
        public void GetResponse_Must_Return_HasMore_True()
        {
            WebHeaderCollection mockHeader = new WebHeaderCollection();
            mockHeader.Add("x-hasmore", "true");

            var mock = new Mock<HttpWebResponseWrapper>();
            mock.Setup(m => m.StatusCode).Returns(HttpStatusCode.OK);
            mock.Setup(m => m.Headers).Returns(mockHeader);
            mock.Setup(m => m.GetResponseStream()).Returns(new MemoryStream());

            APIClient target = new APIClient(new AuthState
            {
                AccessToken = "token",
                ApiUrl = "https://www.mystore.com/api/v1"
            });

            APIResponse apiResponse = target.GetResponse(mock.Object);

            Assert.AreEqual(true, apiResponse.HasMore);
        }

        [TestMethod]
        public void GetResponse_Must_Return_ApiLimit_100_and_ApiLimitRemaining_10()
        {
            WebHeaderCollection mockHeader = new WebHeaderCollection();
            mockHeader.Add("x-apilimit-remaining", "100/10");

            var mock = new Mock<HttpWebResponseWrapper>();
            mock.Setup(m => m.StatusCode).Returns(HttpStatusCode.OK);
            mock.Setup(m => m.Headers).Returns(mockHeader);
            mock.Setup(m => m.GetResponseStream()).Returns(new MemoryStream());

            APIClient target = new APIClient(new AuthState
            {
                AccessToken = "token",
                ApiUrl = "https://www.mystore.com/api/v1"
            });

            APIResponse apiResponse = target.GetResponse(mock.Object);

            Assert.AreEqual(100, apiResponse.ApiLimit);
            Assert.AreEqual(10, apiResponse.ApiLimitRemaining);
        }

        [TestMethod]
        public void GetResponse_Must_Return_Content_Value_Equals_To_Mock()
        {
            var content =   "{" +
                                "\"count\": \"1000\" " +
                            "}";

            var mock = new Mock<HttpWebResponseWrapper>();
            mock.Setup(m => m.StatusCode).Returns(HttpStatusCode.OK);
            mock.Setup(m => m.Headers).Returns(new WebHeaderCollection());
            mock.Setup(m => m.GetResponseStream()).Returns(new MemoryStream(Encoding.Default.GetBytes(content)));

            APIClient target = new APIClient(new AuthState
            {
                AccessToken = "token",
                ApiUrl = "https://www.mystore.com/api/v1"
            });

            APIResponse apiResponse = target.GetResponse(mock.Object);

            Assert.AreEqual("1000", apiResponse.Content.count.Value);
        }

        [TestMethod]
        public void GetResponse_Must_Return_Object_Error()
        {
            var content = "{" +
                            "\"responseCode\": \"404\"," +
                            "\"message\": \"notfound\"," +
                            "\"errors\": " +
                            "[" +
                                "{" +
                                    "\"message\": \"The requested resource was not found\"," +
                                    "\"code\": \"not_found\"," +
                                    "\"moreInfo\": \"http://wiki\"" +
                                "}" +
                            "]" +
                          "}";

            var mock = new Mock<HttpWebResponseWrapper>();
            mock.Setup(m => m.StatusCode).Returns(HttpStatusCode.NotFound);
            mock.Setup(m => m.Headers).Returns(new WebHeaderCollection());
            mock.Setup(m => m.GetResponseStream()).Returns(new MemoryStream(Encoding.Default.GetBytes(content)));

            APIClient target = new APIClient(new AuthState
            {
                AccessToken = "token",
                ApiUrl = "https://www.mystore.com/api/v1"
            });

            APIResponse apiResponse = target.GetResponse(mock.Object);

            Assert.AreEqual(404, apiResponse.StatusCode);
            Assert.AreEqual("notfound", apiResponse.Error.Message);
            Assert.AreEqual("The requested resource was not found", apiResponse.Error.ErrorDetails.First().Message);
            Assert.AreEqual("not_found", apiResponse.Error.ErrorDetails.First().Code);
            Assert.AreEqual("http://wiki", apiResponse.Error.ErrorDetails.First().MoreInfo);
        }

        [TestMethod]
        public void GetResponse_Must_Return_StatusCode_404_When_Response_Return_Error_And_StatusCode_200()
        {
            var content = "{" +
                              "\"responseCode\": \"404\"" +
                          "}";


            var mock = new Mock<HttpWebResponseWrapper>();
            mock.Setup(m => m.StatusCode).Returns(HttpStatusCode.OK);
            mock.Setup(m => m.Headers).Returns(new WebHeaderCollection());
            mock.Setup(m => m.GetResponseStream()).Returns(new MemoryStream(Encoding.Default.GetBytes(content)));

            APIClient target = new APIClient(new AuthState
            {
                AccessToken = "token",
                ApiUrl = "https://www.mystore.com/api/v1"
            });

            APIResponse apiResponse = target.GetResponse(mock.Object);

            Assert.AreEqual(404, apiResponse.StatusCode);
        }
        #endregion

    }
}
