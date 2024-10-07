using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using RPPP_WebApp.ViewModels.JTable;
using RPPP_WebApp.Extensions;

namespace RPPP_WebApp.Util.ExceptionFilters
{
    public class ErrorStatusTo200WithErrorMessage : ExceptionFilterAttribute
    {
        private readonly ILogger<ErrorStatusTo200WithErrorMessage> logger;

        public ErrorStatusTo200WithErrorMessage(ILogger<ErrorStatusTo200WithErrorMessage> logger)
        {
            this.logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            string exceptionMessage = context.Exception.CompleteExceptionMessage();
            context.ExceptionHandled = true;
            JTableAjaxResult result = JTableAjaxResult.Error(exceptionMessage);
            context.Result = new OkObjectResult(result);
        }
    }
}
