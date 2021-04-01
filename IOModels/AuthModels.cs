using APIUtils;
using location_sharing_backend.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace location_sharing_backend.IOModels {
	public class AuthModels {
		public class LoginInBase {
			[Required]
			[StringLength(30, MinimumLength = 6)]
			public string Username { get; set; }
			[Required]
			[StringLength(30, MinimumLength = 8)]
			public string Password { get; set; }

			public void Validate() {
				ValidateUsername(Username);
				ValidatePassword(Password);
			}

			public static void ValidateUsername(string text) {
				if (text.Length < 6) {
					throw new APIException(APIErrorCode.RECEIVED_DATA_IS_INVALID);
				}
			}

			public static void ValidatePassword(string text) {
				if (text.Length < 8) {
					throw new APIException(APIErrorCode.RECEIVED_DATA_IS_INVALID);
				}
				if (!text.Any(x => char.IsLower(x))) {
					throw new APIException(APIErrorCode.RECEIVED_DATA_IS_INVALID);
				}
				if (!text.Any(x => char.IsUpper(x))) {
					throw new APIException(APIErrorCode.RECEIVED_DATA_IS_INVALID);
				}
				if (!text.Any(x => char.IsDigit(x))) {
					throw new APIException(APIErrorCode.RECEIVED_DATA_IS_INVALID);
				}
			}
		}
		public class LoginIn : LoginInBase {
			public bool Persist { get; set; }
		}

		public class RegisterIn : LoginInBase {
			[Required]
			[EmailAddress]
			public string Email { get; set; }
			public string? ProfilePhotoURL { get; set; }
		}

		public class RefreshOut{
			public DateTime AccessTokenExpires { get; set; }

			public RefreshOut(DateTime accessTokenExpires) {
				AccessTokenExpires = accessTokenExpires;
			}
		}

		public class LoginOut {
			public string UserId { get; set; }
			public string Username { get; set; }
			public string? ProfilePhotoURL { get; set; }
			public DateTime AccessTokenExpires { get; set; }

			public LoginOut(User user, DateTime accessTokenExpires) {
				UserId = user.Id;
				Username = user.Username;
				ProfilePhotoURL = user.ProfilePhotoURL;
				AccessTokenExpires = accessTokenExpires;
			}
		}

	}
}