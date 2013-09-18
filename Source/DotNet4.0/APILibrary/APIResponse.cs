using System.Collections.Generic;
using System.Collections.Specialized;

namespace APILibrary
{
    /// <summary>
    /// Class that represents response message from API
    /// </summary>
    public class APIResponse
    {
        public APIResponse()
        {
            this.Header = new NameValueCollection();
            this.Error = new ErrorResponse();
            this.Error.ErrorDetails = new List<ErrorDetail>();
        }

        public dynamic Content { get; set; }

        public NameValueCollection Header { get; set; }

        public ErrorResponse Error { get; set; }

        public int StatusCode { get; set; }

        public int ApiLimit { get; set; }

        public int ApiLimitRemaining { get; set; }

        public bool HasMore { get; set; }
    }
}
