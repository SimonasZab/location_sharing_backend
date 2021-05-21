using location_sharing_backend.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Models.IO.Auth
{
	public class LoginOut
	{
		public class UserData
		{
			public string Id { get; set; }
			public string Username { get; set; }
			public string? ProfilePhotoURL { get; set; }
		}
		public UserData User { get; set; }
		public DateTime AccessTokenExpires { get; set; }

		public LoginOut(User user, DateTime accessTokenExpires)
		{
			User = new UserData()
			{
				Id = user.Id,
				Username = user.Username,
				ProfilePhotoURL = user.ProfilePhotoURL,
			};
			AccessTokenExpires = accessTokenExpires;
		}
	}
}
