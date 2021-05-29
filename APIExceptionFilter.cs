using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api
{
	public class APIExceptionFilter : IActionFilter, IOrderedFilter
	{
		public int Order { get; } = int.MaxValue - 10;

		public void OnActionExecuting(ActionExecutingContext context) { }

		public void OnActionExecuted(ActionExecutedContext context)
		{
			if (context.Exception is ApiException exception)
			{
				ObjectResult objectResult = new ObjectResult(exception.Payload);
				objectResult.StatusCode = exception.HttpStatusCode;
				context.Result = objectResult;
				context.ExceptionHandled = true;
			}
		}
	}
}
