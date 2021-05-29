using Api.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api
{
	public class ApiException : Exception
	{
		public int HttpStatusCode { get; set; }
		public object? Payload { get; set; }

		public ApiException() => (HttpStatusCode, Payload) = (400, null);

		public ApiException(ApiErrorCode apiErrorCode, int httpStatusCode = 400)
		{
			HttpStatusCode = httpStatusCode;
			Payload = new
			{
				ErrorCode = apiErrorCode.Code
			};
		}
	}
}
