using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using APILibrary;
using Moq;
using System.IO;

namespace ApiLibraryTest
{
    [TestClass]
    public class APIAuthorizerTest
    {
        #region Ctor Tests
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_Do_Not_Accept_Empty_clientId()
        {
            new APIAuthorizer("", "key", new Uri("https://mystore.com/manager"));
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_Do_Not_Accept_Empty_secretKey()
        {
            new APIAuthorizer("client", "", new Uri("https://mystore.com/manager"));
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_Do_Not_Accept_Null_storeUrl()
        {
            new APIAuthorizer("client", "key", null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void Ctor_Do_Not_Accept_Unsafe_storeUrl()
        {
            new APIAuthorizer("client", "key", new Uri("http://mystore.com/manager"));
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void Ctor_Do_Not_Accept_Path_Without_Manager_storeUrl()
        {
            new APIAuthorizer("client", "key", new Uri("http://mystore.com"));
        }
        #endregion

        #region GetAuthorizationUrl Tests
        [TestMethod]
        public void GetAuthorizationUrl_Must_Return_Url_With_Scope()
        {
            var target = new APIAuthorizer("client", "key", new Uri("https://www.mystore.com/manager"));

            Assert.AreEqual("https://www.mystore.com/manager/oauth/authorization.aspx?client_id=client&scope=readDepartment,writeOrder", 
                target.GetAuthorizationUrl(new string[] { "readDepartment", "writeOrder" }));
        }

        [TestMethod]
        public void GetAuthorizationUrl_Must_Return_Url_Whitout_Scope()
        {
            var target = new APIAuthorizer("client", "key", new Uri("https://www.mystore.com/manager"));

            Assert.AreEqual("https://www.mystore.com/manager/oauth/authorization.aspx?client_id=client", target.GetAuthorizationUrl(null));
        }

        [TestMethod]
        public void GetAuthorizationUrl_Should_Not_Duplicate_Slash_When_StoreUrl_Has_Slash()
        {
            var target = new APIAuthorizer("client", "key", new Uri("https://www.mystore.com/manager/"));

            Assert.AreEqual("https://www.mystore.com/manager/oauth/authorization.aspx?client_id=client", target.GetAuthorizationUrl(null));
        }
        #endregion

        #region AuthorizationState Tests
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void AuthorizationState_Must_Not_Accept_Empty_Code()
        {
            var target = new APIAuthorizer("client", "key", new Uri("https://www.mystore.com/manager"));

            target.AuthorizationState("");
        }

        [TestMethod]
        public void AuthorizationState_Must_Defice_Post_Method_and_ContentType()
        {
            var mock = new Mock<HttpHelper>("https://www.mystore.com/manager")
                .SetupProperty(m => m.HttpWebRequest.Method);
            mock.Setup(m => m.HttpWebRequest.GetRequestStream()).Returns(new MemoryStream());
            mock.Setup(m => m.HttpWebRequest.GetResponse().GetResponseStream()).Returns(new MemoryStream());


            var target = new APIAuthorizer("client", "key", new Uri("https://www.mystore.com/manager"));
            target.AuthorizationState("code", mock.Object);


            mock.VerifySet(h => h.HttpWebRequest.Method = "POST");
            mock.VerifySet(h => h.HttpWebRequest.ContentType = "application/x-www-form-urlencoded");
        }

        [TestMethod]
        public void AuthorizationState_Must_Save_Parameters_on_Body()
        {
            string writedString = string.Empty;
            var mockMS = new Mock<MemoryStream>();
            mockMS.Setup(m => m.CanWrite).Returns(true);
            mockMS.Setup(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback(
                (byte[] b, int i1, int i2) => {
                    writedString = new System.Text.ASCIIEncoding().GetString(b, i1, i2);
                });

            var mock = new Mock<HttpHelper>("https://www.mystore.com/manager")
                .SetupProperty(m => m.HttpWebRequest.Method);
            mock.Setup(m => m.HttpWebRequest.GetRequestStream()).Returns(mockMS.Object);
            mock.Setup(m => m.HttpWebRequest.GetResponse().GetResponseStream()).Returns(new MemoryStream());

            var target = new APIAuthorizer("client", "key", new Uri("https://www.mystore.com/manager"));
            target.AuthorizationState("code", mock.Object);

            Assert.AreEqual("client_id=client&client_secret=key&code=code", writedString);
        }
        #endregion

        #region GetAccessToken
        [TestMethod]
        public void GetAccessToken_Must_Return_GetAccessToken_As_Defined()
        {
            var target = new APIAuthorizer("client", "key", new Uri("https://www.mystore.com/mystore/manager"));
            var token = "{" +
                            "\"access_token\": \"token\" " +
                        "}";

            var result = target.GetAccessToken(token);
            Assert.AreEqual("token", result.AccessToken);
            Assert.AreEqual("https://www.mystore.com/mystore/api/v1", result.ApiUrl);
        }

        [TestMethod]
        public void GetAccessToken_Must_Return_Null_When_Invalid_Parameters()
        {
            var target = new APIAuthorizer("client", "key", new Uri("https://www.mystore.com/manager"));

            Assert.IsNull(target.GetAccessToken(null));
            Assert.IsNull(target.GetAccessToken(""));
            Assert.IsNull(target.GetAccessToken(" { \"token\" : \"value\" }"));
        }
        #endregion

    }
}
