using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandingPageKHDN.Application.Exceptions
{
    public class BusinessException : Exception
    {
        public string ErrorCode { get; }
        public Dictionary<string, string[]>? Errors { get; }

        public BusinessException(string message, string errorCode = "BUSINESS_ERROR", Dictionary<string, string[]>? errors = null)
            : base(message)
        {
            ErrorCode = errorCode;
            Errors = errors;
        }
    }
}
