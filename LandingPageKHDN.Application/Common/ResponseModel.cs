using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LandingPageKHDN.Application.Common
{
    public class ResponseModel<T>
    {
        public int Status { get; set; }
        public string Message { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string[]>? Errors { get; set; }

        [JsonIgnore]
        public bool IsSuccess => Status >= 200 && Status < 300;
        public static ResponseModel<T> SuccessResult(T data, string message, int status =200)
        {
            return new ResponseModel<T> { Status = status, Message = message, Data = data };
        }

        public static ResponseModel<T> FailureResult(string message, Dictionary<string, string[]>? errors = null, int status = 400)
        {
            return new ResponseModel<T> { Status = status, Message = message ,Errors=errors};
        }
    }
}
