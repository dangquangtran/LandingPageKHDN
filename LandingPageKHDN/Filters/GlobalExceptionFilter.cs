using LandingPageKHDN.Application.Common;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using LandingPageKHDN.Application.Exceptions;

namespace LandingPageKHDN.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;

            // Ghi log lỗi chi tiết
            _logger.LogError(exception, "Unhandled exception occurred.");

            ObjectResult result;

            switch (exception)
            {
                case BusinessException businessEx:
                    result = new ObjectResult(ResponseModel<string>.FailureResult(businessEx.Message, businessEx.Errors, 400))
                    {
                        StatusCode = 400
                    };
                    break;

                case NotFoundException notFoundEx:
                    result = new ObjectResult(ResponseModel<string>.FailureResult(notFoundEx.Message, null, 404))
                    {
                        StatusCode = 404
                    };
                    break;

                default:
                    result = new ObjectResult(ResponseModel<string>.FailureResult("Đã xảy ra lỗi hệ thống. Bạn Vui lòng thử lại sau.", null, 500))
                    {
                        StatusCode = 500
                    };
                    break;
            }

            context.Result = result;
            context.ExceptionHandled = true;
        }
    }
}
