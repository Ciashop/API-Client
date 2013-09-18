using System.Collections.Generic;

namespace APILibrary
{
    /// <summary>
    /// Class that represents a structure for error
    /// </summary>
    public class ErrorResponse
    {
        public string Message { get; set; }
        public List<ErrorDetail> ErrorDetails { get; set; }
    }

    /// <summary>
    /// Class that represents a structure for error details
    /// </summary>
    public class  ErrorDetail
    {
        public string Message { get; set; }
        public string Code { get; set; }
        public string MoreInfo { get; set; }
    }
}
