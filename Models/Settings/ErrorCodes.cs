using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models.Settings
{
	public class ErrorCodes
	{
		public ApiErrorCode TooManyLoginAttempts { get; set; }
		public ApiErrorCode IncorrectLoginCredentials { get; set; }
		public ApiErrorCode UnverifiedUser { get; set; }
		public ApiErrorCode UserWithUsernameAlreadyExists { get; set; }
		public ApiErrorCode UserWithEmailAlreadyExists { get; set; }
	}
}
