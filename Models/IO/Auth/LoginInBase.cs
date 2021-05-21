using APIUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Models.IO.Auth
{
	public class LoginInBase
	{
		[Required]
		[StringLength(30, MinimumLength = 5)]
		public string Username { get; set; }
		[Required]
		[StringLength(30, MinimumLength = 8)]
		public string Password { get; set; }

		public void Validate()
		{
			ValidateUsername(Username);
			ValidatePassword(Password);
		}

		public static void ValidateUsername(string text)
		{
			if (text.Length < 5)
			{
				throw new APIException(APIErrorCode.RECEIVED_DATA_IS_INVALID);
			}
			if (text.Any(x => char.IsWhiteSpace(x)))
			{
				throw new APIException(APIErrorCode.RECEIVED_DATA_IS_INVALID);
			}
		}

		public static void ValidatePassword(string text)
		{
			if (text.Length < 8)
			{
				throw new APIException(APIErrorCode.RECEIVED_DATA_IS_INVALID);
			}
			if (!text.Any(x => char.IsLower(x)))
			{
				throw new APIException(APIErrorCode.RECEIVED_DATA_IS_INVALID);
			}
			if (!text.Any(x => char.IsUpper(x)))
			{
				throw new APIException(APIErrorCode.RECEIVED_DATA_IS_INVALID);
			}
			if (!text.Any(x => char.IsDigit(x)))
			{
				throw new APIException(APIErrorCode.RECEIVED_DATA_IS_INVALID);
			}
		}
	}
}
