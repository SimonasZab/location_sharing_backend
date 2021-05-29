using Api.Models.DB;
using Api.Models.Internal;
using System;

namespace Api.Models.IO.Auth
{
	public class LoginOut
	{
		public LoginOutUserData User { get; set; }
		public DateTime? AccessTokenExpires { get; set; }

		public LoginOut(User user, RaTokens raTokens)
		{
			User = new LoginOutUserData
			{
				Id = user.Id,
				Username = user.Username,
				ProfilePhotoURL = user.ProfilePhotoURL,
			};
			AccessTokenExpires = raTokens.AccessTokenData.ExpirationDate;
		}
	}
}
