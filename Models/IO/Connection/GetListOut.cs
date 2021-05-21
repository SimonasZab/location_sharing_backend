using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Models.IO.Connection
{
	public class GetListOut
	{
		public class UserData
		{
			public string Id { get; set; }
			public string Username { get; set; }
			public string ProfilePhotoURL { get; set; }
		}
		public List<UserData> Users { get; set; } = new List<UserData>();
	}
}
