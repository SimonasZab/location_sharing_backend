using Api.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models.IO.Auth
{
	public class LoginOutUserData
	{
		public string Id { get; set; }
		public string Username { get; set; }
		public string? ProfilePhotoURL { get; set; }
	}
}